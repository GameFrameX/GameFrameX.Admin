// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯用户合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

using GameFrameX.Entity.Base;
using GameFrameX.Entity.Game.Online;

namespace GameFrameX.Entity.Tenant.Game.Online;

/// <summary>
/// 配置灰度发布（Rollout）版本行。每次发布 / 回滚都追加新行（版本化），历史行不可修改不可删除（vault:C8 S7.8 / VC-7.12）。
/// <para><c>ConfigVersion</c> 由 Admin 按 AppId + ConfigKind + ConfigKey 维度递增分配；
/// 回滚行的 <c>TargetVersion</c> 指向目标历史版本（回滚 = 版本化新下发，不是恢复旧行）；
/// 灰度语义（SegmentId + Percent 1-100 ↔ Online 侧解释 ↔ 既有 RolloutMatcher）三方对照见 <c>OnlineRolloutSemanticMap</c>。</para>
/// </summary>
[SugarTable(null, "配置灰度发布版本")]
[SugarIndex("idx_tenant_game_online_config_rollout_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[TenantTable]
public class TenantGameOnlineConfigRolloutEntity : EntityBasic, IAppScoped
{
    /// <summary>
    /// 应用Id。事实数据应用作用域（TenantId + AppId），必须 &gt; 0。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 目标区服Id（Online 作用域三元组的 ServerId；灰度下发面向特定区服的 Online Runtime）。
    /// </summary>
    [SugarColumn(ColumnDescription = "目标区服Id", DefaultValue = "0")]
    public long ServerId { get; set; }

    /// <summary>
    /// 配置族（RemoteConfig/Announcement/DeviceGroup/ScheduledTask；邮件走既有链路不参与灰度编排）。
    /// </summary>
    [SugarColumn(ColumnDescription = "配置族", DefaultValue = "1")]
    public OnlineConfigRolloutKind ConfigKind { get; set; }

    /// <summary>
    /// 配置业务键（所辖配置域实体 Id 的字符串形式，如远程配置记录 Id）。与 ConfigKind 联合定位「哪份配置」。
    /// </summary>
    [SugarColumn(ColumnDescription = "配置业务键", Length = EntityStringLength.Length256)]
    public string ConfigKey { get; set; } = null!;

    /// <summary>
    /// Admin 侧分配的下发版本号（AppId + ConfigKind + ConfigKey 维度递增；发布与回滚共用同一序列）。
    /// </summary>
    [SugarColumn(ColumnDescription = "下发版本号", DefaultValue = "0")]
    public int ConfigVersion { get; set; }

    /// <summary>
    /// 灰度分群Id（引用 <c>TenantGameOnlinePlayerSegmentEntity.Id</c>；必须为已同步 Online 的分群）。
    /// </summary>
    [SugarColumn(ColumnDescription = "灰度分群Id", DefaultValue = "0")]
    public long SegmentId { get; set; }

    /// <summary>
    /// 灰度百分比（1-100；语义边界见 <c>OnlineRolloutSemanticMap</c>，与记录级 RolloutPercent 0-100 含关闭态不同）。
    /// </summary>
    [SugarColumn(ColumnDescription = "灰度百分比", DefaultValue = "0")]
    public int Percent { get; set; }

    /// <summary>
    /// 版本行状态（Published=生效中；RolledBack=已被替代的历史版本，不可再作为发布基线）。
    /// </summary>
    [SugarColumn(ColumnDescription = "发布状态", DefaultValue = "1")]
    public OnlineConfigRolloutStatus Status { get; set; }

    /// <summary>
    /// 回滚目标版本（仅回滚行有值：指向要恢复到的历史 ConfigVersion；非回滚行为 0）。
    /// </summary>
    [SugarColumn(ColumnDescription = "回滚目标版本", DefaultValue = "0")]
    public int TargetVersion { get; set; }

    /// <summary>
    /// 本次下发理由（发布 / 回滚均必填，进审计）。
    /// </summary>
    [SugarColumn(ColumnDescription = "下发理由", Length = EntityStringLength.Length512)]
    public string Reason { get; set; } = null!;

    /// <summary>
    /// Online 确认版本回执（原样保存 Online 侧返回的版本表示，回显与一致性断言依据；空 = 未取得回执）。
    /// </summary>
    [SugarColumn(ColumnDescription = "Online确认版本回执", Length = EntityStringLength.Length256, IsNullable = true)]
    public string? OnlineConfirmVersion { get; set; }

    /// <summary>
    /// 下发操作人（RealName 优先，回退 Account）。
    /// </summary>
    [SugarColumn(ColumnDescription = "下发操作人", Length = EntityStringLength.Length256)]
    public string DispatchOperator { get; set; } = null!;

    /// <summary>
    /// 下发时间（Unix 秒，UTC）。
    /// </summary>
    [SugarColumn(ColumnDescription = "下发时间(Unix秒)", DefaultValue = "0")]
    public long PublishedTime { get; set; }
}
