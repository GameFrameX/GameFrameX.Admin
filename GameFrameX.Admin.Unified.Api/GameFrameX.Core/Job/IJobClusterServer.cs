namespace GameFrameX.Core.Job;

/// <summary>
/// 调度集群状态回调服务。
/// </summary>
public interface IJobClusterServer
{
    void Start(JobClusterContext context);
    Task WaitingForAsync(JobClusterContext context);
    void Stop(JobClusterContext context);
    void Crash(JobClusterContext context);
}