using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.RedeemCode.GameRedeemCodeUseRecord.Dto;

namespace GameFrameX.Application.Game.RedeemCode.GameRedeemCodeUseRecord;

/// <summary>
/// 兑换记录服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameRedeemCodeUseRecordEntityService
    : GenericCrudService<Entity.Tenant.Game.TenantGameRedeemCodeUseRecordEntity,
        AddTenantGameRedeemCodeUseRecordEntityRequest, UpdateTenantGameRedeemCodeUseRecordEntityRequest,
        TenantGameRedeemCodeUseRecordEntityResponse, PageTenantGameRedeemCodeUseRecordEntityRequest>
{
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAreaEntity> _areaRepository;

    public TenantGameRedeemCodeUseRecordEntityService(
        SqlSugarRepository<Entity.Tenant.Game.TenantGameRedeemCodeUseRecordEntity> repository,
        SqlSugarRepository<Entity.Tenant.Game.TenantGameAreaEntity> areaRepository)
        : base(repository)
    {
        _areaRepository = areaRepository;
    }

    /// <summary>
    /// 新增前：兑换码使用记录由客户端兑换链路产生，事实数据 AppId 必须来自可信写入链路；
    /// 禁止管理后台直接创建使用记录，避免写入 AppId=0 违反事实数据应用作用域铁律。
    /// </summary>
    protected override Task OnAddingAsync(AddTenantGameRedeemCodeUseRecordEntityRequest input)
    {
        throw new Exception("兑换码使用记录由客户端兑换链路产生，不支持通过管理后台直接创建");
    }

    protected override ISugarQueryable<TenantGameRedeemCodeUseRecordEntityResponse> BuildPageQuery(PageTenantGameRedeemCodeUseRecordEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "兑换记录");
        return Repository.AsQueryable()
                         .WhereIF(input.AreaId > 0, u => u.AreaId == input.AreaId)
                         .Where(u => u.AppId == appId)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Code), u => u.Code == input.Code)
                         .WhereIF(input.PlayerId > 0, u => u.PlayerId == input.PlayerId)
                         //处理外键和TreeSelector相关字段的连接
                         .LeftJoin<Entity.Tenant.Game.TenantGameAreaEntity>((u, areaid) => u.AreaId == areaid.Id)
                         //关联兑换码（Code+AreaId 复合，回避跨区同名兑换码歧义）取渠道ID，再关联渠道取名称
                         .LeftJoin<Entity.Tenant.Game.TenantGameRedeemCodeEntity>((u, areaid, code) => u.Code == code.Code && u.AreaId == code.AreaId)
                         .LeftJoin<Entity.Tenant.Game.TenantGameRedeemCodeChannelEntity>((u, areaid, code, channel) => code.ChannelId == channel.Id)
                         .WhereIF(input.ChannelId > 0, (u, areaid, code, channel) => code.ChannelId == input.ChannelId)
                         .Select((u, areaid, code, channel) => new TenantGameRedeemCodeUseRecordEntityResponse
                         {
                             Id = u.Id,
                             AppId = u.AppId,
                             AreaId = u.AreaId,
                             Code = u.Code,
                             PlayerId = u.PlayerId,
                             PlayerIdString = u.PlayerId.ToString(),
                             ChannelId = code.ChannelId,
                             ChannelName = channel.Name,
                             CreatedTime = u.CreatedTime,
                             UpdateTime = u.UpdateTime,
                             CreatedName = u.CreatedName,
                             UpdatedName = u.UpdatedName,
                             // 联表手写投影必须显式带版本号（缺了响应恒 null，乐观锁操作以 null 比较直接 500）
                             RowVersion = u.RowVersion,
                         });
    }

    protected override ISugarQueryable<TenantGameRedeemCodeUseRecordEntityResponse> ApplyOrdering(ISugarQueryable<TenantGameRedeemCodeUseRecordEntityResponse> query, PageTenantGameRedeemCodeUseRecordEntityRequest input)
    {
        if (!string.IsNullOrEmpty(input.OrderBy))
        {
            query = query.OrderByIF(true, $"u.{input.OrderBy} {(input.IsDescending ? "Desc" : "Asc")}");
        }
        else
        {
            query = query.OrderByIF(true, "u.created_time Desc");
        }

        return query;
    }

    /// <summary>
    /// 获取区服下拉列表（AreaId 外键关联）
    /// </summary>
    [HttpGet]
    [ApiDescriptionSettings(Name = "TenantGameAreaEntityAreaIdDropdown")]
    public async Task<List<BaseSelectPageResponse>> TenantGameAreaEntityAreaIdDropdownAsync()
    {
        var list = await _areaRepository.AsQueryable()
                                        .Select(a => new BaseSelectPageResponse { Id = a.Id, Value = a.Name, Label = a.Description, })
                                        .ToListAsync();
        return list;
    }
}