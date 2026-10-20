using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Tenant.Integration.TenantWechatConfig.Dto;
using GameFrameX.Wechat.Abstract;
using Microsoft.Extensions.Logging;
using SKIT.FlurlHttpClient.Wechat.Api;
using SKIT.FlurlHttpClient.Wechat.Api.Models;
using GameFrameX.Core.Service;
using GameFrameX.Entity.Enum;
using GameFrameX.Core.Service.Tenant;

#pragma warning disable CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Tenant.Integration.TenantWechatConfig;

/// <summary>
/// 租户微信公众号 / 小程序配置管理服务。
/// <para>承载租户自有公众号 / 小程序凭证，用于租户业务模板消息等认证后运营场景；公众号 / 小程序网页授权登录已从 Admin 登录移除，本服务不提供登录相关接口。</para>
/// <para>敏感字段（<see cref="TenantWechatConfigResponse.WechatAppSecret"/> / <see cref="TenantWechatConfigResponse.WxOpenSecret"/>）输出脱敏 <c>******</c>；更新时回传 <c>******</c> 保留原值。</para>
/// <para>凭证对完整性约束：公众号 / 小程序凭证对至少配置一组且每组成对完整，不允许空字段补位。</para>
/// <para>调用方必须显式传入配置 ID；缺配置 / 禁用 / 不属于当前租户 / 字段不完整时返回明确错误，禁止 fallback 到静态配置或其它租户配置。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 111)]
public class TenantWechatConfigEntityService
    : GenericCrudService<GameFrameX.Entity.Tenant.ClientApiConfig.TenantWechatConfigEntity,
        AddTenantWechatConfigRequest, UpdateTenantWechatConfigRequest,
        TenantWechatConfigResponse, PageTenantWechatConfigRequest>
{
    private readonly ILogger<TenantWechatConfigEntityService> _logger;
    private readonly WechatApiHttpClientFactory _wechatApiHttpClientFactory;

    public TenantWechatConfigEntityService(SqlSugarRepository<GameFrameX.Entity.Tenant.ClientApiConfig.TenantWechatConfigEntity> repository,
        ILogger<TenantWechatConfigEntityService> logger,
        WechatApiHttpClientFactory wechatApiHttpClientFactory)
        : base(repository)
    {
        _logger = logger;
        _wechatApiHttpClientFactory = wechatApiHttpClientFactory;
    }

    protected override ISugarQueryable<TenantWechatConfigResponse> BuildPageQuery(PageTenantWechatConfigRequest input)
    {
        var searchKey = input.SearchKey?.Trim();
        return Repository.AsQueryable()
                         .WhereIF(!string.IsNullOrWhiteSpace(searchKey), u =>
                                      (u.WechatAppId ?? string.Empty).Contains(searchKey) ||
                                      (u.WxOpenAppId ?? string.Empty).Contains(searchKey) ||
                                      (u.Remark ?? string.Empty).Contains(searchKey))
                         .ApplyAppIdFilter(input.AppId, u => u.AppId)
                         .WhereIF(input.IsActive.HasValue, u => u.IsEnabled == input.IsActive.Value)
                         .Select<TenantWechatConfigResponse>();
    }

    /// <summary>
    /// 新增租户 Wechat 配置。
    /// <para>公众号 / 小程序凭证对至少配置一组且每组成对完整。</para>
    /// </summary>
    public override async Task<TenantWechatConfigResponse> AddAsync(AddTenantWechatConfigRequest input)
    {
        ValidateCredentialPair(input.WechatAppId, input.WechatAppSecret, nameof(input.WechatAppId), nameof(input.WechatAppSecret));
        ValidateCredentialPair(input.WxOpenAppId, input.WxOpenSecret, nameof(input.WxOpenAppId), nameof(input.WxOpenSecret));
        EnsureAtLeastOneCredentialPair(input.WechatAppId, input.WechatAppSecret, input.WxOpenAppId, input.WxOpenSecret);

        var entity = input.Adapt<GameFrameX.Entity.Tenant.ClientApiConfig.TenantWechatConfigEntity>();
        entity.AppId = input.AppId;
        entity.IsEnabled = input.IsEnabled;
        entity.WechatAppSecret = TenantSecretCrypto.Encrypt(entity.WechatAppSecret) ?? string.Empty;
        entity.WxOpenSecret = TenantSecretCrypto.Encrypt(entity.WxOpenSecret) ?? string.Empty;
        await Repository.InsertAsync(entity);
        return entity.Adapt<TenantWechatConfigResponse>();
    }

    /// <summary>
    /// 更新租户 Wechat 配置。
    /// <para>敏感字段回传 <c>******</c> 时保留数据库原值。</para>
    /// </summary>
    public override async Task UpdateAsync(UpdateTenantWechatConfigRequest input)
    {
        var entity = await Repository.GetFirstAsync(e => e.Id == input.Id);
        if (entity == null)
        {
            throw BusinessError.Oh("租户 Wechat 配置不存在");
        }

        var newWechatAppId = input.WechatAppId;
        var newWechatAppSecret = SensitiveFieldMask.PreserveIfMasked(input.WechatAppSecret, entity.WechatAppSecret);
        var newWxOpenAppId = input.WxOpenAppId;
        var newWxOpenAppSecret = SensitiveFieldMask.PreserveIfMasked(input.WxOpenSecret, entity.WxOpenSecret);

        ValidateCredentialPair(newWechatAppId, newWechatAppSecret, nameof(input.WechatAppId), nameof(input.WechatAppSecret));
        ValidateCredentialPair(newWxOpenAppId, newWxOpenAppSecret, nameof(input.WxOpenAppId), nameof(input.WxOpenSecret));
        EnsureAtLeastOneCredentialPair(newWechatAppId, newWechatAppSecret, newWxOpenAppId, newWxOpenAppSecret);

        entity.AppId = input.AppId;
        entity.WechatAppId = newWechatAppId;
        entity.WechatAppSecret = newWechatAppSecret;
        entity.WxOpenAppId = newWxOpenAppId;
        entity.WxOpenSecret = newWxOpenAppSecret;
        // C371：确定最终明文之后、落库之前加密（保留语义下原值可能是存量明文或密文，Encrypt 幂等兼容两者）
        entity.WechatAppSecret = TenantSecretCrypto.Encrypt(entity.WechatAppSecret) ?? string.Empty;
        entity.WxOpenSecret = TenantSecretCrypto.Encrypt(entity.WxOpenSecret) ?? string.Empty;
        entity.Priority = input.Priority;
        entity.IsEnabled = input.IsEnabled;
        entity.Remark = input.Remark;
        entity.RowVersion = input.RowVersion;

        await Repository.AsUpdateable(entity)
                        .UpdateColumns(e => new
                        {
                            e.AppId,
                            e.WechatAppId,
                            e.WechatAppSecret,
                            e.WxOpenAppId,
                            e.WxOpenSecret,
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
    public override async Task<SqlSugarPagedList<TenantWechatConfigResponse>> PageAsync(PageTenantWechatConfigRequest input)
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
    public override async Task<TenantWechatConfigResponse> GetAsync([FromQuery] BaseIdRequest input)
    {
        var entity = await InnerGetAsync(input);
        if (entity == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        var output = entity.Adapt<TenantWechatConfigResponse>();
        MaskSensitiveFields(output);
        return output;
    }

    /// <summary>
    /// 受权限控制的测试发送接口。
    /// <para>按当前配置 <see cref="TenantWechatConfigTestSendRequest.Id"/> 读取公众号 / 小程序凭证，缺租户上下文 / 缺配置 / 禁用 / 字段不完整返回明确错误，禁止 fallback 到静态配置或其它租户配置。</para>
    /// <para>本接口复用租户配置的公众号凭证对发送模板消息，验证凭证有效性 + 模板可达性。小程序订阅消息测试请走 <c>SystemWxOpenService</c>。</para>
    /// <para>执行过程不得向日志 / 异常消息输出 Secret 明文。</para>
    /// </summary>
    [HttpPost]
    [ApiDescriptionSettings(Name = "SendTest")]
    public async Task<bool> SendTestAsync(TenantWechatConfigTestSendRequest input)
    {
        if (input == null)
        {
            throw BusinessError.Oh("测试发送参数不能为空");
        }

        if (!input.Id.HasValue || input.Id.Value <= 0)
        {
            throw BusinessError.Oh("测试发送必须指定租户 Wechat 配置 ID");
        }

        var entity = await Repository.GetFirstAsync(e => e.Id == input.Id.Value && e.IsEnabled == true);
        if (entity == null)
        {
            throw BusinessError.Oh("租户 Wechat 配置不存在或未启用");
        }

        if (string.IsNullOrWhiteSpace(entity.WechatAppId) || string.IsNullOrWhiteSpace(entity.WechatAppSecret))
        {
            throw BusinessError.Oh("租户 Wechat 配置公众号凭证对不完整；测试发送已拦截，禁止跨租户或跨域 fallback。");
        }

        var client = _wechatApiHttpClientFactory.CreateWechatClient(entity.WechatAppId, entity.WechatAppSecret);

        try
        {
            var tokenRes = await client.ExecuteCgibinTokenAsync(new CgibinTokenRequest());
            if (tokenRes.ErrorCode != (int)WechatReturnCodeEnum.请求成功)
            {
                throw BusinessError.Oh($"租户 Wechat 测试发送获取 access_token 失败：{tokenRes.ErrorMessage}");
            }

            var messageData = new Dictionary<string, CgibinMessageTemplateSendRequest.Types.DataItem>();
            foreach (var kv in input.TestData ?? new Dictionary<string, string>())
            {
                messageData[kv.Key] = new CgibinMessageTemplateSendRequest.Types.DataItem { Value = kv.Value ?? string.Empty, };
            }

            var sendRes = await client.ExecuteCgibinMessageTemplateSendAsync(new CgibinMessageTemplateSendRequest
            {
                AccessToken = tokenRes.AccessToken,
                TemplateId = input.TemplateId,
                ToUserOpenId = input.ReceiverOpenId,
                Url = input.Url,
                MiniProgram = new CgibinMessageTemplateSendRequest.Types.MiniProgram
                {
                    AppId = entity.WxOpenAppId ?? entity.WechatAppId,
                    PagePath = input.MiniProgramPagePath,
                },
                Data = messageData,
            });
            if (sendRes.ErrorCode != (int)WechatReturnCodeEnum.请求成功)
            {
                throw BusinessError.Oh($"租户 Wechat 测试发送失败：{sendRes.ErrorMessage} {sendRes.ErrorCode}");
            }

            _logger.LogInformation("租户 Wechat 测试发送成功：id={Id}, appId={AppId}, receiver={Receiver}, templateId={TemplateId}, msgId={MsgId}", entity.Id, entity.AppId, input.ReceiverOpenId, input.TemplateId, sendRes.MessageId);
            return true;
        }
        catch (Exception ex) when (ex is not BusinessException)
        {
            _logger.LogError(ex, "租户 Wechat 测试发送异常：id={Id}, appId={AppId}, receiver={Receiver}, templateId={TemplateId}", entity.Id, entity.AppId, input.ReceiverOpenId, input.TemplateId);
            throw BusinessError.Oh($"租户 Wechat 测试发送失败：{ex.Message}");
        }
    }

    /// <summary>
    /// 对输出对象进行敏感字段脱敏（Output 已拆为纯 POCO，直接操作输出字段）。
    /// </summary>
    private static void MaskSensitiveFields(TenantWechatConfigResponse output)
    {
        if (output == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(output.WechatAppSecret))
        {
            output.WechatAppSecret = SensitiveFieldMask.MaskValue;
        }

        if (!string.IsNullOrEmpty(output.WxOpenSecret))
        {
            output.WxOpenSecret = SensitiveFieldMask.MaskValue;
        }
    }

    /// <summary>
    /// 校验单组凭证是否成对完整（两者要么同时填写，要么同时为空；不允许只填一半）。
    /// </summary>
    private static void ValidateCredentialPair(string appId, string appSecret, string appIdFieldName, string appSecretFieldName)
    {
        var hasAppId = !string.IsNullOrWhiteSpace(appId);
        var hasSecret = !string.IsNullOrWhiteSpace(appSecret);
        if (hasAppId != hasSecret)
        {
            throw BusinessError.Oh($"{appIdFieldName} 与 {appSecretFieldName} 必须同时填写或同时为空，禁止半填。");
        }
    }

    /// <summary>
    /// 校验公众号 / 小程序两组凭证至少配置一组。
    /// </summary>
    private static void EnsureAtLeastOneCredentialPair(string wechatAppId, string wechatAppSecret, string wxOpenAppId, string wxOpenAppSecret)
    {
        var wechatPairReady = !string.IsNullOrWhiteSpace(wechatAppId) && !string.IsNullOrWhiteSpace(wechatAppSecret);
        var wxOpenPairReady = !string.IsNullOrWhiteSpace(wxOpenAppId) && !string.IsNullOrWhiteSpace(wxOpenAppSecret);
        if (!wechatPairReady && !wxOpenPairReady)
        {
            throw BusinessError.Oh("公众号 / 小程序凭证对至少配置一组，且每组必须成对完整。");
        }
    }
}
#pragma warning restore CS8604, CS8629