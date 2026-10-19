namespace GameFrameX.Core.Job;

/// <summary>
/// 调度集群上下文。
/// </summary>
public sealed class JobClusterContext
{
    public JobClusterContext(string clusterId)
    {
        ClusterId = clusterId;
    }

    public string ClusterId { get; }
}