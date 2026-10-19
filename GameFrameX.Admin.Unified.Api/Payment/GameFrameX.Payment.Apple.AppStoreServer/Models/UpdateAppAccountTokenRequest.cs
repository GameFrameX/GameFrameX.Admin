// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json.Serialization;

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The request body that contains an app account token value.
/// <para>包含应用帐户令牌值的请求体。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/updateappaccounttokenrequest</para>
/// </summary>
public class UpdateAppAccountTokenRequest
{
    /// <summary>
    /// The UUID that an app optionally generates to map a customer's in-app purchase with its resulting App Store transaction.
    /// <para>应用可选生成的 UUID，用于将客户的应用内购买与其产生的 App Store 交易映射。</para>
    /// <para>https://developer.apple.com/documentation/appstoreserverapi/appaccounttoken</para>
    /// </summary>
    [JsonPropertyName("appAccountToken")]
    public string AppAccountToken { get; set; } = null!;
}