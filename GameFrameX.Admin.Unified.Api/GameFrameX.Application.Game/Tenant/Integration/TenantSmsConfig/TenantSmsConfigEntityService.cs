using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Tenant.Integration.TenantSmsConfig.Dto;
using GameFrameX.Entity.Base;
using GameFrameX.Sms.Abstract;
using Microsoft.Extensions.Logging;
using GameFrameX.Core.Service;
using GameFrameX.Core.Service.Tenant;

#pragma warning disable CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Tenant.Integration.TenantSmsConfig;

/// <summary>
/// 租户 SMS 短信配置管理服务。
/// <para>承载租户自有阿里云短信凭证；平台不持有 SMS 配置，无静态 JSON / Options fallback。</para>
/// <para>敏感字段（AccessKeyId / AccessKeySecret）输出脱敏 <c>******</c>；更新时回传 <c>******</c> 保留原值；日志与异常禁止明文。</para>
/// <para>实体继承 <see cref="EntityBasic"/> 自动按当前租户过滤；SendTestAsync 必须指定配置 Id，不允许跨租户或 fallback 到其它配置。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 115)]
public class TenantSmsConfigEntityService
    : GenericCrudService<GameFrameX.Entity.Tenant.ClientApiConfig.TenantSmsConfigEntity,
        AddTenantSmsConfigRequest, UpdateTenantSmsConfigRequest,
        TenantSmsConfigResponse, PageTenantSmsConfigRequest>
{
    private readonly ILogger<TenantSmsConfigEntityService> _logger;

    public TenantSmsConfigEntityService(SqlSugarRepository<GameFrameX.Entity.Tenant.ClientApiConfig.TenantSmsConfigEntity> repository,
        ILogger<TenantSmsConfigEntityService> logger)
        : base(repository)
    {
        _logger = logger;
    }

    protected override ISugarQueryable<TenantSmsConfigResponse> BuildPageQuery(PageTenantSmsConfigRequest input)
    {
        var searchKey = input.SearchKey?.Trim();
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u =>
                                      (u.SignName ?? string.Empty).Contains(searchKey) ||
                                      (u.Remark ?? string.Empty).Contains(searchKey))
                         .ApplyAppIdFilter(input.AppId, u => u.AppId)
                         .WhereIF(input.IsActive.HasValue, u => u.IsEnabled == input.IsActive.Value)
                         .Select<TenantSmsConfigResponse>();
    }

    /// <summary>
    /// 新增租户 SMS 配置。
    /// <para>Aliyun Provider 必须提交真实 AccessKeyId / AccessKeySecret / SignName，禁止提交 <c>******</c> 脱敏占位。</para>
    /// </summary>
    public override async Task<TenantSmsConfigResponse> AddAsync(AddTenantSmsConfigRequest input)
    {
        ValidateAliyunRequired(input.Provider, input.AccessKeyId, input.AccessKeySecret, input.SignName, true);

        var entity = input.Adapt<GameFrameX.Entity.Tenant.ClientApiConfig.TenantSmsConfigEntity>();
        entity.AppId = input.AppId;
        entity.Provider = input.Provider;
        entity.IsEnabled = input.IsEnabled;
        entity.AccessKeyId = TenantSecretCrypto.Encrypt(entity.AccessKeyId) ?? string.Empty;
        entity.AccessKeySecret = TenantSecretCrypto.Encrypt(entity.AccessKeySecret) ?? string.Empty;
        await Repository.InsertAsync(entity);
        return entity.Adapt<TenantSmsConfigResponse>();
    }

    /// <summary>
    /// 更新租户 SMS 配置。
    /// <para>AccessKeyId / AccessKeySecret 回传 <c>******</c> 时保留数据库原值。</para>
    /// </summary>
    public override async Task UpdateAsync(UpdateTenantSmsConfigRequest input)
    {
        var entity = await Repository.GetFirstAsync(e => e.Id == input.Id);
        if (entity == null)
        {
            throw BusinessError.Oh("租户 SMS 配置不存在");
        }

        entity.AppId = input.AppId;
        entity.Provider = input.Provider;
        entity.AccessKeyId = SensitiveFieldMask.PreserveIfMasked(input.AccessKeyId, entity.AccessKeyId);
        entity.AccessKeySecret = SensitiveFieldMask.PreserveIfMasked(input.AccessKeySecret, entity.AccessKeySecret);
        entity.SignName = input.SignName;
        entity.TemplateCode = input.TemplateCode;
        entity.Priority = input.Priority;
        entity.IsEnabled = input.IsEnabled;
        entity.Remark = input.Remark;
        entity.RowVersion = input.RowVersion;

        ValidateAliyunRequired(entity.Provider, entity.AccessKeyId, entity.AccessKeySecret, entity.SignName, false);

        // C371：校验通过后（明文形态）、落库之前加密；保留语义下 storedKey 可能是存量明文或密文，Encrypt 幂等兼容两者
        entity.AccessKeyId = TenantSecretCrypto.Encrypt(entity.AccessKeyId) ?? string.Empty;
        entity.AccessKeySecret = TenantSecretCrypto.Encrypt(entity.AccessKeySecret) ?? string.Empty;

        await Repository.AsUpdateable(entity)
                        .UpdateColumns(e => new
                        {
                            e.AppId,
                            e.Provider,
                            e.AccessKeyId,
                            e.AccessKeySecret,
                            e.SignName,
                            e.TemplateCode,
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
    public override async Task<SqlSugarPagedList<TenantSmsConfigResponse>> PageAsync(PageTenantSmsConfigRequest input)
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
    public override async Task<TenantSmsConfigResponse> GetAsync([FromQuery] BaseIdRequest input)
    {
        var entity = await InnerGetAsync(input);
        if (entity == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        var output = entity.Adapt<TenantSmsConfigResponse>();
        MaskSensitiveFields(output);
        return output;
    }

    /// <summary>
    /// 受权限控制的测试发送接口。
    /// <para>按 <see cref="TenantSmsConfigTestSendRequest.Id"/> 加载当前租户配置，缺凭据、禁用、不属于当前租户、模板为空时返回明确错误。</para>
    /// <para>禁止 fallback 到静态配置或其它租户配置；异常与日志不得打印 AccessKey 明文。</para>
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "SendTest")]
    public async Task<bool> SendTestAsync(TenantSmsConfigTestSendRequest input)
    {
        if (input == null)
        {
            throw BusinessError.Oh("测试发送参数不能为空");
        }

        if (input.Id <= 0)
        {
            throw BusinessError.Oh("测试发送必须指定租户 SMS 配置 ID");
        }

        var entity = await Repository.GetFirstAsync(e => e.Id == input.Id && e.IsEnabled == true);
        if (entity == null)
        {
            throw BusinessError.Oh("租户 SMS 配置不存在、已禁用或不属于当前租户");
        }

        var templateCode = !string.IsNullOrWhiteSpace(input.TemplateCode) ? input.TemplateCode : entity.TemplateCode;
        if (string.IsNullOrWhiteSpace(entity.AccessKeyId) ||
            string.IsNullOrWhiteSpace(entity.AccessKeySecret) ||
            string.IsNullOrWhiteSpace(entity.SignName) ||
            string.IsNullOrWhiteSpace(templateCode))
        {
            throw BusinessError.Oh("租户 SMS 配置凭据不完整；测试发送已拦截，禁止 fallback 到其它配置。");
        }

        if (!SmsSenderRegistry.TryGet(entity.Provider, out var sender))
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed);
        }

        var sendOptions = new SmsSendOptions
        {
            PhoneNumbers = input.Phone,
            SignName = entity.SignName,
            TemplateCode = templateCode,
            TemplateParamJson = "{\"code\":\"123456\"}",
            AccessKeyId = entity.AccessKeyId,
            AccessKeySecret = entity.AccessKeySecret,
        };

        try
        {
            var result = await sender.SendAsync(sendOptions).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return true;
            }

            var upstream = !string.IsNullOrWhiteSpace(result.UpstreamErrorCode) || !string.IsNullOrWhiteSpace(result.UpstreamErrorMessage)
                               ? $"{result.UpstreamErrorCode}-{result.UpstreamErrorMessage}"
                               : "未知";
            throw BusinessError.Oh($"短信发送失败：{upstream}");
        }
        catch (Exception ex) when (ex is not BusinessException)
        {
            _logger.LogError(ex, "租户 SMS 测试发送失败：id={Id}, appId={AppId}, phone={Phone}", entity.Id, entity.AppId, input.Phone);
            throw BusinessError.Oh($"租户 SMS 测试发送失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 对输出对象进行敏感字段脱敏（Output 已拆为纯 POCO，直接操作输出字段）。
    /// </summary>
    private static void MaskSensitiveFields(TenantSmsConfigResponse output)
    {
        if (output == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(output.AccessKeyId))
        {
            output.AccessKeyId = SensitiveFieldMask.MaskValue;
        }

        if (!string.IsNullOrEmpty(output.AccessKeySecret))
        {
            output.AccessKeySecret = SensitiveFieldMask.MaskValue;
        }
    }

    /// <summary>
    /// 校验 Aliyun Provider 必填字段；AddAsync 阶段额外拒绝 <c>******</c> 脱敏占位。
    /// </summary>
    private static void ValidateAliyunRequired(string provider, string accessKeyId, string accessKeySecret, string signName, bool isAdd)
    {
        if (!string.Equals(provider, "Aliyun", StringComparison.OrdinalIgnoreCase))
        {
            throw BusinessError.Oh("当前仅支持 Aliyun Provider");
        }

        if (string.IsNullOrWhiteSpace(accessKeyId))
        {
            throw BusinessError.Oh("Aliyun Provider 必须提供 AccessKeyId");
        }

        if (isAdd && SensitiveFieldMask.IsMasked(accessKeyId))
        {
            throw BusinessError.Oh("新增租户 SMS 配置禁止提交脱敏占位 ******；必须提交真实 AccessKeyId");
        }

        if (string.IsNullOrWhiteSpace(accessKeySecret))
        {
            throw BusinessError.Oh("Aliyun Provider 必须提供 AccessKeySecret");
        }

        if (isAdd && SensitiveFieldMask.IsMasked(accessKeySecret))
        {
            throw BusinessError.Oh("新增租户 SMS 配置禁止提交脱敏占位 ******；必须提交真实 AccessKeySecret");
        }

        if (string.IsNullOrWhiteSpace(signName))
        {
            throw BusinessError.Oh("Aliyun Provider 必须提供短信签名 SignName");
        }
    }
}
#pragma warning restore CS8604, CS8629