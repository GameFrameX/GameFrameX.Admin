using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Job;

[AdminEnum]
public enum ScheduleResult
{
    Succeed = 0,
    NotFound = 1,
    Failed = 2,
}