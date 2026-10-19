namespace GameFrameX.Entity.Enum;

/// <summary>
/// 作业触发器运行状态。
/// </summary>
[AdminEnum]
public enum TriggerStatus
{
    Ready = 0,
    Running = 1,
    Pause = 2,
    Completed = 3,
}