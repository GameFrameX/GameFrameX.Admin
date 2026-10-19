namespace GameFrameX.Core.Service.PlatformLogging.Dto;

public sealed class PlatformLoggingFileResponse
{
    public bool IsEnabled { get; set; } = true;

    public string FileName { get; set; } = "data/logs/{0:yyyyMMdd}_{1}.log";

    public bool Append { get; set; } = true;

    public long FileSizeLimitBytes { get; set; } = 10485760;

    public int MaxRollingFiles { get; set; } = 30;
}