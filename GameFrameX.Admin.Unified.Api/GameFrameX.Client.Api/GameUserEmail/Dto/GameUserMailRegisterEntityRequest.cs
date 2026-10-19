using GameFrameX.Core.BaseInputOutput;

namespace GameFrameX.Client.Api.GameUserEmail.Dto;

/// <summary>
/// 游戏用户邮件注册输入参数
/// </summary>
public class GameUserMailRegisterEntityRequest : BaseAddRequest
{
    /// <summary>
    /// 邮件地址
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// 语言
    /// </summary>
    public string Language { get; set; } = null!;
}