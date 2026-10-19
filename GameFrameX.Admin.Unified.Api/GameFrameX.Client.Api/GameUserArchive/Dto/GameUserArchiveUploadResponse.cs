namespace GameFrameX.Client.Api.GameUserArchive.Dto;

/// <summary>
/// 存档上传结果（原 BLUpdUserBranch 出参 UpdUserBranchResp）
/// </summary>
public class GameUserArchiveUploadResponse
{
    /// <summary>
    /// 业务结果码（0 = 成功；沿用旧协议 rcode 语义）
    /// </summary>
    public long Rcode { get; set; }

    /// <summary>
    /// 服务端新版本戳（客户端存作下次上送的 SyncTime）
    /// </summary>
    public long NewSyncTime { get; set; }
}