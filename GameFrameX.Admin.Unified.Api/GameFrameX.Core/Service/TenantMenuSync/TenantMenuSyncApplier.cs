using System.Text.Json;
using GameFrameX.Core.Const;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.Log;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Tenant;
using GameFrameX.Foundation.Utility;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace GameFrameX.Core.Service.TenantMenuSync;

/// <summary>
/// 租户菜单同步 Applier（C353 T5）。每租户一次：出队执行期现场采集引擎输入 → 纯函数 diff → 租户库短事务落库
/// → 失效该租户全部用户的 KeyUserMenu / KeyUserButton 缓存 → 原子更新主库状态行 → 记操作日志。
/// <para>三层权责（业务规则承载）：套餐 ceiling（平台拥有，硬双向：超出即删、缺失即补）；
/// 模板角色 SyncRoles 基线（10 个稳定 Code，平台托管收敛，C189 同语义）；
/// 自定义角色（只夹逼不重写）。</para>
/// <para>特殊输入语义：PackageId == null ⇒ 无 ceiling（模板角色纯 C189 收敛、自定义角色不夹逼）；
/// 套餐悬空（不存在 / 软删 / 禁用）⇒ 跳过并置 failed，不删不补；
/// 套餐存在而清单为空 ⇒ 空 ceiling 全量收敛删除。</para>
/// <para>幂等：期望行主键 = <see cref="StableSeedId.RoleMenu(roleCode, menuKey)"/> 确定性派生，业务键保留现有行（含手工行），
/// 同输入重复执行零变更；租户管理页旧「授权菜单」入口（原 SystemTenantService.GrantMenuAsync，对模板角色手工授权不持久）已于 2026-09-17 随套餐语义接管而彻底移除。</para>
/// <para>租户库写入用普通 Insertable / Deleteable（lesson 2026-07-13：DiffLog 链路曾与租户表名前缀分叉，普通链路安全）。</para>
/// </summary>
public sealed class TenantMenuSyncApplier
{
    /// <summary>
    /// 悬空套餐的 distinct 错误语义（状态行 LastError 前缀，区别于基础设施异常）
    /// </summary>
    public const string DanglingPackageErrorPrefix = "dangling-package:";

    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly ITenantDbConnectionProvider _tenantDbConnectionProvider;
    private readonly SystemCacheService _systemCacheService;
    private readonly ILogger<TenantMenuSyncApplier> _logger;

    public TenantMenuSyncApplier(
        ISqlSugarClient sqlSugarClient,
        ITenantDbConnectionProvider tenantDbConnectionProvider,
        SystemCacheService systemCacheService,
        ILogger<TenantMenuSyncApplier> logger)
    {
        _sqlSugarClient = sqlSugarClient;
        _tenantDbConnectionProvider = tenantDbConnectionProvider;
        _systemCacheService = systemCacheService;
        _logger = logger;
    }

