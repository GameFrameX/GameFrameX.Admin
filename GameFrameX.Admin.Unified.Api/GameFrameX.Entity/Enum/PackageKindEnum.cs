namespace GameFrameX.Entity.Enum;

/// <summary>
/// 分包类型
/// </summary>
[Description("分包类型")]
[AdminEnum]
public enum PackageKindEnum
{
    /// <summary>
    /// 普通分包
    /// </summary>
    [Description("普通")] Normal = 1,

    /// <summary>
    /// 母包
    /// </summary>
    [Description("母包")] Master = 2,

    /// <summary>
    /// 子包
    /// </summary>
    [Description("子包")] Derived = 3,
}