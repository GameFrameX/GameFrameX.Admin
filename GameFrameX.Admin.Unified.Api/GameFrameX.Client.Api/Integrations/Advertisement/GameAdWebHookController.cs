using GameFrameX.Client.Api.Services.PublicCallback;
using GameFrameX.Advertisement.Abstract;
using GameFrameX.Advertisement.AdMob;
using GameFrameX.Advertisement.Pangle;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Client.Api.Integrations.Advertisement.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Client.Api.Services.ClientApiTenantOptions;
using GameFrameX.Core.Service;

namespace GameFrameX.Client.Api.Integrations.Advertisement;

/// <summary>
/// 广告服务器回调。
/// <para>广告网络回调不携带 <c>X-Api-Key</c>，必须先由公开回调 URL 路由参数建立租户上下文，再通过 <see cref="IClientApiTenantOptionsProvider"/> 读取该租户的验签材料。</para>
/// <para>无法解析租户或租户未配置验签材料时安全拒绝，禁止逐租户扫描或回落到进程级 key。</para>
/// <para>TopOn / AdMob / UnityAds 的验签 + 幂等账本 + 发奖链路由 <see cref="GameAdvertisementRewardService"/> 统一处理（C114 / GFX-110）；Pangle 验签由 <see cref="PangleAdvertisementCallbackHandler"/> 处理（仅验签，不进发奖链路）。</para>
/// </summary>
public class GameAdWebHookController : BaseController<TenantGameRoleAdvertisementEntity>
{
    private readonly IClientApiTenantOptionsProvider _optionsProvider;
    private readonly Repository<TenantGameRoleEntity> _roleRepository;
    private readonly GameAdvertisementRewardService _advertisementRewardService;
    private readonly IAdvertisementCallbackFactory _advertisementCallbackFactory;

    /// <summary>
    /// </summary>
    public GameAdWebHookController(SqlSugarRepository<TenantGameRoleAdvertisementEntity> service,
        Repository<TenantGameRoleEntity> roleRepository,
        IClientApiTenantOptionsProvider optionsProvider,
        GameAdvertisementRewardService advertisementRewardService,
        IAdvertisementCallbackFactory advertisementCallbackFactory) : base(service)
    {
        _optionsProvider = optionsProvider;
        _roleRepository = roleRepository;
        _advertisementRewardService = advertisementRewardService;
        _advertisementCallbackFactory = advertisementCallbackFactory;
    }

    /// <summary>
    /// TopOn通知。
    /// <para>验签 + 幂等账本 + 发奖由 <see cref="GameAdvertisementRewardService.ReceiveTopOnAsync"/> 处理；本端点仅保留 C59 公开回调路由与 HTTP 形状。</para>
    /// </summary>
    [PublicCallbackEndpoint("topon", "ad")]
    [HttpGet("{tenantId:long}/{appId:long}/{packageId:long?}")]
    public async Task<JsonResult> TopOnNotify()
    {
        var result = await _advertisementRewardService.ReceiveTopOnAsync();
        ApplyResultStatus(result);
        return Json(result.ToAnonymous());
    }

    /// <summary>
    /// PangleGlobal通知。
    /// <para>租户身份只来自公开回调 URL 路由上下文；验签走 <see cref="PangleAdvertisementCallbackHandler"/>（仅验签，不进发奖链路），签名通过后才允许查询广告发放记录。</para>
    /// </summary>
    [PublicCallbackEndpoint("pangle", "ad")]
    [HttpGet("{tenantId:long}/{appId:long}/{packageId:long?}")]
    public async Task<JsonResult> PangleGlobalNotify([FromQuery] AdvertisementPangleGlobalGameEntityRequest input)
    {
        var advertisementOptions = await _optionsProvider.GetAdvertisementCallbackOptionsAsync();
        if (advertisementOptions == null || string.IsNullOrWhiteSpace(advertisementOptions.PangleSecurityKey))
        {
            LogHelper.Warning($"Pangle 回调未配置验签 key，TransId={input.TransId}，安全拒绝");
            return Json(new { code = 0, isValid = false, msg = "未配置广告回调验签", });
        }

        var handler = _advertisementCallbackFactory.GetHandler("Pangle");
        var verification = handler == null ? null : handler.Verify(advertisementOptions, HttpContext.Request.Query);
        if (verification == null || !verification.Payload.IsValid)
        {
            return Json(new { code = 0, isValid = false, msg = "签名验证失败", });
        }

        var entity = await PRepository.GetSingleAsync(m => m.TransId == input.TransId);
        if (entity != null)
        {
            return Json(new { code = 0, isValid = false, msg = "流水ID已存在", });
        }

        return Json(new { code = 0, isValid = true, });
    }

