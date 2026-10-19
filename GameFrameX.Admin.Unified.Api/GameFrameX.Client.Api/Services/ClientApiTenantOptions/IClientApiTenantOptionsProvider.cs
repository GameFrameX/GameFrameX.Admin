using GameFrameX.Advertisement.Abstract;

namespace GameFrameX.Client.Api.Services.ClientApiTenantOptions;

/// <summary>
/// Client API 请求期租户配置 Provider。
/// <para>按当前请求的 <c>TenantId</c>（来自 <c>TenantContextAccessor.TenantIdKey</c>）与 <c>AppId</c>（来自 <c>AppResolutionMiddleware.AppIdKey</c>）</para>
/// <para>从租户库读取业务配置；作用域优先级：应用级（TenantId + AppId）&gt; 租户默认（TenantId, AppId=0）。</para>
/// <para>缺少租户上下文时，对所有读取方法抛出 <see cref="InvalidOperationException"/>，禁止回落主库或进程级业务参数。</para>
/// </summary>
public interface IClientApiTenantOptionsProvider
{
    /// <summary>
    /// 解析当前请求作用域的租户身份。
    /// </summary>
    /// <returns>租户上下文；缺少 TenantId 时抛出异常。</returns>
    ClientApiTenantResolution ResolveContext();

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