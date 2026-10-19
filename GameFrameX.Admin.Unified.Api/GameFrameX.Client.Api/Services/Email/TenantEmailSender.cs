using GameFrameX.Client.Api.Services.ClientApiTenantOptions;
using GameFrameX.Core.Service.Message;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Email.Abstract;
using GameFrameX.Email.TencentSes;

namespace GameFrameX.Client.Api.Services.Email;

/// <summary>
/// 租户业务邮件发送实现。
/// <para>凭据（SecretId/SecretKey）与发件人地址均通过 <see cref="TenantEmailConfigSelector"/> 从租户库按 <c>TenantId + AppId + Purpose</c> 优先级读取。</para>
/// <para>缺租户上下文或缺配置返回 <c>false</c> 并记录警告，禁止 fallback 到 <see cref="ISystemEmailSender"/>。</para>
/// </summary>
public sealed class TenantEmailSender : ITenantEmailSender
{
    private readonly IClientApiTenantOptionsProvider _optionsProvider;
    private readonly ClientApiTenantDbProvider _tenantDbProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// </summary>
    public TenantEmailSender(IClientApiTenantOptionsProvider optionsProvider,
        ClientApiTenantDbProvider tenantDbProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        _optionsProvider = optionsProvider;
        _tenantDbProvider = tenantDbProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public async Task<bool> SendTemplateAsync(TenantEmailPurpose purpose, string receiverEmail, ulong templateId, string subject, IDictionary<string, string> templateData)
    {
        var snapshot = await GetSnapshotAsync(purpose);
        if (snapshot == null)
        {
            LogHelper.Warning($"租户业务邮件未配置或凭据不完整，purpose={purpose}, receiver={receiverEmail}；禁止 fallback 到系统邮件。");
            return false;
        }

        if (!EmailSenderRegistry.TryGet(snapshot.Provider, out var sender))
        {
            LogHelper.Warning($"租户业务邮件 Provider 未注册：{snapshot.Provider}；禁止 fallback 到系统邮件。");
            return false;
        }

        var options = new EmailSendOptions
        {
            FromEmailAddress = snapshot.FromEmailAddress,
            FromDisplayName = snapshot.DefaultFromName,
            ReceiverEmail = receiverEmail,
            Subject = ApplySubjectPrefix(snapshot, subject),
            Body = string.Empty,
            TemplateId = templateId,
            TemplateDataJson = System.Text.Json.JsonSerializer.Serialize(SendEmailTemplateDataMapper.FromDictionary(templateData)),
            SecretId = snapshot.FromEmailSecretId,
            SecretKey = snapshot.FromEmailSecretKey,
        };

        var result = await sender.SendAsync(options);
        return result.IsSuccess;
    }

    /// <inheritdoc />
    public async Task<string?> GetFromEmailAddressAsync()
    {
        var snapshot = await GetSnapshotAsync(TenantEmailPurpose.Default);
        return snapshot?.FromEmailAddress;
    }

    /// <inheritdoc />
    public async Task<TenantEmailOptionsSnapshot?> GetSnapshotAsync(TenantEmailPurpose purpose)
    {
        var ctx = _httpContextAccessor.HttpContext;
        var tenantIdObj = ctx?.Items[TenantContextAccessor.TenantIdKey] as long?;
        if (!tenantIdObj.HasValue || tenantIdObj.Value <= 0)
        {
            return null;
        }

        var appIdObj = ctx?.Items[Middleware.AppResolutionMiddleware.AppIdKey] as long?;
        var resolution = new ClientApiTenantResolution
        {
            TenantId = tenantIdObj.Value,
            AppId = appIdObj,
        };

        var db = _tenantDbProvider.GetTenantDb(resolution.TenantId);
        var entity = await TenantEmailConfigSelector.SelectAsync(db, resolution, (int)purpose);
        if (entity == null)
        {
            return null;
        }

        // C372（GFX-838）：邮件凭据读取边界解密（C371 起库内密文；无前缀明文直通兼容存量行；
        // 进程级密文缓存兜底，避免每次发送 PBKDF2 派生）。实体为本次查询的请求本地实例，改写无副作用。
        entity.FromEmailSecretId = TenantSecretCrypto.DecryptCached(entity.FromEmailSecretId) ?? string.Empty;
        entity.FromEmailSecretKey = TenantSecretCrypto.DecryptCached(entity.FromEmailSecretKey) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(entity.FromEmailAddress) ||
            string.IsNullOrWhiteSpace(entity.FromEmailSecretId) ||
            string.IsNullOrWhiteSpace(entity.FromEmailSecretKey))
        {
            return null;
        }

        return new TenantEmailOptionsSnapshot
        {
            Id = entity.Id,
            Purpose = entity.Purpose,
            Provider = entity.Provider,
            Region = entity.Region,
            TemplateSet = entity.TemplateSet,
            FromEmailAddress = entity.FromEmailAddress,
            DefaultFromName = entity.DefaultFromName,
            DefaultSubjectPrefix = entity.DefaultSubjectPrefix,
            FromEmailSecretId = entity.FromEmailSecretId,
            FromEmailSecretKey = entity.FromEmailSecretKey,
        };
    }

    private static string ApplySubjectPrefix(TenantEmailOptionsSnapshot snapshot, string subject)
    {
        if (string.IsNullOrWhiteSpace(snapshot.DefaultSubjectPrefix))
        {
            return subject;
        }

        return $"{snapshot.DefaultSubjectPrefix}{subject}";
    }
}