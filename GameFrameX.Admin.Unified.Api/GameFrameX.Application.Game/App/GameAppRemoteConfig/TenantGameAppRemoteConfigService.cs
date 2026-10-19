using System.ComponentModel;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.App.GameAppRemoteConfig.Dto;
using GameFrameX.Application.Game.Online.LiveOps.Dispatch;
using GameFrameX.Application.Game.Online.LiveOps.Dispatch.Dto;
using GameFrameX.Entity.Game.Online;
using GameFrameX.Entity.Tenant.Game.App;

namespace GameFrameX.Application.Game.App.GameAppRemoteConfig;

/// <summary>
/// 远程配置管理服务
/// </summary>
[ApiDescriptionSettings(ClientConst.GroupName, Order = 100)]
public class TenantGameAppRemoteConfigService : GenericCrudService<Entity.Tenant.Game.RemoteConfig.TenantGameAppRemoteConfigEntity, AddGameAppRemoteConfigRequest, UpdateGameAppRemoteConfigRequest, GameAppRemoteConfigResponse, PageGameAppRemoteConfigRequest>
{
    private readonly SqlSugarRepository<TenantGameAppPackageEntity> _packageRepository;
    private readonly IOnlineLiveOpsDispatchService _dispatchService;

    public TenantGameAppRemoteConfigService(SqlSugarRepository<Entity.Tenant.Game.RemoteConfig.TenantGameAppRemoteConfigEntity> repository,
        SqlSugarRepository<TenantGameAppPackageEntity> packageRepository,
        IOnlineLiveOpsDispatchService dispatchService) : base(repository)
    {
        _packageRepository = packageRepository;
        _dispatchService = dispatchService;
    }

