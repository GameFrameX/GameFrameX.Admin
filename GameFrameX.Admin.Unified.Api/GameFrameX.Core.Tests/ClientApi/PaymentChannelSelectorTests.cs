using GameFrameX.Payment.Abstract;

namespace GameFrameX.Core.Tests.ClientApi;

public class PaymentChannelSelectorTests
{
    [Fact]
    public void SelectBestChannel_UsesPreferredChannelWhenAvailable()
    {
        var selector = CreateSelector(
            new[] { "apple", "google", },
            new[] { "apple", "google", });

        var channel = selector.SelectBestChannel(platform: "iOS", preferredChannel: "APPLE");

        Assert.Equal("APPLE", channel);
    }

    [Fact]
    public void SelectBestChannel_SkipsPreferredChannelWhenBackendConfigDoesNotExist()
    {
        var selector = CreateSelector(
            new[] { "apple", "google", },
            new[] { "google", });

        var channel = selector.SelectBestChannel(platform: "Android", preferredChannel: "apple");

        Assert.Equal("google", channel);
    }

    [Fact]
    public void GetAvailableChannels_ReturnsSupportedChannelsWithBackendConfig()
    {
        var selector = CreateSelector(
            new[] { "apple", "google", "xsolla", "alipay", },
            new[] { "apple", "xsolla", });

        var channels = selector.GetAvailableChannels("us", "ios").ToArray();

        Assert.Equal(new[] { "apple", "xsolla", }, channels);
    }

    [Fact]
    public void SelectBestChannel_ReturnsFirstAvailableSupportedChannel()
    {
        var selector = CreateSelector(
            new[] { "apple", "google", "xsolla", },
            new[] { "google", "xsolla", });

        var channel = selector.SelectBestChannel();

        Assert.Equal("google", channel);
    }

    private static PaymentChannelSelector CreateSelector(IEnumerable<string> supportedChannels, IEnumerable<string> configuredChannels)
    {
        var factory = new TestPaymentChannelFactory(supportedChannels, configuredChannels);
        return new PaymentChannelSelector(factory);
    }

    private sealed class TestPaymentChannelFactory : IPaymentChannelFactory
    {
        private readonly List<string> _supportedChannels;
        private readonly HashSet<string> _configuredChannels;

        public TestPaymentChannelFactory(IEnumerable<string> supportedChannels, IEnumerable<string> configuredChannels)
        {
            _supportedChannels = supportedChannels.ToList();
            _configuredChannels = new HashSet<string>(configuredChannels, StringComparer.OrdinalIgnoreCase);
        }

        public IPaymentChannelHandler GetHandler(string channelName)
        {
            throw new NotSupportedException();
        }

        public bool IsSupported(string channelName)
        {
            return _supportedChannels.Contains(channelName, StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerable<string> GetSupportedChannels()
        {
            return _supportedChannels;
        }

        public IPaymentOptionSetting GetOptionSetting(string channelName)
        {
#pragma warning disable CS8603 // 测试桩：未配置渠道返回 null，与 IPaymentOptionSettingProvider 契约一致
            return _configuredChannels.Contains(channelName) ? new TestPaymentOptionSetting() : null;
#pragma warning restore CS8603
        }

        public IEnumerable<IPaymentOptionSetting> GetOptionSettings(string channelName)
        {
            var setting = GetOptionSetting(channelName);
            return setting == null ? Array.Empty<IPaymentOptionSetting>() : new[] { setting, };
        }
    }

    private sealed class TestPaymentOptionSetting : IPaymentOptionSetting
    {
    }
}