namespace GameFrameX.Advertisement.Abstract;

/// <summary>
/// 广告回调处理器工厂（照搬 <c>IPaymentChannelFactory</c>）。
/// </summary>
public interface IAdvertisementCallbackFactory
{
    IAdvertisementCallbackHandler? GetHandler(string providerName);
    bool IsSupported(string providerName);
    IEnumerable<string> GetSupportedProviders();
}