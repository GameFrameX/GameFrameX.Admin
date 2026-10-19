// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

namespace GameFrameX.Entity.Game.Mail;

/// <summary>
/// 运营邮件 Campaign 附件过期处置策略（Server B4）。值域逐字对齐 Server <c>ExpireAttachmentPolicy</c>（跨仓库契约）。
/// <para>撤回或自然过期后未领取附件的处理方式。</para>
/// </summary>
public enum MailCampaignExpirePolicy
{
    /// <summary>
    /// 默认作废：未领取附件直接丢弃，不进入玩家任何资产账户，日志留痕用于审计。
    /// </summary>
    DiscardUnclaimed = 0,

    /// <summary>
    /// 保留未领取：附件保留在邮件中，玩家仍可领取，仅停止向新玩家下发（用于「停发但已命中可继续领」场景）。
    /// </summary>
    KeepUnclaimed = 1,

    /// <summary>
    /// 自动入袋：未领取附件通过统一奖励发放接口自动发放到玩家账户（需谨慎，可能突破发放窗口限制）。
    /// </summary>
    AutoClaim = 2,
}