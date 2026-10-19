// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json.Serialization;

namespace GameFrameX.Payment.Xsolla.Models;

/// <summary>
/// Xsolla交易信息
/// </summary>
public class XsollaTransaction
{
    /// <summary>
    /// 交易ID
    /// </summary>
    [JsonPropertyName("transaction_id")]
    public long TransactionId { get; set; }

    /// <summary>
    /// 外部交易ID
    /// </summary>
    [JsonPropertyName("external_id")]
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>
    /// 用户信息
    /// </summary>
    [JsonPropertyName("user")]
    public XsollaTransactionUser User { get; set; } = new();

    /// <summary>
    /// 交易状态
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 交易金额信息
    /// </summary>
    [JsonPropertyName("amount")]
    public XsollaTransactionAmount Amount { get; set; } = new();

    /// <summary>
    /// 购买信息
    /// </summary>
    [JsonPropertyName("purchase")]
    public XsollaTransactionPurchase Purchase { get; set; } = new();

    /// <summary>
    /// 创建时间
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 是否为沙盒环境
    /// </summary>
    [JsonPropertyName("is_sandbox")]
    public bool IsSandbox { get; set; }
}