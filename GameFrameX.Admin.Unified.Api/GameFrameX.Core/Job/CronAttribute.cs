namespace GameFrameX.Core.Job;

/// <summary>
/// Cron 触发器特性（NCrontab 含秒 6 段表达式，如 <c>0 30 3 * * *</c> 表示每天 03:30:00）。
/// <para>与 <see cref="PeriodSecondsAttribute"/> 同级，供内置作业声明固定时刻的周期触发。</para>
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class CronAttribute : TriggerAttribute
{
    /// <summary>
    /// 以 cron 表达式声明触发器。
    /// </summary>
    /// <param name="expression">NCrontab 含秒 6 段表达式。</param>
    public CronAttribute(string expression)
    {
        Expression = expression;
    }

    /// <summary>
    /// Cron 表达式（含秒 6 段）。
    /// </summary>
    public string Expression { get; }
}