    /// <summary>
    /// 执行单租户同步（成功与业务性跳过均返回结果；基础设施异常上抛由调用方置 failed）。
    /// </summary>
    /// <param name="tenant">目标租户（同步范围枚举口径内的存活启用 Db 租户）</param>
    /// <param name="triggerReason">触发原因（startup / scan / package-saved / manual-*，进操作日志）</param>
    public async Task<TenantMenuSyncResult> SyncTenantAsync(SystemTenantEntity tenant, string triggerReason)
    {
        var startedAt = TimerHelper.UnixTimeSeconds();
        var mainDb = _sqlSugarClient.AsTenant().GetConnectionScope(SqlSugarConst.MainConfigId);

        // 1. 出队执行期现场计算闭包（不用入队时刻的陈旧输入）
        var ceiling = await ComputeCeilingAsync(_sqlSugarClient, tenant);

        if (ceiling.Kind == TenantMenuSyncCeilingKind.Dangling)
        {
            var danglingError = $"{DanglingPackageErrorPrefix}套餐 {tenant.PackageId} 不存在、已软删或已禁用，跳过同步等待人工修正";
            await UpsertStateAsync(mainDb, tenant.Id, TenantMenuSyncStatusEnum.Failed, danglingError, retryDelaySeconds: null);
            await WriteOperationLogAsync(mainDb, tenant.Id, triggerReason, "Failed", 0, 0, danglingError, startedAt);
            _logger.LogWarning("租户 {TenantId} 套餐 {PackageId} 悬空，同步跳过并置 failed。", tenant.Id, tenant.PackageId);
            return new TenantMenuSyncResult(tenant.Id, Succeeded: false, SkippedDangling: true, InsertCount: 0, DeleteCount: 0);
        }

        var tenantDb = _tenantDbConnectionProvider.GetTenantDbConnectionScope(tenant.Id);
        if (tenantDb == null)
        {
            throw new InvalidOperationException($"租户 {tenant.Id} 的租户库连接不可用。");
        }

        // 2. 租户库快照（实体取回后内存映射：record 构造器投影在部分 provider 的 DataReader 绑定层不可靠）
        var roleEntities = await tenantDb.Queryable<TenantRoleEntity>().ToListAsync();
        var roles = roleEntities.Select(u => new TenantMenuSyncEngine.RoleSnapshot(u.Id, u.Code)).ToList();
        var roleMenuEntities = await tenantDb.Queryable<TenantRoleMenuEntity>().ToListAsync();
        var roleMenus = roleMenuEntities.Select(u => new TenantMenuSyncEngine.RoleMenuRowSnapshot(u.Id, u.RoleId, u.MenuId)).ToList();

        // 3. 纯函数 diff
        var input = new TenantMenuSyncEngine.Input(
            tenant.Id,
            ceiling,
            MenuSeedCatalog.AllDefinitions,
            TenantDefaultDataTemplates.RoleTemplates
                                     .Select(u => new TenantMenuSyncEngine.RoleTemplateSnapshot(u.Code, u.SyncRole))
                                     .ToArray(),
            roles,
            roleMenus);
        var diff = TenantMenuSyncEngine.Compute(input);

        // 4. 租户库短事务落库（普通 Insertable / Deleteable）
        if (diff.Inserts.Count > 0 || diff.DeleteRowIds.Count > 0)
        {
            var transaction = await tenantDb.Ado.UseTranAsync(async () =>
            {
                if (diff.DeleteRowIds.Count > 0)
                {
                    await tenantDb.Deleteable<TenantRoleMenuEntity>()
                                  .Where(u => diff.DeleteRowIds.Contains(u.Id))
                                  .ExecuteCommandAsync();
                }

                if (diff.Inserts.Count > 0)
                {
                    var rows = diff.Inserts.Select(u => new TenantRoleMenuEntity
                                       {
                                           Id = u.Id,
                                           RoleId = u.RoleId,
                                           MenuId = u.MenuId,
                                       }).ToArray();
                    await tenantDb.Insertable(rows).ExecuteCommandAsync();
                }
            });
            if (!transaction.IsSuccess)
            {
                throw new InvalidOperationException($"租户 {tenant.Id} role-menu 落库事务失败。", transaction.ErrorException);
            }
        }

        // 5. 失效该租户全部用户的双 key 缓存（现 key 为纯 userId 后缀，按前缀清除会误伤他租户，必须逐 userId Remove）
        var userIds = await tenantDb.Queryable<TenantUserEntity>().Select(u => u.Id).ToListAsync();
        foreach (var userId in userIds)
        {
            _systemCacheService.Remove(CacheConst.KeyUserMenu + userId);
            _systemCacheService.Remove(CacheConst.KeyUserButton + userId);
        }

        // 6. 原子更新状态行（成功语义：版本号推进）
        var warningText = diff.Warnings.Count > 0
            ? TruncateError(string.Join("; ", diff.Warnings.Select(u => $"{u.Code}:{u.Message}")))
            : null;
        await UpsertStateAsync(mainDb, tenant.Id, TenantMenuSyncStatusEnum.Succeeded, warningText, retryDelaySeconds: null);

        // 7. 操作日志（既有 system_log_operation 承载审计）
        await WriteOperationLogAsync(mainDb, tenant.Id, triggerReason, "Succeeded", diff.Inserts.Count, diff.DeleteRowIds.Count, warningText, startedAt);

        return new TenantMenuSyncResult(tenant.Id, Succeeded: true, SkippedDangling: false, diff.Inserts.Count, diff.DeleteRowIds.Count);
    }

