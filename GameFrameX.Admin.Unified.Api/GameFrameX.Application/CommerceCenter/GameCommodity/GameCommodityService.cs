using GameFrameX.Application.CommerceCenter.GameCommodity.Dto;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.CommerceCenter.GameCommodity;

/// <summary>
/// 游戏商品交易服务
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class GameCommodityService
    : GenericCrudService<Entity.Business.Game.GameCommodityEntity,
        AddGameCommodityEntityRequest, UpdateGameCommodityEntityRequest,
        GameCommodityResponse, PageGameCommodityEntityRequest>
{
    public GameCommodityService(SqlSugarRepository<Entity.Business.Game.GameCommodityEntity> repository)
        : base(repository)
    {
    }

    /// <summary>
    /// 更新前校验记录存在。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateGameCommodityEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }
    }

    protected override ISugarQueryable<GameCommodityResponse> BuildPageQuery(PageGameCommodityEntityRequest input)
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
                         .Select<GameCommodityResponse>();
    }
}
#pragma warning restore CS8602, CS8604, CS8629