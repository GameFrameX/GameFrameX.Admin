namespace GameFrameX.Application.Game.Online.Social.Dto;

/// <summary>
/// Online 联动命令请求体（Admin → Online 线缆契约；属性 PascalCase 为 mock 先行口径，联调以 vault 详表回写）。
/// <para>作用域三元组（TenantId/AppId/ServerId）由 OnlineServerClient 在传输层并入请求体顶层，不在此声明。</para>
/// </summary>
public class OnlineLinkCommandRequest
{
    /// <summary>
    /// 处罚事实源行 Id（Online 侧审计回链）。
    /// </summary>
    public long PenaltyId { get; set; }

    /// <summary>
    /// 目标玩家 Id（处罚 TargetId）。
    /// </summary>
    public string PlayerId { get; set; } = null!;

    /// <summary>
    /// 幂等键（同（处罚 + 命令 + 区服）组合重复调用返回首次结果，vault:C2 S1.5）。
    /// </summary>
    public string IdempotencyKey { get; set; } = null!;
}
