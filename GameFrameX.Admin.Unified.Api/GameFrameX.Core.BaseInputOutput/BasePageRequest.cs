using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Core.BaseInputOutput;

/// <summary>
/// 全局分页查询输入参数
/// </summary>
public class BasePageRequest
{
    /// <summary>
    /// 关键字查询
    /// </summary>
    public string? SearchKey { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "当前页码必须大于0")]
    public virtual int Page { get; set; } = 1;

    /// <summary>
    /// 页码容量
    /// </summary>
    [Range(1, 100, ErrorMessage = "页码容量超过最大限制")]
    public virtual int PageSize { get; set; } = 20;

    /// <summary>
    /// 排序字段名（输出类型的属性名，忽略大小写）。
    /// <para>为空时按服务端默认排序（通常是创建时间降序）；传入不存在于输出类型的字段名时同样回退默认排序。</para>
    /// <para>与前端表格排序事件的映射：列头点击事件应把当前列的 prop 名填入本字段，并按事件方向同步 <see cref="IsDescending"/>。</para>
    /// </summary>
    public virtual string OrderBy { get; set; } = string.Empty;

    /// <summary>
    /// 是否降序排序。
    /// <para>false（默认）= 升序，true = 降序；仅当 <see cref="OrderBy"/> 非空时生效。</para>
    /// <para>与前端表格排序事件的映射：ascending 事件填 false，descending 事件填 true。</para>
    /// </summary>
    public virtual bool IsDescending { get; set; } = false;
}