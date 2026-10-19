namespace GameFrameX.Application.Game.TimeJob.GameTimeJob.Dto;

/// <summary>
/// 游戏定时任务输出参数
/// </summary>
public class TenantGameTimeJobEntityResponse : IBasePageResponse<Entity.Tenant.Game.TenantGameTimeJobEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// 应用Id
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 是否一次性任务
    /// </summary>
    public bool IsOnce { get; set; }

    /// <summary>
    /// 执行时间
    /// </summary>
    public string Cron { get; set; } = null!;

    /// <summary>
    /// 任务来源
    /// </summary>
    public string Source { get; set; } = null!;

    /// <summary>
    /// 邮件id
    /// </summary>
    public long MailId { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 行版本号（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }
}