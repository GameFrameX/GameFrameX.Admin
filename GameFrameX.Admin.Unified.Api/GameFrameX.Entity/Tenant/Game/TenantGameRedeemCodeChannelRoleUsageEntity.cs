// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏兑换码渠道角色使用累计。承载兑换码分组（渠道）下每个角色的成功兑换次数，是组级角色限制额度的事实源。
/// <para>应用作用域事实数据（TenantId + AppId），AppId 跟随兑换码自身 AppId，必须 &gt; 0。</para>
/// <para>幂等：ux_redeem_channel_role_usage 在数据库层保证 (TenantId, AppId, ChannelId, RoleId) 全局唯一，并发兑换只产生一行累计，重复回调/并发请求不会插入重复行。</para>
/// <para>边界：该表只承担组级额度累计与并发兜底；兑换流水仍由 <see cref="TenantGameRedeemCodeUseRecordEntity"/> 记录，不在此表保存兑换码快照之外的审计字段。</para>
/// </summary>
[SugarTable(null, "游戏兑换码渠道角色使用累计")]
[SugarIndex("ux_redeem_channel_role_usage", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc, nameof(ChannelId), OrderByType.Asc, nameof(RoleId), OrderByType.Asc, true)]
[SugarIndex("idx_redeem_channel_role_usage_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameRedeemCodeChannelRoleUsageEntity : EntityBasic, IAppScoped
{
    /// <summary>
    /// 应用Id。事实数据应用作用域（TenantId + AppId），来自兑换码自身 AppId，必须 &gt; 0。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 兑换码渠道/分组 Id，来自兑换码 <c>ChannelId</c>（<see cref="TenantGameRedeemCodeEntity.ChannelId"/>）。
    /// </summary>
    [SugarColumn(ColumnDescription = "兑换码渠道Id", DefaultValue = "0")]
    public long ChannelId { get; set; }

    /// <summary>
    /// 角色 Id，对齐 Client API 兑换输入 <c>RoleId</c> 与使用记录 <c>PlayerId</c>。
    /// </summary>
    [SugarColumn(ColumnDescription = "角色Id", DefaultValue = "0")]
    public long RoleId { get; set; }

    /// <summary>
    /// 该角色在该渠道/分组下成功兑换的累计次数。组级额度判断 = <c>UsedCount &lt; TenantGameRedeemCodeChannelEntity.RoleUsageLimit</c>。
    /// </summary>
    [SugarColumn(ColumnDescription = "已使用次数", DefaultValue = "0")]
    public int UsedCount { get; set; }

    /// <summary>
    /// 最近一次成功兑换的兑换码字符串，仅用于排查与审计辅助，不承担业务唯一性。
    /// </summary>
    [SugarColumn(ColumnDescription = "最近兑换码", Length = EntityStringLength.Length256, IsNullable = true)]
    public string LastCode { get; set; } = null!;

    /// <summary>
    /// 最近一次成功兑换时间，秒级 Unix 时间戳。
    /// </summary>
    [SugarColumn(ColumnDescription = "最近兑换时间", DefaultValue = "0")]
    public long LastUseTime { get; set; }
}