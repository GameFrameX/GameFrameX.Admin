namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// 游戏数据域首页错误码（段 605，见 <see cref="ErrorSegmentRegistry"/>；C321 / GFX-730）。
/// </summary>
public enum GameHomeErrorCode
{
    /// <summary>
    /// 数据库连接不可用
    /// </summary>
    [ErrorText("数据库连接不可用，无法读取首页读数", "The database connection is unavailable for home metrics")]
    DbUnavailable = 60500002,
}
