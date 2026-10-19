using System.ComponentModel;
using GameFrameX.Application.Game.Online.Asset.Dto;
using GameFrameX.Application.Game.ServerManager.Online;

namespace GameFrameX.Application.Game.Online.Asset;

/// <summary>
/// Online 资产人工发放/撤销受限命令服务（高危；权限码 onlineAssetGrantCommand:grant / :revoke 仅同步 TenantAdmin）。
/// <para>
/// 路由前缀 /api/onlineAssetGrantCommand（change C340 / vault:C4 S3.6「Admin 人工发放或撤销」六类来源之一）。
/// 经 <see cref="IOnlineServerClient"/> 调 Online Grant API——Admin 侧零直写（守护测试 OnlineAssetCommandWhitelistTests）；
/// <c>Reason</c> 必填进审计；幂等键由服务端按业务单号规则生成（见 <see cref="ResolveBusinessOrderNumber"/>）。
/// </para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 135)]
public class OnlineAssetGrantCommandService : IAdminApiController
{
    /// <summary>
    /// 资产发放命令的 Online 管理 action 名（vault:C4 统一发放入口；mock 先行，联调以 vault 详表为准）。
    /// </summary>
    private const string GrantAssetAction = "grant_asset";

    /// <summary>
    /// 资产撤销命令的 Online 管理 action 名（vault:C4 统一撤销入口；mock 先行，联调以 vault 详表为准）。
    /// </summary>
    private const string RevokeAssetAction = "revoke_asset";

    /// <summary>
    /// 服务端生成的发放业务单号前缀。
    /// </summary>
    private const string GrantOrderNumberPrefix = "GFX-GRANT";

    /// <summary>
    /// 服务端生成的撤销业务单号前缀。
    /// </summary>
    private const string RevokeOrderNumberPrefix = "GFX-REVOKE";

    private readonly IOnlineServerClient _onlineServerClient;
    private readonly OnlineScopeContextFactory _scopeContextFactory;

    /// <summary>
    /// Online 资产受限命令服务构造函数。
    /// </summary>
    public OnlineAssetGrantCommandService(
        IOnlineServerClient onlineServerClient,
        OnlineScopeContextFactory scopeContextFactory)
    {
        _onlineServerClient = onlineServerClient;
        _scopeContextFactory = scopeContextFactory;
    }

