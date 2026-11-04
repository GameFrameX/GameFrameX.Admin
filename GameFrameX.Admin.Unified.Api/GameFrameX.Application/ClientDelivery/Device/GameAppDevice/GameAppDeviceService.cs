using GameFrameX.Application.ClientDelivery.Device.GameAppDevice.Dto;
using GameFrameX.Entity.Tenant.Device;

namespace GameFrameX.Application.ClientDelivery.Device.GameAppDevice;

/// <summary>
/// 设备码服务。
/// <para>机器码 DeviceUniqueIdentifier 在主库内唯一。</para>
/// </summary>
[ApiDescriptionSettings(Order = 100)]
public class GameAppDeviceService : GenericCrudSelectService<TenantGameAppDeviceEntity, AddTenantGameAppDeviceRequest, UpdateTenantGameAppDeviceRequest, GameAppDeviceResponse, GameAppDeviceRequest>
{

    /// <summary>
    /// 设备码服务
    /// </summary>
    /// <param name="repository"></param>
    public GameAppDeviceService(SqlSugarRepository<TenantGameAppDeviceEntity> repository)
        : base(repository)
    {
    }

    /// <summary>
    /// 新增前校验：机器码在主库内唯一。
    /// </summary>
    protected override async Task OnAddingAsync(AddTenantGameAppDeviceRequest input)
    {
        await EnsureDeviceUniqueIdentifierUniqueAsync(input.DeviceUniqueIdentifier, null);
    }

    /// <summary>
    /// 更新前：校验记录存在；机器码唯一性排除自身。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameAppDeviceRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        await EnsureDeviceUniqueIdentifierUniqueAsync(input.DeviceUniqueIdentifier, input.Id);
    }

    /// <summary>
    /// 分页查询主库设备。
    /// </summary>
    protected override ISugarQueryable<GameAppDeviceResponse> BuildPageQuery(GameAppDeviceRequest input)
    {
        return Repository.AsQueryable()
                         .Where(u => u.IsDeleted == false)
                         .WhereIF(input.GroupId > 0,
                                  u => SqlFunc.Subqueryable<TenantGameAppDeviceGroupRelationEntity>()
                                            .Where(r => r.DeviceId == u.Id && r.DeviceGroupId == input.GroupId.Value)
                                            .Any())
                         .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey),
                                  u => (u.Name ?? string.Empty).Contains((input.SearchKey ?? string.Empty).Trim()))
                         .Select<GameAppDeviceResponse>();
    }

    /// <summary>
    /// 获取主库选择器列表。
    /// </summary>
    [HttpPost(CrudActionRoutes.Select)]
    [ApiDescriptionSettings(Name = "Select")]
    public override async Task<List<BaseSelectPageResponse>> SelectAsync(BaseSelectPageRequest input)
    {
        var searchKey = input.SearchKey?.Trim() ?? string.Empty;
        var query = await Repository.AsQueryable()
                                    .Where(u => u.IsDeleted == false)
                                    .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u =>
                                                 (u.Name ?? string.Empty).Contains(searchKey))
                                    .Take(100)
                                    .ToListAsync();
        var list = new List<BaseSelectPageResponse>(query.Count);
        list.AddRange(query.Select(ts => new BaseSelectPageResponse { Id = ts.Id, Value = ts.Name ?? string.Empty, Label = ts.Description ?? string.Empty, }));
        return list;
    }

    /// <summary>
    /// 校验机器码在主库内唯一。
    /// </summary>
    private async Task EnsureDeviceUniqueIdentifierUniqueAsync(string? deviceUniqueIdentifier, long? excludeId)
    {
        if (string.IsNullOrWhiteSpace(deviceUniqueIdentifier))
        {
            return;
        }

        var exists = excludeId.HasValue
                         ? await Repository.IsAnyAsync(u => u.DeviceUniqueIdentifier == deviceUniqueIdentifier && u.Id != excludeId.Value)
                         : await Repository.IsAnyAsync(u => u.DeviceUniqueIdentifier == deviceUniqueIdentifier);
        if (exists)
        {
            throw BusinessError.Oh("机器码已存在");
        }
    }
}