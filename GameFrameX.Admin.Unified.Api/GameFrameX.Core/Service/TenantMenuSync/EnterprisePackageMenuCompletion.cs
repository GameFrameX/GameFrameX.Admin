using GameFrameX.Core.Const;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Entity.Global;
using SqlSugar;

namespace GameFrameX.Core.Service.TenantMenuSync;

/// <summary>
/// Enterprise 套餐菜单清单启动幂等补全（C354 T2）。
/// <para>Enterprise 为默认租户保留套餐（<see cref="TenantDefaultDataTemplates.EnterprisePackageId"/>），
/// 启动时收敛 <c>global_package_menu</c> 为 <see cref="TenantPackageMenuClosure.EnterprisePackageMenuIds"/>：
/// 种子演化跟随（seed 增菜单 → 套餐行自动扩）、手工删行启动恢复、幂等。
/// SuperAdmin 不可手工收窄默认租户天花板（编辑端只读保护见 GlobalPackageService）。</para>
/// <para>调用时机（AddSqlSugar → InitDefaultTenantDatabase）必须早于 TenantMenuSyncWorker 首扫
/// （HostedService 晚于 AddSqlSugar），保证默认租户纳入同步范围时闭包已就绪——「补全先于纳入」无振荡窗口。</para>
/// </summary>
public static class EnterprisePackageMenuCompletion
{
    /// <summary>
    /// 收敛 Enterprise 套餐菜单行 = 当前种子目录全集（delete + insert，幂等）。
    /// </summary>
    /// <param name="mainDb">主库连接</param>
    public static void EnsureCompleted(SqlSugarScopeProvider mainDb)
    {
        var packageId = TenantDefaultDataTemplates.EnterprisePackageId;
        var menuIds = TenantPackageMenuClosure.EnterprisePackageMenuIds(MenuSeedCatalog.AllDefinitions);

        mainDb.Deleteable<GlobalPackageMenuEntity>().Where(u => u.PackageId == packageId).ExecuteCommand();

        var createdTime = TimerHelper.UnixTimeSeconds();
        var rows = menuIds.Select(u => new GlobalPackageMenuEntity
        {
            // 主键与创建时间必须显式赋值，不可依赖 DataExecuting 数据审计：SqlSugarSetup.InitDatabase 的
            // CurrentConnectionConfig 覆盖（SqlSugarSetup.cs:428）会重置 provider 的 AOP，启动上下文实际
            // 没有审计回填，Id 留 0 会让多行插入从第二行起主键冲突致启动崩溃（GFX-798）。
            // 与 DefaultTenantDatabaseSeeder / 各 ISqlSugarEntitySeedData 种子同一约定。
            // 运行期编辑入口（GlobalPackageService.UpdatePackageMenusAsync）不在此列——请求上下文 AOP 正常。
            Id = SnowFlakeIdHelper.Instance.NextId(),
            CreatedTime = createdTime,
            PackageId = packageId,
            MenuId = u,
        }).ToArray();
        if (rows.Length > 0)
        {
            mainDb.Insertable(rows).ExecuteCommand();
        }
    }
}
