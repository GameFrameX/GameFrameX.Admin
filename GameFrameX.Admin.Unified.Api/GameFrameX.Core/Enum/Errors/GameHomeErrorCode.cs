namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// 游戏数据域首页错误码（段 605，见 <see cref="ErrorSegmentRegistry"/>；C321 / GFX-730）。
/// </summary>
public enum GameHomeErrorCode
{
    /// <summary>
    /// 首页读数仅租户上下文可用
    /// </summary>
    [ErrorText("首页读数仅租户上下文可用：请以租户管理员登录，或由平台管理员显式切换目标租户后重试", "Home metrics require a tenant context: sign in as a tenant administrator, or switch the target tenant explicitly")]
    TenantContextRequired = 60500001,

    /// <summary>
    /// 目标租户库连接不可用
    /// </summary>
    [ErrorText("目标租户库连接不可用，无法读取首页读数", "Target tenant database connection is unavailable for home metrics")]
    TenantDbUnavailable = 60500002,
}
