using GameFrameX.Application.Game.App.GameAppVersionDefinition.Dto;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Application.Game.App.GameAppVersionDefinition;

/// <summary>
/// 版本管理服务
/// </summary>
[ApiDescriptionSettings(ClientConst.GroupName, Order = 100)]
public class TenantGameAppVersionDefinitionService
    : GenericCrudSelectService<Entity.Tenant.Game.Release.TenantGameAppVersionDefinitionEntity,
        AddGameAppVersionDefinitionRequest, UpdateGameAppVersionDefinitionRequest,
        GameAppVersionDefinitionPageResponse, PageGameAppVersionDefinitionRequest>
{
    public TenantGameAppVersionDefinitionService(
        SqlSugarRepository<Entity.Tenant.Game.Release.TenantGameAppVersionDefinitionEntity> repository)
        : base(repository)
    {
    }

    protected override ISugarQueryable<GameAppVersionDefinitionPageResponse> BuildPageQuery(PageGameAppVersionDefinitionRequest input)
    {
        var searchKey = input.SearchKey?.Trim() ?? string.Empty;
        return Repository.AsQueryable()
                         .Where(u => u.IsDeleted == false)
                         .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u => (u.Name ?? string.Empty).Contains(searchKey))
                         .Select<GameAppVersionDefinitionPageResponse>();
    }

    protected override async Task OnAddingAsync(AddGameAppVersionDefinitionRequest input)
    {
        await CheckNameUniqueAsync(input.Name ?? string.Empty);
    }

    protected override async Task OnUpdatingAsync(UpdateGameAppVersionDefinitionRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        await CheckNameUniqueAsync(input.Name ?? string.Empty, input.Id);
    }

    /// <summary>
    /// 获取主库版本定义选择器列表。
    /// </summary>
    [HttpPost(CrudActionRoutes.Select)]
    [ApiDescriptionSettings(Name = "Select")]
    public override async Task<List<BaseSelectPageResponse>> SelectAsync(BaseSelectPageRequest input)
    {
        var searchKey = input.SearchKey?.Trim() ?? string.Empty;
        var pageSize = Math.Min(input.PageSize > 0 ? input.PageSize : BaseSelectPageRequest.DefaultPageSize, BaseSelectPageRequest.MaxPageSize);
        var query = await Repository.AsQueryable()
                                    .Where(u => u.IsDeleted == false)
                                    .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u => (u.Name ?? string.Empty).Contains(searchKey))
                                    .Take(pageSize)
                                    .ToListAsync();
        var list = new List<BaseSelectPageResponse>(query.Count);
        list.AddRange(query.Select(ts => new BaseSelectPageResponse { Id = ts.Id, Value = ts.Name, Label = ts.Description, }));
        return list;
    }

    private async Task CheckNameUniqueAsync(string name, long? excludeId = null)
    {
        var exists = excludeId.HasValue
                         ? await Repository.IsAnyAsync(x => x.Name == name && x.Id != excludeId.Value)
                         : await Repository.IsAnyAsync(x => x.Name == name);
        if (exists)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordAlreadyExists);
        }
    }

}
