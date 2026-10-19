namespace GameFrameX.Core.Service.PlatformLogging.Dto;

public sealed class PlatformLoggingFileUpdateRequest
{
    public bool IsEnabled { get; set; } = true;

    [Range(1, 4096)] public string FileName { get; set; } = "data/logs/{0:yyyyMMdd}_{1}.log";

    public bool Append { get; set; } = true;

    [Range(1024, 1073741824)] public long FileSizeLimitBytes { get; set; } = 10485760;

    [Range(1, 365)] public int MaxRollingFiles { get; set; } = 30;
}