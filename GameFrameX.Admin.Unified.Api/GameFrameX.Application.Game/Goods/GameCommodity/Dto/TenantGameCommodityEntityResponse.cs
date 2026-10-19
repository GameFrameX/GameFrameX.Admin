namespace GameFrameX.Application.Game.Goods.GameCommodity.Dto;

/// <summary>
/// 游戏商品交易输出参数
/// </summary>
public class TenantGameCommodityEntityResponse : IBasePageResponse<Entity.Tenant.Game.TenantGameCommodityEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用Id。运营内容应用作用域（TenantId + AppId），必须 &gt; 0。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// Status
    ///</summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// 类别Id（出站按 Id 后缀契约字符串化，前端与目录字典严格相等匹配不受影响）
    ///</summary>
    public long CategoryId { get; set; }

    /// <summary>
    ///Cost
    ///</summary>
    public bool Cost { get; set; }

    /// <summary>
    ///Number
    ///</summary>
    public long Number { get; set; }

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