namespace GameFrameX.Core.Service.PlatformLogging.Dto;

public sealed class PlatformLoggingDatabaseResponse
{
    public bool IsEnabled { get; set; } = true;

    public string MinimumLevel { get; set; } = "Information";
}