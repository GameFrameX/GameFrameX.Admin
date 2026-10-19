// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.Models.Verification;

/// <summary>
/// Apple验证交易信息
/// </summary>
public class AppleVerificationTransactionInfo
{
    public string TransactionId { get; set; } = string.Empty;
    public string OriginalTransactionId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string PurchaseDate { get; set; } = string.Empty;
    public string PurchaseDateMs { get; set; } = string.Empty;
    public string OriginalPurchaseDate { get; set; } = string.Empty;
    public string OriginalPurchaseDateMs { get; set; } = string.Empty;
    public string? ExpiresDate { get; set; }
    public string? ExpiresDateMs { get; set; }
    public string Quantity { get; set; } = "1";
    public string IsTrialPeriod { get; set; } = "false";
    public string IsInIntroOfferPeriod { get; set; } = "false";
    public string? CancellationDate { get; set; }
    public string? CancellationReason { get; set; }
    public string? WebOrderLineItemId { get; set; }
    public string? PromotionalOfferId { get; set; }
    public string? SubscriptionGroupIdentifier { get; set; }
}