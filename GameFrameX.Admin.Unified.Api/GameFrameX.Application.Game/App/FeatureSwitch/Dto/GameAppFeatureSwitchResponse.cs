using GameFrameX.Entity.Tenant.Game.FeatureSwitch;

namespace GameFrameX.Application.Game.App.FeatureSwitch.Dto;

/// <summary>
/// 活动/功能开关管理输出参数（字段名与实体逐字一致，名映射投影）
/// </summary>
public class GameAppFeatureSwitchResponse : IBasePageResponse<TenantGameAppFeatureSwitchEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用作用域，0 / null 表示租户默认层
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 开关键
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
    public string Audience { get; set; } = null!;

    /// <summary>
    /// 开关参数 JSON 原文
    /// </summary>
    public string ValueJson { get; set; } = null!;

    /// <summary>
    /// 生效开始时间（Unix 秒，UTC）
    /// </summary>
    public long? StartTime { get; set; }

    /// <summary>
    /// 生效结束时间（Unix 秒，UTC）
    /// </summary>
    public long? EndTime { get; set; }

    /// <summary>
    /// 平台作用域，default 表示不限
    /// </summary>
    public string Platform { get; set; } = null!;

    /// <summary>
    /// 包名作用域，default 表示不限
    /// </summary>
    public string Package { get; set; } = null!;

    /// <summary>
    /// 语言作用域，default 表示不限
    /// </summary>
    public string Language { get; set; } = null!;

    /// <summary>
    /// 渠道作用域，default 表示不限
    /// </summary>
    public string Channel { get; set; } = null!;

    /// <summary>
    /// 投放优先级
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// 灰度比例 0-100
    /// </summary>
    public int RolloutPercent { get; set; }

    /// <summary>
    /// 设备白名单 JSON 数组
    /// </summary>
    public string? DeviceWhitelistJson { get; set; }

    /// <summary>
    /// 启停标记
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建者
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新者
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 乐观锁版本（编辑回显原样回传）
    /// </summary>
    public long? RowVersion { get; set; }
}
