using System.Reflection;
using GameFrameX.Core.Const;
using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Service.TenantMenuSync;

namespace GameFrameX.Core.Tests.TenantMenuSync;

/// <summary>
/// C353 T9：守护测试。① 模板角色 Code 集锁定（引擎按 Code 命中托管，集合变化须显式评估引擎语义）；
/// ② 引擎/闭包/计划器纯函数性（不持有 SqlSugar 依赖，可在无 DB 环境全量测试）。
/// </summary>
public class TenantMenuSyncGuardTests
{
    private static readonly string[] KnownTemplateCodes =
    {
        "sys_admin",
        "ops_manager",
        "gm_support",
        "content_ops",
        "server_ops",
        "payment_ops",
        "marketing_ops",
        "data_analyst",
        "audit_security",
        "app_config_admin",
    };

    [Fact]
    public void TemplateRoleCodeSet_IsLockedToTenKnownCodes()
    {
        var codes = TenantDefaultDataTemplates.RoleTemplates.Select(u => u.Code).OrderBy(u => u, StringComparer.Ordinal).ToArray();

        Assert.Equal(KnownTemplateCodes.OrderBy(u => u, StringComparer.Ordinal).ToArray(), codes);
    }

    [Fact]
    public void TemplateRoleCodes_AreUnique()
    {
        var codes = TenantDefaultDataTemplates.RoleTemplates.Select(u => u.Code).ToArray();
        Assert.Equal(codes.Length, codes.Distinct(StringComparer.Ordinal).Count());
    }

    [Theory]
    [InlineData(typeof(TenantMenuSyncEngine))]
    [InlineData(typeof(TenantMenuSyncClosureCalculator))]
    [InlineData(typeof(TenantMenuSyncScanPlanner))]
    [InlineData(typeof(TenantMenuSyncPackageCeiling))]
    public void PureSyncTypes_HaveNoSqlSugarOrDbDependencies(Type type)
    {
        var assembly = type.Assembly;
        Assert.All(
            new[] { type },
            _ =>
            {
                // 公共表面（含嵌套 record）不得出现 SqlSugar / DB 连接类型
                var publicTypes = new[] { type }.Concat(type.GetNestedTypes());
                foreach (var nested in publicTypes)
                {
                    Assert.DoesNotContain(nested.GetFields(), field => IsSqlSugarType(field.FieldType));
                    foreach (var ctor in nested.GetConstructors())
                    {
                        Assert.All(ctor.GetParameters(), param => Assert.False(IsSqlSugarType(param.ParameterType), $"{nested.Name} ctor 参数 {param.Name} 依赖 {param.ParameterType.Name}"));
                    }

                    foreach (var method in nested.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    {
                        Assert.All(method.GetParameters(), param => Assert.False(IsSqlSugarType(param.ParameterType), $"{nested.Name}.{method.Name} 参数 {param.Name} 依赖 {param.ParameterType.Name}"));
                    }
                }
            });
    }

    private static bool IsSqlSugarType(Type type)
    {
        return type.FullName != null
               && (type.FullName.StartsWith("SqlSugar.", StringComparison.Ordinal)
                   || type.FullName.Contains("ISqlSugarClient", StringComparison.Ordinal));
    }

    [Fact]
    public void SyncScope_MainConfigTenantIsExcluded_DefaultTenantWithOwnConfigIdIsIncluded()
    {
        // ConfigId=main 的租户（含默认租户行错误落回 main 形态）永远排除——主库不是租户同步目标
        Assert.False(TenantMenuSyncWorker.IsInSyncScope(new Entity.System.SystemTenantEntity
        {
            Id = TenantDefaultDataTemplates.MainTenantId,
            ConfigId = "main",
            TenantType = Entity.Enum.TenantTypeEnum.Db,
            IsEnabled = true,
        }));
        Assert.False(TenantMenuSyncWorker.IsInSyncScope(new Entity.System.SystemTenantEntity
        {
            Id = 2200000000001,
            ConfigId = SqlSugarConst.MainConfigId,
            TenantType = Entity.Enum.TenantTypeEnum.Db,
            IsEnabled = true,
        }));

        // C354 T6：默认租户使用独立 ConfigId（GFX-727 落库形态）时纳入同步范围——
        // Enterprise 套餐菜单启动幂等补全（AddSqlSugar → InitDefaultTenantDatabase）先于 Worker 首扫，无振荡
        Assert.True(TenantMenuSyncWorker.IsInSyncScope(new Entity.System.SystemTenantEntity
        {
            Id = TenantDefaultDataTemplates.MainTenantId,
            ConfigId = TenantDefaultDataTemplates.MainTenantId.ToString(),
            TenantType = Entity.Enum.TenantTypeEnum.Db,
            IsEnabled = true,
        }));
    }

    [Theory]
    [InlineData(false, true, "3301", Entity.Enum.TenantTypeEnum.Db, false)]   // 禁用
    [InlineData(true, false, "3301", Entity.Enum.TenantTypeEnum.Db, false)]   // 软删
    [InlineData(true, true, "3301", Entity.Enum.TenantTypeEnum.Db, true)]     // 合格
    public void SyncScope_EligibilityMatrix(bool isEnabled, bool notDeleted, string configId, Entity.Enum.TenantTypeEnum tenantType, bool expected)
    {
        var tenant = new Entity.System.SystemTenantEntity
        {
            Id = 2200000000002,
            ConfigId = configId,
            TenantType = tenantType,
            IsEnabled = isEnabled,
            IsDeleted = !notDeleted,
        };

        Assert.Equal(expected, TenantMenuSyncWorker.IsInSyncScope(tenant));
    }
}
