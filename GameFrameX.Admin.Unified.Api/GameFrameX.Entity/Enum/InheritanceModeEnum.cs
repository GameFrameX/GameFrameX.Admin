namespace GameFrameX.Entity.Enum;

/// <summary>
/// 配置继承模式
/// </summary>
[Description("配置继承模式")]
[AdminEnum]
public enum InheritanceModeEnum
{
    /// <summary>
    /// 继承母包配置
    /// </summary>
    [Description("继承")] Inherit = 1,

    /// <summary>
    /// 使用子包覆盖配置
    /// </summary>
    [Description("覆盖")] Override = 2,

    /// <summary>
    /// 显式禁用配置
    /// </summary>
    [Description("禁用")] Disable = 3,
}