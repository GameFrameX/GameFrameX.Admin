namespace GameFrameX.Core.Job;

/// <summary>
/// 作业元数据。
/// </summary>
public sealed class JobDetail
{
    public string? JobId { get; set; }
    public string? GroupName { get; set; }
    public string? JobType { get; set; }
    public string? AssemblyName { get; set; }
    public string? Description { get; set; }
    public bool Concurrent { get; set; } = true;
    public bool IncludeAnnotations { get; set; }
    public string? Properties { get; set; } = "{}";
    public DateTime? UpdatedTime { get; set; }
}