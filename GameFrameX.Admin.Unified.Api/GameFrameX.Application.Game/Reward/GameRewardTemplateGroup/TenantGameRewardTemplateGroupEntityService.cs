using GameFrameX.Application.Game.Reward.GameRewardTemplateGroup.Dto;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Reward.GameRewardTemplateGroup;

/// <summary>
/// 奖励模板分组服务。
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameRewardTemplateGroupEntityService
    : GenericCrudSelectService<Entity.Tenant.Game.TenantGameRewardTemplateGroupEntity,
        AddTenantGameRewardTemplateGroupEntityRequest, UpdateTenantGameRewardTemplateGroupEntityRequest,
        TenantGameRewardTemplateGroupEntityResponse, PageTenantGameRewardTemplateGroupEntityRequest>
{

    public TenantGameRewardTemplateGroupEntityService(
        SqlSugarRepository<Entity.Tenant.Game.TenantGameRewardTemplateGroupEntity> repository)
        : base(repository)
    {
    }

    protected override ISugarQueryable<TenantGameRewardTemplateGroupEntityResponse> BuildPageQuery(PageTenantGameRewardTemplateGroupEntityRequest input)
    {
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(input.SearchKey), u => u.Name.Contains(input.SearchKey.Trim()))
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .WhereIF(input.IsEnabled.HasValue, u => u.IsEnabled == input.IsEnabled.Value)
                         .Select<TenantGameRewardTemplateGroupEntityResponse>();
    }

    /// <summary>
    /// 获取主库中启用的奖励模板分组。
    /// </summary>
    [HttpPost(CrudActionRoutes.Select)]
    [ApiDescriptionSettings(Name = "Select")]
    public override async Task<List<BaseSelectPageResponse>> SelectAsync(BaseSelectPageRequest input)
    {
        var searchKey = input.SearchKey?.Trim() ?? string.Empty;
        var pageSize = Math.Min(input.PageSize > 0 ? input.PageSize : BaseSelectPageRequest.DefaultPageSize, BaseSelectPageRequest.MaxPageSize);
        var rows = await Repository.AsQueryable()
                                   .Where(u => u.IsDeleted == false)
                                   .Where(u => u.IsEnabled != false)
                                   .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u => u.Name.Contains(searchKey))
                                   .OrderBy(u => u.Sort)
                                   .OrderBy(u => u.Id)
                                   .Take(pageSize)
                                   .ToListAsync();

        return rows.Select(u => new BaseSelectPageResponse { Id = u.Id, Value = u.Name, Label = u.Name, }).ToList();
    }

    protected override async Task OnUpdatingAsync(UpdateTenantGameRewardTemplateGroupEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

    }
}
#pragma warning restore CS8602, CS8604, CS8629