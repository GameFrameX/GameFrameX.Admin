using GameFrameX.Application.Game.App.GameAppAssetPackage.Dto;

namespace GameFrameX.Application.Game.App.GameAppAssetPackage;

/// <summary>
/// 资源包服务
/// </summary>
[ApiDescriptionSettings(ClientConst.GroupName, Order = 100)]
public class TenantGameAppAssetPackageService : GenericCrudSelectService<Entity.Tenant.Game.Resource.TenantGameAppAssetPackageEntity, AddGameAppAssetPackageRequest, UpdateGameAppAssetPackageRequest, GameAppAssetPackageResponse, PageGameAppAssetPackageRequest>
{
    public TenantGameAppAssetPackageService(SqlSugarRepository<Entity.Tenant.Game.Resource.TenantGameAppAssetPackageEntity> repository) : base(repository)
    {
    }

    /// <summary>
    /// 构建分页查询条件
    /// </summary>
    protected override ISugarQueryable<GameAppAssetPackageResponse> BuildPageQuery(PageGameAppAssetPackageRequest input)
    {
        if (input.AppId is null or <= 0)
        {
            throw BusinessError.Oh("请选择应用后再维护资源包");
        }

        var name = input.Name?.Trim() ?? string.Empty;
        return Repository.AsQueryable()
                         .Where(u => u.GameAppId == input.AppId.Value)
                         .WhereIF(!string.IsNullOrEmpty(name), u => u.Name.Contains(name))
                         .Select<GameAppAssetPackageResponse>();
    }

    /// <summary>
    /// 获取当前应用下的资源包选择器列表。
    /// </summary>
    [HttpPost(CrudActionRoutes.Select)]
    public override async Task<List<BaseSelectPageResponse>> SelectAsync(BaseSelectPageRequest input)
    {
        if (input.AppId is null or <= 0)
        {
            return new List<BaseSelectPageResponse>();
        }

        var searchKey = input.SearchKey?.Trim() ?? string.Empty;
        var pageSize = Math.Min(input.PageSize > 0 ? input.PageSize : BaseSelectPageRequest.DefaultPageSize, BaseSelectPageRequest.MaxPageSize);
        var query = await Repository.AsQueryable()
                                    .Where(u => u.IsDeleted == false && u.GameAppId == input.AppId.Value)
                                    .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u =>
                                                 (u.Name ?? string.Empty).Contains(searchKey))
                                    .OrderBy(u => u.CreatedTime)
                                    .Take(pageSize)
                                    .ToListAsync();
        return query.Select(ts => new BaseSelectPageResponse { Id = ts.Id, Value = ts.Name, Label = ts.Description, }).ToList();
    }

    /// <summary>
    /// 新增前强制资源包绑定具体应用，并校验同一应用内名称唯一。
    /// </summary>
    protected override async Task OnAddingAsync(AddGameAppAssetPackageRequest input)
    {
        if (input.AppId <= 0)
        {
            throw BusinessError.Oh("请选择应用后再新增资源包");
        }

        await CheckAppPackageNameUniqueAsync(input.AppId, input.Name);
    }

    /// <summary>
    /// 更新前校验记录仍属于当前应用，并校验同一应用内名称唯一。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateGameAppAssetPackageRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        if (input.AppId.HasValue && input.AppId.Value > 0 && existing.GameAppId != input.AppId.Value)
        {
            throw BusinessError.Oh("资源包不属于当前应用");
        }

        await CheckAppPackageNameUniqueAsync(existing.GameAppId, input.Name, input.Id);
    }

    private async Task CheckAppPackageNameUniqueAsync(long gameAppId, string name, long? excludeId = null)
    {
        var normalized = name?.Trim() ?? string.Empty;
        var query = Repository.AsQueryable().Where(u => u.GameAppId == gameAppId && u.Name == normalized);
        if (excludeId.HasValue)
        {
            var id = excludeId.Value;
            query = query.Where(u => u.Id != id);
        }

        var exists = await query.AnyAsync();
        if (exists)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordAlreadyExists);
        }
    }
}