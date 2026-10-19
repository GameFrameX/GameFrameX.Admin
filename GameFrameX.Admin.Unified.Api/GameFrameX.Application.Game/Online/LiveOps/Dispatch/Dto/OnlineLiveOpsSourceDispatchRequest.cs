using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Application.Game.Online.LiveOps.Dispatch.Dto;

/// <summary>
/// 既有运营域页面「经 Online 下发」动作的通用输入（来源 change C343 / vault:C8 S7.7）。
/// <para>来源类型由所在 Service 固定注入（远程配置 / 公告 / 邮件 Campaign），不 exposed 给前端选择；
/// 本输入只承载目标区服、可选定向分群与下发理由，内部统一转调 OnlineLiveOpsDispatchService 单一入口。</para>
/// </summary>
public sealed class OnlineLiveOpsSourceDispatchRequest
{
    /// <summary>
    /// 来源域业务主键（所在 Service 的实体 Id，由页面行上下文提供，服务端再校验存在性）。
    /// </summary>
    [Required(ErrorMessage = "业务主键不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "业务主键必须大于0")]
    public long BusinessKey { get; set; }

    /// <summary>
    /// 目标应用Id（页面应用作用域上下文提供）。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "应用Id必须大于0")]
    public long AppId { get; set; }

    /// <summary>
    /// 目标区服Id（Online 命令按区服路由到对应 Runtime）。
    /// </summary>
    [Required(ErrorMessage = "区服Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "区服Id必须大于0")]
    public long ServerId { get; set; }

    /// <summary>
    /// 可选定向分群Id（下发只对分群内玩家生效）。
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "分群Id必须大于0")]
    public long? SegmentId { get; set; }

    /// <summary>
    /// 下发理由（必填，随 MonitorLogging 全链路审计）。
    /// </summary>
    [Required(ErrorMessage = "下发理由不能为空")]
    public string Reason { get; set; } = null!;
}
