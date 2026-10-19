namespace GameFrameX.Core.BaseInputOutput;

/// <summary>
/// 包含名称、描述和是否默认的更新输入参数
/// </summary>
public class BaseIsDefaultNamedUpdateRequest : BaseUpdateRequest
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
    /// 是否默认
    /// </summary>
    public bool IsDefault { get; set; }
}