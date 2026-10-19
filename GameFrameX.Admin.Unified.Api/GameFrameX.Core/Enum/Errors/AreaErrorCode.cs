namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// 游戏数据域区服错误码（段 603，见 <see cref="ErrorSegmentRegistry"/>）。
/// </summary>
public enum AreaErrorCode
{
    /// <summary>
    /// 区服已存在
    /// </summary>
    [ErrorText("区服已存在", "Server area already exists")]
    AreaExists = 60300001,

    /// <summary>
    /// 区服不存在
    /// </summary>
    [ErrorText("区服不存在", "Server area not found")]
    AreaNotFound = 60300002,

    /// <summary>
    /// 当前状态不能注册
    /// </summary>
    [ErrorText("当前状态不能注册", "Registration is not allowed in the current state")]
    AreaCannotRegister = 60300003,

    /// <summary>
    /// 当前区服已满员
    /// </summary>
    [ErrorText("当前区服已满员", "Server area is full")]
    AreaHasFull = 60300004,

    /// <summary>
    /// 当前状态不能登录
    /// </summary>
    [ErrorText("当前状态不能登录", "Login is not allowed in the current state")]
    AreaCannotLogin = 60300005,

    /// <summary>
    /// 当前状态不能购买
    /// </summary>
    [ErrorText("当前状态不能购买", "Purchase is not allowed in the current state")]
    AreaCannotBuy = 60300006,
}