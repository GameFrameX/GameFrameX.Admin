namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// 游戏数据域首页错误码（段 605，见 <see cref="ErrorSegmentRegistry"/>；C321 / GFX-730）。
/// </summary>
public enum GameHomeErrorCode
{
    /// <summary>
    /// 目标租户库连接不可用
    /// </summary>
    [ErrorText("目标租户库连接不可用，无法读取首页读数", "Target tenant database connection is unavailable for home metrics")]
    TenantDbUnavailable = 60500002,
}
