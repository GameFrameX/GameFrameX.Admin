namespace GameFrameX.Core.Service.PlatformLogging.Dto;

public sealed class PlatformLoggingDatabaseUpdateRequest
{
    public bool IsEnabled { get; set; } = true;

    public string MinimumLevel { get; set; } = "Information";
}