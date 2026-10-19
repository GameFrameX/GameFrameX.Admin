// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Mail.GameMail.Dto;

/// <summary>
/// 推送邮件请求参数
/// </summary>
public sealed class PostMailRequest
{
    /// <summary>
    /// 角色ID列表
    /// </summary>
    public List<long> Roles { get; set; } = null!;

    /// <summary>
    /// 是否全员发送
    /// </summary>
    public bool PostType { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 副标题
    /// </summary>
    public string SubTitle { get; set; } = null!;

    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// 道具列表
    /// </summary>
    public List<GamePropItem> Items { get; set; } = new();

    /// <summary>
    /// 过期时间
    /// </summary>
    public long ExpirationTime { get; set; }

    /// <summary>
    /// 邮件类型
    /// </summary>
    public GameMailType MailType { get; set; }

    /// <summary>
    /// 邮件类型
    /// </summary>
    public bool IsTranslate { get; set; }
}