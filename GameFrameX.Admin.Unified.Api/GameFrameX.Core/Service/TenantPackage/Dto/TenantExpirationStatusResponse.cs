namespace GameFrameX.Core.Service.TenantPackage.Dto;

/// <summary>
/// 租户到期状态结果（前端到期横幅用）
/// </summary>
public class TenantExpirationStatusResponse
{
    /// <summary>
    /// 到期状态小写字符串：active（正常）/ ingraceperiod（宽限期，已到期仍在宽限期内）/ expired（已过期）
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// 距到期剩余天数；租户未配置到期时间时为 null
    /// </summary>
    public int? DaysLeft { get; set; }
}