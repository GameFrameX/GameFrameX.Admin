namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// 游戏数据域账号错误码（段 601，见 <see cref="ErrorSegmentRegistry"/>）。
/// 旧段位 6020xxxx（Client.Api/Hub.Api ResultCode 把账号与角色挤在同一 602 段）
/// 按 ADR 根治重排为 601 段，成员序号沿用旧码尾号便于映射。
/// </summary>
public enum AccountErrorCode
{
    /// <summary>
    /// 账号或密码错误（旧 60200001）
    /// </summary>
    [ErrorText("账号或密码错误", "Incorrect account or password")]
    AccountOrPasswordError = 60100001,

    /// <summary>
    /// 账号已存在（旧 60200002）
    /// </summary>
    [ErrorText("账号已存在", "Account already exists")]
    AccountExists = 60100002,

    /// <summary>
    /// 账号不存在（旧 60200003；旧 80100002 GamePlayerNotFound 语义重合，切换时复用本码）
    /// </summary>
    [ErrorText("账号不存在", "Account not found")]
    AccountNotFound = 60100003,

    /// <summary>
    /// 账号已被禁用（旧 60200004）
    /// </summary>
    [ErrorText("账号已被禁用", "Account has been disabled")]
    AccountDisabled = 60100004,

    /// <summary>
    /// 账号未绑定谷歌Id（旧 60200005）
    /// </summary>
    [ErrorText("账号未绑定谷歌Id", "Account has no Google ID bound")]
    AccountNoBindGoogleId = 60100005,

    /// <summary>
    /// 谷歌Id已被其他用户绑定（旧 60200006）
    /// </summary>
    [ErrorText("谷歌Id已被其他用户绑定", "Google ID is already bound to another user")]
    AccountGoogleIdByOtherUser = 60100006,

    /// <summary>
    /// 邮箱的账号或密码错误（旧 60200007）
    /// </summary>
    [ErrorText("邮箱的账号或密码错误", "Incorrect email account or password")]
    AccountOrPasswordByEmailError = 60100007,

    /// <summary>
    /// 邮箱已存在（旧 60200008 AccountExistsByEmailError）
    /// </summary>
    [ErrorText("邮箱已存在", "Email already exists")]
    AccountEmailExists = 60100008,

    /// <summary>
    /// 账号未绑定AppleId（旧 60200009）
    /// </summary>
    [ErrorText("账号未绑定AppleId", "Account has no Apple ID bound")]
    AccountNoBindAppleId = 60100009,

    /// <summary>
    /// 账号未绑定FacebookId（旧 60200010）
    /// </summary>
    [ErrorText("账号未绑定FacebookId", "Account has no Facebook ID bound")]
    AccountNoBindFacebookId = 60100010,

    /// <summary>
    /// 账号未绑定TwitterId（旧 60200011）
    /// </summary>
    [ErrorText("账号未绑定TwitterId", "Account has no Twitter ID bound")]
    AccountNoBindTwitterId = 60100011,

    /// <summary>
    /// 账号未绑定DiscordId（旧 60200012）
    /// </summary>
    [ErrorText("账号未绑定DiscordId", "Account has no Discord ID bound")]
    AccountNoBindDiscordId = 60100012,

    /// <summary>
    /// 账号解绑找不到（旧 60200013）
    /// </summary>
    [ErrorText("账号解绑找不到", "Account unbind target not found")]
    AccountUnBindNotFound = 60100013,

    /// <summary>
    /// 邮箱已被其他用户绑定（旧 60200014）
    /// </summary>
    [ErrorText("邮箱已被其他用户绑定", "Email is already bound to another user")]
    AccountEmailByOtherUser = 60100014,

    /// <summary>
    /// 邮箱解绑找不到（旧 60200015）
    /// </summary>
    [ErrorText("邮箱解绑找不到", "Email unbind target not found")]
    AccountEmailUnBindNotFound = 60100015,

    /// <summary>
    /// 账号未绑定邮箱（旧 60200016）
    /// </summary>
    [ErrorText("账号未绑定邮箱", "Account has no email bound")]
    AccountNoBindEmail = 60100016,

    /// <summary>
    /// Apple Id已被其他用户绑定（旧 60200017）
    /// </summary>
    [ErrorText("Apple Id已被其他用户绑定", "Apple ID is already bound to another user")]
    AccountAppleIdByOtherUser = 60100017,

    /// <summary>
    /// Facebook Id已被其他用户绑定（旧 60200018）
    /// </summary>
    [ErrorText("Facebook Id已被其他用户绑定", "Facebook ID is already bound to another user")]
    AccountFacebookIdByOtherUser = 60100018,

    /// <summary>
    /// 账号未绑定Facebook（旧 60200019）
    /// </summary>
    [ErrorText("账号未绑定Facebook", "Account has no Facebook bound")]
    AccountNoBindFacebook = 60100019,

    /// <summary>
    /// QQ Id已被其他用户绑定（旧 60200020）
    /// </summary>
    [ErrorText("QQ Id已被其他用户绑定", "QQ ID is already bound to another user")]
    AccountQQIdByOtherUser = 60100020,

    /// <summary>
    /// 账号未绑定QQ（旧 60200021）
    /// </summary>
    [ErrorText("账号未绑定QQ", "Account has no QQ bound")]
    AccountNoBindQQ = 60100021,

    /// <summary>
    /// WeChat Id已被其他用户绑定（旧 60200022）
    /// </summary>
    [ErrorText("WeChat Id已被其他用户绑定", "WeChat ID is already bound to another user")]
    AccountWeChatIdByOtherUser = 60100022,

    /// <summary>
    /// 账号未绑定WeChat（旧 60200023）
    /// </summary>
    [ErrorText("账号未绑定WeChat", "Account has no WeChat bound")]
    AccountNoBindWeChat = 60100023,

    /// <summary>
    /// 用户邮件验证码不存在或已过期（旧 80100001 GameUserEmailCodeNotFound，原 801 预留段归并账号域）
    /// </summary>
    [ErrorText("用户邮件验证码不存在或已过期", "Email verification code not found or expired")]
    EmailCodeNotFoundOrExpired = 60100024,

    /// <summary>
    /// 账号处于注销状态（旧 60200300）
    /// </summary>
    [ErrorText("账号处于注销状态", "Account is being deleted")]
    AccountDeleting = 60100300,

    /// <summary>
    /// 账号已被封禁/限制登录（旧 60200301）
    /// </summary>
    [ErrorText("账号已被封禁/限制登录", "Account is banned or restricted from logging in")]
    AccountBanned = 60100301,
}