namespace GameFrameX.Entity.Enum;

/// <summary>
/// 租户类型枚举
/// </summary>
[Description("租户类型枚举")]
[AdminEnum]
public enum TenantTypeEnum
{
    /// <summary>
    /// 库隔离
    /// </summary>
    [Description("库隔离")] Db = 1,
}