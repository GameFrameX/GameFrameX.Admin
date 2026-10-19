using GameFrameX.Application.Game.App.FeatureSwitch;
using GameFrameX.Core.Middleware;
using GameFrameX.Core.Service;
using GameFrameX.Hub.Api.Const;
using GameFrameX.Hub.Api.Dto.FeatureSwitchSync;
using GameFrameX.Hub.Api.Middleware;
using Microsoft.AspNetCore.Authorization;

namespace GameFrameX.Hub.Api.Controllers;

/// <summary>
/// 功能开关服务器同步控制器（C347 Hub 通道）。
/// <para>游戏服务器运行时同步通道：机器对机器调用，由 <see cref="TenantGameAppOpenAccessAuthMiddleware"/>
/// 鉴权（C267 HMAC v2 单头 + nonce 去重 + 时间窗，scope = <see cref="OpenAccessScopeConst.FeatureSwitchSync"/>），
/// 非 JWT，故标记 <c>[AllowAnonymous]</c>（C236 起 JWT 默认鉴权由各控制器显式承担，本控制器不参与，
/// 与 <c>PackageBuildReportController</c> 同模式）。</para>
/// <para>应用作用域只取凭证绑定（<c>HttpContext.Items[TenantGameAppOpenAccessAuthMiddleware.AppIdItemsKey]</c>），
/// 请求侧不可声明 AppId。服务器通道无 platform/package/language/channel 上下文，四维传 null →
/// 仅命中 Default 哨兵行；服务器同步无设备身份，不做设备灰度（灰度是设备维度，语义如此设计）。</para>
/// <para>判定权原则：下发 ≠ 授权。玩法/经济 gate 由 GameServer 业务端点依据开关值与时间窗实时校验，
/// 本端点只负责配置分发；响应不携带任何机密（AppSecret / 签名密钥等）。</para>
/// </summary>
[AllowAnonymous]
public class FeatureSwitchSyncController : BaseController
{
    private readonly FeatureSwitchDeliveryResolver _deliveryResolver;

    /// <summary>
    /// 初始化功能开关服务器同步控制器。
    /// </summary>
    /// <param name="deliveryResolver">开关投放解析器（C345 共享实现，两通道唯一解析入口）。</param>
    public FeatureSwitchSyncController(FeatureSwitchDeliveryResolver deliveryResolver)
    {
        _deliveryResolver = deliveryResolver;
    }

    /// <summary>
    /// 同步功能开关投放结果。
    /// <para>请求可选携带上次 <c>Version</c>：与当前版本相同 → <c>Unchanged=true</c> 且 <c>Switches</c> 为空数组；
    /// 缺失或不同 → 全量返回命中开关（server|both）。版本号构造保证：候选行集合或任一业务字段变化必变，
    /// 活动时间窗进出自然变版本，消费方短轮询即可感知。</para>
    /// </summary>
    /// <param name="request">同步请求（仅可选 Version）。</param>
    /// <returns>版本化差异协议响应（Version / Unchanged / Switches / ServerTime）。</returns>
    [ProducesResponseType(typeof(FeatureSwitchSyncResponse), 200)]
    [HttpPost]
    public async Task<HttpJsonResultData<FeatureSwitchSyncResponse>> Sync([FromBody] FeatureSwitchSyncRequest request)
    {
        // 租户由 HubTenantMiddleware 写入统一键；应用只取凭证绑定注入值，请求侧无可伪造面
        var tenantId = HttpContext.Items[TenantContextAccessor.TenantIdKey] as long?;
        var credentialAppId = HttpContext.Items[TenantGameAppOpenAccessAuthMiddleware.AppIdItemsKey] as long?;
        if (!tenantId.HasValue || tenantId.Value <= 0 || !credentialAppId.HasValue || credentialAppId.Value <= 0)
        {
            return HttpJsonResultData<FeatureSwitchSyncResponse>.Fail("缺少凭证租户或应用上下文");
        }

        var result = await _deliveryResolver.ResolveAsync(new FeatureSwitchDeliveryQuery
        {
            TenantId = tenantId.Value,
            AppId = credentialAppId.Value,
            Audience = FeatureSwitchAudience.Server,
            Platform = null,
            PackageCandidates = null,
            Language = null,
            Channel = null,
            DeviceId = null,
            NowUtc = DateTime.UtcNow,
        });

        var response = new FeatureSwitchSyncResponse
        {
            Version = result.Version,
            // 同版本短路：Switches 置空数组省流量，Version 仍返回供消费方续传
            Unchanged = !string.IsNullOrEmpty(request.Version) && string.Equals(request.Version, result.Version, StringComparison.Ordinal),
            Switches = new List<FeatureSwitchSyncItemResponse>(),
            ServerTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        };

        if (!response.Unchanged)
        {
            response.Switches = result.Switches
                                        .Select(item => new FeatureSwitchSyncItemResponse
                                        {
                                            SwitchKey = item.SwitchKey,
                                            ValueJson = item.ValueJson,
                                            StartTime = ToUnixSeconds(item.StartTime),
                                            EndTime = ToUnixSeconds(item.EndTime),
                                        })
                                        .ToList();
        }

        return HttpJsonResultData<FeatureSwitchSyncResponse>.Success(response);
    }

    /// <summary>
    /// UTC 时间转 Unix 秒（时间窗在实体层即 Unix 秒语义）。
    /// </summary>
    /// <param name="utcTime">UTC 时间；null 表示不限。</param>
    /// <returns>Unix 秒；null 表示不限。</returns>
    private static long? ToUnixSeconds(DateTime? utcTime)
    {
        return utcTime.HasValue ? new DateTimeOffset(utcTime.Value).ToUnixTimeSeconds() : null;
    }
}
