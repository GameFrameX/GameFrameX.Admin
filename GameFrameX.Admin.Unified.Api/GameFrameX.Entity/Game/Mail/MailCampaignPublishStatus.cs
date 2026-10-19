// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

namespace GameFrameX.Entity.Game.Mail;

/// <summary>
/// 运营邮件 Campaign 的 Admin 侧生命周期状态。
/// <para>镜像 Server <c>MailCampaignStatus</c>（Published/Revoked）并扩展 Admin 草稿态：<c>Draft</c>（未发布，可编辑）→ <c>Published</c>（Server 已接受，主体不可变 B1）→ <c>Revoked</c>（撤回不可逆 B3）；<c>Failed</c> 为 Server 拒绝后的中间态，仍停留草稿可重试发布。</para>
/// </summary>
public enum MailCampaignPublishStatus
{
    /// <summary>
    /// 草稿：Admin 编辑中，未调用 Server 发布；可自由增删改与发布重试。
    /// </summary>
    Draft = 0,

    /// <summary>
    /// 已发布：Server 已接受 Campaign 并分配 <c>ServerCampaignId</c>，主体字段不可修改、不可删除（B1）；仅可撤回。
    /// </summary>
    Published = 1,

    /// <summary>
    /// 已撤回：撤回不可逆（B3 已发放资产不回滚）；新玩家不再命中，未领取附件按 <c>ExpireAttachmentPolicy</c> 处理（B4）。
    /// </summary>
    Revoked = 2,

    /// <summary>
    /// 发布失败：Server 拒绝（参数非法 / 已存在等），停留草稿态可修正后重试。<c>LastServerCode</c>/<c>LastServerMessage</c> 记录原因。
    /// </summary>
    Failed = 3,
}