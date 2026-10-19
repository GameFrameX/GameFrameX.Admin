namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// 错误码大域，对应 8 位错误码 `域1-模块2-序号5` 中的首位数字；
/// Admin 业务域与预留域各覆盖多个首位。分段归属见 <see cref="ErrorSegmentRegistry"/>。
/// </summary>
public enum ErrorDomain
{
    /// <summary>
    /// 信封保留（成功码 0，不承载错误语义）。
    /// </summary>
    None = 0,

    /// <summary>
    /// 系统域（首位 1）。
    /// </summary>
    System = 1,

    /// <summary>
    /// 协议域（首位 2）。
    /// </summary>
    Protocol = 2,

    /// <summary>
    /// Admin 业务域（首位 3-5，按 Application.System 服务域一段一模块）。
    /// </summary>
    AdminBusiness = 3,

    /// <summary>
    /// 游戏数据域（首位 6）。
    /// </summary>
    GameData = 6,

    /// <summary>
    /// 游戏交易域（首位 7）。
    /// </summary>
    GameTrade = 7,

    /// <summary>
    /// 预留域（首位 8-9，新模块先登记段再写码）。
    /// </summary>
    Reserved = 8,
}