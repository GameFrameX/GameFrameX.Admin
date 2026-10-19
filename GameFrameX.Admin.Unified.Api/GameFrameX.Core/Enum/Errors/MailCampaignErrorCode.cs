namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// Admin 运营邮件 Campaign错误码（段 314，见 <see cref="ErrorSegmentRegistry"/>；C265 自旧 D 码体系平移）。
/// </summary>
public enum MailCampaignErrorCode
{
    /// <summary>
    /// 邮件已发布，主体字段不可修改
    /// </summary>
    [ErrorText("邮件已发布，主体字段不可修改", "The mail campaign is published or revoked; content fields cannot be modified")]
    AlreadyPublishedOrRevoked = 31400001,

    /// <summary>
    /// 邮件已撤回，不可重复撤回
    /// </summary>
    [ErrorText("邮件已撤回，不可重复撤回", "The mail campaign is already revoked")]
    AlreadyRevoked = 31400002,

    /// <summary>
    /// 邮件记录不存在
    /// </summary>
    [ErrorText("邮件记录不存在", "Mail campaign does not exist")]
    CampaignNotFound = 31400003,

    /// <summary>
    /// 邮件参数校验失败
    /// </summary>
    [ErrorText("邮件参数校验失败", "Mail campaign parameter validation failed")]
    ParameterInvalid = 31400004,

    /// <summary>
    /// 邮件当前状态不允许该操作
    /// </summary>
    [ErrorText("邮件当前状态不允许该操作", "The current mail campaign state does not allow this operation")]
    StateOperationForbidden = 31400005,

    /// <summary>
    /// 邮件发布调用服务端失败
    /// </summary>
    [ErrorText("邮件发布调用服务端失败", "Failed to call the server for the mail campaign")]
    ServerCallFailed = 31400006,
}