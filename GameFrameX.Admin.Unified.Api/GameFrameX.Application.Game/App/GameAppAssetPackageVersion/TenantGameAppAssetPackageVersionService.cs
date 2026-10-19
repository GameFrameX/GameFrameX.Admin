using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.App.GameAppAssetPackageVersion.Dto;
using GameFrameX.Application.Game.App.GameAppAssetPackageVersionHistory.Dto;
using GameFrameX.Entity.Tenant.Game.App;
using GameFrameX.Entity.Tenant.Game.Resource;


namespace GameFrameX.Application.Game.App.GameAppAssetPackageVersion;

/// <summary>
/// 资源包版本服务
/// </summary>
[ApiDescriptionSettings(ClientConst.GroupName, Order = 100)]
public class TenantGameAppAssetPackageVersionService : GenericCrudService<Entity.Tenant.Game.Release.TenantGameAppAssetPackageVersionEntity, AddGameAppAssetPackageVersionRequest, UpdateGameAppAssetPackageVersionRequest, GameAppAssetPackageVersionResponse, PageGameAppAssetPackageVersionRequest>
{
    protected readonly SqlSugarRepository<Entity.Tenant.Game.Release.TenantGameAppAssetPackageVersionHistoryEntity> RepositoryAssetPackageVersionHistory;
    private readonly SqlSugarRepository<TenantGameAppPackageEntity> _packageRepository;
    private readonly SqlSugarRepository<TenantGameAppAssetPackageEntity> _assetPackageRepository;

    public TenantGameAppAssetPackageVersionService(
        SqlSugarRepository<Entity.Tenant.Game.Release.TenantGameAppAssetPackageVersionEntity> service,
        SqlSugarRepository<Entity.Tenant.Game.Release.TenantGameAppAssetPackageVersionHistoryEntity> rep2,
        SqlSugarRepository<TenantGameAppPackageEntity> packageRepository,
        SqlSugarRepository<TenantGameAppAssetPackageEntity> assetPackageRepository) : base(service)
    {
        RepositoryAssetPackageVersionHistory = rep2;
        _packageRepository = packageRepository;
        _assetPackageRepository = assetPackageRepository;
    }

