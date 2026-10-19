namespace GameFrameX.Client.Api.Services.GameLoginTicket;

/// <summary>
/// Game 登录票据公钥输出。
/// </summary>
public sealed record GameLoginTicketPublicKey(string Kid, string Algorithm, string PublicKeyPem, long? NotBeforeTime, long? ExpireTime);