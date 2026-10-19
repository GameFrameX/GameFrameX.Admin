// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

using GameFrameX.Entity.Game.Mail;

namespace GameFrameX.Application.Game.ServerManager.Mail;

/// <summary>
/// Server Campaign 状态快照（查询返回的列表项）。仅承载 Admin 关心的字段。
/// </summary>
public sealed class ServerMailCampaignState
{
    /// <summary>Campaign 唯一 ID。</summary>
    public long CampaignId { get; set; }

    /// <summary>Server 生命周期状态（Published / Revoked）。</summary>
    public MailCampaignPublishStatus Status { get; set; }

    /// <summary>发布版本号。</summary>
    public int PublishVersion { get; set; }

    /// <summary>发布时间（Unix 秒，UTC）。</summary>
    public long PublishedTime { get; set; }

    /// <summary>撤回时间（Unix 秒，UTC）。未撤回时为 0。</summary>
    public long RevokedTime { get; set; }
}