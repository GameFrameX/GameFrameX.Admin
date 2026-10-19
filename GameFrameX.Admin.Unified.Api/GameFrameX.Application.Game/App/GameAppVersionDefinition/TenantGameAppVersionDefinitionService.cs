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
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> _gameAppRepository;

    public TenantGameAppVersionDefinitionService(
        SqlSugarRepository<Entity.Tenant.Game.Release.TenantGameAppVersionDefinitionEntity> repository,
        SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> gameAppRepository)
        : base(repository)
    {
        _gameAppRepository = gameAppRepository;
    }

    protected override ISugarQueryable<GameAppVersionDefinitionPageResponse> BuildPageQuery(PageGameAppVersionDefinitionRequest input)
    {
        var searchKey = input.SearchKey?.Trim() ?? string.Empty;
        var hasAppId = input.AppId.HasValue;
        var appId = input.AppId.GetValueOrDefault();
        return Repository.AsQueryable()
                         .Where(u => u.IsDeleted == false)
                         .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u => (u.Name ?? string.Empty).Contains(searchKey))
                         .WhereIF(hasAppId, u => u.AppId == appId)
                         .Select<GameAppVersionDefinitionPageResponse>();
    }

    protected override async Task OnAddingAsync(AddGameAppVersionDefinitionRequest input)
    {
        await ValidateGameAppBelongsToTenantAsync(input.AppId);
        await CheckNameUniqueInAppAsync(input.AppId, input.Name ?? string.Empty);
    }

    protected override async Task OnUpdatingAsync(UpdateGameAppVersionDefinitionRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.AppId;
        await ValidateGameAppBelongsToTenantAsync(input.AppId);
        await CheckNameUniqueInAppAsync(input.AppId, input.Name ?? string.Empty, input.Id);
    }

    /// <summary>
    /// 获取当前应用下的版本定义选择器列表。
    /// </summary>
    [HttpPost(CrudActionRoutes.Select)]
    [ApiDescriptionSettings(Name = "Select")]
    public override async Task<List<BaseSelectPageResponse>> SelectAsync(BaseSelectPageRequest input)
    {
        var searchKey = input.SearchKey?.Trim() ?? string.Empty;
        var hasAppId = input.AppId.HasValue;
        var appId = input.AppId.GetValueOrDefault();
        var pageSize = Math.Min(input.PageSize > 0 ? input.PageSize : BaseSelectPageRequest.DefaultPageSize, BaseSelectPageRequest.MaxPageSize);
        var query = await Repository.AsQueryable()
                                    .Where(u => u.IsDeleted == false)
                                    .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u => (u.Name ?? string.Empty).Contains(searchKey))
                                    .WhereIF(hasAppId, u => u.AppId == appId)
                                    .Take(pageSize)
                                    .ToListAsync();
        var list = new List<BaseSelectPageResponse>(query.Count);
        list.AddRange(query.Select(ts => new BaseSelectPageResponse { Id = ts.Id, Value = ts.Name, Label = ts.Description, }));
        return list;
    }

    private async Task CheckNameUniqueInAppAsync(long appId, string name, long? excludeId = null)
    {
        var exists = excludeId.HasValue
                         ? await Repository.IsAnyAsync(x => x.AppId == appId && x.Name == name && x.Id != excludeId.Value)
                         : await Repository.IsAnyAsync(x => x.AppId == appId && x.Name == name);
        if (exists)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordAlreadyExists);
        }
    }

    /// <summary>
    /// 校验游戏应用属于当前租户：Id 必须大于 0，且多租户过滤仓储能查到。
    /// </summary>
    private async Task ValidateGameAppBelongsToTenantAsync(long appId)
    {
        if (appId <= 0)
        {
            throw BusinessError.Oh("游戏应用Id不能为空");
        }

        var gameApp = await _gameAppRepository.GetFirstAsync(u => u.Id == appId);
        if (gameApp == null)
        {
            throw BusinessError.Oh("游戏应用不存在");
        }
    }
}