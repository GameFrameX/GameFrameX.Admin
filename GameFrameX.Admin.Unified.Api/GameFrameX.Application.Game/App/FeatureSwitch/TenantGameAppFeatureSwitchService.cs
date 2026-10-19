using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.App.FeatureSwitch.Dto;
using GameFrameX.Core.Const;
using GameFrameX.Entity.Tenant.Game.App;
using GameFrameX.Entity.Tenant.Game.FeatureSwitch;

namespace GameFrameX.Application.Game.App.FeatureSwitch;

/// <summary>
/// 活动/功能开关管理服务（开关注册表的运营维护面）。
/// <para>写入侧负责把作用域语义固化为可投放事实：消费方标记合法、时间窗不倒置、作用域维度空值归一为
/// Default 哨兵、租户默认层不得携带具体包名、同作用域组合不重复。</para>
/// </summary>
[ApiDescriptionSettings(ClientConst.GroupName, Order = 100)]
public class TenantGameAppFeatureSwitchService : GenericCrudService<TenantGameAppFeatureSwitchEntity, AddGameAppFeatureSwitchRequest, UpdateGameAppFeatureSwitchRequest, GameAppFeatureSwitchResponse, PageGameAppFeatureSwitchRequest>
{
    private readonly SqlSugarRepository<TenantGameAppPackageEntity> _packageRepository;

    /// <summary>
    /// 初始化活动/功能开关管理服务。
    /// </summary>
    /// <param name="repository">开关注册表仓储。</param>
    /// <param name="packageRepository">分包仓储，供包名作用域校验使用。</param>
    public TenantGameAppFeatureSwitchService(SqlSugarRepository<TenantGameAppFeatureSwitchEntity> repository,
        SqlSugarRepository<TenantGameAppPackageEntity> packageRepository) : base(repository)
    {
        _packageRepository = packageRepository;
    }

    /// <summary>
    /// 构建分页查询条件：应用作用域按「正值看应用行、留空看租户默认层」收口，避免两层行混列。
    /// </summary>
    /// <param name="input">分页查询输入。</param>
    /// <returns>分页查询。</returns>
    protected override ISugarQueryable<GameAppFeatureSwitchResponse> BuildPageQuery(PageGameAppFeatureSwitchRequest input)
    {
        var switchKey = input.SwitchKey?.Trim() ?? string.Empty;
        var audience = input.Audience?.Trim() ?? string.Empty;
        var appId = input.AppId.GetValueOrDefault();
        var query = Repository.AsQueryable();
        query = appId > 0
                    ? query.Where(u => u.AppId == appId)
                    : query.Where(u => u.AppId == null || u.AppId == 0);
        return query.WhereIF(!string.IsNullOrEmpty(switchKey), u => u.SwitchKey.Contains(switchKey))
                    .WhereIF(!string.IsNullOrEmpty(audience), u => u.Audience == audience)
                    .WhereIF(input.IsEnabled.HasValue, u => u.IsEnabled == input.IsEnabled.Value)
                    .Select<GameAppFeatureSwitchResponse>();
    }

    /// <summary>
    /// 新增前：归一化与校验业务字段 + 包名作用域校验 + 同作用域组合查重。
    /// </summary>
    /// <param name="input">新增输入。</param>
    /// <returns>异步任务。</returns>
    protected override async Task OnAddingAsync(AddGameAppFeatureSwitchRequest input)
    {
        NormalizeAndValidate(input);
        await PackageScopeValidator.ValidateAsync(input.AppId, input.Package, _packageRepository.AsQueryable());
        await EnsureScopeUniqueAsync(input, null);
    }

    /// <summary>
    /// 更新前：记录存在 + 应用作用域锁定（不可跨应用改挂）+ 归一化与校验 + 包名作用域校验 + 同作用域组合查重（排除自身）。
    /// </summary>
    /// <param name="input">更新输入。</param>
    /// <returns>异步任务。</returns>
    protected override async Task OnUpdatingAsync(UpdateGameAppFeatureSwitchRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.AppId;
        NormalizeAndValidate(input);
        await PackageScopeValidator.ValidateAsync(input.AppId, input.Package, _packageRepository.AsQueryable());
        await EnsureScopeUniqueAsync(input, input.Id);
    }

