namespace GameFrameX.Application.Game.Tenant.Integration.TenantAdvertisementCallbackConfig.Dto;

/// <summary>
/// 租户广告回调验签配置分页查询输入参数
/// </summary>
public class PageTenantAdvertisementCallbackConfigRequest : BaseAppActivePageRequest
{
    /// <summary>
    /// 广告平台 Provider。
    /// </summary>
    public string Provider { get; set; } = null!;
}