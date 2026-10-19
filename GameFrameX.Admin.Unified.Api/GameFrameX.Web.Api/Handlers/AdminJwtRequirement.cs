using Microsoft.AspNetCore.Authorization;

namespace GameFrameX.Web.Api.Handlers;

/// <summary>
/// Admin API 权限要求。
/// </summary>
public sealed class AdminJwtRequirement : IAuthorizationRequirement
{
}