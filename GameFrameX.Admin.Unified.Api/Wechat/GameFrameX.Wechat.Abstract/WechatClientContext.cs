using GameFrameX.Entity.Tenant.ClientApiConfig;
using SKIT.FlurlHttpClient.Wechat.Api;

namespace GameFrameX.Wechat.Abstract;

/// <summary>
/// 微信客户端与租户配置解析结果。
/// </summary>
public sealed class WechatClientContext
{
    /// <summary>微信 API 客户端。</summary>
    public WechatApiClient Client { get; }

    /// <summary>命中的租户微信配置。</summary>
    public TenantWechatConfigEntity Config { get; }

    public WechatClientContext(WechatApiClient client, TenantWechatConfigEntity config)
    {
        Client = client;
        Config = config;
    }
}