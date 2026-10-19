namespace GameFrameX.Advertisement.Abstract;

/// <summary>
/// 广告回调配置 schema 提供器。
/// <para>与支付配置页面一致，按 Provider 返回动态字段定义，前端据此渲染表单，后端据此校验 ConfigJson。</para>
/// </summary>
public interface IAdvertisementCallbackConfigSchemaProvider
{
    /// <summary>
    /// 获取指定广告 Provider 的配置 schema。
    /// </summary>
    AdvertisementCallbackConfigSchema? GetSchema(string provider);

    /// <summary>
    /// 获取所有首批支持的广告 Provider。
    /// </summary>
    List<AdvertisementCallbackProviderItem> GetProviders();
}