    /// <summary>
    /// 归一化 + 业务校验：开关键与显示名去空白、消费方标记收小写并校验取值域、作用域维度空值归一为
    /// Default 哨兵、ValueJson 须为合法 JSON 原文（不重写，保留下发原文）、时间窗不得倒置、灰度比例限 0-100、
    /// 设备白名单须为 JSON 数组。
    /// </summary>
    /// <param name="input">新增或更新输入。</param>
    private static void NormalizeAndValidate(GameAppFeatureSwitchBaseRequest input)
    {
        input.SwitchKey = (input.SwitchKey ?? string.Empty).Trim();
        if (input.SwitchKey.Length == 0)
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "SwitchKey", });
        }

        input.DisplayName = (input.DisplayName ?? string.Empty).Trim();
        if (input.DisplayName.Length == 0)
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "DisplayName", });
        }

        input.Audience = (input.Audience ?? string.Empty).Trim().ToLowerInvariant();
        if (!FeatureSwitchAudienceValues.IsValid(input.Audience))
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "Audience", expected = "client/server/both", });
        }

        // 作用域维度空值统一落 Default 哨兵：投放侧「空 = 不限」的语义靠列值本身表达，
        // 不让 null / 空串 / "default" 三种写法在库里并存。
        input.Platform = NormalizeScope(input.Platform, GameDimensionConst.PlatformDefault);
        input.Package = NormalizeScope(input.Package, GameDimensionConst.PackageNameDefault);
        input.Language = NormalizeScope(input.Language, GameDimensionConst.LanguageDefault);
        input.Channel = NormalizeScope(input.Channel, GameDimensionConst.ChannelNameDefault);

        ValidateValueJson(input);
        ValidateTimeWindow(input);
        ValidateRollout(input);
    }

    /// <summary>
    /// 校验开关参数 JSON：必须是合法 JSON（对象 / 数组 / 标量均可），但按契约原样下发，不做重写或归一化。
    /// </summary>
    /// <param name="input">新增或更新输入。</param>
    private static void ValidateValueJson(GameAppFeatureSwitchBaseRequest input)
    {
        if (string.IsNullOrWhiteSpace(input.ValueJson))
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "ValueJson", });
        }

        try
        {
            using (var document = JsonDocument.Parse(input.ValueJson))
            {
                _ = document.RootElement.ValueKind;
            }
        }
        catch (JsonException)
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "ValueJson", message = "开关参数必须是合法 JSON", });
        }
    }

    /// <summary>
    /// 校验时间窗：两端均填时要求开始早于结束（区间左闭右开，等长窗无意义）。
    /// </summary>
    /// <param name="input">新增或更新输入。</param>
    private static void ValidateTimeWindow(GameAppFeatureSwitchBaseRequest input)
    {
        if (input.StartTime.HasValue && input.EndTime.HasValue && input.StartTime.Value >= input.EndTime.Value)
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "StartTime/EndTime", message = "生效开始时间必须早于结束时间", });
        }
    }

    /// <summary>
    /// 校验灰度字段：比例限 0-100；白名单为空时落 null（空串会让投放侧的 JSON 解析走异常分支），
    /// 非空时必须是 JSON 数组。
    /// </summary>
    /// <param name="input">新增或更新输入。</param>
    private static void ValidateRollout(GameAppFeatureSwitchBaseRequest input)
    {
        if (input.RolloutPercent < 0 || input.RolloutPercent > 100)
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "RolloutPercent", });
        }

        if (string.IsNullOrWhiteSpace(input.DeviceWhitelistJson))
        {
            input.DeviceWhitelistJson = null;
            return;
        }

        try
        {
            using (var document = JsonDocument.Parse(input.DeviceWhitelistJson))
            {
                if (document.RootElement.ValueKind != JsonValueKind.Array)
                {
                    throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "DeviceWhitelistJson", message = "设备白名单必须是 JSON 数组", });
                }
            }
        }
        catch (JsonException)
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "DeviceWhitelistJson", message = "设备白名单必须是 JSON 数组", });
        }

        input.DeviceWhitelistJson = input.DeviceWhitelistJson.Trim();
    }

    /// <summary>
    /// 同作用域组合查重：开关键 + 应用作用域 + 四个作用域维度全部相同即视为重复（租户维度由仓储过滤器收口）。
    /// <para>排重不下沉到 DB 唯一索引：作用域维度可空组合多，索引无法表达「租户默认层」的等价类。</para>
    /// </summary>
    /// <param name="input">新增或更新输入，作用域维度须已归一化。</param>
    /// <param name="excludeId">更新时排除自身；新增传 null。</param>
    /// <returns>异步任务。</returns>
    private async Task EnsureScopeUniqueAsync(GameAppFeatureSwitchBaseRequest input, long? excludeId)
    {
        var appId = input.AppId.GetValueOrDefault();
        var duplicated = await Repository.AsQueryable()
                                         .WhereIF(appId > 0, u => u.AppId == appId)
                                         .WhereIF(appId <= 0, u => u.AppId == null || u.AppId == 0)
                                         .Where(u => u.SwitchKey == input.SwitchKey
                                                     && u.Platform == input.Platform
                                                     && u.Package == input.Package
                                                     && u.Language == input.Language
                                                     && u.Channel == input.Channel)
                                         .WhereIF(excludeId.HasValue, u => u.Id != excludeId.Value)
                                         .AnyAsync();
        if (duplicated)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordAlreadyExists, new { switchKey = input.SwitchKey, input.Platform, input.Package, input.Language, input.Channel, });
        }
    }

    /// <summary>
    /// 作用域维度归一化：空白值统一落该维度的 Default 哨兵。
    /// </summary>
    /// <param name="value">维度原值；null / 空白表示不限。</param>
    /// <param name="defaultValue">该维度的 Default 哨兵值。</param>
    /// <returns>归一化后的维度值。</returns>
    private static string NormalizeScope(string? value, string defaultValue)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return normalized.Length == 0 ? defaultValue : normalized;
    }
}
