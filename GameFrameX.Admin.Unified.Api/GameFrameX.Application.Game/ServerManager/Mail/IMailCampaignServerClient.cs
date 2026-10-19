// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

namespace GameFrameX.Application.Game.ServerManager.Mail;

/// <summary>
/// 运营邮件 Campaign Server 接口客户端抽象。抽象接口便于单元测试注入假实现，验证 Admin 服务状态迁移。
/// </summary>
public interface IMailCampaignServerClient
{
    /// <summary>
    /// 发布 Campaign（写入不可变快照，返回 CampaignId + Version）。
    /// </summary>
    /// <param name="area">目标区服 <c>TenantGameAreaEntity</c>，提供 <c>HttpManageUrl</c> 根地址与 <c>FulfillmentApiPrefix</c> 能力前缀。</param>
    Task<PublishMailCampaignResponse> PublishAsync(TenantGameAreaEntity area, PublishMailCampaignRequest request);

    /// <summary>
    /// 预览 / 校验 Campaign（不落库，返回预估命中玩家数）。
    /// </summary>
    /// <param name="area">目标区服 <c>TenantGameAreaEntity</c>，提供 <c>HttpManageUrl</c> 根地址与 <c>FulfillmentApiPrefix</c> 能力前缀。</param>
    Task<PreviewMailCampaignResponse> PreviewAsync(TenantGameAreaEntity area, PreviewMailCampaignRequest request);

    /// <summary>
    /// 撤回 Campaign（撤回不可逆 B3）。
    /// </summary>
    /// <param name="area">目标区服 <c>TenantGameAreaEntity</c>，提供 <c>HttpManageUrl</c> 根地址与 <c>FulfillmentApiPrefix</c> 能力前缀。</param>
    Task<RevokeMailCampaignResponse> RevokeAsync(TenantGameAreaEntity area, RevokeMailCampaignRequest request);

    /// <summary>
    /// 查询 Campaign 发布状态。
    /// </summary>
    /// <param name="area">目标区服 <c>TenantGameAreaEntity</c>，提供 <c>HttpManageUrl</c> 根地址与 <c>FulfillmentApiPrefix</c> 能力前缀。</param>
    Task<QueryMailCampaignResponse> QueryAsync(TenantGameAreaEntity area, QueryMailCampaignRequest request);
}