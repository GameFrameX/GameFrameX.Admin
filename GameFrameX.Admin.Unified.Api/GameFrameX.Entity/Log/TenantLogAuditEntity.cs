namespace GameFrameX.Entity.Log;

/// <summary>
/// 租户审计日志表（当前租户库 tenant_log_* 归属）
/// </summary>
[SugarTable(null, "租户审计日志表")]
[IncrementTable]
[TenantTable]
public class TenantLogAuditEntity : LogAuditFields
{
}