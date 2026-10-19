namespace GameFrameX.Client.Api.GameRole.Dto;

public sealed class PlayerChangeNameResponse
{
    public string Name { get; set; } = null!;

    public int CurrentTimes { get; set; }
}