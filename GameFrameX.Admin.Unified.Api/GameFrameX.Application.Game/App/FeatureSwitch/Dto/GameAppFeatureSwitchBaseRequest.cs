using GameFrameX.Entity.Tenant.Game.FeatureSwitch;

namespace GameFrameX.Application.Game.App.FeatureSwitch.Dto;

/// <summary>
/// 活动/功能开关管理公共输入字段。
/// </summary>
public class GameAppFeatureSwitchBaseRequest : BaseAddRequest
{
    /// <summary>
    /// 应用作用域；null / 0 表示租户默认层
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 开关键（同一应用作用域 + 相同作用域维度组合内唯一）
    /// </summary>
    public string SwitchKey { get; set; } = null!;

    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 消费方标记：client / server / both
    /// </summary>
    public string Audience { get; set; } = FeatureSwitchAudienceValues.Both;

    /// <summary>
    /// 开关参数 JSON 原文（下发侧自行解析，服务端只校验其为合法 JSON）
    /// </summary>
    public string ValueJson { get; set; } = null!;

    /// <summary>
    /// 生效开始时间（Unix 秒，UTC）；null 表示该端不限
    /// </summary>
    public long? StartTime { get; set; }

    /// <summary>
    /// 生效结束时间（Unix 秒，UTC）；null 表示该端不限，区间左闭右开
    /// </summary>
    public long? EndTime { get; set; }

    /// <summary>
    /// 平台作用域；空 / default 表示不限
    /// </summary>
    public string? Platform { get; set; }

    /// <summary>
    /// 包名作用域；空 / default 表示不限，租户默认层不允许携带具体包名
    /// </summary>
    public string? Package { get; set; }

    /// <summary>
    /// 语言作用域；空 / default 表示不限
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// 渠道作用域；空 / default 表示不限
    /// </summary>
    public string? Channel { get; set; }

    /// <summary>
    /// 投放优先级。同一开关键下作用域评分相同时数值越大越优先
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// 灰度比例 0-100
    /// </summary>
    public int RolloutPercent { get; set; } = 100;

    /// <summary>
    /// 设备白名单 JSON 数组；命中白名单的设备绕过灰度比例
    /// </summary>
    public string? DeviceWhitelistJson { get; set; }

    /// <summary>
    /// 启停标记；false 时投放解析不下发该行
    /// </summary>
    public bool? IsEnabled { get; set; } = true;
}
