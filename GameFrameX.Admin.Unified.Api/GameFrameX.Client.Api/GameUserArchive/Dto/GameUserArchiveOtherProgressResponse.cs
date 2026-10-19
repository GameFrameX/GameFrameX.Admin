namespace GameFrameX.Client.Api.GameUserArchive.Dto;

/// <summary>
/// 查询目标玩家存档摘要响应（原 BLGetOtherProgress 出参 GetOtherProgressResp）
/// <para>Rcode：0 = 有云端数据；1 = 无云端数据（客户端展示兜底值）；2 = 版本过旧。</para>
/// <para>差异说明：旧协议 gem/gold/skipprop 为 JSON 字符串求和格式，新通道直接映射存档实体列值
/// （客户端无消费方，随接线再定形态）。</para>
/// </summary>
public class GameUserArchiveOtherProgressResponse
{
    /// <summary>
    /// 业务结果码（0 = 有数据；1 = 无云端数据；2 = 版本过旧）
    /// </summary>
    public long Rcode { get; set; }

    /// <summary>
    /// 目标存档版本戳
    /// </summary>
    public long SyncTime { get; set; }

    /// <summary>
    /// 宝石
    /// </summary>
    public string Gem { get; set; } = string.Empty;

    /// <summary>
    /// 跳过券
    /// </summary>
    public string SkipProp { get; set; } = string.Empty;

    /// <summary>
    /// 金币
    /// </summary>
    public string Gold { get; set; } = string.Empty;

    /// <summary>
    /// 等级（数字字符串）
    /// </summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// 当前任务
    /// </summary>
    public string CurrentTask { get; set; } = string.Empty;

    /// <summary>
    /// 当前章节
    /// </summary>
    public long CurrentChapter { get; set; }

    /// <summary>
    /// 订单所属章节 id
    /// </summary>
    public long OrderChapterId { get; set; }

    /// <summary>
    /// 订单组 id
    /// </summary>
    public long OrderGroupId { get; set; }

    /// <summary>
    /// 当前佩戴徽章
    /// </summary>
    public string Badge { get; set; } = string.Empty;

    /// <summary>
    /// 徽章数量
    /// </summary>
    public long BadgeCount { get; set; }

    /// <summary>
    /// 当前头像框
    /// </summary>
    public string Frame { get; set; } = string.Empty;

    /// <summary>
    /// 头像框数量
    /// </summary>
    public long FrameCount { get; set; }
}