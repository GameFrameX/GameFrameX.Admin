using GameFrameX.Client.Api.GameWebShop.Dto;
using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Client.Api.GameUserEmail.Dto;

/// <summary>
/// 游戏用户邮件注册输入参数
/// </summary>
public class GameWebShpBuySendMailRequest : BaseAddRequest
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
    public List<PropertyItemResponse> Items { get; set; } = new();

    /// <summary>
    /// 是否翻译
    /// </summary>
    public bool IsTranslate { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public long ExpirationTime { get; set; }

    /// <summary>
    /// 邮件类型
    /// </summary>
    public GameMailType MailType { get; set; }
}