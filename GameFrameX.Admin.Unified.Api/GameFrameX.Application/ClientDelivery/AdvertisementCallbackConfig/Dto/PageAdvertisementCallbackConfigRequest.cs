namespace GameFrameX.Application.ClientDelivery.AdvertisementCallbackConfig.Dto;

/// <summary>
/// 广告回调验签配置分页查询输入参数
/// </summary>
public class PageAdvertisementCallbackConfigRequest : BaseAppActivePageRequest
{
    /// <summary>
    /// 广告平台 Provider。
    /// </summary>
    public string Provider { get; set; } = null!;
}