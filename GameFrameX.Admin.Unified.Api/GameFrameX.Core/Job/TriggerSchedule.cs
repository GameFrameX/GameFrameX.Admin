using System.Globalization;
using GameFrameX.Core.Utility;

namespace GameFrameX.Core.Job;

/// <summary>
/// 触发器调度解释器：把触发器的 <c>TriggerType + Args</c> 解释为下一次运行时间。
/// <para>类型约定（引擎内部）：</para>
/// <list type="bullet">
/// <item><c>period</c>：Args 为 JSON 字符串数组 <c>["秒数"]</c>（与任务调度 UI AddJobTrigger 的 <c>["300"]</c> 格式同构）。</item>
/// <item><c>cron</c>：Args 为 JSON 字符串数组 <c>["表达式"]</c>（NCrontab 含秒 6 段，与业务定时任务 cron 同一口径）。</item>
/// <item>历史遗留完整类型名（含 <c>PeriodTrigger</c> / <c>CronTrigger</c> 的 <c>TriggerType</c>）按同名语义归一解释。</item>
/// <item>其余类型（含无参 <c>[Daily]</c>）解释不了，返回 null，触发器保持休眠。</item>
/// </list>
/// </summary>
public static class TriggerSchedule
{
    /// <summary>
    /// 周期触发器类型标识。
    /// </summary>
    public const string PeriodTriggerType = "period";

    /// <summary>
    /// Cron 触发器类型标识。
    /// </summary>
    public const string CronTriggerType = "cron";

    /// <summary>
    /// Cron 解释钩子：由引用 NCrontab 的程序集（<c>GameFrameX.Application</c> 的 ModuleInitializer）安装。
    /// <para>Core 不直接依赖 NCrontab 包；未安装钩子的进程（如部分测试进程）里 cron 触发器保持休眠。</para>
    /// </summary>
    public static Func<string, DateTime, DateTime>? CronNextOccurrenceParser { get; set; }

    /// <summary>
    /// 计算触发器在 <paramref name="from"/> 之后的下一次运行时间；解释不了（类型未知 / 参数缺失 / 表达式非法 / 周期非正）返回 null。
    /// </summary>
    /// <param name="trigger">触发器。</param>
    /// <param name="from">起点时刻。</param>
    /// <returns>下一次运行时间；无法解释时为 null（触发器休眠，调用方记录一次告警即可）。</returns>
    public static DateTime? GetNextOccurrence(Trigger trigger, DateTime from)
    {
        if (trigger == null || string.IsNullOrWhiteSpace(trigger.TriggerType))
        {
            return null;
        }

        var triggerType = trigger.TriggerType.Trim();
        var firstArgument = FirstArgument(trigger.Args);
        var isPeriod = triggerType.Equals(PeriodTriggerType, StringComparison.OrdinalIgnoreCase)
                       || triggerType.Contains("PeriodTrigger", StringComparison.OrdinalIgnoreCase);
        var isCron = triggerType.Equals(CronTriggerType, StringComparison.OrdinalIgnoreCase)
                     || triggerType.Contains("CronTrigger", StringComparison.OrdinalIgnoreCase);

        if (isPeriod)
        {
            if (int.TryParse(firstArgument, out var seconds) && seconds > 0)
            {
                return from.AddSeconds(seconds);
            }

            return null;
        }

        if (isCron)
        {
            var parser = CronNextOccurrenceParser;
            if (parser == null || string.IsNullOrWhiteSpace(firstArgument))
            {
                return null;
            }

            try
            {
                return parser(firstArgument, from);
            }
            catch (Exception)
            {
                // 表达式非法：休眠而不是反复抛错（调用方对 null 记录一次告警）。
                return null;
            }
        }

        return null;
    }

    /// <summary>
    /// 构造 period 触发器的 Args 载荷。
    /// </summary>
    /// <param name="seconds">周期秒数。</param>
    /// <returns>JSON 字符串数组载荷。</returns>
    public static string PeriodArguments(int seconds)
    {
        return Json.Serialize(new[] { seconds.ToString(CultureInfo.InvariantCulture), });
    }

    /// <summary>
    /// 构造 cron 触发器的 Args 载荷。
    /// </summary>
    /// <param name="expression">cron 表达式。</param>
    /// <returns>JSON 字符串数组载荷。</returns>
    public static string CronArguments(string expression)
    {
        return Json.Serialize(new[] { expression, });
    }

    /// <summary>
    /// 取 Args 的首个参数。Args 兼容两种形态：JSON 字符串数组（标准）与裸字符串（防御 UI 直改库）。
    /// </summary>
    private static string? FirstArgument(string? arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
        {
            return null;
        }

        var trimmed = arguments.Trim();
        if (!trimmed.StartsWith('['))
        {
            return trimmed;
        }

        try
        {
            var items = Json.Deserialize<string[]>(trimmed);
            return items is { Length: > 0, } ? items[0] : null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
