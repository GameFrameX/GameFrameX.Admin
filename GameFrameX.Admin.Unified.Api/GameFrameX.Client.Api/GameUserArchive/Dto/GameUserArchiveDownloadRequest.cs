namespace GameFrameX.Client.Api.GameUserArchive.Dto;

/// <summary>
/// 存档下载请求（原 BLGetUserBranch 入参）
/// </summary>
public class GameUserArchiveDownloadRequest
{
    /// <summary>
    /// 玩家 id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 客户端版本
    /// </summary>
    public string Version { get; set; } = string.Empty;
}