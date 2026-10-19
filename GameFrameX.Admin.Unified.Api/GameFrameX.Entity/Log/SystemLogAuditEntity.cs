using GameFrameX.Entity.Routing;

namespace GameFrameX.Entity.Log;

/// <summary>
/// 平台审计日志表（主库 system_log_* 归属）
/// </summary>
[SugarTable(null, "平台审计日志表")]
[IncrementTable]
[PlatformTable]
public class SystemLogAuditEntity : LogAuditFields
{
}