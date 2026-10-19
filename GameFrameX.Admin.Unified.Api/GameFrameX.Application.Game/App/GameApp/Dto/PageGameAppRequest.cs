namespace GameFrameX.Application.Game.App.GameApp.Dto;

/// <summary>
/// 游戏应用分页查询输入参数
/// </summary>
public class PageGameAppRequest : BasePageRequest
{
    /// <summary>
    /// 应用名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsActive { get; set; }
}