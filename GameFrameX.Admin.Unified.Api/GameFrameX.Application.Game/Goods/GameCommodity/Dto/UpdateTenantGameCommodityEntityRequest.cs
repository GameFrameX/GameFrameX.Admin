namespace GameFrameX.Application.Game.Goods.GameCommodity.Dto;

/// <summary>
/// 游戏商品交易更新输入参数
/// </summary>
public class UpdateTenantGameCommodityEntityRequest : BaseUpdateRequest
{
    /// <summary>
    /// 应用Id。事实数据应用作用域，必须 &gt; 0。
    /// <para>过渡期：后台运营当前应用上下文尚未建立，由前端手填。</para>
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

    /// <summary>
    /// 创建者姓名
    /// </summary>
    [Required(ErrorMessage = "创建者姓名不能为空")]
    public string CreatedName { get; set; } = null!;

    /// <summary>
    /// 修改者姓名
    /// </summary>
    [Required(ErrorMessage = "修改者姓名不能为空")]
    public string UpdatedName { get; set; } = null!;
}