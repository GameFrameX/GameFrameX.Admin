namespace GameFrameX.Application.Game.Tenant.Penalty.TenantGamePenalty.Dto;

/// <summary>
/// 处罚同步 Online 运行时输入参数（联动命令按 ActionType 服务端推导，不暴露给请求体）。
/// </summary>
public class SyncOnlineRuntimeTenantGamePenaltyRequest
{
    /// <summary>
    /// 处罚事实源行 Id（必须处于生效中）。
    /// </summary>
    [Required(ErrorMessage = "处罚Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "处罚Id必须大于0")]
    public long Id { get; set; }
}
