using System.ComponentModel;
using GameFrameX.Application.Game.Online.Asset.Dto;
using GameFrameX.Application.Game.ServerManager.Online;

namespace GameFrameX.Application.Game.Online.Asset;

/// <summary>
/// Online 资产交易详情查询服务。路由前缀 /api/onlineAssetTransactionQuery（change C340 / vault:C4 S3.7、VC-3.14）。
/// <para>按 TransactionId 反查交易全字段（IdempotencyKey、来源、原因、业务单号、操作者、Grant 明细、时间）；
/// Admin 端点为 GET + <c>[FromQuery]</c> 复杂查询对象（C257 §4），Online 侧查询经 POST 通道携带 body。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 133)]
public class OnlineAssetTransactionQueryService : IAdminApiController
{
    /// <summary>
    /// 交易详情查询 action 名（mock 先行，联调以 vault 详表为准）。
    /// </summary>
    private const string QueryTransactionDetailAction = "query_transaction_detail";

    private readonly IOnlineServerClient _onlineServerClient;
    private readonly OnlineScopeContextFactory _scopeContextFactory;

    /// <summary>
    /// Online 资产交易详情查询服务构造函数。
    /// </summary>
    public OnlineAssetTransactionQueryService(
        IOnlineServerClient onlineServerClient,
        OnlineScopeContextFactory scopeContextFactory)
    {
        _onlineServerClient = onlineServerClient;
        _scopeContextFactory = scopeContextFactory;
    }

    /// <summary>
    /// 交易详情查询：按 TransactionId 返回交易/Grant 全字段（变更来源完整可查的 Admin 展示半边）。
    /// </summary>
    /// <param name="input">作用域（必填）+ 交易 Id（GET 查询对象，[FromQuery] 绑定）。</param>
    /// <returns>交易详情（含 Grant 明细列表）。</returns>
    [HttpGet(CrudActionRoutes.Detail)]
    [ApiDescriptionSettings(Name = "Detail")]
    [DisplayName("资产交易详情查询")]
    public async Task<OnlineAssetTransactionDetailResponse> DetailAsync([FromQuery] OnlineAssetTransactionDetailRequest input)
    {
        if (string.IsNullOrWhiteSpace(input.TransactionId))
        {
            // DTO [Required] 之外的兜底（GET 查询对象绑定不走 ApiController 自动 400）。
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = "TransactionId", });
        }

        var scope = await _scopeContextFactory.CreateAsync(input.AppId, input.ServerId, "asset-transaction-detail");
        return await _onlineServerClient.PostAsync<OnlineAssetTransactionDetailResponse>(QueryTransactionDetailAction, new OnlineAssetTransactionDetailQueryRequest
        {
            TransactionId = input.TransactionId,
        }, scope);
    }
}
