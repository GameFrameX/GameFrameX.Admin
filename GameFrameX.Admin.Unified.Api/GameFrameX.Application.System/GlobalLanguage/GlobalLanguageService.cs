using GameFrameX.Application.System.GlobalLanguage.Dto;

namespace GameFrameX.Application.System.GlobalLanguage;

/// <summary>
/// 语言管理服务
/// </summary>
[ApiDescriptionSettings(Order = 100)]
public class GlobalLanguageService
    : GenericCrudSelectService<GameFrameX.Entity.Global.GlobalLanguageEntity,
        AddGlobalLanguageRequest, UpdateGlobalLanguageRequest,
        GlobalLanguageResponse, PageGlobalLanguageRequest>
{
    public GlobalLanguageService(SqlSugarRepository<GameFrameX.Entity.Global.GlobalLanguageEntity> repository)
        : base(repository)
    {
    }

    /// <summary>
    /// 获取语言选择器列表
    /// </summary>
    [HttpPost(CrudActionRoutes.Select)]
    [ApiDescriptionSettings(Name = "Select")]
    public override async Task<List<BaseSelectPageResponse>> SelectAsync(BaseSelectPageRequest input)
    {
        var searchKey = input.SearchKey?.Trim() ?? string.Empty;
        var query = await Repository.AsQueryable()
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

    protected override async Task OnAddingAsync(AddGlobalLanguageRequest input)
    {
        await CheckNameUniqueAsync(input.Name);
    }
}