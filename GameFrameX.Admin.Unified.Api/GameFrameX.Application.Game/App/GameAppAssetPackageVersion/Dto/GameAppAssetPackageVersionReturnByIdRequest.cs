namespace GameFrameX.Application.Game.App.GameAppAssetPackageVersion.Dto;

/// <summary>
/// 资源包版本更新输入参数
/// </summary>
public class GameAppAssetPackageVersionReturnByIdRequest : BaseUpdateRequest
{
    /// <summary>
    /// 记录id
    /// </summary>
    [Required(ErrorMessage = "资源包id不能为空")]
    public long ResourceVersionHistoryId { get; set; }
}