using GameFrameX.Application.ClientDelivery.Device.DeviceGroup.Dto;
using GameFrameX.Entity.Business.Device;

namespace GameFrameX.Application.ClientDelivery.Device.DeviceGroup;

/// <summary>
/// 设备码分组服务。
/// <para>分组绑定主库中真实存在的设备。</para>
/// </summary>
[ApiDescriptionSettings(Order = 100)]
public class DeviceGroupService : GenericCrudSelectService<GameAppDeviceGroupEntity, AddDeviceGroupRequest, UpdateDeviceGroupRequest, DeviceGroupResponse, DeviceGroupRequest>
{

    /// <summary>
    /// 设备码分组服务
    /// </summary>
    /// <param name="repository"></param>
    public DeviceGroupService(SqlSugarRepository<GameAppDeviceGroupEntity> repository)
        : base(repository)
    {
    }

    /// <summary>
    /// 增加设备码分组
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public override async Task<DeviceGroupResponse> AddAsync(AddDeviceGroupRequest input)
    {
        await EnsureDevicesExistAsync(input.DeviceIds);

        var diviceGroup = new GameAppDeviceGroupEntity
        {
            Name = input.Name ?? string.Empty,
            Description = input.Description,
        };
        var result = await Repository.InsertAsync(diviceGroup);
        if (result && input.DeviceIds != null && input.DeviceIds.Count > 0)
        {
            var relation = input.DeviceIds.Select(did => new GameAppDeviceGroupRelationEntity
            {
                DeviceGroupId = diviceGroup.Id,
                DeviceId = did,
            }).ToList();
            await Repository.AsSugarClient().Insertable(relation).ExecuteCommandAsync();
        }

        return diviceGroup.Adapt<DeviceGroupResponse>();
    }

    /// <summary>
    /// 更新前：校验记录存在及分组设备存在。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateDeviceGroupRequest input)
    {
        var existing = await Repository.GetFirstAsync(g => g.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        await EnsureDevicesExistAsync(input.DeviceIds);
    }

    /// <summary>
    /// 更新后（主表已走统一 <c>InnerUpdateAsync</c> 乐观锁）：重建分组关系，只绑定有效设备。
    /// </summary>
    protected override async Task OnUpdatedAsync(UpdateDeviceGroupRequest input, GameAppDeviceGroupEntity before, GameAppDeviceGroupEntity after)
    {
        //删除旧的关系
        await Repository.AsSugarClient().Deleteable<GameAppDeviceGroupRelationEntity>()
                        .Where(g => g.DeviceGroupId == input.Id).ExecuteCommandAsync();
        //插入新关系
        if (input.DeviceIds != null && input.DeviceIds.Count > 0)
        {
            var relation = input.DeviceIds.Select(did => new GameAppDeviceGroupRelationEntity
            {
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
    public async Task<DeviceGroupResponse> GetDetailAsync(BaseIdRequest input)
    {
        var deviceGroup = await Repository.GetByIdAsync(input.Id);
        if (deviceGroup == null)
        {
            return new DeviceGroupResponse();
        }

        //查询分组关联设备
        var devices = await Repository.AsSugarClient().Queryable<GameAppDeviceGroupRelationEntity>()
                                      .Where(d => d.DeviceGroupId == deviceGroup.Id).Select(x => x.DeviceId).ToListAsync();

        var returndeviceGroup = new DeviceGroupResponse
        {
            Id = deviceGroup.Id,
            Name = deviceGroup.Name,
            Description = deviceGroup.Description,
            DeviceIds = devices,
        };

        return returndeviceGroup;
    }

    /// <summary>
    /// 分页查询主库设备。
    /// </summary>
    protected override ISugarQueryable<DeviceGroupResponse> BuildPageQuery(DeviceGroupRequest input)
    {
        return Repository.AsQueryable()
                         .Where(u => u.IsDeleted == false)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey),
                                  u => (u.Name ?? string.Empty).Contains((input.SearchKey ?? string.Empty).Trim()))
                         .Select<DeviceGroupResponse>();
    }

    /// <summary>
    /// 获取主库选择器列表。
    /// </summary>
    [HttpPost(CrudActionRoutes.Select)]
    [ApiDescriptionSettings(Name = "Select")]
    public override async Task<List<BaseSelectPageResponse>> SelectAsync(BaseSelectPageRequest input)
    {
        var searchKey = input.SearchKey?.Trim() ?? string.Empty;
        var pageSize = Math.Min(input.PageSize > 0 ? input.PageSize : BaseSelectPageRequest.DefaultPageSize, BaseSelectPageRequest.MaxPageSize);
        var query = await Repository.AsQueryable()
                                    .Where(u => u.IsDeleted == false)
                                    .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u =>
                                                 (u.Name ?? string.Empty).Contains(searchKey))
                                    .Take(pageSize)
                                    .ToListAsync();
        var list = new List<BaseSelectPageResponse>(query.Count);
        list.AddRange(query.Select(ts => new BaseSelectPageResponse { Id = ts.Id, Value = ts.Name ?? string.Empty, Label = ts.Description ?? string.Empty, }));
        return list;
    }

    /// <summary>
    /// 校验所有设备存在。
    /// </summary>
    private async Task EnsureDevicesExistAsync(List<long>? deviceIds)
    {
        if (deviceIds == null || deviceIds.Count == 0)
        {
            return;
        }

        var distinctIds = deviceIds.Distinct().ToList();
        var matchCount = await Repository.AsSugarClient()
                                         .Queryable<GameAppDeviceEntity>()
                                         .Where(u => distinctIds.Contains(u.Id))
                                         .CountAsync();
        if (matchCount != distinctIds.Count)
        {
            throw BusinessError.Oh("存在无效设备");
        }
    }
}