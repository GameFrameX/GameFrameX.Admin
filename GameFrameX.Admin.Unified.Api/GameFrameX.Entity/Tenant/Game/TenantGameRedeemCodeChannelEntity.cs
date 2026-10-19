// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏兑换码渠道
/// </summary>
[SugarTable(null, "游戏兑换码渠道")]
[SugarIndex("idx_tenant_game_redeem_code_channel_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameRedeemCodeChannelEntity : EntityBasicSelect, IAppScoped
{
    /// <summary>
    /// 应用Id。运营内容应用作用域（TenantId + AppId），必须 &gt; 0。
    /// <para>来源：Admin 后台当前应用上下文写入。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 每角色可用次数（兑换码分组角色限制）。
    /// <para>语义：0 = 不限制，保持现有按单码 (Code + RoleId) 的兑换行为；1 = 同一渠道/分组下每个角色最多成功兑换一次；N = 同一渠道/分组下每个角色最多成功兑换 N 次。</para>
    /// <para>事实源：该字段是组级额度上限，实际已用次数由 <see cref="TenantGameRedeemCodeChannelRoleUsageEntity"/> 在 (TenantId, AppId, ChannelId, RoleId) 维度累计。</para>
    /// <para>维护约束：必须 &gt;= 0；负数非法。修改限制不影响历史累计行。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "每角色可用次数", DefaultValue = "0")]
    public int RoleUsageLimit { get; set; }
}