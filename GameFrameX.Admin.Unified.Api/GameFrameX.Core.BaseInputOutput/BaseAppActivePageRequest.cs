namespace GameFrameX.Core.BaseInputOutput;

/// <summary>
/// 按启用状态筛选的分页查询输入参数
/// </summary>
public class BaseAppActivePageRequest : BasePageRequest
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public virtual bool? IsActive { get; set; }
}