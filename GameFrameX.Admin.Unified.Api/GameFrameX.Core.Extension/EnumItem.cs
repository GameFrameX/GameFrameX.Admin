namespace GameFrameX.Core.Extension;

/// <summary>
/// 枚举列表项（枚举转列表输出的单项 DTO，非实体映射）
/// </summary>
public class EnumItem
{
    /// <summary>
    /// 枚举的描述
    /// </summary>
    public string Describe { get; set; } = string.Empty;

    /// <summary>
    /// 枚举名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 枚举对象的值
    /// </summary>
    public int Value { get; set; }
}