namespace GameFrameX.Application.Game.Online.Console.Dto;

/// <summary>
/// 统一审计行来源标识（change C344 / vault:C9 S8.2；三来源口径见 OnlineAuditSourceMap）。
/// </summary>
public enum OnlineAuditSourceKind
{
    /// <summary>
    /// Admin 操作日志（TenantLogOperationEntity，MonitorLogging 全 action 自动落库）。
    /// </summary>
    AdminOperation = 1,

    /// <summary>
    /// Online 审计事件（经 Online API 实时透传）。
    /// </summary>
    OnlineAuditEvent = 2,

    /// <summary>
    /// 各域命令记录（C350 处罚落账 / C343 灰度发布记录等 Admin 侧事实源表）。
    /// </summary>
    DomainCommandRecord = 3,
}
