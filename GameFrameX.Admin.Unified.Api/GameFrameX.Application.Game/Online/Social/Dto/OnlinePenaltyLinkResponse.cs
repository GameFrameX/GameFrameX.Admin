using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Online.Social.Dto;

/// <summary>
/// 处罚联动命令聚合响应（按联动目标区服集逐区服执行后的聚合结果）。
/// </summary>
public class OnlinePenaltyLinkResponse
{
    /// <summary>
    /// 处罚事实源行 Id。
    /// </summary>
    public long PenaltyId { get; set; }

    /// <summary>
    /// 联动命令类型（Kick / Ban / Mute，三值硬白名单）。
    /// </summary>
    public OnlinePenaltyCommandKind Command { get; set; }

    /// <summary>
    /// 各区服执行结果（全区服处罚 = App 下每个区服一条）。
    /// </summary>
    public List<OnlinePenaltyLinkResultResponse> Results { get; set; } = new();

    /// <summary>
    /// 是否全部区服成功（false 时存在可重试目标；重试幂等依据 IdempotencyKey 命中 Online 首次结果）。
    /// </summary>
    public bool AllSucceeded { get; set; }
}
