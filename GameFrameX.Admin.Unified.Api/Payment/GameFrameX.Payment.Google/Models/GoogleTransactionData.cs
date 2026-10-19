// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using GameFrameX.Payment.Abstract.Interface;

namespace GameFrameX.Payment.Google.Models;

/// <summary>
/// Google Play 交易数据，实现 ITransactionData 接口
/// </summary>
public class GoogleTransactionData : ITransactionData
{
    /// <summary>
    /// 购买令牌（必需）
    /// </summary>
    public string PurchaseToken { get; set; } = string.Empty;

    /// <summary>
    /// 产品ID/SKU（必需）
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// 应用包名（必需）
    /// </summary>
    public string PackageName { get; set; } = string.Empty;

    /// <summary>
    /// 订单ID（可选，用于验证）
    /// </summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>
    /// 用户ID（可选，用于验证）
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 混淆的外部账号ID（可选，用于校验 Google Play 返回的订单归属）
    /// </summary>
    public string ObfuscatedExternalAccountId { get; set; } = string.Empty;

    /// <summary>
    /// 混淆的外部配置文件ID（可选，用于校验 Google Play 返回的用户归属）
    /// </summary>
    public string ObfuscatedExternalProfileId { get; set; } = string.Empty;

    /// <summary>
    /// 期望的购买状态（可选，默认为0-已购买）
    /// </summary>
    public int ExpectedPurchaseState { get; set; } = 0;

    /// <summary>
    /// 期望的消耗状态（可选）
    /// </summary>
    public int? ExpectedConsumptionState { get; set; }

    /// <summary>
    /// 期望的确认状态（可选）
    /// </summary>
    public int? ExpectedAcknowledgementState { get; set; }

    /// <summary>
    /// 开发者载荷（可选，用于验证）
    /// </summary>
    public string DeveloperPayload { get; set; } = string.Empty;

    /// <summary>
    /// 是否为沙盒环境
    /// </summary>
    public bool IsSandbox { get; set; } = true;
}