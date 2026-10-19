namespace GameFrameX.Entity.Enum;

/// <summary>
/// 租户菜单同步状态枚举
/// </summary>
[Description("租户菜单同步状态枚举")]
[AdminEnum]
public enum TenantMenuSyncStatusEnum
{
    /// <summary>
    /// 待同步（状态行缺失即视为待同步，通常不落库）
    /// </summary>
    [Description("待同步")] Pending = 1,

    /// <summary>
    /// 同步成功
    /// </summary>
    [Description("同步成功")] Succeeded = 2,

    /// <summary>
    /// 同步失败（保留旧目录版本号，等待周期扫描自愈重试）
    /// </summary>
    [Description("同步失败")] Failed = 3,
}
