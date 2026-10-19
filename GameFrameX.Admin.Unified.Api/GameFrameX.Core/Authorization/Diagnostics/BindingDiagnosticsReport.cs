namespace GameFrameX.Core.Authorization.Diagnostics;

/// <summary>
/// 诊断报告。
/// </summary>
public sealed class BindingDiagnosticsReport
{
    /// <summary>生成时间戳（UTC 秒）。</summary>
    public long GeneratedTime { get; set; }

    /// <summary>发现列表（确定性排序：Category → HttpMethod → RoutePath → DetailAsync）。</summary>
    public IList<BindingDriftFinding> Findings { get; set; } = new List<BindingDriftFinding>();

    /// <summary>按类别汇总（仅含 Count &gt; 0 的类别，按 Category 升序）。</summary>
    public IList<BindingDriftCategoryCount> Summary { get; set; } = new List<BindingDriftCategoryCount>();

    /// <summary>是否全一致（零漂移）。</summary>
    public bool IsClean
    {
        get { return Findings.Count == 0; }
    }
}