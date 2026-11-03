using GameFrameX.Advertisement.Abstract;

namespace GameFrameX.Client.Api.Services.ClientApiTenantOptions;

/// <summary>
/// Client API 主库请求期业务配置 Provider。
/// </summary>
public interface IClientApiTenantOptionsProvider
{
    /// <summary>
    /// 读取租户业务邮件配置；未配置返回 null，调用方必须返回明确错误，不得 fallback 到系统邮件。
    /// </summary>
    Task<TenantEmailOptions> GetEmailOptionsAsync();

    /// <summary>
    /// 读取租户广告回调验签配置；未配置返回 null。
    /// </summary>
    Task<TenantAdvertisementCallbackOptions> GetAdvertisementCallbackOptionsAsync();

    /// <summary>
    /// 读取 Client API 访问令牌策略；有效期已受平台默认与上限钳制，SigningKey 未配置时为 null。
    /// </summary>
    Task<TenantClientApiTokenPolicyOptions> GetTokenPolicyAsync();

    /// <summary>
    /// 读取 Game 登录票据策略（非 key 部分）；有效期已受平台范围钳制，未配置时返回平台默认值。
    /// </summary>
    Task<TenantGameLoginTicketPolicyOptions> GetGameLoginTicketPolicyAsync();

    /// <summary>
    /// 读取订单 / 支付测试策略；未配置返回默认（关闭沙箱充值发货）。
    /// </summary>
    Task<TenantOrderPolicyOptions> GetOrderPolicyAsync();

    /// <summary>
    /// 读取区服信息输出策略；未配置返回默认（不返回 HttpManageUrl）。
    /// </summary>
    Task<TenantAreaPolicyOptions> GetAreaPolicyAsync();
}