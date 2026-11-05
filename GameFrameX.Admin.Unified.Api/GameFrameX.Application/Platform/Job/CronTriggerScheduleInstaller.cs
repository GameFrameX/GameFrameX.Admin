using System.Runtime.CompilerServices;
using GameFrameX.Core.Job;
using NCrontab;

namespace GameFrameX.Application.Platform.Job;

/// <summary>
/// 安装 NCrontab 的 cron 解释钩子。
/// <para><c>GameFrameX.Core</c> 不引用 NCrontab 包，cron 触发器经 <see cref="TriggerSchedule.CronNextOccurrenceParser"/>
/// 钩子解释；本类随程序集加载进程级安装（与 SqlSugarSnowflakeFallbackInitializer 同一 ModuleInitializer 模式），
/// 覆盖一切加载 GameFrameX.Application 的进程（Web.Api host 与单测进程）。</para>
/// <para>表达式口径与业务定时任务一致：含秒 6 段（<c>IncludingSeconds = true</c>）。</para>
/// </summary>
internal static class CronTriggerScheduleInstaller
{
    [ModuleInitializer]
    internal static void Install()
    {
        TriggerSchedule.CronNextOccurrenceParser = ParseNextOccurrence;
    }

    private static DateTime ParseNextOccurrence(string expression, DateTime from)
    {
        var schedule = CrontabSchedule.Parse(expression, new CrontabSchedule.ParseOptions { IncludingSeconds = true, });
        return schedule.GetNextOccurrence(from);
    }
}
