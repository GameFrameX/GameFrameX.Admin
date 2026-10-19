using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Job;

[AdminEnum]
public enum PersistenceBehavior
{
    Appended = 0,
    Updated = 1,
    Removed = 2,
}