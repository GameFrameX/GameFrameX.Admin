using GameFrameX.Entity.Routing;

namespace GameFrameX.Entity.Log;

/// <summary>
/// 平台访问日志表（主库 system_log_* 归属）
/// </summary>
[SugarTable(null, "平台访问日志表")]
[IncrementTable]
[PlatformTable]
public class SystemLogVisitEntity : LogVisitFields
{
}