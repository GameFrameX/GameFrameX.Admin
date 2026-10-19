using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Goods.GameGoods.Dto;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Goods.GameGoods;

/// <summary>
/// 游戏资源服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameGoodsEntityService
    : GenericCrudSelectService<Entity.Tenant.Game.TenantGameGoodsEntity,
        AddTenantGameGoodsEntityRequest, UpdateTenantGameGoodsEntityRequest,
        TenantGameGoodsEntityResponse, PageTenantGameGoodsEntityRequest>
{
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> _gameAppRepository;

    public TenantGameGoodsEntityService(SqlSugarRepository<Entity.Tenant.Game.TenantGameGoodsEntity> repository, SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> gameAppRepository)
        : base(repository)
    {
        _gameAppRepository = gameAppRepository;
    }

    protected override async Task OnAddingAsync(AddTenantGameGoodsEntityRequest input)
    {
        AppScopeRules.EnsureFactAppId(input.AppId, "游戏资源");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);

        var exists = await Repository.IsAnyAsync(u => u.ConfigId == input.ConfigId);
        if (exists)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordAlreadyExists);
        }
    }

    /// <summary>
    /// 更新前：AppId 不可变（从既有实体回填）并校验应用归属。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameGoodsEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.AppId;
        AppScopeRules.EnsureFactAppId(input.AppId, "游戏资源");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
    }

    protected override ISugarQueryable<TenantGameGoodsEntityResponse> BuildPageQuery(PageTenantGameGoodsEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "游戏资源");
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                                      u.Name.Contains(input.SearchKey.Trim()))
                         .WhereIF(input.ConfigId > 0, u => u.ConfigId == input.ConfigId)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .Where(u => u.AppId == appId)
                         .Select<TenantGameGoodsEntityResponse>();
    }

    /// <summary>
    /// 获取物品选择列表（含分类信息）
    /// </summary>
    [HttpPost(CrudActionRoutes.SelectString)]
    [ApiDescriptionSettings(Name = "SelectString")]
    public override async Task<List<BaseSelectPageStringResponse>> SelectStringAsync(BaseSelectPageRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "游戏资源");
        var query = await Repository.AsQueryable()
                                    .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                                                 u.Name.Contains(input.SearchKey.Trim()))
                                    .Where(u => u.AppId == appId)
                                    .LeftJoin<Entity.Tenant.Game.TenantGameGoodsCatalogueEntity>((g, c) => g.ClassifyId == c.Id)
                                    .Select((g, c) => new TenantGameGoodsEntitySelectPageStringResponse
                                    {
                                        Id = g.Id.ToString(),
                                        Value = g.Name,
                                        Label = g.Description,
                                        Star = g.Star,
                                        AppId = g.AppId,
                                        ClassifyId = g.ClassifyId,
                                        ClassifyIdName = c.Name,
                                    })
                                    .ToListAsync();

        return query.Cast<BaseSelectPageStringResponse>().ToList();
    }
}
#pragma warning restore CS8602, CS8604, CS8629