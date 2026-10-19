using System.Reflection;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Routing;
using GameFrameX.Foundation.Orm.Attribute;
using GameFrameX.Entity.System;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

// C34 阶段 A 锚点测试：实体名即表名、Attribute 即归属。
// 这三个测试在阶段 A 预期为 RED（失败），其失败列表即实体命名矩阵与重命名/补 Attribute 的待办清单；
// 阶段 B 完成实体重命名、Attribute 补齐、SqlSugarEntityBoundary 中心化映射删除后转 GREEN。
//
// 互斥边界 Attribute 集合为：PlatformTable / TenantTable / GlobalTable。
// [SystemTable] 来自外部包，属兼容元数据，不参与 Admin 数据边界路由，不计入互斥校验。

public class EntityTableNameAlignmentTests
{
    /// <summary>
    /// 受管持久化实体必须且只能拥有一个边界 Attribute（PlatformTable/TenantTable/GlobalTable）。
    /// 阶段 A 预期 RED：当前大量实体未补齐边界 Attribute，依赖 SqlSugarEntityBoundary 的实体名 HashSet 判定归属。
    /// </summary>
    [Fact]
    public void AllEntities_HaveExactlyOneBoundaryAttribute()
    {
        var failures = EntityTypes()
                       .Select(type => new { Type = type, Count = BoundaryAttributeCount(type), })
                       .Where(item => item.Count != 1)
                       .Select(item => $"{item.Type.Name} boundary-count={item.Count}")
                       .ToList();

        Assert.Empty(failures);
    }

    /// <summary>
    /// 实体类名按 UtilMethods.ToUnderLine 转换后必须等于真实物理表名，
    /// 不依赖 SqlSugarEntityBoundary.ExplicitBaseTableNames 中心化映射。
    /// 阶段 A 预期 RED：当前 Sys*/Game* 等实体类名下划线化与物理表名不一致。
    /// 失败列表同时作为 C34 命名矩阵的权威数据源（className 列即目标类名下划线化的反向推导依据）。
    /// </summary>
    [Fact]
    public void AllEntities_ClassNameUnderlinedEqualsPhysicalTableName()
    {
        var failures = EntityTypes()
                       .Select(type => new
                       {
                           Type = type,
                           Expected = UtilMethods.ToUnderLine(type.Name),
                           Actual = SqlSugarEntityBoundary.GetPhysicalTableName(
                               type,
                               type.GetCustomAttribute<SugarTable>()?.TableName ?? type.Name),
                       })
                       .Where(item => !string.Equals(item.Expected, item.Actual, StringComparison.Ordinal))
                       .Select(item => $"{item.Type.Name}|className={item.Expected}|physical={item.Actual}")
                       .ToList();

        Assert.Empty(failures);
    }

    /// <summary>
    /// 实体类名前缀必须与其边界归属的物理前缀一致：
    /// System* -> system_、Global* -> global_、Tenant* -> tenant_、Log* -> log_。
    /// 阶段 A 预期 RED：阶段 B 完成实体重命名 + Attribute 补齐后转 GREEN。
    /// </summary>
    [Fact]
    public void AllEntities_ClassNamePrefixMatchesBoundaryOwnership()
    {
        var failures = EntityTypes()
                       .Select(type => new
                       {
                           Type = type,
                           Ownership = SqlSugarEntityBoundary.GetOwnership(type),
                           ClassName = type.Name,
                       })
                       .Select(item => new
                       {
                           item.Type,
                           item.Ownership,
                           item.ClassName,
                           ExpectedPrefix = ClassExpectedPrefix(item.Ownership),
                       })
                       .Where(item => !item.ClassName.StartsWith(item.ExpectedPrefix, StringComparison.Ordinal))
                       .Select(item => $"{item.ClassName}|ownership={item.Ownership}|expectedPrefix={item.ExpectedPrefix}")
                       .ToList();

        Assert.Empty(failures);
    }

    private static IEnumerable<Type> EntityTypes()
    {
        return typeof(SystemUserEntity).Assembly.GetTypes()
                                       .Where(type => type is { IsClass: true, IsAbstract: false, }
                                                      && type.IsDefined(typeof(SugarTable), false));
    }

    private static int BoundaryAttributeCount(Type type)
    {
        var count = 0;
        if (type.IsDefined(typeof(PlatformTableAttribute), false))
        {
            count++;
        }

        if (type.IsDefined(typeof(TenantTableAttribute), false))
        {
            count++;
        }

        if (type.IsDefined(typeof(GlobalTableAttribute), false))
        {
            count++;
        }

        return count;
    }

    private static string ClassExpectedPrefix(DatabaseOwnership ownership)
    {
        return ownership switch
        {
            DatabaseOwnership.System => "System",
            DatabaseOwnership.Global => "Global",
            DatabaseOwnership.Tenant => "Tenant",
            _                        => string.Empty,
        };
    }

    /// <summary>
    /// 诊断用：导出实体对齐矩阵到 /tmp/c34-matrix.txt，供 notes.md 命名矩阵引用。
    /// 阶段 A 一次性使用，阶段 B 命名收敛后随诊断逻辑一并移除。
    /// </summary>
    [Fact]
    public void Diagnostic_DumpAlignmentMatrix()
    {
        var lines = EntityTypes().Select(type =>
        {
            try
            {
                var ownership = SqlSugarEntityBoundary.GetOwnership(type);
                var physical = SqlSugarEntityBoundary.GetPhysicalTableName(
                    type,
                    type.GetCustomAttribute<SugarTable>()?.TableName ?? type.Name);
                var underlined = UtilMethods.ToUnderLine(type.Name);
                var target = ToPascalCase(physical);
                var needsRename = underlined != physical;
                return string.Join("|", type.Name, type.Namespace ?? string.Empty, ownership, BoundaryAttributeCount(type), physical, underlined, target, needsRename ? "RENAME" : "ok");
            }
            catch (Exception ex)
            {
                return string.Join("|", type.Name, type.Namespace ?? string.Empty, "UNCLASSIFIED", BoundaryAttributeCount(type), "ERR", UtilMethods.ToUnderLine(type.Name), string.Empty, ex.GetType().Name);
            }
        });

        File.WriteAllText("/tmp/c34-matrix.txt", string.Join("\n", lines) + "\n");
        Assert.True(true);
    }

    private static string ToPascalCase(string underlined)
    {
        var parts = underlined.Split('_');
        var result = string.Empty;
        foreach (var part in parts)
        {
            if (part.Length == 0)
            {
                continue;
            }

            result += char.ToUpperInvariant(part[0]) + part.Substring(1);
        }

        return result;
    }
}