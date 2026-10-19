namespace GameFrameX.Application.Game.Tenant.OpenAccess.TenantGameAppOpenAccess.Dto;

/// <summary>
/// 按 Id 操作应用开放访问凭证输入（吊销 / 启用 / 禁用 / 续期）。
/// </summary>
public class TenantGameAppOpenAccessActionRequest : BaseUpdateRequest
{
    /// <summary>
    /// 当前应用 Id（校验凭证归属当前 App）。
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "请选择应用")]
    public long AppId { get; set; }

    /// <summary>
    /// 操作原因（吊销 / 续期时记录，便于审计排查）。
    /// </summary>
    [StringLength(EntityStringLength.Length512)]
    public string? Reason { get; set; }
}