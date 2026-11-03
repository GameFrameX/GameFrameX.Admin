namespace GameFrameX.Client.Api.Services.GameLoginTicket;

/// <summary>
/// Game 登录票据签发上下文。
/// </summary>
public sealed record GameLoginTicketIssueContext(long UserId, long PlayerId, long AreaId, string RouteTarget);