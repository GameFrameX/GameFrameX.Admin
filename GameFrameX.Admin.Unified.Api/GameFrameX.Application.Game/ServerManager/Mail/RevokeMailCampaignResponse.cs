// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

using GameFrameX.Entity.Game.Mail;

namespace GameFrameX.Application.Game.ServerManager.Mail;

/// <summary>
/// 撤回 Campaign 响应体。逐字对齐 Server。
/// </summary>
public sealed class RevokeMailCampaignResponse
{
    /// <summary>业务码。</summary>
    public MailCampaignServerCode Code { get; set; }

    /// <summary>Campaign ID。</summary>
    public long CampaignId { get; set; }

    /// <summary>撤回时间（Unix 秒，UTC）。撤回失败时为 0。</summary>
    public long RevokedTime { get; set; }

    /// <summary>提示信息。</summary>
    public string Message { get; set; } = null!;
}