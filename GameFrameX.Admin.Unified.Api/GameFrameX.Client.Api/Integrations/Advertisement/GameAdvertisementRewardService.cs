using GameFrameX.Advertisement.Abstract;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.ServerManager;
using GameFrameX.Client.Api.Services.ClientApiTenantOptions;
using GameFrameX.Entity.Enum;
using GameFrameX.Foundation.Idempotency;

namespace GameFrameX.Client.Api.Integrations.Advertisement;

/// <summary>
/// 广告奖励发放账本服务（C114 / GFX-110）。
/// <para>统一执行 TopOn / GoogleAdMob / UnityAds 服务端回调的「读 ConfigJson → 反序列化强类型 Option → 按 Option <c>*Param</c> 取参 → 按平台算法验签 → 幂等判定 → 调游戏服发奖 → 回写账本状态」链路。</para>
/// <para>C222 provider 模式：平台特定（验签 + 角色解析）下沉到 <see cref="IAdvertisementCallbackHandler"/> 各实现，本类只保留通用流程 + <see cref="IAdvertisementCallbackFactory"/> 路由。加平台 = 加 Handler + 一行 DI 注册，本类不动。</para>
/// <para>fail closed：验签失败 / 配置缺失 / 角色缺失一律不写账本（验签失败）或不发奖。</para>
/// <para>幂等判定统一走 Foundation 协调器 + 账本表即 Store（<see cref="AdvertisementRewardLedgerIdempotencyStore"/>）：
/// IdempotencyKey = "adreward:" + NormalizedProvider + ":" + TransactionId，占位即 claim 一行 Pending，
/// 同交易号按行状态回放（Success 已发放 / Pending 处理中），Failed 可重试复用同一行；站点规则见 Store 类备注。</para>
/// </summary>
public sealed class GameAdvertisementRewardService
{
    /// <summary>
    /// 验签失败的统一 Code（与既有 TopOn 回调一致）。
    /// </summary>
    private const int CodeSignatureFailed = 601;

    /// <summary>
    /// 其他失败 / 成功共用 Code（用 <see cref="AdvertisementRewardReceiveResult.IsValid"/> 区分）。
    /// </summary>
    private const int CodeGeneral = 0;

    private readonly IClientApiTenantOptionsProvider _optionsProvider;
    private readonly Repository<TenantGameAdvertisementRewardEntity> _rewardRepository;
    private readonly Repository<TenantGameAreaEntity> _areaRepository;
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAdvertisementCallbackFactory _advertisementCallbackFactory;

    /// <summary>
    /// </summary>
    public GameAdvertisementRewardService(IClientApiTenantOptionsProvider optionsProvider,
        Repository<TenantGameAdvertisementRewardEntity> rewardRepository,
        Repository<TenantGameAreaEntity> areaRepository,
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        IAdvertisementCallbackFactory advertisementCallbackFactory)
    {
        _optionsProvider = optionsProvider;
        _rewardRepository = rewardRepository;
        _areaRepository = areaRepository;
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _advertisementCallbackFactory = advertisementCallbackFactory;
    }

    /// <summary>
    /// 接收并处理 TopOn 服务端奖励回调。
    /// </summary>
    public Task<AdvertisementRewardReceiveResult> ReceiveTopOnAsync()
    {
        return ReceiveAsync("TopOn");
    }

    /// <summary>
    /// 接收并处理 Google AdMob 服务端奖励回调。
    /// </summary>
    public Task<AdvertisementRewardReceiveResult> ReceiveAdMobAsync()
    {
        return ReceiveAsync("GoogleAdMob");
    }

    /// <summary>
    /// 接收并处理 Unity Ads 服务端奖励回调。
    /// </summary>
    public Task<AdvertisementRewardReceiveResult> ReceiveUnityAdsAsync()
    {
        return ReceiveAsync("UnityAds");
    }

    /// <summary>
    /// 按平台路由到对应 <see cref="IAdvertisementCallbackHandler"/>，执行通用流程。
    /// </summary>
    private Task<AdvertisementRewardReceiveResult> ReceiveAsync(string providerCanonical)
    {
        var handler = _advertisementCallbackFactory.GetHandler(providerCanonical);
        if (handler == null)
        {
            return Task.FromResult(Fail(CodeGeneral, $"不支持的平台: {providerCanonical}", null));
        }

        return ReceiveAsyncCore(providerCanonical, handler.Verify, handler.ResolveRoleAsync);
    }

