using System.ComponentModel.DataAnnotations;
using GameFrameX.Entity.Game.Online;

namespace GameFrameX.Application.Game.Online.LiveOps.Dispatch.Dto;

/// <summary>
/// LiveOps 统一下发输入（来源 change C343 / vault:C8 S7.7）。
/// <para>五类来源硬枚举 + 业务主键 + Online 作用域三元组的 AppId/ServerId（TenantId 由鉴权上下文强制注入）；
/// <c>Reason</c> 必填进审计；<c>SegmentId</c> 可选定向分群。越枚举值 / 越作用域在入口即拒绝。</para>
/// </summary>
public sealed class OnlineLiveOpsDispatchRequest
{
    /// <summary>
    /// 运营事件来源（RemoteConfig/DeviceGroup/ScheduledTask/Announcement/MailCampaign 严格五值）。
    /// </summary>
    [Required(ErrorMessage = "来源类型不能为空")]
    public LiveOpsSourceKind Source { get; set; }

    /// <summary>
    /// 来源域业务主键（对应来源实体的 Id，如远程配置记录 Id）。
    /// </summary>
    [Required(ErrorMessage = "业务主键不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "业务主键必须大于0")]
    public long BusinessKey { get; set; }

    /// <summary>
    /// 目标应用Id（经 Online 作用域三元组租户归属校验）。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "应用Id必须大于0")]
    public long AppId { get; set; }

    /// <summary>
    /// 目标区服Id（经 Online 作用域三元组租户归属校验；Online 命令按区服路由到对应 Runtime）。
    /// </summary>
    [Required(ErrorMessage = "区服Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "区服Id必须大于0")]
    public long ServerId { get; set; }

    /// <summary>
    /// 可选定向分群Id（引用玩家分群行；下发只对分群内玩家生效，语义对照见 OnlineRolloutSemanticMap）。
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "分群Id必须大于0")]
    public long? SegmentId { get; set; }

    /// <summary>
    /// 下发理由（必填，随 MonitorLogging 全链路审计）。
    /// </summary>
    [Required(ErrorMessage = "下发理由不能为空")]
    public string Reason { get; set; } = null!;
}
