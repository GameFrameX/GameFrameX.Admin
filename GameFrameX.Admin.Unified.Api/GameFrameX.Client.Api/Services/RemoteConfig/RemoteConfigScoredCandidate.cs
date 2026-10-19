using GameFrameX.Entity.Tenant.Game.RemoteConfig;

namespace GameFrameX.Client.Api.Services.RemoteConfig;

/// <summary>
/// 远程配置候选记录与作用域精确度。
/// </summary>
/// <param name="Record">候选远程配置记录。</param>
/// <param name="Score">作用域精确度分数。</param>
public sealed record RemoteConfigScoredCandidate(TenantGameAppRemoteConfigEntity Record, int Score);