    /// <summary>
    /// 人工发放资产（高危端点）：明细 + 必填理由经 Online Grant API 执行，响应透传 Online 首次结果。
    /// </summary>
    /// <param name="input">作用域（必填）+ 目标玩家 + 发放明细 + 理由（必填）+ 可选业务单号。</param>
    /// <returns>Online 首次执行结果（交易 Id / 幂等键 / 状态 / 时间）。</returns>
    [HttpPost("grant")]
    [ApiDescriptionSettings(Name = "Grant")]
    [DisplayName("Online资产人工发放")]
    public async Task<OnlineAssetGrantAckResponse> GrantAsync(OnlineAssetGrantRequest input)
    {
        ValidateReason(input.Reason);
        ValidatePlayerId(input.PlayerId);
        if (input.GrantItems == null || input.GrantItems.Count == 0)
        {
            // 发放必须至少携带一条明细（DTO [Required] 之外的兜底：集合空值语义）。
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "GrantItems", });
        }

        foreach (var item in input.GrantItems)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.ItemOrCurrencyType) || item.Quantity <= 0)
            {
                throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "GrantItems.ItemOrCurrencyType/Quantity", });
            }
        }

        var scope = await _scopeContextFactory.CreateAsync(input.AppId, input.ServerId, $"asset-grant:{input.PlayerId}");
        var businessOrderNumber = ResolveBusinessOrderNumber(input.BusinessOrderNumber, GrantOrderNumberPrefix);
        return await _onlineServerClient.PostAsync<OnlineAssetGrantAckResponse>(GrantAssetAction, new OnlineAssetGrantCommandRequest
        {
            PlayerId = input.PlayerId,
            GrantItems = input.GrantItems,
            Reason = input.Reason,
            BusinessOrderNumber = businessOrderNumber,
            IdempotencyKey = businessOrderNumber,
        }, scope);
    }

    /// <summary>
    /// 人工撤销资产（高危端点）：按目标交易 Id 撤销已生效变更，经 Online Grant/Transaction API 执行。
    /// </summary>
    /// <param name="input">作用域（必填）+ 目标玩家 + 被撤销交易 Id + 理由（必填）+ 可选业务单号。</param>
    /// <returns>Online 首次执行结果。</returns>
    [HttpPost("revoke")]
    [ApiDescriptionSettings(Name = "Revoke")]
    [DisplayName("Online资产人工撤销")]
    public async Task<OnlineAssetGrantAckResponse> RevokeAsync(OnlineAssetRevokeRequest input)
    {
        ValidateReason(input.Reason);
        ValidatePlayerId(input.PlayerId);
        if (string.IsNullOrWhiteSpace(input.TransactionId))
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "TransactionId", });
        }

        var scope = await _scopeContextFactory.CreateAsync(input.AppId, input.ServerId, $"asset-revoke:{input.TransactionId}");
        var businessOrderNumber = ResolveBusinessOrderNumber(input.BusinessOrderNumber, RevokeOrderNumberPrefix);
        return await _onlineServerClient.PostAsync<OnlineAssetGrantAckResponse>(RevokeAssetAction, new OnlineAssetRevokeCommandRequest
        {
            PlayerId = input.PlayerId,
            TransactionId = input.TransactionId,
            Reason = input.Reason,
            BusinessOrderNumber = businessOrderNumber,
            IdempotencyKey = businessOrderNumber,
        }, scope);
    }

    /// <summary>
    /// 撤销原因必填校验（进审计；DTO [Required] 之外的兜底）。
    /// </summary>
    private static void ValidateReason(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "Reason", });
        }
    }

    /// <summary>
    /// 目标玩家必填校验（DTO [Required] 之外的兜底）。
    /// </summary>
    private static void ValidatePlayerId(string playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId))
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "PlayerId", });
        }
    }

    /// <summary>
    /// 幂等键（业务单号）解析——Admin 侧资产命令的唯一幂等语义入口。
    /// <para>
    /// 幂等键语义（vault:C4 S3.5 / VC-1.4）：调用方携带 <c>BusinessOrderNumber</c> 时以其为幂等键——
    /// 同一业务单号的重复调用（网络重试 / 运营重复点击）在 Online 侧只生效一次，并返回首次执行结果，
    /// 不产生第二笔资产变更；未提供时服务端按业务单号规则生成
    /// <c>{prefix}-{yyyyMMddHHmmss}-{Guid 前 8 位}</c> 形态的新单号（发放 GFX-GRANT- / 撤销 GFX-REVOKE-），
    /// 每次调用都是新业务意图（不构成对先前命令的重放）。幂等键随命令下发（IdempotencyKey = BusinessOrderNumber），
    /// 调用方拿到的回执中 IdempotencyKey 即本键。
    /// </para>
    /// <para>
    /// 6xxx 冲突行为：Online 侧判定「同幂等键但业务参数不一致」等冲突场景时返回 6xxx 协议码，
    /// 经 <see cref="OnlineServerClient"/> 段级映射为镜像码 <see cref="OnlineErrorCode.DuplicateRequest"/>（32500001）
    /// 抛 <c>BusinessError</c>，原始协议码随 errors 透出；调用方收到该错误应视为「该业务单号已绑定首次结果」，
    /// 以同单号反查交易详情获取首次结果，而不是换单号重发命令。
    /// </para>
    /// </summary>
    /// <param name="requested">调用方携带的业务单号（空白视为未提供）。</param>
    /// <param name="prefix">服务端生成时的单号前缀（发放/撤销区分）。</param>
    /// <returns>幂等键（= 业务单号）。</returns>
    private static string ResolveBusinessOrderNumber(string? requested, string prefix)
    {
        if (!string.IsNullOrWhiteSpace(requested))
        {
            return requested;
        }

        return $"{prefix}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N").Substring(0, 8)}";
    }
}