    /// <summary>
    /// 构建分页查询条件
    /// </summary>
    protected override ISugarQueryable<GameAppRemoteConfigResponse> BuildPageQuery(PageGameAppRemoteConfigRequest input)
    {
        var appId = RequireAppId(input.AppId, "请选择应用后再维护客户端远程配置");
        var searchKey = input.SearchKey?.Trim() ?? string.Empty;
        var channel = input.Channel?.Trim() ?? string.Empty;
        var content = input.Content?.Trim() ?? string.Empty;
        var checkResourceUrl = input.CheckResourceUrl?.Trim() ?? string.Empty;
        var checkAppVersionUrl = input.CheckAppVersionUrl?.Trim() ?? string.Empty;
        var platform = input.Platform?.Trim() ?? string.Empty;
        var appVersion = input.AppVersion?.Trim() ?? string.Empty;
        var package = input.Package?.Trim() ?? string.Empty;
        var language = input.Language?.Trim() ?? string.Empty;
        var createUserName = input.CreatedName?.Trim() ?? string.Empty;
        var updateUserName = input.UpdatedName?.Trim() ?? string.Empty;
        var description = input.Description?.Trim() ?? string.Empty;
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrEmpty(searchKey), u =>
                                      u.Channel.Contains(searchKey)
                                      || u.Content.Contains(searchKey)
                                      || u.CheckResourceUrl.Contains(searchKey)
                                      || u.CheckAppVersionUrl.Contains(searchKey)
                                      || u.Platform.Contains(searchKey)
                                      || u.AppVersion.Contains(searchKey)
                                      || u.Package.Contains(searchKey)
                                      || u.Language.Contains(searchKey)
                                      || (u.CreatedName ?? string.Empty).Contains(searchKey)
                                      || (u.UpdatedName ?? string.Empty).Contains(searchKey)
                                      || u.Description.Contains(searchKey)
                         )
                         .WhereIF(!string.IsNullOrEmpty(channel), u => u.Channel.Contains(channel))
                         .WhereIF(!string.IsNullOrEmpty(content), u => u.Content.Contains(content))
                         .WhereIF(!string.IsNullOrEmpty(checkResourceUrl), u => u.CheckResourceUrl.Contains(checkResourceUrl))
                         .WhereIF(!string.IsNullOrEmpty(checkAppVersionUrl), u => u.CheckAppVersionUrl.Contains(checkAppVersionUrl))
                         .WhereIF(!string.IsNullOrEmpty(platform), u => u.Platform.Contains(platform))
                         .WhereIF(!string.IsNullOrEmpty(appVersion), u => u.AppVersion.Contains(appVersion))
                         .WhereIF(!string.IsNullOrEmpty(package), u => u.Package == package)
                         .WhereIF(!string.IsNullOrEmpty(language), u => u.Language.Contains(language))
                         .WhereIF(!string.IsNullOrEmpty(createUserName), u => (u.CreatedName ?? string.Empty).Contains(createUserName))
                         .WhereIF(!string.IsNullOrEmpty(updateUserName), u => (u.UpdatedName ?? string.Empty).Contains(updateUserName))
                         .WhereIF(!string.IsNullOrEmpty(description), u => u.Description.Contains(description))
                         .Where(u => u.AppId == appId)
                         .Select<GameAppRemoteConfigResponse>();
    }

    /// <summary>
    /// 新增前：校验包名作用域 + 从 ModeContents 解析 Content
    /// </summary>
    protected override async Task OnAddingAsync(AddGameAppRemoteConfigRequest input)
    {
        input.AppId = RequireAppId(input.AppId, "请选择应用后再新增客户端远程配置");
        await PackageScopeValidator.ValidateAsync(input.AppId, input.Package, _packageRepository.AsQueryable());
        ValidateDeliveryFields(input);
        ResolveContentFromMode(input);
    }

    /// <summary>
    /// 更新前：校验包名作用域 + 从 ModeContents 解析 Content
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateGameAppRemoteConfigRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.AppId;
        RequireAppId(input.AppId, "客户端远程配置不属于当前应用");
        await PackageScopeValidator.ValidateAsync(input.AppId, input.Package, _packageRepository.AsQueryable());
        ValidateDeliveryFields(input);
        ResolveContentFromMode(input);
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

    /// <summary>
    /// 校验记录级投放字段，避免非法灰度比例或白名单 JSON 写入配置事实源。
    /// </summary>
    private static void ValidateDeliveryFields<T>(T input) where T : class
    {
        var rolloutPercentProp = typeof(T).GetProperty("RolloutPercent");
        if (rolloutPercentProp?.GetValue(input) is int rolloutPercent && (rolloutPercent < 0 || rolloutPercent > 100))
        {
            throw BusinessError.Oh("灰度比例必须在 0 到 100 之间");
        }

        var whitelistProp = typeof(T).GetProperty("DeviceWhitelistJson");
        var whitelistJson = whitelistProp?.GetValue(input) as string;
        if (string.IsNullOrWhiteSpace(whitelistJson))
        {
            whitelistProp?.SetValue(input, null);
            return;
        }

        try
        {
            using var doc = JsonDocument.Parse(whitelistJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Array)
            {
                throw BusinessError.Oh("设备白名单必须是合法 JSON 数组");
            }
        }
        catch (JsonException)
        {
            throw BusinessError.Oh("设备白名单必须是合法 JSON 数组");
        }

        whitelistProp?.SetValue(input, whitelistJson.Trim());
    }

    /// <summary>
    /// 从 ModeContents 中提取当前激活模式的内容写入 Content
    /// </summary>
    private static void ResolveContentFromMode<T>(T input) where T : class
    {
        var modeContentsProp = typeof(T).GetProperty("ModeContents");
        var activeModeCodeProp = typeof(T).GetProperty("ActiveModeCode");
        var contentProp = typeof(T).GetProperty("Content");

        if (modeContentsProp == null || activeModeCodeProp == null || contentProp == null)
        {
            return;
        }

        var modeContents = modeContentsProp.GetValue(input) as string;
        if (string.IsNullOrEmpty(modeContents))
        {
            return;
        }

        var activeModeCode = activeModeCodeProp.GetValue(input) as string ?? "formal";
        activeModeCodeProp.SetValue(input, activeModeCode);

        Dictionary<string, JsonElement>? dict;
        try
        {
            dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(modeContents);
        }
        catch (JsonException)
        {
            throw BusinessError.Oh("模式内容必须是合法 JSON 对象");
        }

        if (dict?.TryGetValue(activeModeCode, out var activeContent) == true)
        {
            if (activeContent.ValueKind != JsonValueKind.Object)
            {
                throw BusinessError.Oh("当前模式内容必须是 JSON 对象");
            }

            contentProp.SetValue(input, activeContent.GetRawText());
        }
    }

    /// <summary>
    /// 经 Online 统一下发当前远程配置（来源 change C343 / vault:C8 S7.7）：转调 OnlineLiveOpsDispatchService 单一入口（publishRemoteConfig 通道），
    /// 来源类型固定注入 RemoteConfig，存在性 / 应用归属 / 作用域校验在单一入口完成。
    /// </summary>
    [HttpPost("dispatchOnline")]
    [ApiDescriptionSettings(Name = "DispatchOnline")]
    [DisplayName("经Online统一下发")]
    public Task<OnlineLiveOpsDispatchResponse> DispatchOnlineAsync(OnlineLiveOpsSourceDispatchRequest input)
    {
        return _dispatchService.DispatchAsync(new OnlineLiveOpsDispatchRequest
        {
            Source = LiveOpsSourceKind.RemoteConfig,
            BusinessKey = input.BusinessKey,
            AppId = input.AppId,
            ServerId = input.ServerId,
            SegmentId = input.SegmentId,
            Reason = input.Reason,
        });
    }
}