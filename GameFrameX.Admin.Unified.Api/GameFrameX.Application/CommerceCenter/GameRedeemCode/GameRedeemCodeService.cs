using GameFrameX.Application.CommerceCenter.GameRedeemCode.Dto;
using GameFrameX.Foundation.Json;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.CommerceCenter.GameRedeemCode;

/// <summary>
/// 兑换码服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class GameRedeemCodeService : GenericCrudService<Entity.Tenant.Game.TenantGameRedeemCodeEntity, AddTenantGameRedeemCodeEntityRequest, UpdateTenantGameRedeemCodeEntityRequest, GameRedeemCodeResponse, PageTenantGameRedeemCodeEntityRequest>
{
    public GameRedeemCodeService(SqlSugarRepository<Entity.Tenant.Game.TenantGameRedeemCodeEntity> repository) : base(repository)
    {
    }

    /// <summary>
    /// 构建分页查询条件
    /// </summary>
    protected override ISugarQueryable<GameRedeemCodeResponse> BuildPageQuery(PageTenantGameRedeemCodeEntityRequest input)
    {
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                                      u.Code.Contains(input.SearchKey.Trim())
                         )
                         .WhereIF(input.AreaId > 0, u => u.AreaId == input.AreaId)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Code), u => u.Code.Contains(input.Code.Trim()))
                         .WhereIF(input.UsageLimit > 0, u => u.UsageLimit == input.UsageLimit)
                         .WhereIF(input.UsedCount > 0, u => u.UsedCount == input.UsedCount)
                         .WhereIF(input.ChannelId > 0, u => u.ChannelId == input.ChannelId)
                         .WhereIF(input.IsActive.HasValue, u => u.IsActive == input.IsActive.Value)
                         .Select<GameRedeemCodeResponse>();
    }

    /// <summary>
    /// 新增前解析并校验附件道具。
    /// </summary>
    protected override async Task OnAddingAsync(AddTenantGameRedeemCodeEntityRequest input)
    {

        var gamePropItemEntities = JsonHelper.Deserialize<List<GamePropItem>>(input.Contents);
        input.Contents = string.Empty;

        // 批量预加载所有商品，避免 N+1 查询
        var goodsIds = gamePropItemEntities.Select(p => p.Id).Distinct().ToList();
        var goodsDict = (await Repository.AsSugarClient()
                                         .Queryable<Entity.Tenant.Game.TenantGameGoodsEntity>()
                                         .Where(m => goodsIds.Contains(m.Id))
                                         .ToListAsync())
            .ToDictionary(m => m.Id);

        var newList = new List<GamePropItem>();
        foreach (var attachmentItem in gamePropItemEntities)
        {
            if (goodsDict.TryGetValue(attachmentItem.Id, out var gameGoodsEntity))
            {
                attachmentItem.ItemId = gameGoodsEntity.ConfigId;
                newList.Add(attachmentItem);
            }
        }

        input.Contents = JsonHelper.Serialize(newList, EmbeddedJsonOptions.Instance);
    }

    /// <summary>
    /// 更新前验证记录存在。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameRedeemCodeEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }
    }
}
#pragma warning restore CS8602, CS8604, CS8629