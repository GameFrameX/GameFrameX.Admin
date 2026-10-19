namespace GameFrameX.Core.Service.TenantMenuSync;

/// <summary>
/// 租户套餐菜单天花板计算结果（C353 三层权责的 ceiling 层，平台拥有、硬双向）。
/// </summary>
public sealed record TenantMenuSyncPackageCeiling
{
    /// <summary>
    /// 无 ceiling（PackageId == null）单例。
    /// </summary>
    public static readonly TenantMenuSyncPackageCeiling NoCeiling = new(TenantMenuSyncCeilingKind.None, new HashSet<long>());

    /// <summary>
    /// 套餐悬空单例。
    /// </summary>
    public static readonly TenantMenuSyncPackageCeiling Dangling = new(TenantMenuSyncCeilingKind.Dangling, new HashSet<long>());

    /// <summary>
    /// 构建有界 ceiling。
    /// </summary>
    public static TenantMenuSyncPackageCeiling Bounded(IReadOnlySet<long> menuIds)
    {
        return new TenantMenuSyncPackageCeiling(TenantMenuSyncCeilingKind.Bounded, menuIds);
    }

    private TenantMenuSyncPackageCeiling(TenantMenuSyncCeilingKind kind, IReadOnlySet<long> menuIds)
    {
        Kind = kind;
        MenuIds = menuIds;
    }

    /// <summary>
    /// ceiling 形态
    /// </summary>
    public TenantMenuSyncCeilingKind Kind { get; }

    /// <summary>
    /// 闭包菜单 ID 集合（仅 Bounded 形态非空）
    /// </summary>
    public IReadOnlySet<long> MenuIds { get; }
}
