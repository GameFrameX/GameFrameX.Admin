namespace GameFrameX.Application.Game.App.GameAppPackage.Dto;

/// <summary>
/// 游戏应用分包启停输入参数
/// </summary>
public class SetGameAppPackageStatusRequest : BaseIdRequest
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
}