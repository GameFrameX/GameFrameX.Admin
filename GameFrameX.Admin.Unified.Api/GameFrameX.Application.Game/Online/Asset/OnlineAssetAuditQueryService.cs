using System.ComponentModel;
using GameFrameX.Application.Game.Online.Asset.Dto;
using GameFrameX.Application.Game.ServerManager.Online;

namespace GameFrameX.Application.Game.Online.Asset;

/// <summary>
/// Online 资产审计查询服务。路由前缀 /api/onlineAssetAuditQuery（change C340 / vault:C4 S3.7）。
/// <para>命令审计行（操作者/命令类型/目标玩家/时间窗）实时透传 Online；Admin 侧审计另有
/// MonitorLogging 全 action 自动落库（TenantLogOperationEntity），本服务覆盖「按 Online 命令语义反查」的展示半边。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 134)]
public class OnlineAssetAuditQueryService : IAdminApiController
{
    /// <summary>
    /// 资产审计查询 action 名（mock 先行，联调以 vault 详表为准）。
    /// </summary>
    private const string QueryAssetAuditAction = "query_asset_audit";

    private readonly IOnlineServerClient _onlineServerClient;
    private readonly OnlineScopeContextFactory _scopeContextFactory;

    /// <summary>
    /// Online 资产审计查询服务构造函数。
    /// </summary>
    public OnlineAssetAuditQueryService(
        IOnlineServerClient onlineServerClient,
        OnlineScopeContextFactory scopeContextFactory)
    {
        _onlineServerClient = onlineServerClient;
        _scopeContextFactory = scopeContextFactory;
    }

    /// <summary>
    /// 资产审计分页查询：操作者 / 命令类型 / 目标玩家 / 时间窗过滤，游标翻页往返透传 Online。
    /// </summary>
    /// <param name="input">作用域（必填）+ 操作者/玩家/命令类型/时间窗过滤 + 游标。</param>
    /// <returns>审计行 + 下一页游标。</returns>
    [HttpPost(CrudActionRoutes.Page)]
    [ApiDescriptionSettings(Name = "Page")]
    [DisplayName("资产审计分页查询")]
    public async Task<OnlineAssetAuditPageResponse> PageAsync(PageOnlineAssetAuditRequest input)
    {
        var scope = await _scopeContextFactory.CreateAsync(input.AppId, input.ServerId, "asset-audit-page");
        var data = await _onlineServerClient.PostAsync<OnlineAssetAuditQueryResultResponse>(QueryAssetAuditAction, new OnlineAssetAuditQueryRequest
        {
            OperatorId = input.OperatorId,
            PlayerId = input.PlayerId,
            CommandType = input.CommandType,
            StartTime = input.StartTime,
            EndTime = input.EndTime,
            Cursor = input.Cursor,
            PageSize = input.PageSize,
        }, scope);

        return new OnlineAssetAuditPageResponse
        {
            Items = data.Items,
            NextCursor = data.NextCursor,
            HasMore = data.HasMore,
        };
    }
}
