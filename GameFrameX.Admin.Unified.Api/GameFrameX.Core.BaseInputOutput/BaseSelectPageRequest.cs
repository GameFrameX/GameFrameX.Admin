using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Core.BaseInputOutput;

/// <summary>
/// 全局选择器查询输入参数
/// </summary>
public class BaseSelectPageRequest : IBaseRequest
{
    /// <summary>
    /// 选择器默认返回数量
    /// </summary>
    public const int DefaultPageSize = 500;

    /// <summary>
    /// 选择器最大返回数量
    /// </summary>
    public const int MaxPageSize = 1000;

    /// <summary>
    /// 关键字查询
    /// </summary>
    public string? SearchKey { get; set; }



    /// <summary>
    /// 返回数量上限。默认 500，最大 1000。
    /// </summary>
    [Range(1, MaxPageSize, ErrorMessage = "返回数量必须在 1-1000 之间")]
    public int PageSize { get; set; } = DefaultPageSize;
}
