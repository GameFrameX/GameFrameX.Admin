namespace GameFrameX.Client.Api.Services.Auth;

/// <summary>
/// 登录期 per-session 会话密钥签发结果（change C7 phase2-so-account-token）。
/// <para>Kid 进客户端加密信封 JSON 的 kid 字段；Secret 为 64 位小写 hex 明文，仅在登录响应一次性回显；ExpireTime 与 AccessToken 到期时间对齐。</para>
/// </summary>
public sealed record ClientApiSessionKeyIssueResponse(string Kid, string Secret, long ExpireTime);