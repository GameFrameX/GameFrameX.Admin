using GameFrameX.Core.Cache;
using GameFrameX.Core.Option;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.System;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.System;
using SqlSugar;
using Xunit;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>
/// CodeFirst schema 演化回归（真实 PostgreSQL，2026-09-11 23502 事故守护）。
/// <para>背景：SQLite provider 的 CodeFirst 对存量表静默不加列、<c>DefaultValue</c> 不进建表 DDL，
/// SQLite 单测全部绿也无法证明生产 PG 可启动（password_configured 23502 与 global_menu 四列
/// EnsureColumn 修补两次同根因爆发）。本测试必须连真实 PG（<see cref="PostgresTestDatabase"/>），
/// PG 不可达时 Assert.Skip 黄跳过。</para>
/// <para>场景构造方式：先 InitDatabase 建全表并插入老行，再 DROP COLUMN 模拟"实体演化前的存量库"，
/// 二次 InitDatabase 触发 CodeFirst 加列——与生产升级（老库 + 新实体重启）完全同构。</para>
/// <para>政策：非 nullable 值类型列必须带 <c>DefaultValue</c>（PG 11+ 零重写加列，存量行读默认值），
/// 见 gfx-doc/decisions/2026-09-11-codefirst-column-default-policy.md。</para>
/// </summary>
[Collection("MenuSeedCatalog")]
public sealed class CodeFirstSchemaEvolutionPostgresTests
{
    private const long LegacyMenuId = 990001;
    private const long LegacySuperAdminId = 1300000000101;
    private const long LegacyRegionId = 99990001;

    public CodeFirstSchemaEvolutionPostgresTests()
    {
        // 种子与初始化链路会触碰静态运行期缓存，先就位测试替身
        SqlSugarCache.Configure(new FakeCache());
    }

    /// <summary>
    /// global_menu 四个种子元数据列（2026-07 C83 引入）对"缺列存量库"重放：加列成功且老行读到
    /// 与原 EnsureColumn 回填一致的默认值（false/0/false/Tenant），不再依赖手工修补。
    /// </summary>
    [SkippableFact]
    public void InitDatabase_OnLegacyGlobalMenuWithoutSeedColumns_AddsColumnsWithDefaults()
    {
        Skip.IfNot(PostgresTestDatabase.TryCreate(out var postgres, out var unavailableReason), unavailableReason);

        using (postgres)
        using (var scope = CreateScope(postgres, out var config))
        {
            var mainDatabase = scope.GetConnectionScope("main");

            // 首次初始化：全表（含四列）+ 模拟存量行
            SqlSugarSetup.InitDatabase(scope, config, new[] { typeof(GlobalMenuEntity), typeof(SystemUserEntity), });
            mainDatabase.Insertable(BuildLegacyMenu()).ExecuteCommand();

            // 模拟 2026-07 之前的存量库：DROP 四列（老行四列数据随列丢弃）
            mainDatabase.Ado.ExecuteCommand("ALTER TABLE global_menu_entity DROP COLUMN super_admin_only");
            mainDatabase.Ado.ExecuteCommand("ALTER TABLE global_menu_entity DROP COLUMN sync_roles");
            mainDatabase.Ado.ExecuteCommand("ALTER TABLE global_menu_entity DROP COLUMN allow_tenant_when_super_admin_only");
            mainDatabase.Ado.ExecuteCommand("ALTER TABLE global_menu_entity DROP COLUMN app_scope");

            // 二次初始化 = 生产升级重放：CodeFirst 检出缺列 → ALTER ADD COLUMN NOT NULL DEFAULT（零重写）
            SqlSugarSetup.InitDatabase(scope, config, new[] { typeof(GlobalMenuEntity), typeof(SystemUserEntity), });

            var legacyRow = mainDatabase.Ado.SqlQuery<dynamic>(
                "SELECT super_admin_only, sync_roles, allow_tenant_when_super_admin_only, app_scope FROM global_menu_entity WHERE id = @id",
                new SugarParameter("@id", LegacyMenuId)).Single();

            // 老行读到实体 DefaultValue（与原 EnsureColumn 回填值一致：false/0/false/Tenant=0）
            Assert.False((bool)legacyRow.super_admin_only);
            Assert.Equal(0, (int)legacyRow.sync_roles);
            Assert.False((bool)legacyRow.allow_tenant_when_super_admin_only);
            Assert.Equal((int)MenuAppScopeEnum.Tenant, (int)legacyRow.app_scope);
        }
    }


    private static GlobalMenuEntity BuildLegacyMenu()
    {
        return new GlobalMenuEntity
        {
            Id = LegacyMenuId,
            Pid = 0,
            Key = "legacy:menu",
            Title = "legacy",
            TitleCode = "legacy",
            Name = "legacy",
            Path = "/legacy",
            Type = MenuTypeEnum.Dir,
            OrderNo = 9999,
            Icon = string.Empty,
            IsEnabled = true,
            IsDeleted = false,
            CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };
    }

    private static SqlSugarScope CreateScope(PostgresTestDatabase postgres, out DbConnectionConfig config)
    {
        // BuildDbConfig 已走 SqlSugarSetup.SetDbConfig 装配列名映射等外部服务（snake_case），与生产同链路
        config = postgres.BuildDbConfig("main", enableSeed: false);
        return new SqlSugarScope(new List<ConnectionConfig> { config, });
    }
}
