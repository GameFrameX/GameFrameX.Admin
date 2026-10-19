using System.ComponentModel;
using GameFrameX.Application.Game.Online.Asset.Dto;
using GameFrameX.Application.Game.ServerManager.Online;

namespace GameFrameX.Application.Game.Online.Asset;

/// <summary>
/// Online 资产流水查询服务。路由前缀 /api/onlineAssetLedgerQuery（change C340 / vault:C4 S3.7）。
/// <para>查询实时透传 Online（Admin 不落资产镜像）；作用域经 <see cref="OnlineScopeContextFactory"/> 强制校验，
/// 游标分页往返透传（Cursor + HasMore，vault:C2 S1.2），不套页码偏移。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 132)]
public class OnlineAssetLedgerQueryService : IAdminApiController
{
    /// <summary>
    /// 资产流水查询 action 名（mock 先行，联调以 vault 详表为准）。
    /// </summary>
    private const string QueryWalletLedgerAction = "query_wallet_ledger";

    private readonly IOnlineServerClient _onlineServerClient;
    private readonly OnlineScopeContextFactory _scopeContextFactory;

    /// <summary>
    /// Online 资产流水查询服务构造函数。
    /// </summary>
    public OnlineAssetLedgerQueryService(
        IOnlineServerClient onlineServerClient,
        OnlineScopeContextFactory scopeContextFactory)
    {
        _onlineServerClient = onlineServerClient;
        _scopeContextFactory = scopeContextFactory;
    }

    /// <summary>
    /// 资产流水分页查询：玩家 + 货币/道具类型 + 时间窗过滤，游标翻页往返透传 Online。
    /// </summary>
    /// <param name="input">作用域（必填）+ 目标玩家 + 资产维度/时间窗过滤 + 游标。</param>
    /// <returns>流水行 + 下一页游标。</returns>
    [HttpPost(CrudActionRoutes.Page)]
    [ApiDescriptionSettings(Name = "Page")]
    [DisplayName("资产流水分页查询")]
    public async Task<OnlineAssetLedgerPageResponse> PageAsync(PageOnlineAssetLedgerRequest input)
    {
        var scope = await _scopeContextFactory.CreateAsync(input.AppId, input.ServerId, "asset-ledger-page");
        var data = await _onlineServerClient.PostAsync<OnlineAssetLedgerQueryResultResponse>(QueryWalletLedgerAction, new OnlineAssetLedgerQueryRequest
        {
            PlayerId = input.PlayerId,
            CurrencyOrItemType = input.CurrencyOrItemType,
            StartTime = input.StartTime,
            EndTime = input.EndTime,
            Cursor = input.Cursor,
            PageSize = input.PageSize,
        }, scope);

        return new OnlineAssetLedgerPageResponse
        {
            Items = data.Items,
            NextCursor = data.NextCursor,
            HasMore = data.HasMore,
        };
    }
}
