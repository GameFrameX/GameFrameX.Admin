namespace GameFrameX.Application.CommerceCenter.GameCommodity.Dto;

/// <summary>
/// 游戏商品交易更新输入参数
/// </summary>
public class UpdateTenantGameCommodityEntityRequest : BaseUpdateRequest
{
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