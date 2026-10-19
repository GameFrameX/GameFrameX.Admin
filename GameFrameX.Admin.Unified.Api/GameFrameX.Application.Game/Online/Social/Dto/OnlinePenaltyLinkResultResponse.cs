namespace GameFrameX.Application.Game.Online.Social.Dto;

/// <summary>
/// 单区服联动命令执行结果（VC-2.3 Admin 展示半边：旧 Token 吊销确认字段）。
/// </summary>
public class OnlinePenaltyLinkResultResponse
{
    /// <summary>
    /// 目标区服 Id。
    /// </summary>
    public long ServerId { get; set; }

    /// <summary>
    /// 该区服命令是否成功。
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Online 原始协议码（失败时透出，供按数字码排查；成功时为 null）。
    /// </summary>
    public int? OnlineCode { get; set; }

    /// <summary>
    /// 失败原因摘要（成功时为 null）。
    /// </summary>
    public string? FailureMessage { get; set; }

    /// <summary>
    /// 旧 Token 吊销确认（vault:C2 S1.4：命令生效后 Online 确认已吊销该玩家旧 Token；
    /// 服务端旧 Token 被拒的验证归 vault 侧用例，Admin 断言展示层确认字段）。
    /// </summary>
    public bool TokenRevokedConfirmed { get; set; }
}
