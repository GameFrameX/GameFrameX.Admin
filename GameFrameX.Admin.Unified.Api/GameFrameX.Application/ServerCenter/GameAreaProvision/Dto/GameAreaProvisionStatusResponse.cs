namespace GameFrameX.Application.ServerCenter.GameAreaProvision.Dto;

/// <summary>开服向导StatusResponse契约。</summary>
public sealed class GameAreaProvisionStatusResponse
{
    public long ServerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool RecordCreated { get; set; }
    public bool ContainerCreated { get; set; }
    public bool Registered { get; set; }
    public bool IsEnabled { get; set; }
    public long OpenedAtUnixMilliseconds { get; set; }
    public long MergedIntoServerId { get; set; }
}
