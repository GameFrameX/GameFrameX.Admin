using GameFrameX.Entity.Tenant.Game.Release;

namespace GameFrameX.Application.Game.App.GameAppAssetPackageVersion.Dto;

/// <summary>
/// 资源包版本输出参数
/// </summary>
public class GameAppAssetPackageVersionResponse : IBasePageResponse<TenantGameAppAssetPackageVersionEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用作用域，0 / null 表示租户默认配置
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 渠道
    /// </summary>
    public string Channel { get; set; } = null!;

    /// <summary>
    /// 平台
    /// </summary>
    public string Platform { get; set; } = null!;

    /// <summary>
    /// 游戏版本
    /// </summary>
    public string AppVersion { get; set; } = null!;

    /// <summary>
    /// 包名
    /// </summary>
    public string Package { get; set; } = null!;

    /// <summary>
    /// 语言
    /// </summary>
    public string Language { get; set; } = null!;

    /// <summary>
    /// 资源包名称
    /// </summary>
    public string AssetPackageName { get; set; } = null!;

    /// <summary>
    /// 资源下载根路径
    /// </summary>
    public string RootPath { get; set; } = null!;

    /// <summary>
    /// 资源版本
    /// </summary>
    public string ResourceVersion { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// 机器码分组id
    /// </summary>
    public long? DeviceGroupId { get; set; }

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