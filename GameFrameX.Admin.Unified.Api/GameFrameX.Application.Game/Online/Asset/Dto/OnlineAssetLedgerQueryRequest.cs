namespace GameFrameX.Application.Game.Online.Asset.Dto;

/// <summary>
/// Online 资产流水查询请求体（Admin → Online 线缆契约；mock 先行，联调以 vault 详表回写）。
/// <para>不含作用域字段——TenantId/AppId/ServerId 由 <c>OnlineServerClient</c> 按鉴权作用域注入，请求体不可覆盖。</para>
/// </summary>
public class OnlineAssetLedgerQueryRequest
{
    /// <summary>
    /// 目标玩家 Id。
    /// </summary>
    public string PlayerId { get; set; } = null!;

    /// <summary>
    /// 货币或道具类型过滤（null = 不过滤）。
    /// </summary>
    public string? CurrencyOrItemType { get; set; }

    /// <summary>
    /// 时间窗起点（Unix 秒；null = 不限）。
    /// </summary>
    public long? StartTime { get; set; }

    /// <summary>
    /// 时间窗终点（Unix 秒；null = 不限）。
    /// </summary>
    public long? EndTime { get; set; }

    /// <summary>
    /// 游标（首页不传）。
    /// </summary>
    public string? Cursor { get; set; }

    /// <summary>
    /// 页大小（1～100）。
    /// </summary>
    public int PageSize { get; set; } = 20;
}
