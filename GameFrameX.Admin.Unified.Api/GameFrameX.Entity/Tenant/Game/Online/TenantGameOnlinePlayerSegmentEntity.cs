// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯用户合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

using GameFrameX.Entity.Base;
using GameFrameX.Entity.Game.Online;

namespace GameFrameX.Entity.Tenant.Game.Online;

/// <summary>
/// 玩家分群定义。Admin 是分群定义的事实源（编排），Online 侧按其事件数据解释与命中判定（vault:C8 S7.5/S7.8）。
/// <para>条件字段白名单（活跃天数 / 留存天数 / 累计付费金额三族起步，锚定 S7.5 标准事件族）集中登记于
/// <c>OnlineRolloutSemanticMap</c>，本实体只承载 JSON 表达式不解释语义；同步状态反映最近一次 syncOnline 结果。</para>
/// </summary>
[SugarTable(null, "玩家分群定义")]
[SugarIndex("idx_tenant_game_online_player_segment_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[TenantTable]
public class TenantGameOnlinePlayerSegmentEntity : EntityBasic, IAppScoped
{
    /// <summary>
    /// 应用Id。事实数据应用作用域（TenantId + AppId），必须 &gt; 0；分群面向特定应用的玩家群体。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 分群名称（Admin 侧识别用；同应用内唯一性由 Service 校验）。
    /// </summary>
    [SugarColumn(ColumnDescription = "分群名称", Length = EntityStringLength.Length256)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 分群描述（用途 / 命中口径备注，运营侧说明，不出现在 Online 契约）。
    /// </summary>
    [SugarColumn(ColumnDescription = "分群描述", Length = EntityStringLength.Length512, IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// 分群条件表达式 JSON（字段 - 操作符 - 值 数组）。字段名必须在 <c>OnlineRolloutSemanticMap</c> 白名单内，
    /// Admin 不解释条件语义（解释权在 Online 侧，命中记录可查 VC-7.11）。
    /// </summary>
    [SugarColumn(ColumnDescription = "分群条件JSON", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string? ConditionsJson { get; set; }

    /// <summary>
    /// Online 侧同步状态（NotSynced/Synced/Failed；反映最近一次 syncOnline 动作结果）。
    /// </summary>
    [SugarColumn(ColumnDescription = "同步状态", DefaultValue = "0")]
    public OnlinePlayerSegmentSyncStatus SyncStatus { get; set; }

    /// <summary>
    /// 最近一次同步时间（Unix 秒，UTC）。0 表示从未同步。
    /// </summary>
    [SugarColumn(ColumnDescription = "最近同步时间(Unix秒)", DefaultValue = "0")]
    public long LastSyncTime { get; set; }

    /// <summary>
    /// 最近一次同步的目标区服Id（Online 作用域三元组的 ServerId）。
    /// </summary>
    [SugarColumn(ColumnDescription = "最近同步区服Id", DefaultValue = "0")]
    public long LastSyncServerId { get; set; }

    /// <summary>
    /// Online 侧分群Id回执（同步成功后回写；0 表示尚未取得回执）。
    /// </summary>
    [SugarColumn(ColumnDescription = "Online分群Id回执", DefaultValue = "0")]
    public long LastOnlineSegmentId { get; set; }

    /// <summary>
    /// 最近一次同步的 Online 协议码（0 = 成功；非 0 为拒绝码，与 <c>LastServerMessage</c> 配对排查）。
    /// </summary>
    [SugarColumn(ColumnDescription = "最近同步协议码", DefaultValue = "0")]
    public int LastServerCode { get; set; }

    /// <summary>
    /// 最近一次同步的拒绝原因 / 回执消息。
    /// </summary>
    [SugarColumn(ColumnDescription = "最近同步消息", Length = EntityStringLength.Length512, IsNullable = true)]
    public string? LastServerMessage { get; set; }
}
