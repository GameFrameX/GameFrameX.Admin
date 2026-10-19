// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

using GameFrameX.Entity.Game.Mail;

namespace GameFrameX.Application.Game.ServerManager.Mail;

/// <summary>
/// 查询 Campaign 请求体。所有过滤条件为 AND 关系，空 / 0 表示不限。逐字对齐 Server。
/// </summary>
public sealed class QueryMailCampaignRequest
{
    /// <summary>Campaign ID。大于 0 时精确查询单条，忽略其余过滤条件。</summary>
    public long CampaignId { get; set; }

    /// <summary>状态过滤；null 表示不限。</summary>
    public MailCampaignPublishStatus? Status { get; set; }

    /// <summary>类型过滤；null 表示不限。</summary>
    public MailCampaignMailType? MailType { get; set; }

    /// <summary>服务器 ID 过滤；≤ 0 表示不限。</summary>
    public int ServerId { get; set; }

    /// <summary>渠道 ID 过滤；≤ 0 表示不限。</summary>
    public int ChannelId { get; set; }

    /// <summary>最低等级过滤；≤ 0 表示不限。</summary>
    public int MinLevel { get; set; }

    /// <summary>创建时间下限（Unix 秒，UTC）；≤ 0 表示不限。</summary>
    public long CreatedFrom { get; set; }

    /// <summary>创建时间上限（Unix 秒，UTC）；≤ 0 表示不限。</summary>
    public long CreatedTo { get; set; }

    /// <summary>返回条数上限；≤ 0 表示不限。</summary>
    public int Limit { get; set; }
}