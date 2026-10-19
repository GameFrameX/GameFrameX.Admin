using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.Abstract.Interface;
using GameFrameX.Payment.Abstract.Models;

namespace GameFrameX.Core.Tests.ClientApi;

public class PaymentChannelFactoryTests
{
    [Fact]
    public void IsSupported_MatchesChannelNamesCaseInsensitively()
    {
        var factory = CreateFactory(
            new TestPaymentChannelHandler("apple"),
            new TestPaymentChannelHandler("Google"));

        Assert.True(factory.IsSupported("APPLE"));
        Assert.True(factory.IsSupported("google"));
        Assert.False(factory.IsSupported("xsolla"));
    }

    [Fact]
    public void GetHandler_ReturnsRegisteredHandlerIgnoringCase()
    {
        var appleHandler = new TestPaymentChannelHandler("apple");
        var factory = CreateFactory(appleHandler);

        var handler = factory.GetHandler("APPLE");

        Assert.Same(appleHandler, handler);
    }

    [Fact]
    public void GetOptionSetting_DelegatesToProvider()
    {
        var appleSetting = new TestPaymentOptionSetting();
        var provider = new TestPaymentOptionSettingProvider(new Dictionary<string, IPaymentOptionSetting>
        {
            ["apple"] = appleSetting,
        });
        var factory = new PaymentChannelFactory(Array.Empty<IPaymentChannelHandler>(), provider);

        var setting = factory.GetOptionSetting("APPLE");

        Assert.Same(appleSetting, setting);
    }

    [Fact]
    public void GetOptionSettings_DelegatesToProvider()
    {
        var appleSetting = new TestPaymentOptionSetting();
        var provider = new TestPaymentOptionSettingProvider(new Dictionary<string, IPaymentOptionSetting>
        {
            ["apple"] = appleSetting,
        });
        var factory = new PaymentChannelFactory(Array.Empty<IPaymentChannelHandler>(), provider);

        var settings = factory.GetOptionSettings("APPLE").ToArray();

        Assert.Single(settings);
        Assert.Same(appleSetting, settings[0]);
    }

    private static PaymentChannelFactory CreateFactory(params IPaymentChannelHandler[] handlers)
    {
        return new PaymentChannelFactory(handlers, new TestPaymentOptionSettingProvider());
    }

    private sealed class TestPaymentChannelHandler : IPaymentChannelHandler
    {
        public TestPaymentChannelHandler(string channelName)
        {
            ChannelName = channelName;
        }

        public string ChannelName { get; }

        public Task<PaymentChannelCreateOrderResponse> CreateOrderAsync(IGameOrderBase entity, object input, object user, HttpClient httpClient, IPaymentOptionSetting optionSetting)
        {
            throw new NotSupportedException();
        }

        public Task<PaymentChannelVerifyReceiptResponse> VerifyReceiptAsync(IGameOrderBase entity, GameFrameX.Payment.Abstract.Interface.ITransactionData transactionData, HttpClient httpClient, IPaymentOptionSetting optionSetting)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class TestPaymentOptionSettingProvider : IPaymentOptionSettingProvider
    {
        private readonly Dictionary<string, IPaymentOptionSetting> _settings;

        public TestPaymentOptionSettingProvider()
            : this(new Dictionary<string, IPaymentOptionSetting>())
        {
        }

        public TestPaymentOptionSettingProvider(Dictionary<string, IPaymentOptionSetting> settings)
        {
            _settings = new Dictionary<string, IPaymentOptionSetting>(settings, StringComparer.OrdinalIgnoreCase);
        }

        public IPaymentOptionSetting GetOptionSetting(string channelName)
        {
            _settings.TryGetValue(channelName, out var setting);
#pragma warning disable CS8603 // 测试桩：TryGetValue 未命中返回 null，与 IPaymentOptionSettingProvider 契约一致
            return setting;
#pragma warning restore CS8603
        }

        public IEnumerable<IPaymentOptionSetting> GetOptionSettings(string channelName)
        {
            var setting = GetOptionSetting(channelName);
            return setting == null ? Array.Empty<IPaymentOptionSetting>() : new[] { setting, };
        }

        public T GetOptionSetting<T>(string channelName) where T : class, IPaymentOptionSetting
        {
#pragma warning disable CS8603 // 测试桩：as 转换失败返回 null，与契约一致
            return GetOptionSetting(channelName) as T;
#pragma warning restore CS8603
        }

        public IEnumerable<T> GetOptionSettings<T>(string channelName) where T : class, IPaymentOptionSetting
        {
            return GetOptionSettings(channelName).OfType<T>();
        }
    }

    private sealed class TestPaymentOptionSetting : IPaymentOptionSetting
    {
    }
}