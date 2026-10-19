using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Core.BaseInputOutput;

/// <summary>
/// 带乐观锁版本的删除输入。
/// </summary>
public class BaseDeleteRequest : BaseIdRequest
{
    /// <summary>
    /// 客户端读取记录时的版本号，用于阻止删除已被其他请求修改的记录。
    /// </summary>
    [Required(ErrorMessage = "版本号不能为空")]
    [Range(0, long.MaxValue, ErrorMessage = "版本号不能小于0")]
    public long? RowVersion { get; set; }
}