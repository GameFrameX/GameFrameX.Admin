using System.Reflection;
using GameFrameX.Core.Extension;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.System;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>
/// C30 表名路由对齐：验证 RepositoryExtension.GetQualifiedTableName（.AS 路径）与
/// SqlSugarEntityBoundary.GetPhysicalTableName（建表/默认 CRUD 路径）对全部实体输出一致。
/// </summary>
public class TableNameRoutingAlignmentTests
{
    [Fact]
    public void AllSugarTableEntities_QualifiedName_Matches_PhysicalName()
    {
        var entityTypes = typeof(SystemConfigEntity).Assembly.GetTypes()
                                                    .Where(t => t.IsClass && !t.IsAbstract && t.IsDefined(typeof(SugarTable), false))
                                                    .OrderBy(t => t.FullName, StringComparer.Ordinal)
                                                    .ToList();

        var getQualifiedMethod = typeof(RepositoryExtension)
            .GetMethod("GetQualifiedTableName", BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(getQualifiedMethod);

        var mismatches = new List<string>();
        var skipped = new List<string>();
        foreach (var entityType in entityTypes)
        {
            string physical;
            try
            {
                physical = SqlSugarEntityBoundary.GetPhysicalTableName(entityType, entityType.Name);
            }
            catch (InvalidOperationException)
            {
                skipped.Add($"{entityType.FullName} (无归属)");
                continue;
            }

            string qualified;
            try
            {
                var qualifiedObj = getQualifiedMethod.MakeGenericMethod(entityType).Invoke(null, null);
                if (qualifiedObj == null)
                {
                    skipped.Add($"{entityType.FullName} (反射调用返回 null)");
                    continue;
                }

                qualified = (string)qualifiedObj;
            }
            catch (ArgumentException)
            {
                skipped.Add($"{entityType.FullName} (不满足 EntityBase 约束)");
                continue;
            }

            if (!string.Equals(physical, qualified, StringComparison.Ordinal))
            {
                var ownership = SqlSugarEntityBoundary.GetOwnership(entityType);
                mismatches.Add($"{entityType.FullName} [{ownership}]: physical=\"{physical}\", qualified=\"{qualified}\"");
            }
        }

        Assert.True(mismatches.Count == 0,
                    $"表名路由不一致 ({mismatches.Count} 项):\n" + string.Join("\n", mismatches)
                                                         + (skipped.Count > 0 ? $"\n\n跳过 {skipped.Count} 项:\n" + string.Join("\n", skipped) : ""));
    }
}