// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

namespace GameFrameX.Entity.Game.Mail;

/// <summary>
/// 运营邮件 Campaign 附件槽位。字段名逐字对齐 Server <c>MailAttachmentState</c>（跨仓库契约）。
/// <para>玩家领取时按 <c>SlotId</c> 逐项走统一奖励发放接口。</para>
/// </summary>
public sealed class MailCampaignAttachment
{
    /// <summary>
    /// 附件槽位 ID，Campaign 内唯一。用于领取幂等与展示排序。
    /// </summary>
    public int SlotId { get; set; }

    /// <summary>
    /// 奖励类型（普通道具 / 隐藏道具 / 月卡 / VIP 点 等）。一期仅 <c>NormalItem</c> 可发放，其它由 Server 返回 <c>Unsupported</c>。
    /// </summary>
    public int RewardType { get; set; }

    /// <summary>
    /// 物品 ID（普通道具对应 <c>TbItemConfig</c> 配置 ID）。
    /// </summary>
    public int ItemId { get; set; }

    /// <summary>
    /// 数量。必须大于 0，否则 Server 发布校验返回 <c>InvalidReward</c>。
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// 展示用图标 ID（可选，缺省时由客户端按 ItemId 兜底）。
    /// </summary>
    public int IconId { get; set; }
}