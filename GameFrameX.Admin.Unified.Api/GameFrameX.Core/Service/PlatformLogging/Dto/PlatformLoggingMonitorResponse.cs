namespace GameFrameX.Core.Service.PlatformLogging.Dto;

public sealed class PlatformLoggingMonitorResponse
{
    public bool GlobalEnabled { get; set; } = true;

    public string BahLogLevel { get; set; } = "Information";

    public bool WithReturnValue { get; set; } = true;

    public int ReturnValueThreshold { get; set; } = 500;

    public string JsonBehavior { get; set; } = "OnlyJson";

    public bool JsonIndented { get; set; } = false;

    public bool UseUtcTimestamp { get; set; } = false;
}