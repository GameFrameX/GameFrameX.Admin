// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Abstract.Models;

/// <summary>
/// 渠道侧退款状态归一枚举（C310 / GFX-718）：各渠道原始状态（Xsolla status / 支付宝 refund_status / Apple 退款历史 / Google voided purchases）统一映射到本枚举。
/// </summary>
public enum RefundChannelStatusEnum
{
    /// <summary>
    /// 无退款记录（交易正常，未发生退款）。
    /// </summary>
    None = 0,

    /// <summary>
    /// 退款处理中（如 Xsolla awaitingRefund）。
    /// </summary>
    Pending = 1,

    /// <summary>
    /// 已全额退款。
    /// </summary>
    Refunded = 2,

    /// <summary>
    /// 已部分退款（如 Xsolla partiallyRefunded）。
    /// </summary>
    PartiallyRefunded = 3,

    /// <summary>
    /// 退款失败 / 被渠道拒绝。
    /// </summary>
    Failed = 4,

    /// <summary>
    /// 渠道未提供可归一的状态（查询口径受限时如实报告，不假装成功）。
    /// </summary>
    Unknown = 5,
}
