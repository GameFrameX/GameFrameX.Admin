using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏资源
/// </summary>
[SugarTable(null, "定时任务")]
[TenantTable]
[IncrementTable]
public class TenantGameTimeJobEntity : EntityBasicSelect
{

    /// <summary>
    ///是否一次性任务
    ///</summary>
    [SugarColumn(ColumnDescription = "是否一次性任务", DefaultValue = "false")]
    public bool IsOnce { get; set; }

    /// <summary>
    ///执行时间
    ///</summary>
    [SugarColumn(ColumnDescription = "执行时间")]
    public string Cron { get; set; } = null!;

    /// <summary>
    ///任务来源
    ///</summary>
    [SugarColumn(ColumnDescription = "任务来源")]
    public string Source { get; set; } = null!;

    /// <summary>
    ///邮件id
    ///</summary>
    [SugarColumn(ColumnDescription = "邮件id", DefaultValue = "0")]
    public long MailId { get; set; }
}