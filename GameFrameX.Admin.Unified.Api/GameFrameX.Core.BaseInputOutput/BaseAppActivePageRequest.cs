namespace GameFrameX.Core.BaseInputOutput;

/// <summary>
/// 按游戏应用标识与启用状态筛选的分页查询输入参数
/// </summary>
public class BaseAppActivePageRequest : BasePageRequest
{
    /// <summary>
    /// 游戏应用标识
    /// </summary>
    public virtual long? AppId { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public virtual bool? IsActive { get; set; }
}