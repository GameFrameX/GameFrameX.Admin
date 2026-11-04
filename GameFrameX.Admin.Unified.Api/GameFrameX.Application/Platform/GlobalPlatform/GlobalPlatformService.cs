namespace GameFrameX.Application.Platform.GlobalPlatform;

/// <summary>
/// 平台定义选择器服务（仅保留下拉数据源端点，供保留页的平台选择器调用）
/// </summary>
[ApiDescriptionSettings(Order = 100)]
public class GlobalPlatformService : IAdminApiController
{
    private readonly SqlSugarRepository<GameFrameX.Entity.Global.GlobalPlatformEntity> _repository;

    public GlobalPlatformService(SqlSugarRepository<GameFrameX.Entity.Global.GlobalPlatformEntity> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 获取平台选择器列表
    /// </summary>
    [HttpPost(CrudActionRoutes.Select)]
    [DisplayName("获取平台选择器列表")]
    [ApiDescriptionSettings(Name = "Select")]
    public async Task<List<BaseSelectPageResponse>> SelectAsync(BaseSelectPageRequest input)
    {
        var searchKey = input.SearchKey?.Trim() ?? string.Empty;
        var pageSize = Math.Min(input.PageSize > 0 ? input.PageSize : BaseSelectPageRequest.DefaultPageSize, BaseSelectPageRequest.MaxPageSize);
        var query = await _repository.AsQueryable()
                                     .Where(u => u.IsDeleted == false)
                                     .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u => (u.Name ?? string.Empty).Contains(searchKey))
                                     .Take(pageSize)
                                     .ToListAsync();

        return query.Select(u => new BaseSelectPageResponse { Id = u.Id, Value = u.Name ?? string.Empty, Label = u.Description ?? string.Empty, })
                    .ToList();
    }

    /// <summary>
    /// 获取平台选择器列表（字符串 ID）
    /// </summary>
    [HttpPost(CrudActionRoutes.SelectString)]
    [DisplayName("获取平台选择器列表（字符串 ID）")]
    [ApiDescriptionSettings(Name = "SelectString")]
    public async Task<List<BaseSelectPageStringResponse>> SelectStringAsync(BaseSelectPageRequest input)
    {
        var searchKey = input.SearchKey?.Trim() ?? string.Empty;
        var pageSize = Math.Min(input.PageSize > 0 ? input.PageSize : BaseSelectPageRequest.DefaultPageSize, BaseSelectPageRequest.MaxPageSize);
        var query = await _repository.AsQueryable()
                                     .Where(u => u.IsDeleted == false)
                                     .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u => (u.Name ?? string.Empty).Contains(searchKey))
                                     .Take(pageSize)
                                     .ToListAsync();

        return query.Select(u => new BaseSelectPageStringResponse { Id = u.Id.ToString(), Value = u.Name ?? string.Empty, Label = u.Description, })
                     .ToList();
    }
}
