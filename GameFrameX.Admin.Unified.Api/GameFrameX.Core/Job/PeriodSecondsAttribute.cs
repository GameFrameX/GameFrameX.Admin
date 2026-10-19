namespace GameFrameX.Core.Job;

public sealed class PeriodSecondsAttribute : TriggerAttribute
{
    public PeriodSecondsAttribute(int seconds)
    {
        Seconds = seconds;
    }

    public int Seconds { get; }
}