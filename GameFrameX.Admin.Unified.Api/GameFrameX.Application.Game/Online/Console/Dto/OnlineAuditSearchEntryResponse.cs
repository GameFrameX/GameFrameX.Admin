namespace GameFrameX.Application.Game.Online.Console.Dto;

/// <summary>
/// 统一审计检索行（三来源五维统一字段；change C344 / vault:C9 S8.2）。
/// <para>字段映射唯一来源是 OnlineAuditSourceMap——禁止在 Service 内散落第二套映射。</para>
/// </summary>
public class OnlineAuditSearchEntryResponse
{
    /// <summary>
    /// 来源标识（AdminOperation / OnlineAuditEvent / DomainCommandRecord）。
    /// </summary>
    public OnlineAuditSourceKind Source { get; set; }

    /// <summary>
    /// 操作者标识（Admin 操作日志 = 账号；Online 审计 = 操作者 Id；域命令 = 操作者 Id/账号）。
    /// </summary>
    public string OperatorIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// 目标对象标识（玩家 Id 等；提取不到时为 null，不伪造）。
    /// </summary>
    public string? TargetPlayerIdentifier { get; set; }

    /// <summary>
    /// 应用作用域（提取不到时为 null）。
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 区服作用域（提取不到时为 null）。
    /// </summary>
    public long? ServerId { get; set; }

    /// <summary>
    /// 命令类型（各来源原值：Controller.Action / Online CommandType / 域命令 ActionType）。
    /// </summary>
    public string CommandType { get; set; } = string.Empty;

    /// <summary>
    /// 发生时间（Unix 秒，三来源统一排序键）。
    /// </summary>
    public long OccurredAt { get; set; }

    /// <summary>
    /// 摘要（操作内容/原因概述）。
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// 关联 Id（可观测关联键；无则为 null）。
    /// </summary>
    public string? CorrelationId { get; set; }
}