    /// <summary>
    /// 构建分页查询条件
    /// </summary>
    protected override ISugarQueryable<GameAppAssetPackageVersionResponse> BuildPageQuery(PageGameAppAssetPackageVersionRequest input)
    {
        if (input.AppId is null or <= 0)
        {
            throw BusinessError.Oh("请选择应用后再维护资源包版本");
        }

        var channel = input.Channel?.Trim() ?? string.Empty;
        var platform = input.Platform?.Trim() ?? string.Empty;
        var appVersion = input.AppVersion?.Trim() ?? string.Empty;
        var package = input.Package?.Trim() ?? string.Empty;
        var language = input.Language?.Trim() ?? string.Empty;
        var assetPackageName = input.AssetPackageName?.Trim() ?? string.Empty;
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrEmpty(channel), u => (u.Channel ?? string.Empty).Contains(channel))
                         .WhereIF(!string.IsNullOrEmpty(platform), u => (u.Platform ?? string.Empty).Contains(platform))
                         .WhereIF(!string.IsNullOrEmpty(appVersion), u => (u.AppVersion ?? string.Empty).Contains(appVersion))
                         .WhereIF(!string.IsNullOrEmpty(package), u => u.Package == package)
                         .WhereIF(!string.IsNullOrEmpty(language), u => (u.Language ?? string.Empty).Contains(language))
                         .WhereIF(!string.IsNullOrEmpty(assetPackageName), u => (u.AssetPackageName ?? string.Empty).Contains(assetPackageName))
                         .Where(u => u.AppId == input.AppId.Value)
                         .Select<GameAppAssetPackageVersionResponse>();
    }

    /// <summary>
    /// 新增前：校验包名属于当前租户 + 当前应用下启用分包或为 default
    /// </summary>
    protected override async Task OnAddingAsync(AddGameAppAssetPackageVersionRequest input)
    {
        await PackageScopeValidator.ValidateAsync(input.AppId, input.Package, _packageRepository.AsQueryable());
        await AssetPackageScopeValidator.ValidateAsync(input.AppId, input.AssetPackageName, _assetPackageRepository.AsQueryable());
        await CheckAssetPackageVersionUniqueAsync(input.AppId.GetValueOrDefault(), input.Channel, input.Platform, input.AppVersion, input.Package, input.Language, input.AssetPackageName);
    }

    /// <summary>
    /// 获取当前应用 + 当前分包下可选的资源版本列表。
    /// </summary>
    [HttpPost(CrudActionRoutes.Select)]
    [ApiDescriptionSettings(Name = "Select")]
    public async Task<List<BaseSelectPageResponse>> SelectAsync(PageGameAppAssetPackageVersionSelectRequest input)
    {
        if (input.AppId is null or <= 0)
        {
            throw BusinessError.Oh("请选择应用后再查询资源版本");
        }

        var appId = input.AppId.Value;
        if (input.AppPackageId is null or <= 0)
        {
            return new List<BaseSelectPageResponse>();
        }

        var package = await _packageRepository.AsQueryable()
                                              .Where(u => u.Id == input.AppPackageId.Value && u.GameAppId == appId)
                                              .Select(u => new { u.Id, u.PackageName, })
                                              .FirstAsync();
        if (package == null)
        {
            throw BusinessError.Oh("分包不属于当前应用");
        }

        var searchKey = input.SearchKey?.Trim() ?? string.Empty;
        var versions = await Repository.AsQueryable()
                                       .Where(u => u.AppId == appId && u.Package == package.PackageName)
                                       .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u =>
                                                    (u.ResourceVersion ?? string.Empty).Contains(searchKey)
                                                    || (u.Description ?? string.Empty).Contains(searchKey))
                                       .OrderByDescending(u => u.CreatedTime)
                                       .OrderByDescending(u => u.Id)
                                       .Take(100)
                                       .Select(u => new
                                       {
                                           u.Id,
                                           u.ResourceVersion,
                                       })
                                       .ToListAsync();

        var list = new List<BaseSelectPageResponse>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in versions)
        {
            var version = item.ResourceVersion?.Trim() ?? string.Empty;
            if (version.Length == 0 || !seen.Add(version))
            {
                continue;
            }

            list.Add(new BaseSelectPageResponse
            {
                Id = item.Id,
                Value = version,
                Label = version,
            });
        }

        return list;
    }

    private async Task CheckAssetPackageVersionUniqueAsync(long appId, string? channel, string? platform, string? appVersion, string? package, string? language, string? assetPackageName)
    {
        var normalizedChannel = channel?.Trim() ?? string.Empty;
        var normalizedPlatform = platform?.Trim() ?? string.Empty;
        var normalizedAppVersion = appVersion?.Trim() ?? string.Empty;
        var normalizedPackage = package?.Trim() ?? string.Empty;
        var normalizedLanguage = language?.Trim() ?? string.Empty;
        var normalizedAssetPackageName = assetPackageName?.Trim() ?? string.Empty;
        var exists = await Repository.AsQueryable()
                                     .Where(u => u.AppId == appId
                                                 && u.Channel == normalizedChannel
                                                 && u.Platform == normalizedPlatform
                                                 && u.AppVersion == normalizedAppVersion
                                                 && u.Package == normalizedPackage
                                                 && u.Language == normalizedLanguage
                                                 && u.AssetPackageName == normalizedAssetPackageName)
                                     .AnyAsync();
        if (exists)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordAlreadyExists);
        }
    }

    /// <summary>
    /// 新增后自动插入版本历史记录
    /// </summary>
    protected override async Task OnAddedAsync(AddGameAppAssetPackageVersionRequest input, Entity.Tenant.Game.Release.TenantGameAppAssetPackageVersionEntity entity)
    {
        var historyInput = new AddGameAppAssetPackageVersionHistoryRequest();
        var historyEntity = historyInput.Adapt<Entity.Tenant.Game.Release.TenantGameAppAssetPackageVersionHistoryEntity>();
        historyEntity.CreatedTime = entity.CreatedTime;
        historyEntity.CreatedId = entity.CreatedId;
        historyEntity.CreatedName = entity.CreatedName;
        historyEntity.ResourceVersion = input.ResourceVersion;
        historyEntity.OldVersion = string.Empty;
        historyEntity.VersionId = entity.Id;
        await RepositoryAssetPackageVersionHistory.InsertAsync(historyEntity);
    }

    /// <summary>
    /// 更新后自动插入版本历史记录（仅当版本号变化时）
    /// </summary>
    protected override async Task OnUpdatedAsync(UpdateGameAppAssetPackageVersionRequest input, Entity.Tenant.Game.Release.TenantGameAppAssetPackageVersionEntity before, Entity.Tenant.Game.Release.TenantGameAppAssetPackageVersionEntity after)
    {
        if (input.ResourceVersion == before.ResourceVersion)
        {
            return;
        }

        var historyInput = new AddGameAppAssetPackageVersionHistoryRequest();
        var entity = historyInput.Adapt<Entity.Tenant.Game.Release.TenantGameAppAssetPackageVersionHistoryEntity>();
        entity.CreatedTime = after.CreatedTime;
        entity.CreatedId = after.CreatedId;
        entity.CreatedName = after.CreatedName;
        entity.ResourceVersion = input.ResourceVersion;
        entity.OldVersion = before.ResourceVersion;
        entity.VersionId = input.Id;
        await RepositoryAssetPackageVersionHistory.InsertAsync(entity);
    }

    /// <summary>
    /// 回退资源包版本
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Return")]
    // C368：方法名与基类 CRUD UpdateAsync 同名构成双路由重载（update/return），MenuSeedRouteName.Of 按方法名
    // 推导无法消歧；按语义（回退）改名，路由经 Name="Return" 保持 api/.../return 不变。
    public async Task ReturnAsync(GameAppAssetPackageVersionReturnByIdRequest input)
    {
        var gameAppAssetPackageVersion = await Repository.GetByIdAsync(input.Id);
        if (gameAppAssetPackageVersion == null)
        {
            return;
        }

        //查询资源包
        var repositoryAssetPackageVersionHistory = await RepositoryAssetPackageVersionHistory.GetByIdAsync(input.ResourceVersionHistoryId);
        if (repositoryAssetPackageVersionHistory == null || string.IsNullOrWhiteSpace(repositoryAssetPackageVersionHistory.OldVersion))
        {
            return;
        }

        var version = gameAppAssetPackageVersion.ResourceVersion;
        gameAppAssetPackageVersion.ResourceVersion = repositoryAssetPackageVersionHistory.OldVersion;
        gameAppAssetPackageVersion.RowVersion = input.RowVersion;

        var result = await Repository.Context.UpdateWithOptimisticLockAsync(gameAppAssetPackageVersion);
        if (result == 0)
        {
            return;
        }

        var historyInput = new AddGameAppAssetPackageVersionHistoryRequest();
        var entity = historyInput.Adapt<Entity.Tenant.Game.Release.TenantGameAppAssetPackageVersionHistoryEntity>();
        entity.VersionId = input.Id;
        entity.CreatedId = repositoryAssetPackageVersionHistory.CreatedId;
        entity.CreatedName = repositoryAssetPackageVersionHistory.CreatedName;
        entity.ResourceVersion = repositoryAssetPackageVersionHistory.OldVersion;
        entity.OldVersion = version;

        await RepositoryAssetPackageVersionHistory.InsertAsync(entity);
    }
}