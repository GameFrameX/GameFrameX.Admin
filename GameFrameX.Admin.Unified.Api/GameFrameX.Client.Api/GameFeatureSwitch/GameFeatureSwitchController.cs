using GameFrameX.Application.Game.App.FeatureSwitch;
using GameFrameX.Client.Api.GameFeatureSwitch.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Client.Api.Services.PackageVersion;
using GameFrameX.Core.Service;

namespace GameFrameX.Client.Api.GameFeatureSwitch;

/// <summary>
/// 活动/功能开关聚合下发控制器（C347 客户端通道）。
/// <para>客户端运行时拉取通道：按租户 + 应用聚合下发命中开关，支持版本化差异感知（游戏运行中动态生效）。
/// 解析全部委托 <see cref="FeatureSwitchDeliveryResolver"/>（C345 唯一实现）：消费方过滤 client|both、
/// 作用域评分 winner、时间窗（服务端 UTC 过滤）、设备灰度（稳定设备标识取模 + 白名单旁路，缺 DeviceId 仅全量命中）。</para>
/// <para>判定权原则：下发 ≠ 授权。玩法/经济 gate 由 GameServer 业务端点依据开关值与时间窗实时校验，
/// 本端点只负责展示层与配置分发；响应不携带任何机密（AppSecret / 签名密钥等）。</para>
/// </summary>
public class GameFeatureSwitchController : BaseController
{
    private readonly FeatureSwitchDeliveryResolver _deliveryResolver;
    private readonly GameAppPackageResolver _packageResolver;

    /// <summary>
    /// 初始化活动/功能开关聚合下发控制器。
    /// </summary>
    /// <param name="deliveryResolver">开关投放解析器（C345 共享实现）。</param>
    /// <param name="packageResolver">母包解析器，将请求包名展开为母包候选集合。</param>
    public GameFeatureSwitchController(FeatureSwitchDeliveryResolver deliveryResolver, GameAppPackageResolver packageResolver)
    {
        _deliveryResolver = deliveryResolver;
        _packageResolver = packageResolver;
    }

    /// <summary>
    /// 获取活动/功能开关投放结果。
    /// <para>请求可选携带上次 <c>Version</c>：与当前版本相同 → <c>Unchanged=true</c> 且 <c>Switches</c> 为空数组；
    /// 缺失或不同 → 全量返回命中开关。版本号构造保证：候选行集合或任一业务字段变化必变，
    /// 活动时间窗进出自然变版本，消费方短轮询即可感知。</para>
    /// </summary>
    /// <param name="request">开关下发请求（语言/包名/平台/渠道必填）。</param>
    /// <returns>版本化差异协议响应（Version / Unchanged / Switches / ServerTime）。</returns>
    [ProducesResponseType(typeof(FeatureSwitchResponse), 200)]
    [HttpPost]
    public async Task<HttpJsonResultData<FeatureSwitchResponse>> GetInfo([FromBody] FeatureSwitchRequest request)
    {
        // 强制租户 + 应用上下文，缺失即拒绝（缺租户抛 MissingTenantContextException、缺应用 401，C314 语义）；
        // 应用作用域只取上下文注入值，请求体声明不作为授权来源
        var tenantAppContext = HttpContext.RequireTenantAppContext();
        request.AppId = tenantAppContext.AppId;

        var result = await _deliveryResolver.ResolveAsync(new FeatureSwitchDeliveryQuery
        {
            TenantId = tenantAppContext.TenantId,
            AppId = tenantAppContext.AppId,
            Audience = FeatureSwitchAudience.Client,
            Platform = request.Platform,
            PackageCandidates = BuildPackageCandidates(request.PackageName),
            Language = request.Language,
            Channel = request.Channel,
            DeviceId = request.DeviceId,
            NowUtc = DateTime.UtcNow,
        });

        var response = new FeatureSwitchResponse
        {
            Version = result.Version,
            // 同版本短路：Switches 置空数组省流量，Version 仍返回供消费方续传
            Unchanged = !string.IsNullOrEmpty(request.Version) && string.Equals(request.Version, result.Version, StringComparison.Ordinal),
            Switches = new List<FeatureSwitchItemResponse>(),
            ServerTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        };

        if (!response.Unchanged)
        {
            response.Switches = result.Switches
                                        .Select(item => new FeatureSwitchItemResponse
                                        {
                                            SwitchKey = item.SwitchKey,
                                            ValueJson = item.ValueJson,
                                            StartTime = ToUnixSeconds(item.StartTime),
                                            EndTime = ToUnixSeconds(item.EndTime),
                                        })
                                        .ToList();
        }

        return HttpJsonResultData<FeatureSwitchResponse>.Success(response);
    }

    /// <summary>
    /// 构建包名候选集合：请求包名 + 母包名（继承模式子包参与母包回退，与远程配置投放同语义）。
    /// </summary>
    /// <param name="packageName">请求包名。</param>
    /// <returns>包名候选（至少含请求包名自身）。</returns>
    private string[] BuildPackageCandidates(string packageName)
    {
        var values = new List<string> { packageName, };
        var masterName = _packageResolver.Resolve(packageName);
        if (!string.IsNullOrWhiteSpace(masterName) && !string.Equals(masterName, packageName, StringComparison.Ordinal))
        {
            values.Add(masterName);
        }

        return values.Distinct(StringComparer.Ordinal).ToArray();
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
