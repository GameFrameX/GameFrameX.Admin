namespace GameFrameX.Application.Game.Online.LiveOps.Dispatch.Dto;

/// <summary>
/// 配置灰度发布 / 回滚下发 Online 结果（<c>OnlineLiveOpsDispatchService</c> Rollout 两方法专用回执，来源 change C343 / vault:C8 S7.7）。
/// <para>由灰度版本行 Service 消费并回写版本行 OnlineConfirmVersion。</para>
/// </summary>
public sealed class OnlineConfigRolloutDispatchResponse
{
    /// <summary>
    /// 下发通道命令名（<c>publishConfigRollout</c> / <c>rollbackConfigRollout</c>，审计展示）。
    /// </summary>
    public string OnlineAction { get; set; } = null!;

    /// <summary>
    /// Online 确认版本回执（原样存入版本行 OnlineConfirmVersion；空 = 未取得回执，调用方按空保存）。
    /// </summary>
    public string? ConfirmVersion { get; set; }

    /// <summary>
    /// 提示消息（Online 侧原文，排查辅助）。
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 下发时间（Unix 秒，UTC）。
    /// </summary>
    public long DispatchedTime { get; set; }
}
