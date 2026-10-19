using GameFrameX.Payment.Abstract;

namespace GameFrameX.Payment.Apple.Option;

/// <summary>
/// Apple支付配置
/// </summary>
[PaymentChannel("apple", "Apple")]
public class ApplePaymentOptionSetting : IPaymentOptionSetting
{
    /// <summary>
    /// 应用Bundle ID
    /// </summary>
    [PaymentField(Label = "应用 Bundle ID", Required = true)]
    public string ApplePaymentBundleId { get; set; } = string.Empty;

    /// <summary>
    /// 环境 (sandbox/production)
    /// </summary>
    [PaymentField(Label = "环境", Options = new[] { "sandbox", "production", })]
    public string ApplePaymentEnvironment { get; set; } = "sandbox";

    /// <summary>
    /// App Store Connect API密钥ID
    /// </summary>
    [PaymentField(Label = "API 密钥 ID")]
    public string ApplePaymentApiKeyId { get; set; } = string.Empty;

    /// <summary>
    /// 发行商ID
    /// </summary>
    [PaymentField(Label = "发行商 ID")]
    public string ApplePaymentIssuerId { get; set; } = string.Empty;

    /// <summary>
    /// App Store Connect API 密钥 .p8 私钥内容（完整文本，凭证禁止提交入库）。
    /// 缺失时 Server API v2 调用 fail-closed。
    /// </summary>
    [PaymentField(Label = "私钥内容", Sensitive = true, Multiline = true, Help = "App Store Connect API 密钥 .p8 文件完整文本，从后台支付渠道配置中填写，禁止提交凭证文件")]
    public string ApplePaymentPrivateKey { get; set; } = string.Empty;

    /// <summary>
    /// 共享密钥
    /// </summary>
    [PaymentField(Label = "共享密钥", Sensitive = true)]
    public string ApplePaymentSharedSecret { get; set; } = string.Empty;

    /// <summary>
    /// Apple Root CA 证书内容（PEM 格式或 Base64 编码的 DER 格式）
    /// 从 https://www.apple.com/certificateauthority/ 下载，用于验证 App Store Server Notifications V2 的 JWS 签名
    /// </summary>
    [PaymentField(Label = "Root CA 证书", Multiline = true, Help = "PEM 或 Base64(DER)，从 https://www.apple.com/certificateauthority/ 下载")]
    public string ApplePaymentRootCertificate { get; set; } = string.Empty;

    /// <summary>
    /// 是否允许 Server API v2 验证新路径（C271 语义定稿）。
    /// <para>true（默认）：仅上报 transactionId 的 StoreKit 2 验证走 App Store Server API v2
    /// （GetTransactionInfo + JWS 验签）；receiptData 形态不受此开关影响，始终走 verifyReceipt 旧路径。</para>
    /// <para>false：transactionId 形态验证被明确拒绝（不降级旧路径）。</para>
    /// </summary>
    [PaymentField(Label = "使用 Server API v2")]
    public bool ApplePaymentUseServerApiV2 { get; set; } = true;

    /// <summary>
    /// 收据验证超时时间（秒）
    /// </summary>
    [PaymentField(Label = "收据验证超时（秒）")]
    public int ApplePaymentReceiptValidationTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// 是否为沙盒环境
    /// </summary>
    public bool IsSandbox
    {
        get { return ApplePaymentEnvironment.ToLower() == "sandbox"; }
    }

    /// <summary>
    /// 验证URL
    /// </summary>
    public string VerifyReceiptUrl
    {
        get
        {
            return IsSandbox
                       ? "https://sandbox.itunes.apple.com/verifyReceipt"
                       : "https://buy.itunes.apple.com/verifyReceipt";
        }
    }

    /// <summary>
    /// Server API基础URL
    /// </summary>
    public string ServerApiBaseUrl
    {
        get
        {
            return IsSandbox
                       ? "https://api.storekit-sandbox.itunes.apple.com"
                       : "https://api.storekit.itunes.apple.com";
        }
    }
}