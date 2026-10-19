using GameFrameX.Entity.Tenant.Game.Release;

namespace GameFrameX.Application.Game.App.GameAppAssetPackageVersionHistory.Dto;

/// <summary>
/// 资源包版本记录输出参数
/// </summary>
public class GameAppAssetPackageVersionHistoryResponse : IBasePageResponse<TenantGameAppAssetPackageVersionHistoryEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 资源包版本Id
    /// </summary>
    public long VersionId { get; set; }

    /// <summary>
    /// 旧资源版本
    /// </summary>
    public string OldVersion { get; set; } = null!;

    /// <summary>
    /// 新资源版本
    /// </summary>
    public string ResourceVersion { get; set; } = null!;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 行版本号（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }
}