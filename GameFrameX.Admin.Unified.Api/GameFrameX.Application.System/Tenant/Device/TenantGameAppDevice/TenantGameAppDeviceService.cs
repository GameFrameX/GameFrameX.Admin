using GameFrameX.Application.System.Tenant.Device.AppScope;
using GameFrameX.Application.System.Tenant.Device.TenantGameAppDevice.Dto;
using GameFrameX.Entity.Tenant.Device;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Application.System.Tenant.Device.TenantGameAppDevice;

/// <summary>
/// 设备码服务（应用级设备资源）。
/// <para>设备码归属具体应用（AppId &gt; 0），同一租户下不同应用的机器码彼此独立；
/// 机器码唯一性 = TenantId + AppId + DeviceUniqueIdentifier。</para>
/// </summary>
[ApiDescriptionSettings(Order = 100)]
public class TenantGameAppDeviceService : GenericCrudSelectService<TenantGameAppDeviceEntity, AddTenantGameAppDeviceRequest, UpdateTenantGameAppDeviceRequest, TenantGameAppDeviceResponse, TenantGameAppDeviceRequest>
{
    private readonly SqlSugarRepository<TenantGameAppEntity> _gameAppRepository;

    /// <summary>
    /// 设备码服务
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="gameAppRepository"></param>
    public TenantGameAppDeviceService(SqlSugarRepository<TenantGameAppDeviceEntity> repository,
        SqlSugarRepository<TenantGameAppEntity> gameAppRepository)
        : base(repository)
    {
        _gameAppRepository = gameAppRepository;
    }

    /// <summary>
    /// 新增前校验：AppId 必填且应用属当前租户；机器码在同应用内唯一。
    /// </summary>
    protected override async Task OnAddingAsync(AddTenantGameAppDeviceRequest input)
    {
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
        await EnsureDeviceUniqueIdentifierUniqueAsync(input.AppId, input.DeviceUniqueIdentifier, null);
    }

    /// <summary>
    /// 更新前：AppId 不可变（从既有实体回填）并校验应用归属；机器码唯一性排除自身。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameAppDeviceRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.AppId;
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
        await EnsureDeviceUniqueIdentifierUniqueAsync(input.AppId, input.DeviceUniqueIdentifier, input.Id);
    }

    /// <summary>
    /// 分页查询按当前应用收口。
    /// </summary>
    protected override ISugarQueryable<TenantGameAppDeviceResponse> BuildPageQuery(TenantGameAppDeviceRequest input)
    {
        if (input.AppId == null || input.AppId <= 0)
        {
            throw BusinessError.Oh("应用Id不能为空");
        }

        var appId = input.AppId.Value;
        return Repository.AsQueryable()
                         .Where(u => u.IsDeleted == false)
                         .Where(u => u.AppId == appId)
                         .WhereIF(input.GroupId > 0,
                                  u => SqlFunc.Subqueryable<TenantGameAppDeviceGroupRelationEntity>()
                                            .Where(r => r.AppId == appId && r.DeviceId == u.Id && r.DeviceGroupId == input.GroupId.Value)
                                            .Any())
                         .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey),
                                  u => (u.Name ?? string.Empty).Contains((input.SearchKey ?? string.Empty).Trim()))
                         .Select<TenantGameAppDeviceResponse>();
    }

    /// <summary>
    /// 获取选择器列表（按当前应用过滤）。
    /// </summary>
    [HttpPost(CrudActionRoutes.Select)]
    [ApiDescriptionSettings(Name = "Select")]
    public override async Task<List<BaseSelectPageResponse>> SelectAsync(BaseSelectPageRequest input)
    {
        if (input.AppId == null || input.AppId <= 0)
        {
            throw BusinessError.Oh("应用Id不能为空");
        }

        var appId = input.AppId.Value;
        var searchKey = input.SearchKey?.Trim() ?? string.Empty;
        var query = await Repository.AsQueryable()
                                    .Where(u => u.IsDeleted == false)
                                    .Where(u => u.AppId == appId)
                                    .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u =>
                                                 (u.Name ?? string.Empty).Contains(searchKey))
                                    .Take(100)
                                    .ToListAsync();
        var list = new List<BaseSelectPageResponse>(query.Count);
        list.AddRange(query.Select(ts => new BaseSelectPageResponse { Id = ts.Id, Value = ts.Name ?? string.Empty, Label = ts.Description ?? string.Empty, }));
        return list;
    }

    /// <summary>
    /// 校验机器码在同应用内唯一（TenantId 由租户仓储隔离保证）。
    /// </summary>
    private async Task EnsureDeviceUniqueIdentifierUniqueAsync(long appId, string? deviceUniqueIdentifier, long? excludeId)
    {
        if (string.IsNullOrWhiteSpace(deviceUniqueIdentifier))
        {
            return;
        }

        var exists = excludeId.HasValue
                         ? await Repository.IsAnyAsync(u => u.AppId == appId && u.DeviceUniqueIdentifier == deviceUniqueIdentifier && u.Id != excludeId.Value)
                         : await Repository.IsAnyAsync(u => u.AppId == appId && u.DeviceUniqueIdentifier == deviceUniqueIdentifier);
        if (exists)
        {
            throw BusinessError.Oh("机器码已存在");
        }
    }
}