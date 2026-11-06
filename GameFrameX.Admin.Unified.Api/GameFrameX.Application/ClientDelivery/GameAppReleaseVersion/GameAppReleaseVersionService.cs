using GameFrameX.Application.ClientDelivery.AppScope;
using GameFrameX.Application.ClientDelivery.GameAppReleaseVersion.Dto;
using GameFrameX.Entity.Business.Game.App;

namespace GameFrameX.Application.ClientDelivery.GameAppReleaseVersion;

/// <summary>
/// 游戏版本管理服务
/// </summary>
[ApiDescriptionSettings(ClientConst.GroupName, Order = 100)]
public class GameAppReleaseVersionService : GenericCrudService<Entity.Business.Game.Release.GameAppReleaseVersionEntity, AddGameAppReleaseVersionRequest, UpdateGameAppReleaseVersionRequest, GameAppReleaseVersionResponse, PageGameAppReleaseVersionRequest>
{
    private readonly SqlSugarRepository<GameAppPackageEntity> _packageRepository;

    public GameAppReleaseVersionService(SqlSugarRepository<Entity.Business.Game.Release.GameAppReleaseVersionEntity> repository, SqlSugarRepository<GameAppPackageEntity> packageRepository) : base(repository)
    {
        _packageRepository = packageRepository;
    }

    /// <summary>
    /// 构建分页查询条件
    /// </summary>
    protected override ISugarQueryable<GameAppReleaseVersionResponse> BuildPageQuery(PageGameAppReleaseVersionRequest input)
    {
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

                         .Select<GameAppReleaseVersionResponse>();
    }

    /// <summary>
    /// 新增前：校验包名属于主库中启用分包或为 default
    /// </summary>
    protected override async Task OnAddingAsync(AddGameAppReleaseVersionRequest input)
    {
        await PackageScopeValidator.ValidateAsync(input.Package, _packageRepository.AsQueryable());
        await CheckReleaseVersionUniqueAsync(input.Channel, input.Platform, input.OldVersion, input.AppVersion, input.Package, input.Language);
    }

    /// <summary>
    /// 更新前：确认记录存在并校验启用分包
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateGameAppReleaseVersionRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        await PackageScopeValidator.ValidateAsync(input.Package, _packageRepository.AsQueryable());
        await CheckReleaseVersionUniqueAsync(input.Channel, input.Platform, input.OldVersion, input.AppVersion, input.Package, input.Language, input.Id);
    }



    private async Task CheckReleaseVersionUniqueAsync(string? channel, string? platform, string? oldVersion, string? appVersion, string? package, string? language, long? excludeId = null)
    {
        var normalizedChannel = channel?.Trim() ?? string.Empty;
        var normalizedPlatform = platform?.Trim() ?? string.Empty;
        var normalizedOldVersion = oldVersion?.Trim() ?? string.Empty;
        var normalizedAppVersion = appVersion?.Trim() ?? string.Empty;
        var normalizedPackage = package?.Trim() ?? string.Empty;
        var normalizedLanguage = language?.Trim() ?? string.Empty;
        var query = Repository.AsQueryable()
                              .Where(u => u.Channel == normalizedChannel
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