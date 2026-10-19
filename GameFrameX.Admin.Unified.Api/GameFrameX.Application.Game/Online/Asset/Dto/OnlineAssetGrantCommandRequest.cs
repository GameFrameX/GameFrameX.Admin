namespace GameFrameX.Application.Game.Online.Asset.Dto;

/// <summary>
/// Online 资产发放命令请求体（Admin → Online 线缆契约；mock 先行，联调以 vault 详表回写）。
/// <para>不含作用域字段——由 <c>OnlineServerClient</c> 按鉴权作用域注入；<c>IdempotencyKey</c> 由 Admin 服务端按业务单号规则生成后随命令下发。</para>
/// </summary>
public class OnlineAssetGrantCommandRequest
{
    /// <summary>
    /// 目标玩家 Id。
    /// </summary>
    public string PlayerId { get; set; } = null!;

    /// <summary>
    /// 发放明细。
    /// </summary>
    public List<OnlineAssetGrantItemRequest> GrantItems { get; set; } = new();

    /// <summary>
    /// 发放原因（进 Online 审计）。
    /// </summary>
    public string Reason { get; set; } = null!;

    /// <summary>
    /// 业务单号（幂等键的语义载体，同单号重试只生效一次）。
    /// </summary>
    public string BusinessOrderNumber { get; set; } = null!;

    /// <summary>
    /// 幂等键（= BusinessOrderNumber；Online 侧按 vault:C2 S1.5 契约以此去重，重复请求返回首次结果）。
    /// </summary>
    public string IdempotencyKey { get; set; } = null!;
}
