using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Core.BaseInputOutput;

/// <summary>
/// 基础更新输入接口约定基类
/// </summary>
/// <typeparam name="T"></typeparam>
public class BaseUpdateRequest : BaseAddRequest, ICrudUpdateRequest
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "主键Id必须大于0")]
    public long Id { get; set; }

    /// <summary>
    /// 客户端读取记录时的版本号，用于阻止过期数据覆盖较新的修改。
    /// </summary>
    [Required(ErrorMessage = "版本号不能为空")]
    [Range(0, long.MaxValue, ErrorMessage = "版本号不能小于0")]
    public long? RowVersion { get; set; }
}