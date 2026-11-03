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
    public TenantGameCommodityEntityService(SqlSugarRepository<Entity.Tenant.Game.TenantGameCommodityEntity> repository)
        : base(repository)
    {
    }

    /// <summary>
    /// 更新前校验记录存在。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameCommodityEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }
    }

    protected override ISugarQueryable<TenantGameCommodityEntityResponse> BuildPageQuery(PageTenantGameCommodityEntityRequest input)
    {
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
                         .Select<TenantGameCommodityEntityResponse>();
    }
}
#pragma warning restore CS8602, CS8604, CS8629