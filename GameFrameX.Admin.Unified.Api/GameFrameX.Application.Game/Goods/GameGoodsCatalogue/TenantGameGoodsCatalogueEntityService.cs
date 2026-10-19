using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Goods.GameGoodsCatalogue.Dto;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Goods.GameGoodsCatalogue;

/// <summary>
/// 游戏道具分类服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameGoodsCatalogueEntityService
    : GenericCrudSelectService<Entity.Tenant.Game.TenantGameGoodsCatalogueEntity,
        AddTenantGameGoodsCatalogueEntityRequest, UpdateTenantGameGoodsCatalogueEntityRequest,
        TenantGameGoodsCatalogueEntityResponse, PageTenantGameGoodsCatalogueEntityRequest>
{
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> _gameAppRepository;

    public TenantGameGoodsCatalogueEntityService(SqlSugarRepository<Entity.Tenant.Game.TenantGameGoodsCatalogueEntity> repository, SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> gameAppRepository)
        : base(repository)
    {
        _gameAppRepository = gameAppRepository;
    }

    protected override async Task OnAddingAsync(AddTenantGameGoodsCatalogueEntityRequest input)
    {
        AppScopeRules.EnsureFactAppId(input.AppId, "游戏资源分类");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
        await CheckNameUniqueAsync(input.Name);
    }

    protected override async Task OnUpdatingAsync(UpdateTenantGameGoodsCatalogueEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.AppId;
        AppScopeRules.EnsureFactAppId(input.AppId, "游戏资源分类");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
        await CheckNameUniqueAsync(input.Name, input.Id);
    }

    protected override ISugarQueryable<TenantGameGoodsCatalogueEntityResponse> BuildPageQuery(PageTenantGameGoodsCatalogueEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "游戏资源分类");
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Description), u => u.Description.Contains(input.Description.Trim()))
                         .Where(u => u.AppId == appId)
                         .Select<TenantGameGoodsCatalogueEntityResponse>();
    }
}
#pragma warning restore CS8602, CS8604, CS8629