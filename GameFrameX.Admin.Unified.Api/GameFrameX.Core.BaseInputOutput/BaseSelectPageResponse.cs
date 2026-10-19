namespace GameFrameX.Core.BaseInputOutput;

/// <summary>
/// 全局选择器查询输出参数
/// </summary>
public class BaseSelectPageResponse
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    public string Label { get; set; } = string.Empty;
}