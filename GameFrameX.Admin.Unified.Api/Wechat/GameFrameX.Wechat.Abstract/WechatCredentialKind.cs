namespace GameFrameX.Wechat.Abstract;

/// <summary>
/// 微信租户配置凭据类型。
/// <para>公众号方法只能使用公众号凭证对（<c>WechatAppId/Secret</c>），小程序方法只能使用小程序凭证对（<c>WxOpenAppId/Secret</c>），禁止跨类型补位。</para>
/// </summary>
public enum WechatCredentialKind
{
    /// <summary>
    /// 微信公众号。
    /// </summary>
    Wechat = 0,

    /// <summary>
    /// 微信小程序。
    /// </summary>
    WxOpen = 1,
}