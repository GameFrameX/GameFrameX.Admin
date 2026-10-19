using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Core.BaseInputOutput;

/// <summary>
/// 包含名称、描述和是否默认的增加输入参数
/// </summary>
public class BaseIsDefaultNamedAddRequest : BaseNamedDescriptionRequest
{
    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public override string? Name { get; set; }

    /// <summary>
    /// 是否默认
    /// </summary>
    public bool IsDefault { get; set; }
}