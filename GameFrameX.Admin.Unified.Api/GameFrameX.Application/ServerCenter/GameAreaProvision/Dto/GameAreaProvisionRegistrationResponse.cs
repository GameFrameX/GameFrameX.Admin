namespace GameFrameX.Application.ServerCenter.GameAreaProvision.Dto;

/// <summary>开服向导RegistrationResponse契约。</summary>
public sealed class GameAreaProvisionRegistrationResponse
{
    public long ServerId { get; set; }
    public long OpenedAtUnixMilliseconds { get; set; }
    public bool FirstRegistration { get; set; }
}
