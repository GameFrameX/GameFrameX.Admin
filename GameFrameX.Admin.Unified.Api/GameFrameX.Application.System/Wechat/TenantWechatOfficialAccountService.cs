using GameFrameX.Wechat.Open.Dto;
using GameFrameX.Wechat.Abstract;
using GameFrameX.Core.Service.Config;
using GameFrameX.Core.Service.PlatformJwt;
using GameFrameX.Entity.Enum;
using GameFrameX.Foundation.Http.Normalization;
using SKIT.FlurlHttpClient.Wechat.Api;
using SKIT.FlurlHttpClient.Wechat.Api.Models;

namespace GameFrameX.Application.System.Wechat;

/// <summary>
/// 租户微信公众号能力服务（JS-SDK 签名、模板消息）。
/// <para>GFX-706 更名豁免说明：原 SystemWechatService 实管租户公众号能力，按名实对齐本应更名
/// TenantWechatUserService，但该名已被同命名空间的微信账号服务（TenantWechatUserEntity 用户 CRUD）占用，
/// 故采用语义精确的 TenantWechatOfficialAccountService（租户公众号）。同时移除死注入
/// _systemWechatUserRepository（仅声明与构造赋值，0 处使用）。</para>
/// </summary>
[ApiDescriptionSettings(Order = 230)]
public class TenantWechatOfficialAccountService : IAdminApiController
{
    private readonly SystemConfigService _systemConfigService;
    private readonly PlatformJwtTokenService _platformJwtTokenService;
    private readonly WechatApiHttpClientFactory _wechatApiHttpClientFactory;
    private readonly TenantWechatConfigResolver _tenantWechatConfigResolver;

    public TenantWechatOfficialAccountService(SystemConfigService systemConfigService,
        WechatApiHttpClientFactory wechatApiHttpClientFactory,
        TenantWechatConfigResolver tenantWechatConfigResolver,
        PlatformJwtTokenService platformJwtTokenService)
    {
        _systemConfigService = systemConfigService;
        _platformJwtTokenService = platformJwtTokenService;
        _wechatApiHttpClientFactory = wechatApiHttpClientFactory;
        _tenantWechatConfigResolver = tenantWechatConfigResolver;
    }

    /// <summary>
    /// 获取配置签名参数(wx.config)。
    /// <para>Admin 会话本身无隐式 AppId；调用方必须显式传入 <c>appId</c>，resolver 仅在 <c>appId=0</c> 时选择租户默认公众号配置。</para>
    /// </summary>
    /// <param name="input">签名参数请求；<c>AppId</c> &gt; 0 选择应用级配置。</param>
    [DisplayName("获取配置签名参数(wx.config)")]
    [ApiDescriptionSettings(Name = "GenConfigPara")]
    public async Task<HttpJsonResultData<WechatJsConfigResponse>> GenConfigParaAsync(WechatSignatureRequest input)
    {
        var wechatApiClient = (await CreateWechatClientAsync(input.AppId)).Client;
        var resCgibinToken = await wechatApiClient.ExecuteCgibinTokenAsync(new CgibinTokenRequest());
        var request = new CgibinTicketGetTicketRequest
        {
            AccessToken = resCgibinToken.AccessToken,
        };
        var response = await wechatApiClient.ExecuteCgibinTicketGetTicketAsync(request);
        if (!response.IsSuccessful())
        {
            throw BusinessError.Oh(response.ErrorMessage ?? "未知错误");
        }

        var jsConfigParameters = wechatApiClient.GenerateParametersForJSSDKConfig(response.Ticket, input.Url);
        return HttpJsonResultData<WechatJsConfigResponse>.Success(new WechatJsConfigResponse
        {
            AppId = jsConfigParameters.TryGetValue("appId", out var appId) ? appId : string.Empty,
            Timestamp = jsConfigParameters.TryGetValue("timestamp", out var timestamp) ? timestamp : string.Empty,
            NonceStr = jsConfigParameters.TryGetValue("nonceStr", out var nonceStr) ? nonceStr : string.Empty,
            Signature = jsConfigParameters.TryGetValue("signature", out var signature) ? signature : string.Empty,
        });
    }

    /// <summary>
    /// 获取模板列表。
    /// </summary>
    /// <param name="input">请求参数；<c>AppId</c> &gt; 0 选择应用级配置。</param>
    [DisplayName("获取模板列表")]
    [ApiDescriptionSettings(Name = "GetMessageTemplateList")]
    public async Task<HttpJsonResultData<List<WechatTemplateResponse>>> GetMessageTemplateListAsync(WechatAppIdRequest input)
    {
        var wechatApiClient = (await CreateWechatClientAsync(input.AppId)).Client;
        var accessToken = await GetCgibinTokenAsync(wechatApiClient);
        var reqTemplate = new CgibinTemplateGetAllPrivateTemplateRequest
        {
            AccessToken = accessToken,
        };
        var resTemplate = await wechatApiClient.ExecuteCgibinTemplateGetAllPrivateTemplateAsync(reqTemplate);
        if (resTemplate.ErrorCode != (int)WechatReturnCodeEnum.请求成功)
        {
            throw BusinessError.Oh(resTemplate.ErrorMessage + " " + resTemplate.ErrorCode);
        }

        return HttpJsonResultData<List<WechatTemplateResponse>>.Success(resTemplate.TemplateList.Select(template => new WechatTemplateResponse
        {
            TemplateId = template.TemplateId,
            Title = template.Title,
            PrimaryIndustry = template.PrimaryIndustry,
            SecondaryIndustry = template.SecondaryIndustry,
            Content = template.Content,
            Example = template.Example,
        }).ToList());
    }

