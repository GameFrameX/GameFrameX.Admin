using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Core.BaseInputOutput;

/// <summary>
/// 主键Id输入参数
/// </summary>
public class BaseIdRequest
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "Id必须大于0")]
    public virtual long Id { get; set; }
}