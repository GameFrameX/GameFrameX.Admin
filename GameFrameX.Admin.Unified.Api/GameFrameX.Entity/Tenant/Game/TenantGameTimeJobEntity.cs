using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏资源
/// </summary>
[SugarTable(null, "定时任务")]
[SugarIndex("idx_tenant_game_time_job_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameTimeJobEntity : EntityBasicSelect, IAppScoped
{
    /// <summary>
    /// 应用Id。运营内容应用作用域（TenantId + AppId），必须 &gt; 0。
    /// <para>来源：Admin 后台当前应用上下文写入。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

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