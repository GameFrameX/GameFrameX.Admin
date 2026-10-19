namespace GameFrameX.Core.BaseInputOutput;

/// <summary>
/// 包含启用状态的通用字段子集基类
/// </summary>
public class BaseEnabledRequest
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public virtual bool? IsEnabled { get; set; }
}