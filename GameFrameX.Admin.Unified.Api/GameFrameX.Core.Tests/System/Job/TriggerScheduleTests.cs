using GameFrameX.Application.Platform.Job;
using GameFrameX.Core.Job;

namespace GameFrameX.Core.Tests.System.Job;

/// <summary>
/// 触发器调度解释器（TriggerSchedule）单测：period / cron 参数解释与归一化。
/// </summary>
public sealed class TriggerScheduleTests
{
    public TriggerScheduleTests()
    {
        // ModuleInitializer 只在 Application 程序集首次执行代码时运行；过滤执行时
        // 可能没有任何用例先触碰过该程序集，这里显式安装 cron 解释钩子保证确定性。
        CronTriggerScheduleInstaller.Install();
    }

    [Fact]
    public void GetNextOccurrence_PeriodArguments_AddsSeconds()
    {
        var trigger = BuildTrigger(TriggerSchedule.PeriodTriggerType, TriggerSchedule.PeriodArguments(30));
        var from = new DateTime(2026, 11, 5, 12, 0, 0);

        var next = TriggerSchedule.GetNextOccurrence(trigger, from);

        Assert.Equal(from.AddSeconds(30), next);
    }

    [Fact]
    public void GetNextOccurrence_CronArguments_ComputesNextOccurrence()
    {
        // ModuleInitializer（Application 程序集）已在测试进程安装 NCrontab 钩子。
        var trigger = BuildTrigger(TriggerSchedule.CronTriggerType, TriggerSchedule.CronArguments("0 30 3 * * *"));
        var from = new DateTime(2026, 11, 5, 12, 0, 0);

        var next = TriggerSchedule.GetNextOccurrence(trigger, from);

        Assert.Equal(new DateTime(2026, 11, 6, 3, 30, 0), next);
    }

    [Fact]
    public void GetNextOccurrence_LegacyFullTriggerTypeNames_Normalized()
    {
        // 任务调度 UI 历史默认值：TriggerType = "GameFrameX.Core.Job.PeriodTrigger"。
        var trigger = BuildTrigger("GameFrameX.Core.Job.PeriodTrigger", TriggerSchedule.PeriodArguments(15));
        var from = new DateTime(2026, 11, 5, 12, 0, 0);

        var next = TriggerSchedule.GetNextOccurrence(trigger, from);

        Assert.Equal(from.AddSeconds(15), next);
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("")]
    public void GetNextOccurrence_UnknownTriggerType_ReturnsNull(string triggerType)
    {
        var trigger = BuildTrigger(triggerType, TriggerSchedule.PeriodArguments(30));

        Assert.Null(TriggerSchedule.GetNextOccurrence(trigger, DateTime.Now));
    }

    [Fact]
    public void GetNextOccurrence_NonPositivePeriod_ReturnsNull()
    {
        var trigger = BuildTrigger(TriggerSchedule.PeriodTriggerType, TriggerSchedule.PeriodArguments(0));

        Assert.Null(TriggerSchedule.GetNextOccurrence(trigger, DateTime.Now));
    }

    [Fact]
    public void GetNextOccurrence_InvalidCronExpression_ReturnsNull()
    {
        var trigger = BuildTrigger(TriggerSchedule.CronTriggerType, TriggerSchedule.CronArguments("not a cron"));

        Assert.Null(TriggerSchedule.GetNextOccurrence(trigger, DateTime.Now));
    }

    [Fact]
    public void GetNextOccurrence_BareStringArguments_AcceptedDefensively()
    {
        // UI 直改库可能产生不带 JSON 数组外壳的 Args；防御性按裸字符串解释。
        var trigger = BuildTrigger(TriggerSchedule.PeriodTriggerType, "20");
        var from = new DateTime(2026, 11, 5, 12, 0, 0);

        var next = TriggerSchedule.GetNextOccurrence(trigger, from);

        Assert.Equal(from.AddSeconds(20), next);
    }

    private static Trigger BuildTrigger(string triggerType, string arguments)
    {
        return new Trigger { TriggerType = triggerType, Args = arguments, };
    }
}
