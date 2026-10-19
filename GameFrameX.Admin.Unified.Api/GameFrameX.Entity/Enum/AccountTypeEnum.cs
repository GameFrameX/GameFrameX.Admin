namespace GameFrameX.Entity.Enum;

/// <summary>
/// 账号类型枚举
/// </summary>
[Description("账号类型枚举")]
[AdminEnum]
public enum AccountTypeEnum
{
    /// <summary>
    /// 超级管理员
    /// </summary>
    [Description("超级管理员")] SuperAdmin = 999,

    /// <summary>
    /// 系统管理员
    /// </summary>
    [Description("系统管理员")] SystemAdmin = 888,

    /// <summary>
    /// 普通账号
    /// </summary>
    [Description("普通账号")] NormalUser = 777,

    /// <summary>
    /// 会员
    /// </summary>
    [Description("会员")] Member = 666,
}