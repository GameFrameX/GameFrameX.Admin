namespace GameFrameX.Entity.Log;

/// <summary>
/// 管理员审计日志表（主库 log_* 归属）
/// </summary>
[SugarTable(null, "管理员审计日志表")]
[IncrementTable]
[BusinessTable]
public class LogAuditEntity : LogAuditFields
{
}