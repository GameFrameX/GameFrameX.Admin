namespace GameFrameX.Application.ServerCenter.GameAreaProvision.Dto;

/// <summary>开服向导PreviewResponse契约。</summary>
public sealed class GameAreaProvisionPreviewResponse
{
    public long TemplateId { get; set; }
    public Dictionary<string, string?> ClonedFields { get; set; } = new();
    public string[] RequiredFields { get; set; } = Array.Empty<string>();
}
