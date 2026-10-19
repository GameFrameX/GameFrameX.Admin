// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The response body the App Store server sends to your notification endpoint.
/// <para>App Store 服务器发送到您的通知端点的响应体。</para>
/// <para>https://developer.apple.com/documentation/appstoreservernotifications/responsebodyv2</para>
/// </summary>
public class ResponseBodyV2
{
    /// <summary>
    /// The payload in JSON Web Signature (JWS) format, signed by the App Store.
    /// <para>由 App Store 签名的 JSON Web 签名 (JWS) 格式的负载。</para>
    /// <para>https://developer.apple.com/documentation/appstoreservernotifications/signedpayload</para>
    /// </summary>
    public string? SignedPayload { get; set; }
}