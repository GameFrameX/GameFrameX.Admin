using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 首页在线人数时序采样（C321 / GFX-730）
/// <para>后台 Job 每 15 分钟按「启用区服 Control GetOnlinePlayer 求和」聚合全服在线数落表，
/// 首页趋势接口按整点桶回读，缺桶返回 null（前端降级画 —）。</para>
/// </summary>
[SugarTable(null, "首页在线人数时序采样")]
[SugarIndex("ux_home_online_sample_tenant_time", nameof(TenantId), OrderByType.Asc, nameof(SampleTime), OrderByType.Asc, true)]
[TenantTable]
[IncrementTable]
public class TenantGameHomeOnlineSampleEntity : EntityBasic
{
    /// <summary>
    /// 采样桶时间（15 分钟桶起点，Unix 秒）
    /// </summary>
    [SugarColumn(ColumnDescription = "采样桶时间（15 分钟桶起点，Unix 秒）", DefaultValue = "0")]
    public long SampleTime { get; set; }

    /// <summary>
    /// 全服在线人数（启用区服在线人数之和，区服不可达或未配置管理地址时按 0 计，语义同 C175）
    /// </summary>
    [SugarColumn(ColumnDescription = "全服在线人数", DefaultValue = "0")]
    public int OnlineCount { get; set; }
}
