using GameFrameX.Application.ClientDelivery.GameAppAssetPackage.Dto;

namespace GameFrameX.Application.ClientDelivery.GameAppAssetPackage;

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
        var name = input.Name?.Trim() ?? string.Empty;
        return Repository.AsQueryable()
                                                  .WhereIF(!string.IsNullOrEmpty(name), u => u.Name.Contains(name))
                         .Select<GameAppAssetPackageResponse>();
    }

    /// <summary>
    /// 获取主库中的资源包选择器列表。
    /// </summary>
    [HttpPost(CrudActionRoutes.Select)]
    public override async Task<List<BaseSelectPageResponse>> SelectAsync(BaseSelectPageRequest input)
    {
        var searchKey = input.SearchKey?.Trim() ?? string.Empty;
        var pageSize = Math.Min(input.PageSize > 0 ? input.PageSize : BaseSelectPageRequest.DefaultPageSize, BaseSelectPageRequest.MaxPageSize);
        var query = await Repository.AsQueryable()
                                    .Where(u => u.IsDeleted == false)
                                    .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u =>
                                                 (u.Name ?? string.Empty).Contains(searchKey))
                                    .OrderBy(u => u.CreatedTime)
                                    .Take(pageSize)
                                    .ToListAsync();
        return query.Select(ts => new BaseSelectPageResponse { Id = ts.Id, Value = ts.Name, Label = ts.Description, }).ToList();
    }

    /// <summary>
    /// 新增前校验主库内资源包名称唯一。
    /// </summary>
    protected override async Task OnAddingAsync(AddGameAppAssetPackageRequest input)
    {
        await CheckPackageNameUniqueAsync(input.Name);
    }

    /// <summary>
    /// 更新前校验记录存在并校验主库内名称唯一。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateGameAppAssetPackageRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        await CheckPackageNameUniqueAsync(input.Name, input.Id);
    }

    private async Task CheckPackageNameUniqueAsync(string name, long? excludeId = null)
    {
        var normalized = name?.Trim() ?? string.Empty;
        var query = Repository.AsQueryable().Where(u => u.Name == normalized);
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