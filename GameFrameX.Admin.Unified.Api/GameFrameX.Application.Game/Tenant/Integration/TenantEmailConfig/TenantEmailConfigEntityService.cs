using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Tenant.Integration.TenantEmailConfig.Dto;
using GameFrameX.Email.Abstract;
using GameFrameX.Email.TencentSes;
using Microsoft.Extensions.Logging;
using GameFrameX.Core.Service;
using GameFrameX.Core.Service.Tenant;

#pragma warning disable CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Tenant.Integration.TenantEmailConfig;

/// <summary>
/// 租户业务邮件配置管理服务。
/// <para>承载玩家注册验证码 / 找回密码 / 运营触达等租户品牌发信配置，按租户 + 应用 + 用途隔离。</para>
/// <para>敏感字段（SecretId / SecretKey）在输出时脱敏，更新时回传 <c>******</c> 保留原值。</para>
/// <para>配置选择优先级由 <c>TenantEmailConfigSelector</c> 实现，本服务只负责 CRUD；禁止跨邮件域 fallback 到平台系统邮件。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 110)]
public class TenantEmailConfigEntityService
    : GenericCrudService<GameFrameX.Entity.Tenant.ClientApiConfig.TenantEmailConfigEntity,
        AddTenantEmailConfigRequest, UpdateTenantEmailConfigRequest,
        TenantEmailConfigResponse, PageTenantEmailConfigRequest>
{
    private readonly ILogger<TenantEmailConfigEntityService> _logger;

    public TenantEmailConfigEntityService(SqlSugarRepository<GameFrameX.Entity.Tenant.ClientApiConfig.TenantEmailConfigEntity> repository,
        ILogger<TenantEmailConfigEntityService> logger)
        : base(repository)
    {
        _logger = logger;
    }

    protected override ISugarQueryable<TenantEmailConfigResponse> BuildPageQuery(PageTenantEmailConfigRequest input)
    {
        var searchKey = input.SearchKey?.Trim();
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u =>
                                      u.FromEmailAddress.Contains(searchKey) ||
                                      (u.Remark ?? string.Empty).Contains(searchKey))
                         .ApplyAppIdFilter(input.AppId)
                         .WhereIF(input.Purpose.HasValue, u => u.Purpose == input.Purpose.Value)
                         .WhereIF(input.IsActive.HasValue, u => u.IsEnabled == input.IsActive.Value)
                         .Select<TenantEmailConfigResponse>();
    }

    /// <summary>
    /// 新增租户业务邮件配置。
    /// <para>AppId 与 Purpose 共同决定匹配层；同层允许多条配置，由 Priority 决定选择顺序。</para>
    /// </summary>
    public override async Task<TenantEmailConfigResponse> AddAsync(AddTenantEmailConfigRequest input)
    {
        var entity = input.Adapt<GameFrameX.Entity.Tenant.ClientApiConfig.TenantEmailConfigEntity>();
        entity.AppId = input.AppId;
        entity.Purpose = input.Purpose;
        entity.IsEnabled = input.IsEnabled;
        entity.FromEmailSecretId = TenantSecretCrypto.Encrypt(entity.FromEmailSecretId) ?? string.Empty;
        entity.FromEmailSecretKey = TenantSecretCrypto.Encrypt(entity.FromEmailSecretKey) ?? string.Empty;
        await Repository.InsertAsync(entity);
        return entity.Adapt<TenantEmailConfigResponse>();
    }

    /// <summary>
    /// 更新租户业务邮件配置。
    /// <para>SecretId / SecretKey 回传 <c>******</c> 时保留数据库原值，避免脱敏回写覆盖真实密钥。</para>
    /// </summary>
    public override async Task UpdateAsync(UpdateTenantEmailConfigRequest input)
    {
        var entity = await Repository.GetFirstAsync(e => e.Id == input.Id);
        if (entity == null)
        {
            throw BusinessError.Oh("邮件配置不存在");
        }

        entity.AppId = input.AppId;
        entity.Purpose = input.Purpose;
        entity.Provider = input.Provider;
        entity.Region = input.Region;
        entity.TemplateSet = input.TemplateSet;
        entity.FromEmailAddress = input.FromEmailAddress;
        entity.DefaultFromName = input.DefaultFromName;
        entity.DefaultSubjectPrefix = input.DefaultSubjectPrefix;
        entity.FromEmailSecretId = SensitiveFieldMask.PreserveIfMasked(input.FromEmailSecretId, entity.FromEmailSecretId);
        entity.FromEmailSecretKey = SensitiveFieldMask.PreserveIfMasked(input.FromEmailSecretKey, entity.FromEmailSecretKey);
        // C371：确定最终明文之后、落库之前加密（保留语义下 storedKey 可能是存量明文或密文，Encrypt 幂等兼容两者）
        entity.FromEmailSecretId = TenantSecretCrypto.Encrypt(entity.FromEmailSecretId) ?? string.Empty;
        entity.FromEmailSecretKey = TenantSecretCrypto.Encrypt(entity.FromEmailSecretKey) ?? string.Empty;
        entity.Priority = input.Priority;
        entity.IsEnabled = input.IsEnabled;
        entity.Remark = input.Remark;
        entity.RowVersion = input.RowVersion;

        await Repository.AsUpdateable(entity)
                        .UpdateColumns(e => new
                        {
                            e.AppId,
                            e.Purpose,
                            e.Provider,
                            e.Region,
                            e.TemplateSet,
                            e.FromEmailAddress,
                            e.DefaultFromName,
                            e.DefaultSubjectPrefix,
                            e.FromEmailSecretId,
                            e.FromEmailSecretKey,
                            e.Priority,
                            e.IsEnabled,
                            e.Remark,
                        })
                        .ExecuteCommandWithOptLockAsync(true);
    }

    /// <summary>
    /// 分页查询（敏感字段脱敏）。
    /// </summary>
    [HttpPost(CrudActionRoutes.Page)]
    [ApiDescriptionSettings(Name = "Page")]
    public override async Task<SqlSugarPagedList<TenantEmailConfigResponse>> PageAsync(PageTenantEmailConfigRequest input)
    {
        var result = await base.PageAsync(input);
        foreach (var item in result.Items)
        {
            item.FromEmailSecretId = SensitiveFieldMask.MaskValue;
            item.FromEmailSecretKey = SensitiveFieldMask.MaskValue;
        }

        return result;
    }

    /// <summary>
    /// 详情（敏感字段脱敏）。
    /// </summary>
    [HttpGet(CrudActionRoutes.Detail)]
    [ApiDescriptionSettings(Name = "Detail")]
    public override async Task<TenantEmailConfigResponse> GetAsync([FromQuery] BaseIdRequest input)
    {
        var entity = await InnerGetAsync(input);
        if (entity == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        entity.FromEmailSecretId = SensitiveFieldMask.MaskValue;
        entity.FromEmailSecretKey = SensitiveFieldMask.MaskValue;
        return entity.Adapt<TenantEmailConfigResponse>();
    }

    /// <summary>
    /// 受权限控制的测试发送接口。
    /// <para>按 <see cref="TenantEmailConfigTestSendRequest.Id"/> 直接读取对应租户业务邮件配置，缺租户上下文 / 缺配置 / 缺凭据返回明确错误，禁止 fallback 到平台系统邮件。</para>
    /// <para>调用方传入 <c>Id</c> 时跳过作用域自动选择；模板变量通过 <see cref="SendEmailTemplateDataMapper"/> 与真实发送保持一致。</para>
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "SendTest")]
    public async Task<bool> SendTestEmailAsync(TenantEmailConfigTestSendRequest input)
    {
        if (input == null)
        {
            throw BusinessError.Oh("测试发送参数不能为空");
        }

        if (!input.Id.HasValue || input.Id.Value <= 0)
        {
            throw BusinessError.Oh("测试发送必须指定租户业务邮件配置 ID");
        }

        var entity = await Repository.GetFirstAsync(e => e.Id == input.Id.Value && e.IsEnabled == true);
        if (entity == null)
        {
            throw BusinessError.Oh("租户业务邮件配置不存在或未启用");
        }

        if (string.IsNullOrWhiteSpace(entity.FromEmailAddress) ||
            string.IsNullOrWhiteSpace(entity.FromEmailSecretId) ||
            string.IsNullOrWhiteSpace(entity.FromEmailSecretKey))
        {
            throw BusinessError.Oh("租户业务邮件配置凭据不完整；测试发送已拦截，禁止 fallback 到平台系统邮件。");
        }

        var options = new EmailSendOptions
        {
            FromEmailAddress = entity.FromEmailAddress,
            ReceiverEmail = input.ReceiverEmail,
            Subject = input.Subject,
            Body = string.Empty,
            TemplateId = input.TemplateId,
            TemplateDataJson = JsonSerializer.Serialize(SendEmailTemplateDataMapper.FromDictionary(input.TemplateData)),
            SecretId = entity.FromEmailSecretId,
            SecretKey = entity.FromEmailSecretKey,
        };

        if (!EmailSenderRegistry.TryGet(entity.Provider, out var sender))
        {
            throw BusinessError.Oh($"租户业务邮件 Provider 未注册：{entity.Provider}");
        }

        try
        {
            var result = await sender.SendAsync(options);
            if (result.IsSuccess)
            {
                return true;
            }

            if (result.FailureReason == EmailSendFailureReason.MissingCredentials)
            {
                throw BusinessError.Oh("租户业务邮件配置凭据不完整；测试发送已拦截，禁止 fallback 到平台系统邮件。");
            }

            throw new InvalidOperationException(result.FailureException?.Message ?? "未知错误", result.FailureException);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "租户业务邮件测试发送失败：id={Id}, appId={AppId}, receiver={Receiver}, templateId={TemplateId}", entity.Id, entity.AppId, input.ReceiverEmail, input.TemplateId);
            throw BusinessError.Oh($"租户业务邮件测试发送失败：{ex.Message}");
        }
    }
}
#pragma warning restore CS8604, CS8629