namespace GameFrameX.Application.Game;

// 定义 JobConfig 类
public class JobConfig
{
    /// <summary>
    /// 任务所属租户 ID；后台执行租户业务任务时必须显式携带，禁止回落主库。
    /// </summary>
    public long TenantId { get; set; }

    public string JobType { get; set; } = null!;
    public string CronExpression { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsOnce { get; set; }
    public long MailId { get; set; }
    public long Id { get; set; }
}