using GameFrameX.Application.CommerceCenter.GameWebShop.Dto;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.CommerceCenter.GameWebShop;

/// <summary>
/// 游戏角色服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameWebShopEntityService : GenericCrudSelectService<Entity.Tenant.Game.TenantGameWebShopEntity, AddTenantGameWebShopEntityRequest, UpdateTenantGameWebShopEntityRequest, TenantGameWebShopEntityResponse, PageTenantGameWebShopEntityRequest>
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// 页面商城服务
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="httpClient"></param>
    /// <returns></returns>
    public TenantGameWebShopEntityService(SqlSugarRepository<Entity.Tenant.Game.TenantGameWebShopEntity> repository, HttpClient httpClient) : base(repository)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// 构建分页查询条件
    /// </summary>
    protected override ISugarQueryable<TenantGameWebShopEntityResponse> BuildPageQuery(PageTenantGameWebShopEntityRequest input)
    {
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u =>
                                      u.Name.Contains(input.SearchKey.Trim())
                         )
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Title), u => u.Name.Contains(input.Title.Trim()))
                         .Select<TenantGameWebShopEntityResponse>();
    }

    /// <summary>
    /// 更新前验证记录存在。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameWebShopEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }
    }
}
#pragma warning restore CS8602, CS8604, CS8629