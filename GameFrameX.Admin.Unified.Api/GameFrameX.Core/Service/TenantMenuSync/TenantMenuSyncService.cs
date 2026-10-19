using GameFrameX.Core.Const;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.Service.TenantMenuSync.Dto;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Service.TenantMenuSync;

/// <summary>
/// 租户菜单同步手动触发服务（C353 T7）。SuperAdmin 运维逃生通道（触发⑤）：
/// 同步本体由后台 Worker 异步执行，端点只做权限校验与入队，返回实际入队租户数（队列去重下可能为 0）。
/// <para>权限口径：端点自身无菜单种子（负面清单禁改 GlobalMenuSeedData），依赖 U6 自动绑定 pid=0 隐藏按钮
/// <c>permission:auto:*</c> —— 租户侧 fail-closed 收敛不可见，SuperAdmin 侧放行（缓存预热即全量端点）。</para>
/// </summary>
[ApiDescriptionSettings(Order = 392)]
public class TenantMenuSyncService : IAdminApiController
{
    private readonly SqlSugarRepository<SystemTenantEntity> _systemTenantRepository;
    private readonly SqlSugarRepository<GlobalPackageEntity> _globalPackageRepository;
    private readonly TenantMenuSyncQueue _queue;
    private readonly CurrentUserContext _currentUserContext;

    public TenantMenuSyncService(
        SqlSugarRepository<SystemTenantEntity> systemTenantRepository,
        SqlSugarRepository<GlobalPackageEntity> globalPackageRepository,
        TenantMenuSyncQueue queue,
        CurrentUserContext currentUserContext)
    {
        _systemTenantRepository = systemTenantRepository;
        _globalPackageRepository = globalPackageRepository;
        _queue = queue;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// 手动同步单个租户菜单（SuperAdmin 逃生通道）
    /// </summary>
    [ApiDescriptionSettings(Name = "SyncTenant")]
    [HttpPost("syncTenant")]
    [DisplayName("手动同步单个租户菜单")]
    public async Task<int> SyncTenantAsync(SyncTenantMenuRequest input)
    {
        AssertSuperAdmin();

        var tenant = await _systemTenantRepository.AsQueryable()
                                  .Filter(null, true)
                                  .FirstAsync(u => u.Id == input.TenantId);
        var inScope = tenant != null
                      && tenant.IsDeleted != true
                      && tenant.IsEnabled == true
                      && tenant.TenantType == TenantTypeEnum.Db
                      && tenant.ConfigId != SqlSugarConst.MainConfigId;
        if (!inScope)
        {
            throw BusinessError.Oh(TenantErrorCode.MenuSyncTenantNotInScope);
        }

        return _queue.TryEnqueue(tenant!.Id, "manual:single") ? 1 : 0;
    }

    /// <summary>
    /// 按套餐手动同步其全部租户菜单（SuperAdmin 逃生通道）
    /// </summary>
    [ApiDescriptionSettings(Name = "SyncPackage")]
    [HttpPost("syncPackage")]
    [DisplayName("按套餐手动同步租户菜单")]
    public async Task<int> SyncPackageAsync(SyncTenantMenuByPackageRequest input)
    {
        AssertSuperAdmin();

        var packageExists = await _globalPackageRepository.AsQueryable()
                                            .Filter(null, true)
                                            .AnyAsync(u => u.Id == input.PackageId);
        if (!packageExists)
        {
            throw BusinessError.Oh(TenantErrorCode.MenuSyncPackageNotFound);
        }

        var tenants = await _systemTenantRepository.AsQueryable()
                                   .Filter(null, true)
                                   .Where(u => u.PackageId == input.PackageId
                                               && u.TenantType == TenantTypeEnum.Db
                                               && u.IsEnabled == true
                                               && u.ConfigId != SqlSugarConst.MainConfigId)
                                   .Select(u => u.Id)
                                   .ToListAsync();

        var enqueued = 0;
        foreach (var tenantId in tenants)
        {
            if (_queue.TryEnqueue(tenantId, "manual:package"))
            {
                enqueued++;
            }
        }

        return enqueued;
    }

    /// <summary>
    /// 手动同步全部租户菜单（SuperAdmin 逃生通道；含套餐悬空租户，由 Applier 置 failed 不删不补）
    /// </summary>
    [ApiDescriptionSettings(Name = "SyncAll")]
    [HttpPost("syncAll")]
    [DisplayName("手动同步全部租户菜单")]
    public async Task<int> SyncAllAsync()
    {
        AssertSuperAdmin();

        var tenants = await _systemTenantRepository.AsQueryable()
                                   .Filter(null, true)
                                   .Where(u => u.TenantType == TenantTypeEnum.Db
                                               && u.IsEnabled == true
                                               && u.ConfigId != SqlSugarConst.MainConfigId)
                                   .Select(u => u.Id)
                                   .ToListAsync();

        var enqueued = 0;
        foreach (var tenantId in tenants)
        {
            if (_queue.TryEnqueue(tenantId, "manual:all"))
            {
                enqueued++;
            }
        }

        return enqueued;
    }

    /// <summary>
    /// SuperAdmin 才可手动触发（同步引擎五触发中的逃生通道，非运维入口常态使用面）
    /// </summary>
    private void AssertSuperAdmin()
    {
        if (!_currentUserContext.SuperAdmin)
        {
            throw BusinessError.Oh(TenantErrorCode.MenuSyncSuperAdminOnly);
        }
    }
}
