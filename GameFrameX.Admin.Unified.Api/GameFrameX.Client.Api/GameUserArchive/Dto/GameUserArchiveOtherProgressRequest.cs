namespace GameFrameX.Client.Api.GameUserArchive.Dto;

/// <summary>
/// 查询目标玩家存档摘要请求（原 BLGetOtherProgress 入参 GetOtherProgressReq）
/// <para>注：客户端调用方已在 C7 删除（当前死代码），服务端按契约先建能力，接线随冲突窗恢复。</para>
/// </summary>
public class GameUserArchiveOtherProgressRequest
{
    /// <summary>
    /// 目标玩家 userId
    /// </summary>
    public long OtherId { get; set; }

    /// <summary>
    /// 客户端版本号
    /// </summary>
    public string Version { get; set; } = string.Empty;
}