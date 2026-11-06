namespace GameFrameX.Application.ServerCenter.GameArea.Dto;

/// <summary>
/// 游戏区服增加输入参数
/// </summary>
public class AddGameAreaEntityRequest : GameAreaBaseRequest
{
    /// <summary>
    /// 是否启用
    /// </summary>
    [Required(ErrorMessage = "是否启用不能为空")]
    public bool IsEnabled { get; set; }

    // Id 和所有其他公共字段已从 GameAreaBaseRequest 继承
}