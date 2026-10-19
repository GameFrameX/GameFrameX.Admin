namespace GameFrameX.Entity.Enum;

/// <summary>
/// 游戏邮件状态
/// </summary>
[Description("游戏邮件状态")]
[AdminEnum]
public enum GameMailStatus
{
    /// <summary>
    /// 邮件已创建但还未发送。
    /// </summary>
    [Description("草稿")] Draft,

    /// <summary>
    /// 邮件已发送给收件人，但还未被查看。
    /// </summary>
    [Description("已发送")] Sent,

    /// <summary>
    /// 邮件已被收件人查看。
    /// </summary>
    [Description("已阅读")] Read,

    /// <summary>
    /// 邮件中的附件已被收件人领取。
    /// </summary>
    [Description("已领取附件")] AttachmentClaimed,

    /// <summary>
    /// 邮件已从收件人的邮箱中被删除。
    /// </summary>
    [Description("已删除")] Deleted,

    /// <summary>
    /// 邮件由于过期或其他原因而被系统回收。
    /// </summary>
    [Description("已过期")] Expired,

    /// <summary>
    /// 邮件因其他特殊原因被标记为无效。
    /// </summary>
    [Description("无效")] Invalid,
}