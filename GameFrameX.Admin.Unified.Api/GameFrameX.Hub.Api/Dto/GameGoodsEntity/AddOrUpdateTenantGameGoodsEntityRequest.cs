using System.ComponentModel.DataAnnotations;
using GameFrameX.Core.BaseInputOutput;

namespace GameFrameX.Hub.Api.Dto.GameGoodsEntity;

/// <summary>
/// 游戏资源增加或更新输入参数
/// </summary>
public class AddOrUpdateTenantGameGoodsEntityRequest : BaseAddRequest
{
    /// <summary>
    /// 配置ID
    /// </summary>
    [Required(ErrorMessage = "配置ID不能为空")]
    public int ConfigId { get; set; }

    /// <summary>
    /// 类别id
    /// </summary>
    [Required(ErrorMessage = "类别id不能为空")]
    public long ClassifyId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    [Required(ErrorMessage = "详细描述不能为空")]
    public string Description { get; set; } = null!;

    /// <summary>
    /// 是否支持星级
    /// </summary>
    public bool? Star { get; set; }
}