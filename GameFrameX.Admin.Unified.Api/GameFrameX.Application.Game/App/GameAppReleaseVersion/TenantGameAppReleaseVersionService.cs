using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.App.GameAppReleaseVersion.Dto;
using GameFrameX.Entity.Tenant.Game.App;

namespace GameFrameX.Application.Game.App.GameAppReleaseVersion;

/// <summary>
/// 游戏版本管理服务
/// </summary>
[ApiDescriptionSettings(ClientConst.GroupName, Order = 100)]
public class TenantGameAppReleaseVersionService : GenericCrudService<Entity.Tenant.Game.Release.TenantGameAppReleaseVersionEntity, AddGameAppReleaseVersionRequest, UpdateGameAppReleaseVersionRequest, GameAppReleaseVersionResponse, PageGameAppReleaseVersionRequest>
{
    private readonly SqlSugarRepository<TenantGameAppPackageEntity> _packageRepository;

    public TenantGameAppReleaseVersionService(SqlSugarRepository<Entity.Tenant.Game.Release.TenantGameAppReleaseVersionEntity> repository, SqlSugarRepository<TenantGameAppPackageEntity> packageRepository) : base(repository)
    {
        _packageRepository = packageRepository;
    }

    /// <summary>
    /// 构建分页查询条件
    /// </summary>
    protected override ISugarQueryable<GameAppReleaseVersionResponse> BuildPageQuery(PageGameAppReleaseVersionRequest input)
    {
        if (input.AppId is null or <= 0)
        {
            throw BusinessError.Oh("请选择应用后再维护发布版本");
        }

        var searchKey = input.SearchKey?.Trim() ?? string.Empty;
        var channel = input.Channel?.Trim() ?? string.Empty;
        var platform = input.Platform?.Trim() ?? string.Empty;
        var appVersion = input.AppVersion?.Trim() ?? string.Empty;
        var package = input.Package?.Trim() ?? string.Empty;
        var language = input.Language?.Trim() ?? string.Empty;
        var description = input.Description?.Trim() ?? string.Empty;
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrEmpty(searchKey), u =>
                                      u.Description.Contains(searchKey)
                         )
                         .WhereIF(!string.IsNullOrEmpty(channel), u => u.Channel == channel)
                         .WhereIF(!string.IsNullOrEmpty(platform), u => u.Platform == platform)
                         .WhereIF(!string.IsNullOrEmpty(appVersion), u => u.AppVersion == appVersion)
                         .WhereIF(!string.IsNullOrEmpty(package), u => u.Package == package)
                         .WhereIF(!string.IsNullOrEmpty(language), u => u.Language == language)
                         .WhereIF(!string.IsNullOrEmpty(description), u => u.Description.Contains(description))
                         .Where(u => u.AppId == input.AppId.Value)
                         .Select<GameAppReleaseVersionResponse>();
    }

    /// <summary>
    /// 新增前：校验包名属于当前租户 + 当前应用下启用分包或为 default
    /// </summary>
    protected override async Task OnAddingAsync(AddGameAppReleaseVersionRequest input)
    {
        var appId = RequireAppId(input.AppId, "请选择应用后再新增发布版本");
        await PackageScopeValidator.ValidateAsync(input.AppId, input.Package, _packageRepository.AsQueryable());
        await CheckReleaseVersionUniqueAsync(appId, input.Channel, input.Platform, input.OldVersion, input.AppVersion, input.Package, input.Language);
    }

    /// <summary>
    /// 更新前：AppId 不可变，从既有实体回填后校验包名作用域
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateGameAppReleaseVersionRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.AppId;
        var appId = RequireAppId(input.AppId, "发布版本不属于当前应用");
        await PackageScopeValidator.ValidateAsync(input.AppId, input.Package, _packageRepository.AsQueryable());
        await CheckReleaseVersionUniqueAsync(appId, input.Channel, input.Platform, input.OldVersion, input.AppVersion, input.Package, input.Language, input.Id);
    }

    private static long RequireAppId(long? appId, string message)
    {
        var resolvedAppId = appId ?? 0;
        if (resolvedAppId <= 0)
        {
            throw BusinessError.Oh(message);
        }

        return resolvedAppId;
    }

    private async Task CheckReleaseVersionUniqueAsync(long appId, string? channel, string? platform, string? oldVersion, string? appVersion, string? package, string? language, long? excludeId = null)
    {
        var normalizedChannel = channel?.Trim() ?? string.Empty;
        var normalizedPlatform = platform?.Trim() ?? string.Empty;
        var normalizedOldVersion = oldVersion?.Trim() ?? string.Empty;
        var normalizedAppVersion = appVersion?.Trim() ?? string.Empty;
        var normalizedPackage = package?.Trim() ?? string.Empty;
        var normalizedLanguage = language?.Trim() ?? string.Empty;
        var query = Repository.AsQueryable()
                              .Where(u => u.AppId == appId
                                          && u.Channel == normalizedChannel
                                          && u.Platform == normalizedPlatform
                                          && u.OldVersion == normalizedOldVersion
                                          && u.AppVersion == normalizedAppVersion
                                          && u.Package == normalizedPackage
                                          && u.Language == normalizedLanguage);
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