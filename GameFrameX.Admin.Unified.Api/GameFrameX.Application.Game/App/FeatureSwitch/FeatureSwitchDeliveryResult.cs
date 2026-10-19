namespace GameFrameX.Application.Game.App.FeatureSwitch;

/// <summary>
/// 活动/功能开关投放解析结果。
/// </summary>
/// <param name="Version">
/// 版本号 = 设备灰度过滤前有效候选集的 canonical md5（小写 32 位 hex）。
/// <para>构造保证：候选行集合或任一业务字段变化必变；行序与设备灰度结论不影响。
/// 消费方可携带上次 Version 做 Unchanged 短路，活动时间窗进出会自然变版本。</para>
/// </param>
/// <param name="Switches">灰度过滤后的每个开关键 winner 集合</param>
public sealed record FeatureSwitchDeliveryResult(string Version, IReadOnlyList<FeatureSwitchEffectiveItem> Switches);
