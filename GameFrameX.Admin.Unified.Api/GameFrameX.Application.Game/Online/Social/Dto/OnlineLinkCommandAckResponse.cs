namespace GameFrameX.Application.Game.Online.Social.Dto;

/// <summary>
/// Online 联动命令响应载荷（Admin → Online 线缆契约；mock 先行，联调以 vault 详表回写）。
/// </summary>
public class OnlineLinkCommandAckResponse
{
    /// <summary>
    /// 旧 Token 吊销确认（vault:C2 S1.4：命令生效后 Online 确认已吊销该玩家旧 Token）。
    /// </summary>
    public bool TokenRevokedConfirmed { get; set; }
}
