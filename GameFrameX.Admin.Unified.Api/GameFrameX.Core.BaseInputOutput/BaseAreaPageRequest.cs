namespace GameFrameX.Core.BaseInputOutput;

/// <summary>
/// 按区服标识筛选的分页查询输入参数
/// </summary>
public class BaseAreaPageRequest : BasePageRequest
{
    /// <summary>
    /// 区服标识
    /// </summary>
    public virtual long? AreaId { get; set; }
}