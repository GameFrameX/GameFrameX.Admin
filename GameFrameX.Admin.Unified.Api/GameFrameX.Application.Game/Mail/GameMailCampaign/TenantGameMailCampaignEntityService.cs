// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任。

using System.ComponentModel;
using GameFrameX.Application.Game.ServerManager.Mail;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Mail.GameMailCampaign.Dto;
using GameFrameX.Application.Game.Online.LiveOps.Dispatch;
using GameFrameX.Application.Game.Online.LiveOps.Dispatch.Dto;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Game.Mail;
using GameFrameX.Entity.Game.Online;
using GameFrameX.Foundation.Json;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Mail.GameMailCampaign;

/// <summary>
/// 运营邮件 Campaign 草稿/发布/撤回管理服务。路由前缀 /api/tenantGameMailCampaignEntity（历史框架 去后缀 + lowerCamelCase）。
/// <para>状态机 B1/B3/B4：Draft 可自由增删改与发布重试；Published/Revoked 主体不可编辑/删除（B1），仅 Published 可撤回（撤回不可逆 B3）；Server 拒绝落 Failed。</para>
/// <para>与既有 <c>TenantGameMailEntityService</c>（直发 post_mail）是两套独立系统，不合并。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameMailCampaignEntityService
    : GenericCrudService<TenantGameMailCampaignEntity,
        AddTenantGameMailCampaignEntityRequest, UpdateTenantGameMailCampaignEntityRequest,
        TenantGameMailCampaignEntityResponse, PageTenantGameMailCampaignEntityRequest>
{
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> _gameAppRepository;
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGameAreaEntity> _gameAreaRepository;
    private readonly IMailCampaignServerClient _serverClient;
    private readonly IOnlineLiveOpsDispatchService _dispatchService;
    private readonly CurrentUserContext _currentUserContext;

    /// <summary>
    ///
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="gameAppRepository"></param>
    /// <param name="gameAreaRepository"></param>
    /// <param name="serverClient"></param>
    /// <param name="dispatchService"></param>
    /// <param name="currentUserContext"></param>
    public TenantGameMailCampaignEntityService(SqlSugarRepository<TenantGameMailCampaignEntity> repository,
        SqlSugarRepository<Entity.Tenant.Game.TenantGameAppEntity> gameAppRepository,
        SqlSugarRepository<Entity.Tenant.Game.TenantGameAreaEntity> gameAreaRepository,
        IMailCampaignServerClient serverClient,
        IOnlineLiveOpsDispatchService dispatchService,
        CurrentUserContext currentUserContext)
        : base(repository)
    {
        _gameAppRepository = gameAppRepository;
        _gameAreaRepository = gameAreaRepository;
        _serverClient = serverClient;
        _dispatchService = dispatchService;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// 分页查询：按 AppId / Name / MailType / PublishStatus / TemplateId / ServerCampaignId / AreaId 过滤。
    /// </summary>
    protected override ISugarQueryable<TenantGameMailCampaignEntityResponse> BuildPageQuery(PageTenantGameMailCampaignEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "运营邮件Campaign");
        return Repository.AsQueryable()
                         .Where(u => u.AppId == appId)
                         .WhereIF(input.AreaId.HasValue, u => u.AreaId == input.AreaId.Value)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .WhereIF(input.MailType.HasValue, u => u.MailType == input.MailType.Value)
                         .WhereIF(input.PublishStatus.HasValue, u => u.PublishStatus == input.PublishStatus.Value)
                         .WhereIF(input.TemplateId.HasValue, u => u.TemplateId == input.TemplateId.Value)
                         .WhereIF(input.ServerCampaignId.HasValue, u => u.ServerCampaignId == input.ServerCampaignId.Value)
                         .Select<TenantGameMailCampaignEntityResponse>();
    }

    /// <summary>
    /// 新增前：应用作用域 EnsureFactAppId（事实数据，AppId&gt;0）+ 校验应用归属当前租户 + 内容校验（多语言标题至少 1 项、附件数量&gt;0）。
    /// </summary>
    protected override async Task OnAddingAsync(AddTenantGameMailCampaignEntityRequest input)
    {
        AppScopeRules.EnsureFactAppId(input.AppId, "运营邮件Campaign");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
        ValidateDraftContent(input.TitlesJson, input.AttachmentsJson);
    }

    /// <summary>
    /// 更新前：仅 Draft 可编辑（Published/Revoked 主体不可变 B1）；AppId/AreaId 从既有实体回填不可变。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameMailCampaignEntityRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        if (existing.PublishStatus != MailCampaignPublishStatus.Draft && existing.PublishStatus != MailCampaignPublishStatus.Failed)
        {
            throw BusinessError.Oh(MailCampaignErrorCode.AlreadyPublishedOrRevoked);
        }

        input.AppId = existing.AppId;
        input.AreaId = existing.AreaId;
        // 状态机字段回填：编辑只改内容，发布状态/最近服务端结果保持原值
        input.PublishStatus = existing.PublishStatus;
        input.LastServerCode = existing.LastServerCode;
        input.LastServerMessage = existing.LastServerMessage;
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
        ValidateDraftContent(input.TitlesJson, input.AttachmentsJson);
    }

    /// <summary>
    /// 删除：仅 Draft/Failed 可删除（Published/Revoked 主体不可删 B1）。
    /// </summary>
    public override async Task DeleteAsync(BaseDeleteRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        if (existing.PublishStatus != MailCampaignPublishStatus.Draft && existing.PublishStatus != MailCampaignPublishStatus.Failed)
        {
            throw BusinessError.Oh(MailCampaignErrorCode.AlreadyPublishedOrRevoked);
        }

        await base.DeleteAsync(input);
    }

    /// <summary>
    /// 预览 / 校验 Campaign：调用 Server preview，不落库、不改状态。预估命中数一期为 -1（未知）。
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Preview")]
    public async Task<PreviewMailCampaignEntityResponse> PreviewAsync(PreviewMailCampaignEntityRequest input)
    {
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id) ?? throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        var gameArea = await ResolveAreaAsync(entity.AreaId);

        var request = BuildPreviewRequest(entity);
        var response = await _serverClient.PreviewAsync(gameArea, request);
        return new PreviewMailCampaignEntityResponse
        {
            Code = response.Code,
            EstimatedHitCount = response.EstimatedHitCount,
            Message = response.Message,
        };
    }

    /// <summary>
    /// 发布 Campaign：调用 Server publish。Ok 时回写 ServerCampaignId/PublishVersion/PublishedTime 并置 Published；非 Ok 时置 Failed 并记录 LastServerCode/Message，抛出友好错误。
    /// <para>仅 Draft/Failed 可发布（Published/Revoked 主体不可重复发布 B1）。</para>
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Publish")]
    public async Task<PublishMailCampaignEntityResponse> PublishAsync(PublishMailCampaignEntityRequest input)
    {
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id) ?? throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        if (entity.PublishStatus != MailCampaignPublishStatus.Draft && entity.PublishStatus != MailCampaignPublishStatus.Failed)
        {
            throw BusinessError.Oh(MailCampaignErrorCode.AlreadyPublishedOrRevoked);
        }

        var gameArea = await ResolveAreaAsync(entity.AreaId);
        var request = BuildPublishRequest(entity);
        PublishMailCampaignResponse response;
        try
        {
            response = await _serverClient.PublishAsync(gameArea, request);
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is InvalidOperationException || ex is TaskCanceledException)
        {
            // 网络级失败（Server 不可达/超时/信封异常）不返回业务 Code，直接抛异常会跳过
            // 下方置 Failed 的分支；与"非 Ok 置 Failed"同样落库，保证失败状态在列表可见、可重试
            entity.LastServerCode = MailCampaignServerCode.NetworkError;
            entity.LastServerMessage = ex.Message;
            entity.PublishStatus = MailCampaignPublishStatus.Failed;
            await Repository.Context.Updateable(entity)
                            .UpdateColumns(p => new { p.PublishStatus, p.LastServerCode, p.LastServerMessage, })
                            .ExecuteCommandAsync();
            throw BusinessError.Oh(MailCampaignErrorCode.ServerCallFailed, new { Message = ex.Message, });
        }

        entity.LastServerCode = response.Code;
        entity.LastServerMessage = response.Message;

        if (response.Code == MailCampaignServerCode.Ok)
        {
            entity.ServerCampaignId = response.CampaignId;
            entity.PublishVersion = response.Version;
            entity.PublishedTime = response.PublishedTime;
            entity.PublishStatus = MailCampaignPublishStatus.Published;
            entity.PublishOperator = GetCurrentOperator();
            await Repository.Context.Updateable(entity)
                            .UpdateColumns(p => new
                            {
                                p.ServerCampaignId, p.PublishVersion, p.PublishedTime, p.PublishStatus,
                                p.PublishOperator, p.LastServerCode, p.LastServerMessage,
                            })
                            .ExecuteCommandAsync();
        }
        else
        {
            entity.PublishStatus = MailCampaignPublishStatus.Failed;
            await Repository.Context.Updateable(entity)
                            .UpdateColumns(p => new { p.PublishStatus, p.LastServerCode, p.LastServerMessage, })
                            .ExecuteCommandAsync();
            throw BusinessError.Oh(MapServerCodeToErrorCode(response.Code), new { response.Message, });
        }

        return new PublishMailCampaignEntityResponse
        {
            Code = response.Code,
            CampaignId = response.CampaignId,
            Version = response.Version,
            PublishedTime = response.PublishedTime,
            Message = response.Message,
        };
    }

    /// <summary>
    /// 撤回 Campaign：调用 Server revoke。仅 Published 可撤回（撤回不可逆 B3，已发放资产不回滚）。Ok 时回写 RevokedTime/RevokeOperator 并置 Revoked。
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "Revoke")]
    public async Task<RevokeMailCampaignEntityResponse> RevokeAsync(RevokeMailCampaignEntityRequest input)
    {
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id) ?? throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        if (entity.PublishStatus != MailCampaignPublishStatus.Published)
        {
            throw BusinessError.Oh(MailCampaignErrorCode.StateOperationForbidden);
        }

        var gameArea = await ResolveAreaAsync(entity.AreaId);
        var request = new RevokeMailCampaignRequest
        {
            CampaignId = entity.ServerCampaignId,
            Operator = GetCurrentOperator(),
        };
        var response = await _serverClient.RevokeAsync(gameArea, request);

        entity.LastServerCode = response.Code;
        entity.LastServerMessage = response.Message;

        if (response.Code == MailCampaignServerCode.Ok)
        {
            entity.RevokedTime = response.RevokedTime;
            entity.RevokeOperator = GetCurrentOperator();
            entity.PublishStatus = MailCampaignPublishStatus.Revoked;
            await Repository.Context.Updateable(entity)
                            .UpdateColumns(p => new { p.RevokedTime, p.RevokeOperator, p.PublishStatus, p.LastServerCode, p.LastServerMessage, })
                            .ExecuteCommandAsync();
        }
        else
        {
            throw BusinessError.Oh(MapServerCodeToErrorCode(response.Code), new { response.Message, });
        }

        return new RevokeMailCampaignEntityResponse
        {
            Code = response.Code,
            CampaignId = response.CampaignId,
            RevokedTime = response.RevokedTime,
            Message = response.Message,
        };
    }

    /// <summary>
    /// 经 Online 统一下发当前 Campaign（来源 change C343 / vault:C8 S7.7，方案C）：转调 OnlineLiveOpsDispatchService 单一入口（publish_mail_campaign 通道）。
    /// <para>邮件不重复下发：单一入口只收 Published（同目标 ServerId==AreaId 合并返回首次发布回执）；Draft/Failed/Revoked 拒绝并指引走本服务 publish 链路。</para>
    /// </summary>
    [HttpPost("dispatchOnline")]
    [ApiDescriptionSettings(Name = "DispatchOnline")]
    [DisplayName("经Online统一下发")]
    public Task<OnlineLiveOpsDispatchResponse> DispatchOnlineAsync(OnlineLiveOpsSourceDispatchRequest input)
    {
        return _dispatchService.DispatchAsync(new OnlineLiveOpsDispatchRequest
        {
            Source = LiveOpsSourceKind.MailCampaign,
            BusinessKey = input.BusinessKey,
            AppId = input.AppId,
            ServerId = input.ServerId,
            SegmentId = input.SegmentId,
            Reason = input.Reason,
        });
    }

    /// <summary>
    /// 解析目标区服（调用 Server 接口的目标地址由区服 HttpManageUrl + FulfillmentApiPrefix 承载，来源 change C175）。
    /// </summary>
    private async Task<Entity.Tenant.Game.TenantGameAreaEntity> ResolveAreaAsync(long areaId)
    {
        var area = await _gameAreaRepository.GetFirstAsync(u => u.Id == areaId);
        if (area == null || string.IsNullOrWhiteSpace(area.HttpManageUrl))
        {
            throw BusinessError.Oh("区服不存在或未配置管理地址(HttpManageUrl)");
        }

        return area;
    }

    /// <summary>
    /// 校验草稿内容：多语言标题至少 1 项；附件每项数量 &gt; 0（与 Server 校验对齐，提前在 Admin 拦截）。
    /// </summary>
    private static void ValidateDraftContent(string titlesJson, string attachmentsJson)
    {
        var titles = string.IsNullOrWhiteSpace(titlesJson)
                         ? new List<MailLocalizedContent>()
                         : JsonHelper.Deserialize<List<MailLocalizedContent>>(titlesJson);
        if (titles.Count == 0)
        {
            throw BusinessError.Oh("多语言标题至少需要1项");
        }

        if (!string.IsNullOrWhiteSpace(attachmentsJson))
        {
            var attachments = JsonHelper.Deserialize<List<MailCampaignAttachment>>(attachmentsJson);
            if (attachments.Any(a => a.Amount <= 0))
            {
                throw BusinessError.Oh("附件数量必须大于0");
            }
        }
    }

    private static PreviewMailCampaignRequest BuildPreviewRequest(TenantGameMailCampaignEntity entity)
    {
        return new PreviewMailCampaignRequest
        {
            MailType = entity.MailType,
            Titles = DeserializeList<MailLocalizedContent>(entity.TitlesJson),
            Contents = DeserializeList<MailLocalizedContent>(entity.ContentsJson),
            Attachments = DeserializeList<MailCampaignAttachment>(entity.AttachmentsJson),
            ServerIds = DeserializeIntList(entity.ServerIdsJson),
            ChannelIds = DeserializeIntList(entity.ChannelIdsJson),
            MinLevel = entity.MinLevel,
            MaxLevel = entity.MaxLevel,
            ExpireAttachmentPolicy = entity.ExpireAttachmentPolicy,
            ExpireTime = entity.ExpireTime,
        };
    }

    private static PublishMailCampaignRequest BuildPublishRequest(TenantGameMailCampaignEntity entity)
    {
        return new PublishMailCampaignRequest
        {
            CampaignId = entity.ServerCampaignId,
            TemplateId = entity.TemplateId,
            MailType = entity.MailType,
            Titles = DeserializeList<MailLocalizedContent>(entity.TitlesJson),
            Contents = DeserializeList<MailLocalizedContent>(entity.ContentsJson),
            SenderNames = DeserializeList<MailLocalizedContent>(entity.SenderNamesJson),
            Attachments = DeserializeList<MailCampaignAttachment>(entity.AttachmentsJson),
            ServerIds = DeserializeIntList(entity.ServerIdsJson),
            ChannelIds = DeserializeIntList(entity.ChannelIdsJson),
            MinLevel = entity.MinLevel,
            MaxLevel = entity.MaxLevel,
            ExpireAttachmentPolicy = entity.ExpireAttachmentPolicy,
            ExpireTime = entity.ExpireTime,
        };
    }

    private static List<T> DeserializeList<T>(string json)
    {
        return string.IsNullOrWhiteSpace(json) ? new List<T>() : JsonHelper.Deserialize<List<T>>(json);
    }

    private static List<int> DeserializeIntList(string json)
    {
        return string.IsNullOrWhiteSpace(json) ? new List<int>() : JsonHelper.Deserialize<List<int>>(json);
    }

    /// <summary>
    /// 当前操作人显示名（RealName 优先，回退 Account）。
    /// </summary>
    private string GetCurrentOperator()
    {
        var realName = _currentUserContext.RealName;
        if (!string.IsNullOrWhiteSpace(realName))
        {
            return realName;
        }

        return _currentUserContext.Account ?? $"user_{_currentUserContext.UserId}";
    }

    /// <summary>
    /// Server 业务码 → Admin ErrorCode（C119）映射。
    /// </summary>
    private static MailCampaignErrorCode MapServerCodeToErrorCode(MailCampaignServerCode code)
    {
        switch (code)
        {
            case MailCampaignServerCode.HasExist:
                return MailCampaignErrorCode.AlreadyPublishedOrRevoked;
            case MailCampaignServerCode.CampaignAlreadyRevoked:
                return MailCampaignErrorCode.AlreadyRevoked;
            case MailCampaignServerCode.CampaignNotFound:
                return MailCampaignErrorCode.CampaignNotFound;
            case MailCampaignServerCode.InvalidCampaignParameter:
            case MailCampaignServerCode.InvalidReward:
            case MailCampaignServerCode.Unsupported:
                return MailCampaignErrorCode.ParameterInvalid;
            case MailCampaignServerCode.Ok:
            default:
                return MailCampaignErrorCode.ServerCallFailed;
        }
    }
}
#pragma warning restore CS8602, CS8604, CS8629