namespace GameFrameX.Core.BaseInputOutput;

/// <summary>
/// 包含名称、描述、创建人、更新人的基础输入参数
/// </summary>
public class BaseNamedDescriptionRequest : BaseAddRequest
{
    /// <summary>
    /// 名称
    /// </summary>
    public virtual string? Name { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public virtual string? Description { get; set; }

    /// <summary>
    /// 创建人姓名
    /// </summary>
    public virtual string? CreatedName { get; set; }

    /// <summary>
    /// 更新人姓名
    /// </summary>
    public virtual string? UpdatedName { get; set; }
}