    /// <summary>
    /// 基础设施异常路径的失败状态行落库（Worker 捕获异常后调用；失败保留旧 LastCatalogVersion，版本比对仍会重新入队）。
    /// </summary>
    public async Task MarkFailedAsync(long tenantId, string error, int retryDelaySeconds)
    {
        var mainDb = _sqlSugarClient.AsTenant().GetConnectionScope(SqlSugarConst.MainConfigId);
        await UpsertStateAsync(mainDb, tenantId, TenantMenuSyncStatusEnum.Failed, TruncateError(error), retryDelaySeconds);
    }

    private static string TruncateError(string error)
    {
        if (string.IsNullOrEmpty(error))
        {
            return "unknown-error";
        }

        return error.Length > 1000 ? error[..1000] : error;
    }

    /// <summary>
    /// 计算租户套餐天花板（出队执行期 / 新租户创建期现场计算，公共入口供同步与新租户初始授权共用）。
    /// <para>Enterprise（默认租户保留套餐）闭包额外并入全部模板角色 SyncRoles 期望集的并集：
    /// 闭包计算器的 SAO 防御性剔除会剥掉带角色旗标的 SAO 菜单（含 tenant-readable 辅助按钮），
    /// 若不并回，默认租户同步会把 C189 seeder 写入的这批行当 surplus 删除、重启又被 seeder 补回——振荡。
    /// 并回后 Enterprise 闭包 ⊇ 每个模板角色的引擎期望集（夹取恒为无操作），默认租户收敛结果 = C189 全量（守护测试锁定）；
    /// 无任何模板角色旗标的纯 SAO 菜单仍在闭包外。</para>
    /// </summary>
    /// <param name="sqlSugarClient">SqlSugar 客户端（内部解析主库连接）</param>
    /// <param name="tenant">目标租户（仅消费 PackageId）</param>
    public static async Task<TenantMenuSyncPackageCeiling> ComputeCeilingAsync(ISqlSugarClient sqlSugarClient, SystemTenantEntity tenant)
    {
        var mainDb = sqlSugarClient.AsTenant().GetConnectionScope(SqlSugarConst.MainConfigId);

        bool packageUsable;
        if (tenant.PackageId == null)
        {
            packageUsable = false;
        }
        else
        {
            var package = await mainDb.Queryable<GlobalPackageEntity>()
                                      .Filter(null, true)
                                      .FirstAsync(u => u.Id == tenant.PackageId);
            packageUsable = package != null && package.IsDeleted != true && package.IsEnabled == true;
        }

        List<long> packageMenuIds = new();
        if (tenant.PackageId != null && packageUsable)
        {
            packageMenuIds = await mainDb.Queryable<GlobalPackageMenuEntity>()
                                         .Where(u => u.PackageId == tenant.PackageId)
                                         .Select(u => u.MenuId)
                                         .ToListAsync();
        }

        var ceiling = TenantMenuSyncClosureCalculator.Compute(tenant.PackageId, packageUsable, packageMenuIds, MenuSeedCatalog.AllDefinitions);
        if (tenant.PackageId == TenantDefaultDataTemplates.EnterprisePackageId
            && ceiling.Kind == TenantMenuSyncCeilingKind.Bounded)
        {
            // 无振荡不变式：Enterprise ceiling ⊇ 全部模板角色 SyncRoles 期望集的并集 ⇒ 引擎夹取对模板角色恒为无操作，
            // C189 seeder 写入的默认租户基线（含 tenant-readable SAO 按钮与少量带角色旗标的 SAO 菜单）是不动点。
            // 真正的纯 SAO 菜单（无任何模板角色旗标）不在此并集内，仍被排除（守护测试锁定）。
            var menuIds = new HashSet<long>(ceiling.MenuIds.Count);
            menuIds.UnionWith(ceiling.MenuIds);
            foreach (var template in TenantDefaultDataTemplates.RoleTemplates)
            {
                if (template.SyncRole == TenantMenuSyncRole.None)
                {
                    continue;
                }

                menuIds.UnionWith(MenuSeedCatalog.GetSyncedMenuIds(MenuSeedCatalog.AllDefinitions, template.SyncRole));
            }

            ceiling = TenantMenuSyncPackageCeiling.Bounded(menuIds);
        }

        return ceiling;
    }

