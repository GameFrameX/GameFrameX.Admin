using System.ComponentModel;
using GameFrameX.Application.Game.Online.Social.Dto;
using GameFrameX.Application.Game.ServerManager.Online;

namespace GameFrameX.Application.Game.Online.Social;

/// <summary>
/// Online 消息审计查询服务。路由前缀 /api/onlineMessageAuditQuery（change C350 / vault:C7）。
/// <para>查询条件实时透传 Online（Admin 不落消息镜像）；消息原文经 <see cref="OnlineMessageAuditDisplayMapper"/>
/// 集中脱敏后输出（vault:C2 S1.6：Online 侧已脱敏 + Admin 侧兜底脱敏双保险）。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 129)]
public class OnlineMessageAuditQueryService : IAdminApiController
{
    /// <summary>
    /// 消息审计查询 action 名（mock 先行，联调以 vault 详表为准）。
    /// </summary>
    private const string QueryChatMessagesAction = "query_chat_messages";

    private readonly IOnlineServerClient _onlineServerClient;
    private readonly OnlineScopeContextFactory _scopeContextFactory;

    /// <summary>
    /// Online 消息审计查询服务构造函数。
    /// </summary>
    public OnlineMessageAuditQueryService(
        IOnlineServerClient onlineServerClient,
        OnlineScopeContextFactory scopeContextFactory)
    {
        _onlineServerClient = onlineServerClient;
        _scopeContextFactory = scopeContextFactory;
    }

    /// <summary>
    /// 消息审计分页查询：过滤条件透传 Online，游标分页往返，展示前集中脱敏。
    /// </summary>
    /// <param name="input">作用域（必填）+ 频道/参与者/关键词/时间窗过滤 + 游标。</param>
    /// <returns>脱敏后的消息行 + 下一页游标。</returns>
    [HttpPost(CrudActionRoutes.Page)]
    [ApiDescriptionSettings(Name = "Page")]
    [DisplayName("消息审计分页查询")]
    public async Task<OnlineMessageAuditPageResponse> PageAsync(PageOnlineMessageAuditRequest input)
    {
        var scope = await _scopeContextFactory.CreateAsync(input.AppId, input.ServerId, "message-audit-page");
        var data = await _onlineServerClient.PostAsync<OnlineMessageAuditQueryResultResponse>(QueryChatMessagesAction, new OnlineMessageAuditQueryRequest
        {
            ChannelKind = input.ChannelKind,
            ParticipantPlayerId = input.ParticipantPlayerId,
            Keyword = input.Keyword,
            StartTime = input.StartTime,
            EndTime = input.EndTime,
            Cursor = input.Cursor,
            PageSize = input.PageSize,
        }, scope);

        return new OnlineMessageAuditPageResponse
        {
            Items = data.Items.Select(OnlineMessageAuditDisplayMapper.MapToItemResponse).ToList(),
            NextCursor = data.NextCursor,
            HasMore = data.HasMore,
        };
    }
}
