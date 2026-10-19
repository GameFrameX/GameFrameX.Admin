// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

using GameFrameX.Entity.Enum;
using GameFrameX.Foundation.Http.Extension;
using GameFrameX.Foundation.Http.Normalization;

namespace GameFrameX.Application.Game.ServerManager.Mail;

/// <summary>
/// 运营邮件 Campaign Server 接口 HTTP 客户端。
/// <para>路由：POST <c>{HttpManageUrl}/{FulfillmentApiPrefix}/{snake_case_action}</c>，
/// action 名 <c>publish_mail_campaign</c>/<c>preview_mail_campaign</c>/<c>revoke_mail_campaign</c>/<c>query_mail_campaign</c>，
/// 均为 POST（与既有 <c>post_mail</c> 同前缀同算法）。</para>
/// <para>来源 change C150 §3 / C175：前缀不再读全局 <c>GameServerOptions.MailCampaignRoutePrefix</c>，改由区服 <c>FulfillmentApiPrefix</c> 经 <see cref="GameServerEndpointUrlBuilder"/> 按 <c>Fulfillment</c> 能力构造。</para>
/// <para>请求 / 响应 DTO 字段名逐字对齐 Server（跨仓库契约）。Server 响应走 <c>HttpJsonResultData<string></c> 信封，<see cref="HttpJsonResultExtensions.ToHttpJsonResult{T}"/> 解包。</para>
/// </summary>
public sealed class MailCampaignServerClient : IMailCampaignServerClient
{
    /// <summary>发布路由 action。</summary>
    public const string PublishAction = "publish_mail_campaign";

    /// <summary>预览路由 action。</summary>
    public const string PreviewAction = "preview_mail_campaign";

    /// <summary>撤回路由 action。</summary>
    public const string RevokeAction = "revoke_mail_campaign";

    /// <summary>查询路由 action。</summary>
    public const string QueryAction = "query_mail_campaign";

    private readonly HttpClient _httpClient;

    /// <summary>
    ///
    /// </summary>
    /// <param name="httpClient"></param>
    public MailCampaignServerClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<PublishMailCampaignResponse> PublishAsync(TenantGameAreaEntity area, PublishMailCampaignRequest request)
    {
        return await PostAsync<PublishMailCampaignRequest, PublishMailCampaignResponse>(area, PublishAction, request);
    }

    /// <inheritdoc />
    public async Task<PreviewMailCampaignResponse> PreviewAsync(TenantGameAreaEntity area, PreviewMailCampaignRequest request)
    {
        return await PostAsync<PreviewMailCampaignRequest, PreviewMailCampaignResponse>(area, PreviewAction, request);
    }

    /// <inheritdoc />
    public async Task<RevokeMailCampaignResponse> RevokeAsync(TenantGameAreaEntity area, RevokeMailCampaignRequest request)
    {
        return await PostAsync<RevokeMailCampaignRequest, RevokeMailCampaignResponse>(area, RevokeAction, request);
    }

    /// <inheritdoc />
    public async Task<QueryMailCampaignResponse> QueryAsync(TenantGameAreaEntity area, QueryMailCampaignRequest request)
    {
        return await PostAsync<QueryMailCampaignRequest, QueryMailCampaignResponse>(area, QueryAction, request);
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(TenantGameAreaEntity area, string action, TRequest request)
        where TResponse : class
    {
        var url = GameServerEndpointUrlBuilder.Build(area, GameServerEndpointKindEnum.Fulfillment, action);

        var content = await _httpClient.PostJsonToStringAsync(url, request);
        var resultData = content.ToHttpJsonResult<TResponse>();
        if (!resultData.IsSuccess)
        {
            throw new InvalidOperationException($"调用 Server 邮件 Campaign 接口失败：{resultData.Message}");
        }

        return resultData.Data;
    }
}