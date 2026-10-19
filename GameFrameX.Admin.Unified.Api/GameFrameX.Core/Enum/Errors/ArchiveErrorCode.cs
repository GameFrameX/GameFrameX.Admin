namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// 游戏数据域存档错误码（段 604，见 <see cref="ErrorSegmentRegistry"/>）。
/// </summary>
public enum ArchiveErrorCode
{
    /// <summary>
    /// 存档乐观锁冲突（客户端上送 SyncTime 与服务端版本戳不符，
    /// 语义对齐旧协议「下载-合并-重传」，客户端应先 Download 再重放上传）
    /// </summary>
    [ErrorText("存档同步冲突，请先下载最新存档后重试", "Archive sync conflict: download the latest archive and retry")]
    ArchiveSyncConflict = 60400001,
}