namespace GameFrameX.Application.Game;

// 定义 JobConfig 类
public class JobConfig
{
    public string JobType { get; set; } = null!;
    public string CronExpression { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsOnce { get; set; }
    public long MailId { get; set; }
    public long Id { get; set; }
}