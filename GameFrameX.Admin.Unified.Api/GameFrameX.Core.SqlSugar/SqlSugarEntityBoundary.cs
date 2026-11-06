using System.Reflection;
using GameFrameX.Entity.Routing;
using SqlSugar;

namespace GameFrameX.Core.SqlSugar;

public static class SqlSugarEntityBoundary
{
    private static readonly IReadOnlyDictionary<DatabaseOwnership, string> Prefixes =
        new Dictionary<DatabaseOwnership, string>
        {
            [DatabaseOwnership.System] = "system_",
            [DatabaseOwnership.Global] = "global_",
            [DatabaseOwnership.Business] = string.Empty,
        };

    public static bool IsPlatformTable(Type entityType)
    {
        return GetOwnership(entityType) == DatabaseOwnership.System;
    }

    public static bool IsBusinessTable(Type entityType)
    {
        return GetOwnership(entityType) == DatabaseOwnership.Business;
    }

    public static bool IsMainDatabaseTable(Type entityType)
    {
        _ = GetOwnership(entityType);
        return true;
    }

    /// <summary>
    /// 实体归属完全由三类边界 Attribute 决定：<see cref="GlobalTableAttribute"/>（global_）、
    /// <see cref="PlatformTableAttribute"/>（system_）、<see cref="BusinessTableAttribute"/>（无前缀）。
    /// 未标注任一边界 Attribute 的受管持久化实体将抛出 <see cref="InvalidOperationException"/>。
    /// C156 起：日志不再使用独立日志库，平台日志归入 <c>system_log_*</c>（system_ 归属），业务日志归入 <c>log_*</c>（业务归属，无前缀）。
    /// </summary>
    public static DatabaseOwnership GetOwnership(Type entityType)
    {
        EnsureSingleBoundary(entityType);

        if (entityType.IsDefined(typeof(GlobalTableAttribute), false))
        {
            return DatabaseOwnership.Global;
        }

        if (entityType.IsDefined(typeof(PlatformTableAttribute), false))
        {
            return DatabaseOwnership.System;
        }

        if (entityType.IsDefined(typeof(BusinessTableAttribute), false))
        {
            return DatabaseOwnership.Business;
        }

        throw new InvalidOperationException($"Entity {entityType.FullName} is not assigned to system_, business, or global_ ownership.");
    }

    public static string GetPhysicalTableName(Type entityType, string currentTableName)
    {
        var ownership = GetOwnership(entityType);
        var prefix = Prefixes[ownership];
        var baseName = ResolveBaseTableName(entityType, currentTableName);
        return prefix + StripKnownPrefix(baseName);
    }

    public static bool IsPhysicalTableNameAllowedForDatabase(DatabaseOwnership databaseOwnership, string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            return false;
        }

        return databaseOwnership switch
        {
            DatabaseOwnership.System => tableName.StartsWith(Prefixes[DatabaseOwnership.System], StringComparison.Ordinal)
                                        || tableName.StartsWith(Prefixes[DatabaseOwnership.Global], StringComparison.Ordinal),
            DatabaseOwnership.Global => tableName.StartsWith(Prefixes[DatabaseOwnership.System], StringComparison.Ordinal)
                                        || tableName.StartsWith(Prefixes[DatabaseOwnership.Global], StringComparison.Ordinal),
            // 业务表不带前缀；只排除平台/全局前缀，防止误占保留前缀。
            DatabaseOwnership.Business => !tableName.StartsWith(Prefixes[DatabaseOwnership.System], StringComparison.Ordinal)
                                          && !tableName.StartsWith(Prefixes[DatabaseOwnership.Global], StringComparison.Ordinal),
            _                         => false,
        };
    }

    public static bool HasBoundaryConflict(Type entityType)
    {
        var markerCount = 0;
        markerCount += entityType.IsDefined(typeof(PlatformTableAttribute), false) ? 1 : 0;
        markerCount += entityType.IsDefined(typeof(BusinessTableAttribute), false) ? 1 : 0;
        markerCount += entityType.IsDefined(typeof(GlobalTableAttribute), false) ? 1 : 0;
        return markerCount > 1;
    }

    public static void EnsureSingleBoundary(Type entityType)
    {
        if (HasBoundaryConflict(entityType))
        {
            throw new InvalidOperationException($"Entity {entityType.FullName} has multiple database routing attributes.");
        }
    }

    private static string ResolveBaseTableName(Type entityType, string currentTableName)
    {
        var sugarTable = entityType.GetCustomAttribute<SugarTable>();
        var tableName = !string.IsNullOrWhiteSpace(sugarTable?.TableName)
                            ? sugarTable.TableName
                            : currentTableName;

        return UtilMethods.ToUnderLine(tableName);
    }

    private static string StripKnownPrefix(string tableName)
    {
        foreach (var prefix in Prefixes.Values)
        {
            // 空前缀（业务归属）不参与剥离：空前缀 StartsWith 恒为真。
            if (prefix.Length == 0)
            {
                continue;
            }

            if (tableName.StartsWith(prefix, StringComparison.Ordinal))
            {
                return tableName[prefix.Length..];
            }
        }

        return tableName;
    }
}
