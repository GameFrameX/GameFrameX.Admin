namespace GameFrameX.Application.Game.App.FeatureSwitch;

/// <summary>
/// 单条生效开关的投放投影。
/// <para>时间窗以 UTC <see cref="DateTime"/> 输出（实体列为 Unix 秒），消费方按展示层需要自行转 long 秒下发。</para>
/// </summary>
/// <param name="SwitchKey">开关键</param>
/// <param name="ValueJson">开关参数 JSON 原文</param>
/// <param name="StartTime">生效开始时间（UTC）；null 表示不限</param>
/// <param name="EndTime">生效结束时间（UTC）；null 表示不限</param>
public sealed record FeatureSwitchEffectiveItem(string SwitchKey, string ValueJson, DateTime? StartTime, DateTime? EndTime);
