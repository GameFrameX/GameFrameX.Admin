using GameFrameX.Application.CommerceCenter.GameGoodsCatalogue.Dto;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.CommerceCenter.GameGoodsCatalogue;

/// <summary>
/// 游戏道具分类服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class GameGoodsCatalogueService
    : GenericCrudSelectService<Entity.Tenant.Game.TenantGameGoodsCatalogueEntity,
        AddTenantGameGoodsCatalogueEntityRequest, UpdateTenantGameGoodsCatalogueEntityRequest,
        GameGoodsCatalogueResponse, PageTenantGameGoodsCatalogueEntityRequest>
{
    public GameGoodsCatalogueService(SqlSugarRepository<Entity.Tenant.Game.TenantGameGoodsCatalogueEntity> repository)
        : base(repository)
    {
    }

    protected override async Task OnAddingAsync(AddTenantGameGoodsCatalogueEntityRequest input)
    {
        await CheckNameUniqueAsync(input.Name);
    }

    protected override async Task OnUpdatingAsync(UpdateTenantGameGoodsCatalogueEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        await CheckNameUniqueAsync(input.Name, input.Id);
    }

    protected override ISugarQueryable<GameGoodsCatalogueResponse> BuildPageQuery(PageTenantGameGoodsCatalogueEntityRequest input)
    {
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Description), u => u.Description.Contains(input.Description.Trim()))
                         .Select<GameGoodsCatalogueResponse>();
    }
}
#pragma warning restore CS8602, CS8604, CS8629