    /// <summary>
    /// 三 Provider 同构处理流程（fail closed，顺序严格按 change.md §3；幂等判定统一走 Foundation 接口）。
    /// </summary>
    private async Task<AdvertisementRewardReceiveResult> ReceiveAsyncCore(string providerCanonical,
        Func<TenantAdvertisementCallbackOptions, IQueryCollection, OptionVerification> verify,
        Func<object, IQueryCollection, Task<RoleResolution?>> resolveRole)
    {
        // 1. 取配置
        var advertisementOptions = await _optionsProvider.GetAdvertisementCallbackOptionsAsync();
        if (advertisementOptions == null || string.IsNullOrWhiteSpace(advertisementOptions.ConfigJson))
        {
            return Fail(CodeGeneral, "未配置广告回调验签", null);
        }

        var query = _httpContextAccessor.HttpContext?.Request?.Query;
        if (query == null)
        {
            return Fail(CodeGeneral, "缺少请求上下文", null);
        }

        // 2 + 3. 解析 Option + schema 取参 + 验签
        OptionVerification verification;
        try
        {
            verification = verify(advertisementOptions, query);
        }
        catch (Exception ex)
        {
            LogHelper.Error($"广告回调验签异常 provider={providerCanonical}: {ex.Message}", ex);
            return Fail(CodeSignatureFailed, "签名验证失败", null);
        }

        var payload = verification.Payload;
        if (!payload.IsValid)
        {
            // 验签失败 fail closed：不查角色、不写账本、不发奖
            return Fail(CodeSignatureFailed, "签名验证失败", null);
        }

        if (string.IsNullOrWhiteSpace(payload.TransactionId))
        {
            return Fail(CodeSignatureFailed, "签名验证失败", null);
        }

        // 4. 幂等判定（Foundation 协调器 + 账本表即 Store）：占位 → 执行 → 完成落定 / 失败落定
        var normalizedProvider = ClientApiTenantOptionsProvider.NormalizeAdvertisementProvider(providerCanonical);
        var idempotencyKey = "adreward:" + normalizedProvider + ":" + payload.TransactionId;
        var ledgerTemplate = BuildLedger(providerCanonical, idempotencyKey, payload, null, TimerHelper.UnixTimeSeconds());
        var store = new AdvertisementRewardLedgerIdempotencyStore(_rewardRepository, ledgerTemplate);
        var coordinator = new IdempotencyCoordinator(store, SystemClock.Instance, AdvertisementRewardLedgerIdempotencyStore.CreateCoordinatorOptions());
        var identifier = new IdempotencyRecordIdentifier(AdvertisementRewardLedgerIdempotencyStore.IdempotencyScope, idempotencyKey);
        var decision = await coordinator.BeginAsync(new IdempotencyBeginRequest(AdvertisementRewardLedgerIdempotencyStore.IdempotencyScope, idempotencyKey, AdvertisementRewardLedgerIdempotencyStore.RequestDigest));
        if (decision.DecisionKind == IdempotencyDecisionKind.Replay)
        {
            // 同交易号已发放：回放首次结果，不再查角色、不写账本、不发奖
            return new AdvertisementRewardReceiveResult(true, CodeGeneral, "奖励已发放", idempotencyKey, AdvertisementRewardStatusEnum.Success);
        }

        if (decision.DecisionKind == IdempotencyDecisionKind.Busy)
        {
            // 并发占位未落定（Pending 等待超时）：稍后重试
            return new AdvertisementRewardReceiveResult(false, CodeGeneral, "奖励发放处理中", idempotencyKey, AdvertisementRewardStatusEnum.Pending);
        }

        // 5. 解析角色 + 应用作用域（仅占位成功后执行）
        RoleResolution? resolution = null;
        string? roleFailureReason = null;
        try
        {
            resolution = await resolveRole(verification.Option, query);
            if (resolution?.Role == null)
            {
                roleFailureReason = "角色或区服不存在";
            }
            else if (!AppScopeRules.IsValidFactAppId(resolution.Role.AppId))
            {
                roleFailureReason = "应用作用域无效";
                resolution = null;
            }
        }
        catch (Exception ex)
        {
            LogHelper.Error($"广告回调角色解析异常 provider={providerCanonical}: {ex.Message}", ex);
            roleFailureReason = "角色或区服不存在";
        }

        // 6. 回读占位账本行，补写角色事实（占位已写入 / Failed 复用，业务事实归服务）
        var ledger = await _rewardRepository.GetFirstAsync(x => x.IdempotencyKey == idempotencyKey);
        if (resolution != null)
        {
            // 应用作用域事实守卫（双保险，Valid 已过；保留 EnsureFactAppId 链式赋值语义）
            ledger.AppId = AppScopeRules.EnsureFactAppId(resolution.Role.AppId, "游戏广告奖励");
            ledger.RoleId = resolution.Role.Id.ToString();
            ledger.UserId = resolution.UserId;
            ledger.AreaId = resolution.Role.AreaId;
            await _rewardRepository.UpdateAsync(ledger);
        }
        else
        {
            // 角色缺失：写失败原因（业务事实）+ 幂等落定失败（Failed 可重试），不发奖
            var missingRoleReason = roleFailureReason ?? "角色或区服不存在";
            await WriteFailureReasonAsync(idempotencyKey, missingRoleReason);
            await coordinator.FailAsync(new IdempotencyFailureRequest(identifier, 0));
            return new AdvertisementRewardReceiveResult(false, CodeGeneral, missingRoleReason, idempotencyKey, AdvertisementRewardStatusEnum.Failed);
        }

        // 7. 发放
        string? failureReason = null;
        var dispatchSuccess = false;
        try
        {
            var area = await _areaRepository.AsQueryable().Filter(null, true).Where(u => u.IsDeleted == false).FirstAsync(x => x.Id == resolution.Role.AreaId);
            if (area == null || string.IsNullOrWhiteSpace(area.HttpManageUrl))
            {
                failureReason = "区服不存在或管理地址缺失";
            }
            else
            {
                var request = new GameAdWebHookController.PlayerSendAdvertisementItemRequest
                {
                    RoleId = resolution.Role.Id,
                    IsTest = payload.IsSandbox,
                };
                if (payload.AdvertisementPackageId.HasValue)
                {
                    request.AdPackageId = payload.AdvertisementPackageId.Value;
                }

                var httpResponse = await _httpClient.PostJsonToStringAsync(GameServerEndpointUrlBuilder.Build(area, GameServerEndpointKindEnum.Fulfillment, "req_player_send_advertisement_item"), request);
                var resultData = httpResponse.ToHttpJsonResult<GameAdWebHookController.PlayerSendAdvertisementItemResponse>();
                if (resultData.IsSuccess)
                {
                    dispatchSuccess = true;
                }
                else
                {
                    failureReason = resultData.Message;
                }
            }
        }
        catch (Exception ex)
        {
            failureReason = ex.Message;
            LogHelper.Error($"广告奖励发放异常 key={idempotencyKey}: {ex.Message}", ex);
        }

        // 8. 幂等落定 + 9. 返回（状态列由 Store 收口；失败原因业务事实先行写入）
        if (dispatchSuccess)
        {
            await coordinator.CompleteAsync(new IdempotencyCompletionRequest(identifier, ReadOnlyMemory<byte>.Empty, 0));
            return new AdvertisementRewardReceiveResult(true, CodeGeneral, "发放成功", idempotencyKey, AdvertisementRewardStatusEnum.Success);
        }

        var resolvedFailureReason = Truncate(failureReason ?? "发放失败", 1024);
        await WriteFailureReasonAsync(idempotencyKey, resolvedFailureReason);
        await coordinator.FailAsync(new IdempotencyFailureRequest(identifier, 0));
        return new AdvertisementRewardReceiveResult(false, CodeGeneral, resolvedFailureReason, idempotencyKey, AdvertisementRewardStatusEnum.Failed);
    }

