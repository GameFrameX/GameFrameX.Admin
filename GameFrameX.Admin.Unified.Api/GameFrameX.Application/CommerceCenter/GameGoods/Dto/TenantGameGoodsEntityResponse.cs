namespace GameFrameX.Application.CommerceCenter.GameGoods.Dto;

/// <summary>
/// 游戏资源输出参数
/// </summary>
public class TenantGameGoodsEntityResponse : IBasePageResponse<Entity.Tenant.Game.TenantGameGoodsEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// 配置ID
    ///</summary>
    public int ConfigId { get; set; }

    /// <summary>
    ///类别id
    ///</summary>
    public long ClassifyId { get; set; }

    /// <summary>
    /// 是否支持星级
    /// </summary>
    public bool? Star { get; set; }

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