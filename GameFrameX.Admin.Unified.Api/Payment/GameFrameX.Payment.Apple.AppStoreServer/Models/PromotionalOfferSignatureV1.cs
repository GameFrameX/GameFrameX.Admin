// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The promotional offer signature you generate using an earlier signature version.
/// <para>您使用早期签名版本生成的促销优惠签名。</para>
/// <para>https://developer.apple.com/documentation/retentionmessaging/promotionaloffersignaturev1</para>
/// </summary>
public class PromotionalOfferSignatureV1
{
    /// <summary>
    /// The Base64-encoded cryptographic signature you generate using the offer parameters.
    /// <para>您使用优惠参数生成的 Base64 编码的加密签名。</para>
    /// </summary>
    public string EncodedSignature { get; set; } = null!;

    /// <summary>
    /// The subscription's product identifier.
    /// <para>订阅的产品标识符。</para>
    /// <para>https://developer.apple.com/documentation/retentionmessaging/productid</para>
    /// </summary>
    public string ProductId { get; set; } = null!;

    /// <summary>
    /// A one-time-use UUID antireplay value you generate.
    /// <para>您生成的一次性使用 UUID 反重放值。</para>
    /// </summary>
    public string Nonce { get; set; } = null!;

    /// <summary>
    /// The UNIX time, in milliseconds, when you generate the signature.
    /// <para>您生成签名时的 UNIX 时间（以毫秒为单位）。</para>
    /// </summary>
    public long Timestamp { get; set; }

    /// <summary>
    /// A string that identifies the private key you use to generate the signature.
    /// <para>标识您用于生成签名的私钥的字符串。</para>
    /// </summary>
    public string KeyId { get; set; } = null!;

    /// <summary>
    /// The subscription offer identifier that you set up in App Store Connect.
    /// <para>您在 App Store Connect 中设置的订阅优惠标识符。</para>
    /// </summary>
    public string OfferIdentifier { get; set; } = null!;

    /// <summary>
    /// A UUID that you provide to associate with the transaction if the customer accepts the promotional offer.
    /// <para>如果客户接受促销优惠，您提供的用于关联交易的 UUID。</para>
    /// </summary>
    public string? AppAccountToken { get; set; }
}