namespace GameFrameX.Core.Authorization.Diagnostics;

/// <summary>
/// 类别计数汇总（用于报告 Summary）。
/// </summary>
public sealed class BindingDriftCategoryCount
{
    /// <summary>类别。</summary>
    public BindingDriftCategory Category { get; set; }

    /// <summary>严重级别。</summary>
    public BindingDriftSeverity Severity { get; set; }

    /// <summary>该类别发现条数。</summary>
    public int Count { get; set; }
}