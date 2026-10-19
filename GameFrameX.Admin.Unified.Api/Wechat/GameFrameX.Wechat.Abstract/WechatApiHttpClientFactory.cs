using SKIT.FlurlHttpClient.Wechat.Api;
using GameFrameX.Core.Exceptions;

namespace GameFrameX.Wechat.Abstract;

/// <summary>
/// 微信 API 客户端工厂（无状态，凭证由调用方传入）。
/// <para>不再持有静态 <c>WechatOptions</c>；每次创建客户端需传入当前租户的有效凭证，避免单例 ↔ 租户上下文生命周期矛盾。</para>
/// </summary>
public partial class WechatApiHttpClientFactory
{
    /// <summary>
    /// 创建微信公众号客户端。
    /// </summary>
    /// <param name="appId">公众号 AppId（来自当前租户的 <c>TenantWechatConfigEntity.WechatAppId</c>）。</param>
    /// <param name="appSecret">公众号 AppSecret（来自当前租户配置）。</param>
    public WechatApiClient CreateWechatClient(string appId, string appSecret)
    {
        if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret))
        {
            throw BusinessError.Oh("微信公众号配置错误");
        }

        var wechatApiClient = new WechatApiClient(new WechatApiClientOptions
        {
            AppId = appId,
            AppSecret = appSecret,
        });

        wechatApiClient.Configure(settings => { settings.JsonSerializer = new SystemTextJsonSerializer(); });

        return wechatApiClient;
    }

    /// <summary>
    /// 创建微信小程序客户端。
    /// </summary>
    /// <param name="appId">小程序 AppId（来自当前租户的 <c>TenantWechatConfigEntity.WxOpenAppId</c>）。</param>
    /// <param name="appSecret">小程序 AppSecret（来自当前租户配置）。</param>
    public WechatApiClient CreateWxOpenClient(string appId, string appSecret)
    {
        if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret))
        {
            throw BusinessError.Oh("微信小程序配置错误");
        }

        var wechatApiClient = new WechatApiClient(new WechatApiClientOptions
        {
            AppId = appId,
            AppSecret = appSecret,
        });

        wechatApiClient.Configure(settings => { settings.JsonSerializer = new SystemTextJsonSerializer(); });

        return wechatApiClient;
    }
}