    /// <summary>
    /// 写入账本失败原因（业务事实，服务侧职责；状态列翻转由 Store 落定）。
    /// </summary>
    private async Task WriteFailureReasonAsync(string idempotencyKey, string failureReason)
    {
        await _rewardRepository.AsUpdateable()
                               .SetColumns(u => u.FailureReason == failureReason)
                               .Where(u => u.IdempotencyKey == idempotencyKey)
                               .ExecuteCommandAsync();
    }

    private static TenantGameAdvertisementRewardEntity BuildLedger(string providerCanonical,
        string idempotencyKey,
        VerifiedPayload payload,
        RoleResolution? resolution,
        long createdTime)
    {
        return new TenantGameAdvertisementRewardEntity
        {
            AppId = resolution?.Role.AppId ?? 0,
            Provider = providerCanonical,
            TransactionId = payload.TransactionId,
            AdvertisementUnit = payload.AdvertisementUnit,
            RoleId = resolution?.Role.Id.ToString() ?? string.Empty,
            UserId = resolution?.UserId ?? 0,
            AreaId = resolution?.Role.AreaId ?? 0,
            RewardItem = payload.RewardItem,
            RewardAmount = payload.RewardAmount,
            Status = AdvertisementRewardStatusEnum.Pending,
            IdempotencyKey = idempotencyKey,
#pragma warning disable CS8625 // 实体 FailureReason 声明非空但 DB 列允许 NULL；新建账本时 FailureReason 默认 NULL
            FailureReason = null,
#pragma warning restore CS8625
            IsSandbox = payload.IsSandbox,
            CreatedTime = createdTime,
        };
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return value.Length <= maxLength ? value : value.Substring(0, maxLength);
    }

    private static AdvertisementRewardReceiveResult Fail(int code, string message, string? idempotencyKey)
    {
        return new AdvertisementRewardReceiveResult(false, code, message, idempotencyKey, null);
    }
}
