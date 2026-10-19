namespace GameFrameX.Core.Service.PlatformLogging.Dto;

public sealed class PlatformLoggingElasticSearchUpdateRequest
{
    public bool IsEnabled { get; set; } = false;

    public List<string> ServerUris { get; set; } = new();

    public string DefaultIndex { get; set; } = "adminnet";
}