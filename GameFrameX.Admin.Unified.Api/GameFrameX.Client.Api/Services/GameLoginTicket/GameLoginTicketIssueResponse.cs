namespace GameFrameX.Client.Api.Services.GameLoginTicket;

/// <summary>
/// Game 登录票据签发结果。ExpireTime 为 null 表示永不过期。
/// </summary>
public sealed record GameLoginTicketIssueResponse(string Ticket, long? ExpireTime, string Jti, string Kid);