    /// <summary>
    /// Google AdMob奖励视频广告回调通知。
    /// <para>验签 + 幂等账本 + 发奖由 <see cref="GameAdvertisementRewardService.ReceiveAdMobAsync"/> 处理。</para>
    /// <para>发放成功后保留对 <see cref="TenantGameRoleAdvertisementEntity"/> 的写入（C75/C91 应用作用域事实，范围外不动）。</para>
    /// </summary>
    [PublicCallbackEndpoint("admob", "ad")]
    [HttpGet("{tenantId:long}/{appId:long}/{packageId:long?}")]
    public async Task<JsonResult> AdMobNotify([FromQuery] AdMobCallbackRequest input)
    {
        var result = await _advertisementRewardService.ReceiveAdMobAsync();
        if (!result.IsValid)
        {
            ApplyResultStatus(result);
            return Json(result.ToAnonymous());
        }

        // 发放成功后补写 C75/C91 角色广告事实（应用作用域 AppId 取自已验签并通过 service 发放的角色实体）
        await WriteAdMobRoleAdvertisementFactAsync(input);

        return Json(result.ToAnonymous());
    }

    /// <summary>
    /// Unity Ads 奖励回调通知。
    /// <para>验签 + 幂等账本 + 发奖由 <see cref="GameAdvertisementRewardService.ReceiveUnityAdsAsync"/> 处理（C114 新增端点）。</para>
    /// </summary>
    [PublicCallbackEndpoint("unityads", "ad")]
    [HttpGet("{tenantId:long}/{appId:long}/{packageId:long?}")]
    public async Task<JsonResult> UnityAdsNotify([FromQuery] AdvertisementUnityAdsCallbackRequest input)
    {
        var result = await _advertisementRewardService.ReceiveUnityAdsAsync();
        ApplyResultStatus(result);
        return Json(result.ToAnonymous());
    }

    /// <summary>
    /// 发放成功后写入 <see cref="TenantGameRoleAdvertisementEntity"/> 角色广告事实（C75/C91 应用作用域，保留不改）。
    /// </summary>
    private async Task WriteAdMobRoleAdvertisementFactAsync(AdMobCallbackRequest input)
    {
        try
        {
            var customData = JsonHelper.Deserialize<AdMobCallbackCustomDataRequest>(input.CustomData);
            if (customData == null
                || !long.TryParse(customData.PlayerId, out var playerId)
                || !long.TryParse(customData.UserId, out var userId))
            {
                return;
            }

            var gameRoleEntity = await _roleRepository.GetByIdAsync(playerId);
            if (gameRoleEntity == null)
            {
                return;
            }

            // 幂等：同 TransId 已写过事实则跳过（与 service 账本幂等协同，避免双写）
            var existingRecord = await PRepository.GetSingleAsync(m => m.TransId == input.TransactionId);
            if (existingRecord != null)
            {
                return;
            }

            var gameRoleAdvertisementEntity = new TenantGameRoleAdvertisementEntity
            {
                TransId = input.TransactionId,
                UserId = userId,
                RoleId = playerId.ToString(),
                AreaId = gameRoleEntity.AreaId,
                // 事实数据应用作用域：AppId 来自已通过签名验证并按 playerId 查到的角色实体（C75 落地），不接受请求体或路由参数覆盖。
                AppId = AppScopeRules.EnsureFactAppId(gameRoleEntity.AppId, "游戏角色广告"),
                IsSendSuccess = true,
                CreatedTime = TimerHelper.UnixTimeSeconds(),
            };
            await PRepository.InsertAsync(gameRoleAdvertisementEntity);
        }
        catch (Exception ex)
        {
            LogHelper.Error($"AdMob TenantGameRoleAdvertisementEntity 写入异常: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 按结果 Code 设置响应状态码（验签失败 601）。
    /// </summary>
    private void ApplyResultStatus(AdvertisementRewardReceiveResult result)
    {
        if (result.Code == 601)
        {
            Response.StatusCode = 601;
        }
    }

    /// <summary>
    /// </summary>
    public sealed class PlayerSendAdvertisementItemRequest
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// 是否测试
        /// </summary>
        public bool IsTest { get; set; }

        /// <summary>
        /// 广告ID
        /// </summary>
        public long AdPackageId { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class PlayerSendAdvertisementItemResponse
    {
    }
}