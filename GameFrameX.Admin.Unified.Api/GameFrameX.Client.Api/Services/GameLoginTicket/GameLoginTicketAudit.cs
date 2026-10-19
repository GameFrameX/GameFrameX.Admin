namespace GameFrameX.Client.Api.Services.GameLoginTicket;

/// <summary>
/// Game 登录票据审计模型。
/// </summary>
public sealed record GameLoginTicketAudit(long TenantId, long AppId, long UserId, long PlayerId, long AreaId, string RouteTarget, string Jti, bool Succeeded, string Reason, long CreatedTime);