using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.WebShop.GameWebShop.Dto;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.WebShop.GameWebShop;

/// <summary>
/// 游戏角色服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameWebShopEntityService : GenericCrudSelectService<Entity.Tenant.Game.TenantGameWebShopEntity, AddTenantGameWebShopEntityRequest, UpdateTenantGameWebShopEntityRequest, TenantGameWebShopEntityResponse, PageTenantGameWebShopEntityRequest>
{
    private readonly HttpClient _httpClient;
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> _gameAppRepository;

    /// <summary>
    /// 页面商城服务
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="httpClient"></param>
    /// <param name="gameAppRepository"></param>
    /// <returns></returns>
    public TenantGameWebShopEntityService(SqlSugarRepository<Entity.Tenant.Game.TenantGameWebShopEntity> repository, HttpClient httpClient, SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> gameAppRepository) : base(repository)
    {
        _httpClient = httpClient;
        _gameAppRepository = gameAppRepository;
    }

    /// <summary>
    /// 构建分页查询条件
    /// </summary>
    protected override ISugarQueryable<TenantGameWebShopEntityResponse> BuildPageQuery(PageTenantGameWebShopEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "网页商城");
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                                      u.Name.Contains(input.SearchKey.Trim())
                         )
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Title), u => u.Name.Contains(input.Title.Trim()))
                         .Where(u => u.AppId == appId)
                         .Select<TenantGameWebShopEntityResponse>();
    }

    /// <summary>
    /// 新增前校验：AppId 必填且必须属于当前租户（运营内容，禁止 AppId=0）。
    /// </summary>
    protected override async Task OnAddingAsync(AddTenantGameWebShopEntityRequest input)
    {
        AppScopeRules.EnsureFactAppId(input.AppId, "网页商城");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
    }

    /// <summary>
    /// 更新前：AppId 不可变（从既有实体回填）并校验应用归属。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameWebShopEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.AppId;
        AppScopeRules.EnsureFactAppId(input.AppId, "网页商城");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
    }
}
#pragma warning restore CS8602, CS8604, CS8629