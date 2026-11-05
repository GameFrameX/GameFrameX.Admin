namespace GameFrameX.Application.ServerCenter.GameAreaProvision.Dto;

/// <summary>开服向导CreateRequest契约。</summary>
public sealed class GameAreaProvisionCreateRequest
{
    public long TemplateId { get; set; }
    public long ServerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HttpManageUrl { get; set; } = string.Empty;
    public string TcpSocketUrl { get; set; } = string.Empty;
    public string WebSocketUrl { get; set; } = string.Empty;
    public string ServerParameters { get; set; } = string.Empty;
}
