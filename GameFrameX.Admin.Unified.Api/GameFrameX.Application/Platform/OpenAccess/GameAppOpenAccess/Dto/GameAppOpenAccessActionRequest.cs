namespace GameFrameX.Application.Platform.OpenAccess.GameAppOpenAccess.Dto;

/// <summary>
/// 按 Id 操作应用开放访问凭证输入（吊销 / 启用 / 禁用 / 续期）。
/// </summary>
public class GameAppOpenAccessActionRequest : BaseUpdateRequest
{
    /// <summary>
    /// 操作原因（吊销 / 续期时记录，便于审计排查）。
    /// </summary>
    [StringLength(EntityStringLength.Length512)]
    public string? Reason { get; set; }
}