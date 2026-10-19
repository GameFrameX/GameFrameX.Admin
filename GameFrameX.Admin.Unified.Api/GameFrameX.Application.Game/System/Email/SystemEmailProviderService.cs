using GameFrameX.Application.Game.System.Email.Dto;
using GameFrameX.Core.Service;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Email.Abstract;
using Microsoft.Extensions.Logging;

#pragma warning disable CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.System.Email;

/// <summary>
/// 平台系统邮件 Provider 配置管理服务。
/// <para>主库 / SuperAdmin-only；与租户业务邮件（<c>TenantEmailConfigEntity</c>）分域，禁止互相 fallback。</para>
/// <para>敏感字段（UserName / Password / SecretId / SecretKey）输出脱敏 <c>******</c>；更新时回传 <c>******</c> 保留原值。</para>
/// <para>调用方必须显式传入 <c>SystemEmailPurpose</c>；缺配置时 <see cref="SendTestEmailAsync"/> 与 <c>SystemMessageService.SendEmailAsync</c> 均返回明确错误，禁止静默成功。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 105)]
public class SystemEmailProviderService
    : GenericCrudService<GameFrameX.Entity.System.SystemEmailProviderEntity,
        AddSystemEmailProviderRequest, UpdateSystemEmailProviderRequest,
        SystemEmailProviderResponse, PageSystemEmailProviderRequest>
{
    private readonly ILogger<SystemEmailProviderService> _logger;

    public SystemEmailProviderService(SqlSugarRepository<GameFrameX.Entity.System.SystemEmailProviderEntity> repository,
        ILogger<SystemEmailProviderService> logger)
        : base(repository)
    {
        _logger = logger;
    }

    protected override ISugarQueryable<SystemEmailProviderResponse> BuildPageQuery(PageSystemEmailProviderRequest input)
    {
        var searchKey = input.SearchKey?.Trim();
        // superadmin 上下文会跳过全局 QueryFilter（SqlSugarSetup 超管豁免），
        // 软删除行会漏进列表；与 GenericCrudService 默认 BuildPageQuery 一致显式过滤
        return Repository.AsQueryable()
                         .Where(u => u.IsDeleted == false)
                         .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u =>
                                      (u.DefaultFromEmail ?? string.Empty).Contains(searchKey) ||
                                      (u.Remark ?? string.Empty).Contains(searchKey))
                         .WhereIF(input.Purpose.HasValue, u => u.Purpose == input.Purpose.Value)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Provider), u => u.Provider == input.Provider)
                         .WhereIF(input.IsActive.HasValue, u => u.IsEnabled == input.IsActive.Value)
                         .Select<SystemEmailProviderResponse>();
    }

    /// <summary>
    /// 新增平台系统邮件 Provider 配置。
    /// <para>缺 Provider / 发件人邮箱 / Provider 必填凭据会直接拒绝；同一 Purpose 可保留多条配置并交给 Priority 决定选择顺序。</para>
    /// </summary>
    public override async Task<SystemEmailProviderResponse> AddAsync(AddSystemEmailProviderRequest input)
    {
        ValidateProviderRequiredFields(input.Provider, input.DefaultFromEmail, input.Host, input.Port, input.SecretId, input.SecretKey, input.TemplateId);

        var entity = input.Adapt<GameFrameX.Entity.System.SystemEmailProviderEntity>();
        entity.IsEnabled = input.IsEnabled;
        EncryptProviderSecrets(entity);
        await Repository.InsertAsync(entity);
        var added = entity.Adapt<SystemEmailProviderResponse>();
        // 凭证已密文化（C370），Add 响应与 Page/Detail 统一走脱敏，不回显任何形态的凭证值
        MaskSensitiveFields(added);
        return added;
    }

    /// <summary>
    /// 更新平台系统邮件 Provider 配置。
    /// <para>敏感字段回传 <c>******</c> 时保留数据库原值。</para>
    /// </summary>
    public override async Task UpdateAsync(UpdateSystemEmailProviderRequest input)
    {
        var entity = await Repository.GetFirstAsync(e => e.Id == input.Id);
        if (entity == null)
        {
            throw BusinessError.Oh("平台系统邮件配置不存在");
        }

        entity.Purpose = input.Purpose;
        entity.Provider = input.Provider;
        entity.Host = input.Host;
        entity.Port = input.Port;
        entity.EnableSsl = input.EnableSsl;
        entity.UseDefaultCredentials = input.UseDefaultCredentials;
        entity.UserName = SensitiveFieldMask.PreserveIfMasked(input.UserName, entity.UserName);
        entity.Password = SensitiveFieldMask.PreserveIfMasked(input.Password, entity.Password);
        entity.Region = input.Region;
        entity.SecretId = SensitiveFieldMask.PreserveIfMasked(input.SecretId, entity.SecretId);
        entity.SecretKey = SensitiveFieldMask.PreserveIfMasked(input.SecretKey, entity.SecretKey);
        entity.DefaultFromEmail = input.DefaultFromEmail;
        entity.DefaultFromName = input.DefaultFromName;
        entity.DefaultToEmail = input.DefaultToEmail;
        entity.TemplateId = input.TemplateId;
        entity.TemplateDataJson = input.TemplateDataJson;
        entity.Priority = input.Priority;
        entity.IsEnabled = input.IsEnabled;
        entity.Remark = input.Remark;
        entity.RowVersion = input.RowVersion;

        ValidateProviderRequiredFields(entity.Provider, entity.DefaultFromEmail, entity.Host, entity.Port, entity.SecretId, entity.SecretKey, entity.TemplateId);

        // SensitiveFieldMask.PreserveIfMasked 已把 ****** 换回库中原值（密文形态），Encrypt 幂等跳过；新明文在此加密后落库（C370）
        EncryptProviderSecrets(entity);

        await Repository.AsUpdateable(entity)
                        .UpdateColumns(e => new
                        {
                            e.Purpose,
                            e.Provider,
                            e.Host,
                            e.Port,
                            e.EnableSsl,
                            e.UseDefaultCredentials,
                            e.UserName,
                            e.Password,
                            e.Region,
                            e.SecretId,
                            e.SecretKey,
                            e.DefaultFromEmail,
                            e.DefaultFromName,
                            e.DefaultToEmail,
                            e.TemplateId,
                            e.TemplateDataJson,
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
    public override async Task<SqlSugarPagedList<SystemEmailProviderResponse>> PageAsync(PageSystemEmailProviderRequest input)
    {
        var result = await base.PageAsync(input);
        foreach (var item in result.Items)
        {
            MaskSensitiveFields(item);
        }

        return result;
    }

    /// <summary>
    /// 详情（敏感字段脱敏）。
    /// </summary>
    [HttpGet(CrudActionRoutes.Detail)]
    [ApiDescriptionSettings(Name = "Detail")]
    public override async Task<SystemEmailProviderResponse> GetAsync([FromQuery] BaseIdRequest input)
    {
        var entity = await InnerGetAsync(input);
        if (entity == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        var output = entity.Adapt<SystemEmailProviderResponse>();
        MaskSensitiveFields(output);
        return output;
    }

    /// <summary>
    /// 受权限控制的测试发送接口。
    /// <para>按当前 <c>Id</c> 读取配置，缺凭据或发送失败返回明确错误，不向 API 输出敏感字段。</para>
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "SendTest")]
    public async Task<bool> SendTestEmailAsync(SystemEmailProviderTestSendRequest input)
    {
        if (input == null)
        {
            throw BusinessError.Oh("测试发送参数不能为空");
        }

        if (input.Id <= 0)
        {
            throw BusinessError.Oh("测试发送必须指定平台系统邮件配置 ID");
        }

        var entity = await Repository.GetFirstAsync(e => e.Id == input.Id && e.IsEnabled == true);
        if (entity == null)
        {
            throw BusinessError.Oh("平台系统邮件配置不存在或未启用");
        }

        EmailSendResult result;
        try
        {
            result = await SystemEmailDispatcher.SendAsync(entity, input.ReceiverEmail, input.Subject, input.Body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "平台系统邮件测试发送失败：purpose={Purpose}, provider={Provider}, receiver={Receiver}", entity.Purpose, entity.Provider, input.ReceiverEmail);
            throw BusinessError.Oh($"平台系统邮件测试发送失败：{ex.Message}");
        }

        if (!result.IsSuccess)
        {
            _logger.LogError("平台系统邮件测试发送失败：purpose={Purpose}, provider={Provider}, receiver={Receiver}, reason={Reason}", entity.Purpose, entity.Provider, input.ReceiverEmail, result.FailureReason);
            throw BusinessError.Oh(SystemErrorCode.SystemEmailTestSendFailed,
                                   $"reason={result.FailureException?.Message ?? result.FailureReason?.ToString() ?? "未知错误"}");
        }

        return true;
    }

    /// <summary>
    /// 对实体进行敏感字段脱敏（输出前调用）：非空值替换为占位符，空值保持为空。
    /// </summary>
    private static void MaskSensitiveFields(SystemEmailProviderResponse output)
    {
        if (output == null)
        {
            return;
        }

        output.UserName = SensitiveFieldMask.MaskIfNotEmpty(output.UserName);
        output.Password = SensitiveFieldMask.MaskIfNotEmpty(output.Password);
        output.SecretId = SensitiveFieldMask.MaskIfNotEmpty(output.SecretId);
        output.SecretKey = SensitiveFieldMask.MaskIfNotEmpty(output.SecretKey);
    }

    /// <summary>
    /// 凭证敏感字段落库前静态加密（C370 <see cref="TenantSecretCrypto"/> 收口）：新明文加密为 <c>enc:sec:v1:</c> 密文，
    /// <see cref="SensitiveFieldMask.PreserveIfMasked"/> 保留的库中旧值已是密文则幂等透传。
    /// </summary>
    private static void EncryptProviderSecrets(GameFrameX.Entity.System.SystemEmailProviderEntity entity)
    {
        entity.UserName = TenantSecretCrypto.Encrypt(entity.UserName);
        entity.Password = TenantSecretCrypto.Encrypt(entity.Password);
        entity.SecretId = TenantSecretCrypto.Encrypt(entity.SecretId);
        entity.SecretKey = TenantSecretCrypto.Encrypt(entity.SecretKey);
    }

    /// <summary>
    /// 校验 Provider 必填字段；SMTP 必须提供主机和端口，腾讯云 SES 必须提供 SecretId/SecretKey/TemplateId。
    /// </summary>
    private static void ValidateProviderRequiredFields(string provider, string? defaultFromEmail, string? host, int? port, string? secretId, string? secretKey, ulong? templateId)
    {
        if (string.IsNullOrWhiteSpace(defaultFromEmail))
        {
            throw BusinessError.Oh("默认发件人邮箱不能为空");
        }

        if (string.Equals(provider, "Smtp", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                throw BusinessError.Oh("SMTP Provider 必须提供 Host");
            }

            if (!port.HasValue || port.Value <= 0)
            {
                throw BusinessError.Oh("SMTP Provider 必须提供有效端口");
            }
        }

        if (string.Equals(provider, "TencentCloudSes", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(secretId) || string.IsNullOrWhiteSpace(secretKey))
            {
                throw BusinessError.Oh("腾讯云 SES Provider 必须提供 SecretId 和 SecretKey");
            }

            if (!templateId.HasValue || templateId.Value == 0)
            {
                throw BusinessError.Oh("腾讯云 SES Provider 必须提供模板 ID");
            }
        }
    }
}
#pragma warning restore CS8604, CS8629