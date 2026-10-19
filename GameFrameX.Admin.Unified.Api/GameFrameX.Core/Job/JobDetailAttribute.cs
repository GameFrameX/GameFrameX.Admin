namespace GameFrameX.Core.Job;

[AttributeUsage(AttributeTargets.Class)]
public sealed class JobDetailAttribute : Attribute
{
    public JobDetailAttribute(string jobId)
    {
        JobId = jobId;
    }

    public string JobId { get; }
    public string? Description { get; set; }
    public string? GroupName { get; set; }
    public bool Concurrent { get; set; } = true;
}