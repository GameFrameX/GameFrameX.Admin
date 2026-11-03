namespace GameFrameX.Application.System.GlobalLanguage;

/// <summary>
/// 语言定义选择器服务（仅保留下拉数据源端点，供保留页的语言选择器调用）
/// </summary>
[ApiDescriptionSettings(Order = 100)]
public class GlobalLanguageService : IAdminApiController
{
    private readonly SqlSugarRepository<GameFrameX.Entity.Global.GlobalLanguageEntity> _repository;

    public GlobalLanguageService(SqlSugarRepository<GameFrameX.Entity.Global.GlobalLanguageEntity> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// 获取语言选择器列表
    /// </summary>
    [HttpPost(CrudActionRoutes.Select)]
    [DisplayName("获取语言选择器列表")]
    [ApiDescriptionSettings(Name = "Select")]
    public async Task<List<BaseSelectPageResponse>> SelectAsync(BaseSelectPageRequest input)
    {
        var searchKey = input.SearchKey?.Trim() ?? string.Empty;
        var query = await _repository.AsQueryable()
                                     .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u =>
                                                 (u.Name ?? string.Empty).Contains(searchKey)
                                                 || (u.Description ?? string.Empty).Contains(searchKey)
                                                 || (u.Code ?? string.Empty).Contains(searchKey))
                                     .ToListAsync();

        return query.OrderByDescending(u => u.IsDefault == true
                                            || u.Name == "default"
                                            || u.Code == "default")
                    .ThenBy(u => u.Id)
                    .Select(u => new BaseSelectPageResponse
                    {
                        Id = u.Id,
                        Value = string.IsNullOrWhiteSpace(u.Code) ? u.Name ?? string.Empty : u.Code.Trim(),
                        Label = string.IsNullOrWhiteSpace(u.Description) ? u.Name ?? string.Empty : u.Description,
                    })
                    .ToList();
    }

    /// <summary>
    /// 获取语言选择器列表（字符串 ID）
    /// </summary>
    [HttpPost(CrudActionRoutes.SelectString)]
    [DisplayName("获取语言选择器列表（字符串 ID）")]
    [ApiDescriptionSettings(Name = "SelectString")]
    public async Task<List<BaseSelectPageStringResponse>> SelectStringAsync(BaseSelectPageRequest input)
    {
        var result = await SelectAsync(input);

        return result.Select(u => new BaseSelectPageStringResponse { Id = u.Id.ToString(), Value = u.Value, Label = u.Label, })
                     .ToList();
    }
}
