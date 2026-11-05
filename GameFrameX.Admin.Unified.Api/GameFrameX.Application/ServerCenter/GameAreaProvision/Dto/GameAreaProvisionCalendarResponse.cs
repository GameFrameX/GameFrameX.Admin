namespace GameFrameX.Application.ServerCenter.GameAreaProvision.Dto;

/// <summary>开服向导CalendarResponse契约。</summary>
public sealed class GameAreaProvisionCalendarResponse
{
    public long ServerId { get; set; }
    public bool Registered { get; set; }
    public long OpenedAtUnixMilliseconds { get; set; }
    public long MergedIntoServerId { get; set; }
    public int OpenServerDay { get; set; }
}
