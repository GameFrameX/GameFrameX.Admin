using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Goods.GameCommodity.Dto;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Goods.GameCommodity;

/// <summary>
/// 游戏商品交易服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameCommodityEntityService
    : GenericCrudService<Entity.Tenant.Game.TenantGameCommodityEntity,
        AddTenantGameCommodityEntityRequest, UpdateTenantGameCommodityEntityRequest,
        TenantGameCommodityEntityResponse, PageTenantGameCommodityEntityRequest>
{
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> _gameAppRepository;

    public TenantGameCommodityEntityService(SqlSugarRepository<Entity.Tenant.Game.TenantGameCommodityEntity> repository, SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> gameAppRepository)
        : base(repository)
    {
        _gameAppRepository = gameAppRepository;
    }

    /// <summary>
    /// 新增前校验：AppId 必填且必须属于当前租户（运营内容，禁止 AppId=0）。
    /// </summary>
    protected override async Task OnAddingAsync(AddTenantGameCommodityEntityRequest input)
    {
        AppScopeRules.EnsureFactAppId(input.AppId, "商品");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
    }

    /// <summary>
    /// 更新前：AppId 不可变（从既有实体回填）并校验应用归属。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameCommodityEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.AppId;
        AppScopeRules.EnsureFactAppId(input.AppId, "商品");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
    }

    protected override ISugarQueryable<TenantGameCommodityEntityResponse> BuildPageQuery(PageTenantGameCommodityEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "商品");
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                                      u.Status.Contains(input.SearchKey.Trim())
                                      || u.CreatedName.Contains(input.SearchKey.Trim())
                                      || u.UpdatedName.Contains(input.SearchKey.Trim()))
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Status), u => u.Status.Contains(input.Status.Trim()))
                         .WhereIF(input.CategoryId > 0, u => u.CategoryId == input.CategoryId)
                         .WhereIF(input.Number > 0, u => u.Number == input.Number)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.CreatedName), u => u.CreatedName.Contains(input.CreatedName.Trim()))
                         .WhereIF(!string.IsNullOrWhiteSpace(input.UpdatedName), u => u.UpdatedName.Contains(input.UpdatedName.Trim()))
                         .Where(u => u.AppId == appId)
                         .Select<TenantGameCommodityEntityResponse>();
    }
}
#pragma warning restore CS8602, CS8604, CS8629