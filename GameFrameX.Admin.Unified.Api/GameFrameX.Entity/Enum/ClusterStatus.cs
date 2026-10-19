namespace GameFrameX.Entity.Enum;

/// <summary>
/// 作业集群节点状态。
/// </summary>
[AdminEnum]
public enum ClusterStatus
{
    Waiting = 0,
    Working = 1,
    Crashed = 2,
}