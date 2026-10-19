using GameFrameX.Entity.Tenant.Game.RemoteConfig;

namespace GameFrameX.Application.Game.App.GameAppRemoteConfigKey.Dto;

/// <summary>
/// 远程配置内容键定义输出参数
/// </summary>
public class GameAppRemoteConfigKeyResponse : IBasePageResponse<TenantGameAppRemoteConfigKeyEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// JSON key 名称
    /// </summary>
    public string KeyName { get; set; } = null!;

    /// <summary>
    /// 显示名称
    /// </summary>
    public string DisplayName { get; set; } = null!;

    /// <summary>
    /// 值类型：string、bool、long
    /// </summary>
    public string ValueType { get; set; } = "string";

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 排序序号
    /// </summary>
    public int SortOrder { get; set; } = 0;

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