using GameFrameX.Entity.Tenant.Game.Release;

namespace GameFrameX.Application.Game.App.GameAppVersionDefinition.Dto;

/// <summary>
/// 版本管理输出参数
/// </summary>
public class GameAppVersionDefinitionPageResponse : IBasePageResponse<TenantGameAppVersionDefinitionEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 游戏应用Id。版本定义必须绑定具体应用，必须 &gt; 0。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    public string Description { get; set; } = null!;

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