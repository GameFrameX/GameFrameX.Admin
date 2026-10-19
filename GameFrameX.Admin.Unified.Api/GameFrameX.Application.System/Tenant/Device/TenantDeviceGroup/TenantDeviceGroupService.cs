using GameFrameX.Application.System.Tenant.Device.AppScope;
using GameFrameX.Application.System.Tenant.Device.TenantDeviceGroup.Dto;
using GameFrameX.Entity.Tenant.Device;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Application.System.Tenant.Device.TenantDeviceGroup;

/// <summary>
/// 设备码分组服务（应用级设备资源）。
/// <para>分组归属具体应用（AppId &gt; 0）；分组只能绑定同一应用下的设备，跨应用设备加入分组直接拒绝。</para>
/// </summary>
[ApiDescriptionSettings(Order = 100)]
public class TenantDeviceGroupService : GenericCrudSelectService<TenantGameAppDeviceGroupEntity, AddTenantDeviceGroupRequest, UpdateTenantDeviceGroupRequest, TenantDeviceGroupResponse, TenantDeviceGroupRequest>
{
    private readonly SqlSugarRepository<TenantGameAppEntity> _gameAppRepository;

    /// <summary>
    /// 设备码分组服务
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="gameAppRepository"></param>
    public TenantDeviceGroupService(SqlSugarRepository<TenantGameAppDeviceGroupEntity> repository,
        SqlSugarRepository<TenantGameAppEntity> gameAppRepository)
        : base(repository)
    {
        _gameAppRepository = gameAppRepository;
    }

    /// <summary>
    /// 增加设备码分组
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public override async Task<TenantDeviceGroupResponse> AddAsync(AddTenantDeviceGroupRequest input)
    {
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
        await EnsureDevicesBelongsToAppAsync(input.AppId, input.DeviceIds);

        var diviceGroup = new TenantGameAppDeviceGroupEntity
        {
            AppId = input.AppId,
            Name = input.Name ?? string.Empty,
            Description = input.Description,
        };
        var result = await Repository.InsertAsync(diviceGroup);
        if (result && input.DeviceIds != null && input.DeviceIds.Count > 0)
        {
            var relation = input.DeviceIds.Select(did => new TenantGameAppDeviceGroupRelationEntity
            {
                AppId = input.AppId,
                DeviceGroupId = diviceGroup.Id,
                DeviceId = did,
            }).ToList();
            await Repository.AsSugarClient().Insertable(relation).ExecuteCommandAsync();
        }

        return diviceGroup.Adapt<TenantDeviceGroupResponse>();
    }

    /// <summary>
    /// 更新前：AppId 不可变（从既有实体回填）并校验应用归属；分组设备必须同应用。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantDeviceGroupRequest input)
    {
        var existing = await Repository.GetFirstAsync(g => g.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.AppId;
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
        await EnsureDevicesBelongsToAppAsync(input.AppId, input.DeviceIds);
    }

    /// <summary>
    /// 更新后（主表已走统一 <c>InnerUpdateAsync</c> 乐观锁）：重建分组关系，只绑定同应用设备。
    /// </summary>
    protected override async Task OnUpdatedAsync(UpdateTenantDeviceGroupRequest input, TenantGameAppDeviceGroupEntity before, TenantGameAppDeviceGroupEntity after)
    {
        //删除旧的关系
        await Repository.AsSugarClient().Deleteable<TenantGameAppDeviceGroupRelationEntity>()
                        .Where(g => g.DeviceGroupId == input.Id).ExecuteCommandAsync();
        //插入新关系
        if (input.DeviceIds != null && input.DeviceIds.Count > 0)
        {
            var relation = input.DeviceIds.Select(did => new TenantGameAppDeviceGroupRelationEntity
            {
                AppId = input.AppId,
                DeviceGroupId = input.Id,
                DeviceId = did,
            }).ToList();
            await Repository.AsSugarClient().Insertable(relation).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 获取设备码详情（含设备列表）
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<TenantDeviceGroupResponse> GetDetailAsync(BaseIdRequest input)
    {
        var deviceGroup = await Repository.GetByIdAsync(input.Id);
        if (deviceGroup == null)
        {
            return new TenantDeviceGroupResponse();
        }

        //查询设备（关系表自带 AppId，天然按应用隔离）
        var devices = await Repository.AsSugarClient().Queryable<TenantGameAppDeviceGroupRelationEntity>()
                                      .Where(d => d.DeviceGroupId == deviceGroup.Id).Select(x => x.DeviceId).ToListAsync();

        var returndeviceGroup = new TenantDeviceGroupResponse
        {
            Id = deviceGroup.Id,
            AppId = deviceGroup.AppId,
            Name = deviceGroup.Name,
            Description = deviceGroup.Description,
            DeviceIds = devices,
        };

        return returndeviceGroup;
    }

    /// <summary>
    /// 分页查询按当前应用收口。
    /// </summary>
    protected override ISugarQueryable<TenantDeviceGroupResponse> BuildPageQuery(TenantDeviceGroupRequest input)
    {
        if (input.AppId == null || input.AppId <= 0)
        {
            throw BusinessError.Oh("应用Id不能为空");
        }

        var appId = input.AppId.Value;
        return Repository.AsQueryable()
                         .Where(u => u.IsDeleted == false)
                         .Where(u => u.AppId == appId)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey),
                                  u => (u.Name ?? string.Empty).Contains((input.SearchKey ?? string.Empty).Trim()))
                         .Select<TenantDeviceGroupResponse>();
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
        var pageSize = Math.Min(input.PageSize > 0 ? input.PageSize : BaseSelectPageRequest.DefaultPageSize, BaseSelectPageRequest.MaxPageSize);
        var query = await Repository.AsQueryable()
                                    .Where(u => u.IsDeleted == false)
                                    .Where(u => u.AppId == appId)
                                    .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u =>
                                                 (u.Name ?? string.Empty).Contains(searchKey))
                                    .Take(pageSize)
                                    .ToListAsync();
        var list = new List<BaseSelectPageResponse>(query.Count);
        list.AddRange(query.Select(ts => new BaseSelectPageResponse { Id = ts.Id, Value = ts.Name ?? string.Empty, Label = ts.Description ?? string.Empty, }));
        return list;
    }

    /// <summary>
    /// 校验所有设备归属当前应用，跨应用设备直接拒绝。
    /// </summary>
    private async Task EnsureDevicesBelongsToAppAsync(long appId, List<long>? deviceIds)
    {
        if (deviceIds == null || deviceIds.Count == 0)
        {
            return;
        }

        var distinctIds = deviceIds.Distinct().ToList();
        var matchCount = await Repository.AsSugarClient()
                                         .Queryable<TenantGameAppDeviceEntity>()
                                         .Where(u => u.AppId == appId && distinctIds.Contains(u.Id))
                                         .CountAsync();
        if (matchCount != distinctIds.Count)
        {
            throw BusinessError.Oh("存在不属于当前应用的设备");
        }
    }
}