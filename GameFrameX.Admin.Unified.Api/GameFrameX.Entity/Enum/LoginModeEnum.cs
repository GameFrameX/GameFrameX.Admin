namespace GameFrameX.Entity.Enum;

/// <summary>
/// 登录模式枚举
/// </summary>
[Description("登录模式枚举")]
[AdminEnum]
public enum LoginModeEnum
{
    /// <summary>
    /// PC模式
    /// </summary>
    [Description("PC模式")] PC = 1,

    /// <summary>
    /// APP
    /// </summary>
    [Description("APP")] APP = 2,
}