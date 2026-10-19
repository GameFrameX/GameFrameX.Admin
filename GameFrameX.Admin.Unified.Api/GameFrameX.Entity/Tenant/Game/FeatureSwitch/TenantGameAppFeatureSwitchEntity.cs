using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game.FeatureSwitch;

/// <summary>
/// 活动/功能开关注册表（租户库）。
/// <para>每行一个可运行时投放的开关/活动事实源：客户端经 Client.Api、游戏服务器经 Hub.Api
/// 按 <see cref="Audience"/> 过滤后拉取，玩法 gate 判定权在使用方业务端点，本表只承载配置分发。</para>
/// </summary>
[SugarTable(null, "活动/功能开关注册表")]
[SugarIndex("idx_tenant_game_app_feature_switch_tenant_app_key", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc, nameof(SwitchKey), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public sealed class TenantGameAppFeatureSwitchEntity : EntityBasic
{
    /// <summary>
    /// 应用作用域，0 / null 表示租户默认层（未命中的应用行由该层按开关键兜底）
    /// </summary>
    [SugarColumn(ColumnDescription = "应用作用域", IsNullable = true)]
    public long? AppId { get; set; }

    /// <summary>
    /// 开关键（同一应用作用域 + 相同作用域维度组合内唯一）
    /// </summary>
    [SugarColumn(ColumnDescription = "开关键", Length = EntityStringLength.Length256)]
    public string SwitchKey { get; set; } = null!;

    /// <summary>
    /// 显示名称
    /// </summary>
    [SugarColumn(ColumnDescription = "显示名称", Length = EntityStringLength.Length256)]
    public string DisplayName { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    [SugarColumn(ColumnDescription = "详细描述", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// 消费方标记：client / server / both，默认 both。
    /// <para>下发通道按本列过滤——客户端通道取 client|both，服务器通道取 server|both。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "消费方标记", Length = EntityStringLength.Length256, DefaultValue = "both")]
    public string Audience { get; set; } = FeatureSwitchAudienceValues.Both;

    /// <summary>
    /// 开关参数 JSON 原文（下发侧自行解析，服务端不做 schema 校验）
    /// </summary>
    [SugarColumn(ColumnDescription = "开关参数 JSON", Length = -1)]
    public string ValueJson { get; set; } = null!;

    /// <summary>
    /// 生效开始时间（Unix 秒，UTC）。null 表示该端不限；未开始的开关不下发。
    /// <para>时间窗进出会改变投放版本号，消费方短轮询即可感知。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "生效开始时间", IsNullable = true)]
    public long? StartTime { get; set; }

    /// <summary>
    /// 生效结束时间（Unix 秒，UTC）。null 表示该端不限；已结束的开关不下发。区间语义为左闭右开。
    /// </summary>
    [SugarColumn(ColumnDescription = "生效结束时间", IsNullable = true)]
    public long? EndTime { get; set; }

    /// <summary>
    /// 平台作用域，空 / default 表示不限
    /// </summary>
    [SugarColumn(ColumnDescription = "平台", Length = EntityStringLength.Length256, DefaultValue = "default")]
    public string Platform { get; set; } = null!;

    /// <summary>
    /// 包名作用域，空 / default 表示不限；租户默认层（AppId &lt;= 0）不允许携带具体包名
    /// </summary>
    [SugarColumn(ColumnDescription = "包名", Length = EntityStringLength.Length256, DefaultValue = "default")]
    public string Package { get; set; } = null!;

    /// <summary>
    /// 语言作用域，空 / default 表示不限
    /// </summary>
    [SugarColumn(ColumnDescription = "语言", Length = EntityStringLength.Length256, DefaultValue = "default")]
    public string Language { get; set; } = null!;

    /// <summary>
    /// 渠道作用域，空 / default 表示不限
    /// </summary>
    [SugarColumn(ColumnDescription = "渠道", Length = EntityStringLength.Length256, DefaultValue = "default")]
    public string Channel { get; set; } = null!;

    /// <summary>
    /// 投放优先级。同一开关键下作用域评分相同时，数值越大越优先
    /// </summary>
    [SugarColumn(ColumnDescription = "投放优先级", DefaultValue = "0")]
    public int Priority { get; set; }

    /// <summary>
    /// 灰度比例 0-100。未命中设备白名单时，按稳定设备标识取模判断是否命中；服务器通道不做设备灰度
    /// </summary>
    [SugarColumn(ColumnDescription = "灰度比例", DefaultValue = "100")]
    public int RolloutPercent { get; set; } = 100;

    /// <summary>
    /// 设备白名单 JSON 数组。命中白名单的设备绕过灰度比例，直接命中该行
    /// </summary>
    [SugarColumn(ColumnDescription = "设备白名单 JSON", Length = -1, IsNullable = true)]
    public string? DeviceWhitelistJson { get; set; }
}