    /// <summary>
    /// 发送模板消息。
    /// <para>使用当前租户公众号凭证创建客户端；跳转小程序的 AppId 从同一条配置读取，不复用其他租户配置。</para>
    /// </summary>
    /// <param name="input">模板消息请求；<c>AppId</c> &gt; 0 选择应用级配置。</param>
    [DisplayName("发送模板消息")]
    [ApiDescriptionSettings(Name = "SendTemplateMessage")]
    public async Task<HttpJsonResultData<WechatSendResultResponse>> SendTemplateMessageAsync(WechatMessageTemplateSendRequest input)
    {
        var dataInfo = input.Data.ToDictionary(k => k.Key, k => k.Value);
        var messageData = new global::System.Collections.Generic.Dictionary<string, CgibinMessageTemplateSendRequest.Types.DataItem>();
        foreach (var item in dataInfo)
        {
            messageData.Add(item.Key, new CgibinMessageTemplateSendRequest.Types.DataItem { Value = "" + item.Value.Value.ToString() + "", });
        }

        var wechatClientContext = await CreateWechatClientAsync(input.AppId);
        var wechatApiClient = wechatClientContext.Client;
        var config = wechatClientContext.Config;
        var accessToken = await GetCgibinTokenAsync(wechatApiClient);
        var reqMessage = new CgibinMessageTemplateSendRequest
        {
            AccessToken = accessToken,
            TemplateId = input.TemplateId,
            ToUserOpenId = input.ToUserOpenId,
            Url = input.Url,
            MiniProgram = new CgibinMessageTemplateSendRequest.Types.MiniProgram
            {
                AppId = config.WechatAppId,
                PagePath = input.MiniProgramPagePath,
            },
            Data = messageData,
        };
        var resMessage = await wechatApiClient.ExecuteCgibinMessageTemplateSendAsync(reqMessage);
        return HttpJsonResultData<WechatSendResultResponse>.Success(new WechatSendResultResponse
        {
            MessageId = resMessage.MessageId,
            ErrorCode = resMessage.ErrorCode,
            ErrorMessage = resMessage.ErrorMessage,
        });
    }

    /// <summary>
    /// 删除模板。
    /// </summary>
    /// <param name="input">删除模板请求；<c>AppId</c> &gt; 0 选择应用级配置。</param>
    [ApiDescriptionSettings(Name = "DeleteMessageTemplate")]
    [HttpPost]
    [DisplayName("删除模板")]
    public async Task<HttpJsonResultData<WechatDeleteResultResponse>> DeleteMessageTemplateAsync(WechatDeleteMessageTemplateRequest input)
    {
        var wechatApiClient = (await CreateWechatClientAsync(input.AppId)).Client;
        var accessToken = await GetCgibinTokenAsync(wechatApiClient);
        var reqMessage = new CgibinTemplateDeletePrivateTemplateRequest
        {
            AccessToken = accessToken,
            TemplateId = input.TemplateId,
        };
        var resTemplate = await wechatApiClient.ExecuteCgibinTemplateDeletePrivateTemplateAsync(reqMessage);
        return HttpJsonResultData<WechatDeleteResultResponse>.Success(new WechatDeleteResultResponse
        {
            ErrorCode = resTemplate.ErrorCode,
            ErrorMessage = resTemplate.ErrorMessage,
        });
    }

    private async Task<WechatClientContext> CreateWechatClientAsync(long? appId)
    {
        var config = await _tenantWechatConfigResolver.ResolveAsync(appId, WechatCredentialKind.Wechat);
        return new WechatClientContext(_wechatApiHttpClientFactory.CreateWechatClient(config.WechatAppId, config.WechatAppSecret), config);
    }

    /// <summary>
    /// 获取Access_token
    /// </summary>
    private static async Task<string> GetCgibinTokenAsync(WechatApiClient wechatApiClient)
    {
        var reqCgibinToken = new CgibinTokenRequest();
        var resCgibinToken = await wechatApiClient.ExecuteCgibinTokenAsync(reqCgibinToken);
        if (resCgibinToken.ErrorCode != (int)WechatReturnCodeEnum.请求成功)
        {
            throw BusinessError.Oh(resCgibinToken.ErrorMessage + " " + resCgibinToken.ErrorCode);
        }

        return resCgibinToken.AccessToken;
    }
}