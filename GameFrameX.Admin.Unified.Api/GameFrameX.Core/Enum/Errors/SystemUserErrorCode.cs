namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// Admin 系统用户与认证错误码（段 300，见 <see cref="ErrorSegmentRegistry"/>；C265 自旧 D 码体系平移）。
/// </summary>
public enum SystemUserErrorCode
{
    /// <summary>
    /// 验证码错误
    /// </summary>
    [ErrorText("验证码错误", "Captcha is incorrect")]
    CaptchaInvalid = 30000001,

    /// <summary>
    /// 账号不存在
    /// </summary>
    [ErrorText("账号不存在", "Account does not exist")]
    AccountNotFound = 30000002,

    /// <summary>
    /// 非法操作，禁止删除自己
    /// </summary>
    [ErrorText("非法操作，禁止删除自己", "Illegal operation: deleting yourself is forbidden")]
    DeleteSelfForbidden = 30000003,

    /// <summary>
    /// 账号已存在
    /// </summary>
    [ErrorText("账号已存在", "Account already exists")]
    AccountAlreadyExists = 30000004,

    /// <summary>
    /// 旧密码输入错误
    /// </summary>
    [ErrorText("旧密码输入错误", "Old password is incorrect")]
    OldPasswordMismatch = 30000005,

    /// <summary>
    /// 非法操作，未登录
    /// </summary>
    [ErrorText("非法操作，未登录", "Illegal operation: not signed in")]
    NotLoggedIn = 30000006,

    /// <summary>
    /// 禁止删除超级管理员
    /// </summary>
    [ErrorText("禁止删除超级管理员", "Deleting the super administrator is forbidden")]
    DeleteSuperAdminForbidden = 30000007,

    /// <summary>
    /// 禁止修改超级管理员状态
    /// </summary>
    [ErrorText("禁止修改超级管理员状态", "Modifying the super administrator status is forbidden")]
    ModifySuperAdminStatusForbidden = 30000008,

    /// <summary>
    /// 没有权限
    /// </summary>
    [ErrorText("没有权限", "No permission")] NoPermission = 30000009,

    /// <summary>
    /// 账号已冻结
    /// </summary>
    [ErrorText("账号已冻结", "Account is frozen")]
    AccountFrozen = 30000010,

    /// <summary>
    /// 禁止为超级管理员分配角色
    /// </summary>
    [ErrorText("禁止为超级管理员分配角色", "Assigning roles to the super administrator is forbidden")]
    AssignRoleToSuperAdminForbidden = 30000011,

    /// <summary>
    /// 邮箱自助注册未开启
    /// </summary>
    [ErrorText("邮箱自助注册未开启", "Email self-service registration is disabled")]
    EmailRegisterDisabled = 30000012,

    /// <summary>
    /// 验证码错误或已过期
    /// </summary>
    [ErrorText("验证码错误或已过期", "Verification code is invalid or expired")]
    EmailCodeNotFoundOrExpired = 30000013,

    /// <summary>
    /// 验证码发送太频繁
    /// </summary>
    [ErrorText("验证码发送太频繁，请稍后再试", "Verification code is being sent too frequently, please retry later")]
    EmailCodeSendTooFrequent = 30000014,

    /// <summary>
    /// 今日验证码发送次数已达上限
    /// </summary>
    [ErrorText("今日验证码发送次数已达上限", "Daily verification code sending limit has been reached")]
    EmailCodeDailyLimitReached = 30000015,

    /// <summary>
    /// 邮件发送失败
    /// </summary>
    [ErrorText("邮件发送失败，请稍后重试", "Failed to send the email, please retry later")]
    EmailSendFailed = 30000016,

    /// <summary>
    /// 平台管理员密码尚未初始化，需先完成首装设置
    /// </summary>
    [ErrorText("平台管理员密码尚未初始化，请先完成初始化设置", "The platform administrator password has not been initialized; complete the setup first")]
    SuperAdminPasswordNotConfigured = 30000017,

    /// <summary>
    /// 平台管理员密码已初始化，禁止重复设置
    /// </summary>
    [ErrorText("平台管理员密码已初始化，禁止重复设置", "The platform administrator password has already been initialized; repeated setup is forbidden")]
    SuperAdminPasswordAlreadyConfigured = 30000018,

    /// <summary>
    /// 两次输入的密码不一致
    /// </summary>
    [ErrorText("两次输入的密码不一致", "The two entered passwords do not match")]
    SetupPasswordConfirmMismatch = 30000019,

    /// <summary>
    /// 操作过于频繁
    /// </summary>
    [ErrorText("操作过于频繁，请稍后再试", "The operation is too frequent; please retry later")]
    OperationTooFrequent = 30000020,

    /// <summary>
    /// 今日操作次数已达上限
    /// </summary>
    [ErrorText("今日操作次数已达上限", "The daily operation limit has been reached")]
    DailyOperationLimitReached = 30000021,
}