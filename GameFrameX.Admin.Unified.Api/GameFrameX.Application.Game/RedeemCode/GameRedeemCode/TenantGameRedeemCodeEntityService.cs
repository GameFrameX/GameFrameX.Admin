using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.RedeemCode.GameRedeemCode.Dto;
using GameFrameX.Foundation.Json;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.RedeemCode.GameRedeemCode;

/// <summary>
/// 兑换码服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameRedeemCodeEntityService : GenericCrudService<Entity.Tenant.Game.TenantGameRedeemCodeEntity, AddTenantGameRedeemCodeEntityRequest, UpdateTenantGameRedeemCodeEntityRequest, TenantGameRedeemCodeEntityResponse, PageTenantGameRedeemCodeEntityRequest>
{
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> _gameAppRepository;

    public TenantGameRedeemCodeEntityService(SqlSugarRepository<Entity.Tenant.Game.TenantGameRedeemCodeEntity> repository, SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> gameAppRepository) : base(repository)
    {
        _gameAppRepository = gameAppRepository;
    }

    /// <summary>
    /// 构建分页查询条件
    /// </summary>
    protected override ISugarQueryable<TenantGameRedeemCodeEntityResponse> BuildPageQuery(PageTenantGameRedeemCodeEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "游戏兑换码");
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                                      u.Code.Contains(input.SearchKey.Trim())
                         )
                         .WhereIF(input.AreaId > 0, u => u.AreaId == input.AreaId)
                         .Where(u => u.AppId == appId)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Code), u => u.Code.Contains(input.Code.Trim()))
                         .WhereIF(input.UsageLimit > 0, u => u.UsageLimit == input.UsageLimit)
                         .WhereIF(input.UsedCount > 0, u => u.UsedCount == input.UsedCount)
                         .WhereIF(input.ChannelId > 0, u => u.ChannelId == input.ChannelId)
                         .WhereIF(input.IsActive.HasValue, u => u.IsActive == input.IsActive.Value)
                         .Select<TenantGameRedeemCodeEntityResponse>();
    }

    /// <summary>
    /// 新增前的自定义处理：保留既有道具配置解析，并校验 AppId 必填且必须属于当前租户（事实数据，禁止 AppId=0）。
    /// </summary>
    protected override async Task OnAddingAsync(AddTenantGameRedeemCodeEntityRequest input)
    {
        AppScopeRules.EnsureFactAppId(input.AppId, "游戏兑换码");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);

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
    /// 更新前：AppId 不可变（从既有实体回填，禁止兑换码跨应用迁移）并校验应用归属。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameRedeemCodeEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.AppId;
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
    }
}
#pragma warning restore CS8602, CS8604, CS8629