    private static async Task UpsertStateAsync(
        SqlSugarScopeProvider mainDb,
        long tenantId,
        TenantMenuSyncStatusEnum status,
        string? error,
        int? retryDelaySeconds)
    {
        var now = TimerHelper.UnixTimeSeconds();
        var existing = await mainDb.Queryable<SystemTenantMenuSyncStateEntity>()
                                   .Filter(null, true)
                                   .FirstAsync(u => u.Id == tenantId);
        if (existing == null)
        {
            await mainDb.Insertable(new SystemTenantMenuSyncStateEntity
            {
                Id = tenantId,
                LastCatalogVersion = status == TenantMenuSyncStatusEnum.Succeeded ? MenuSeedCatalog.CatalogVersion : string.Empty,
                Status = status,
                LastSyncTime = now,
                LastError = error,
                NextRetryTime = status == TenantMenuSyncStatusEnum.Failed && retryDelaySeconds != null ? now + retryDelaySeconds : null,
            }).ExecuteCommandAsync();
            return;
        }

        // 失败保留旧 LastCatalogVersion（版本不推进，扫描按版本比对仍会重新入队）；成功推进版本并清空错误
        if (status == TenantMenuSyncStatusEnum.Succeeded)
        {
            await mainDb.Updateable<SystemTenantMenuSyncStateEntity>()
                        .SetColumns(u => new SystemTenantMenuSyncStateEntity
                        {
                            LastCatalogVersion = MenuSeedCatalog.CatalogVersion,
                            Status = status,
                            LastSyncTime = now,
                            LastError = error,
                            NextRetryTime = null,
                            UpdateTime = now,
                        })
                        .Where(u => u.Id == tenantId)
                        .ExecuteCommandAsync();
        }
        else
        {
            await mainDb.Updateable<SystemTenantMenuSyncStateEntity>()
                        .SetColumns(u => new SystemTenantMenuSyncStateEntity
                        {
                            Status = status,
                            LastSyncTime = now,
                            LastError = error,
                            NextRetryTime = retryDelaySeconds != null ? now + retryDelaySeconds : null,
                            UpdateTime = now,
                        })
                        .Where(u => u.Id == tenantId)
                        .ExecuteCommandAsync();
        }
    }

    private static async Task WriteOperationLogAsync(
        SqlSugarScopeProvider mainDb,
        long tenantId,
        string triggerReason,
        string status,
        int insertCount,
        int deleteCount,
        string? detail,
        long startedAtUnixSeconds)
    {
        var summary = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            ["tenantId"] = tenantId,
            ["trigger"] = triggerReason,
            ["inserts"] = insertCount,
            ["deletes"] = deleteCount,
            ["detail"] = detail ?? string.Empty,
        });
        var elapsed = TimerHelper.UnixTimeSeconds() - startedAtUnixSeconds;

        await mainDb.Insertable(new SystemLogOperationEntity
        {
            ControllerName = "TenantMenuSync",
            ActionName = "SyncTenant",
            DisplayTitle = "套餐驱动的租户菜单权限自动同步",
            Status = status,
            Message = summary,
            Elapsed = elapsed,
        }).ExecuteCommandAsync();
    }
}

/// <summary>
/// 单租户同步执行结果。
/// </summary>
public sealed record TenantMenuSyncResult(long TenantId, bool Succeeded, bool SkippedDangling, int InsertCount, int DeleteCount);
