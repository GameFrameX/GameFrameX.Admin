using System.Reflection;
using GameFrameX.Application.Game.Order.GamePaymentConfig;
using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.AliPay;
using GameFrameX.Payment.Apple.Option;
using GameFrameX.Payment.Google.Option;
using GameFrameX.Payment.Wechat;
using GameFrameX.Payment.Xsolla.Option;

namespace GameFrameX.Core.Tests.PayConfig;

public class PaymentConfigSchemaTests
{
    private readonly PaymentConfigSchemaProvider _provider = new();

    public static IEnumerable<object[]> Channels()
    {
        yield return new object[] { "apple", typeof(ApplePaymentOptionSetting), };
        yield return new object[] { "google", typeof(GooglePaymentOptionSetting), };
        yield return new object[] { "xsolla", typeof(XsollaPayOptionSetting), };
        yield return new object[] { "alipay", typeof(AliPayOptionSetting), };
        yield return new object[] { "wechat", typeof(WechatPayOptionSetting), };
    }

    [Theory]
    [MemberData(nameof(Channels))]
    public void Schema_FieldsMatchOptionWritableProperties(string channel, Type optionType)
    {
        var schema = _provider.GetSchema(channel);

        Assert.NotNull(schema);
        var expectedKeys = optionType
                           .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                           .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0)
                           .Select(p => p.Name)
                           .OrderBy(name => name)
                           .ToList();
        var actualKeys = schema.Fields.Select(f => f.Key).OrderBy(name => name).ToList();

        Assert.Equal(expectedKeys, actualKeys);
    }

    [Fact]
    public void Schema_AppleExcludesReadOnlyComputedProperties()
    {
        var schema = _provider.GetSchema("apple");

        Assert.NotNull(schema);
        var keys = schema.Fields.Select(f => f.Key).ToList();
        Assert.DoesNotContain("IsSandbox", keys);
        Assert.DoesNotContain("VerifyReceiptUrl", keys);
        Assert.DoesNotContain("ServerApiBaseUrl", keys);
    }

    [Fact]
    public void GetChannels_ReturnsAllFiveChannels()
    {
        var channels = _provider.GetChannels();

        var values = channels.Select(c => c.Value).ToList();
        Assert.Contains("apple", values);
        Assert.Contains("google", values);
        Assert.Contains("xsolla", values);
        Assert.Contains("alipay", values);
        Assert.Contains("wechat", values);
        Assert.Equal(5, values.Count);
    }

    [Fact]
    public void ResolveType_MapsFieldTypesCorrectly()
    {
        var apple = _provider.GetSchema("apple");
        var xsolla = _provider.GetSchema("xsolla");

        Assert.NotNull(apple);
        Assert.NotNull(xsolla);
        Assert.Equal("secret", Field(apple, "ApplePaymentSharedSecret").Type);
        Assert.Equal("multiline", Field(apple, "ApplePaymentRootCertificate").Type);
        Assert.Equal("bool", Field(apple, "ApplePaymentUseServerApiV2").Type);
        Assert.Equal("select", Field(apple, "ApplePaymentEnvironment").Type);
        Assert.Equal("number", Field(xsolla, "XsollaPaymentMerchantId").Type);
    }

    [Fact]
    public void ResolveType_MultilineTakesPrecedenceOverSensitive()
    {
        // 多行密钥内容（PEM/JSON）必须用 multiline 渲染，不能退化成单行 secret
        var alipay = _provider.GetSchema("alipay");
        var google = _provider.GetSchema("google");

        Assert.NotNull(alipay);
        Assert.NotNull(google);
        Assert.Equal("multiline", Field(alipay, "AliPayMerchantPrivateKey").Type);
        Assert.Equal("multiline", Field(alipay, "AliPayPublicKey").Type);
        Assert.Equal("multiline", Field(google, "GooglePaymentServiceAccountKey").Type);
    }

    [Fact]
    public void GoogleSchema_IncludesPubSubPushAuthenticationFields()
    {
        var google = _provider.GetSchema("google");

        Assert.NotNull(google);
        Assert.True(Field(google, "GooglePaymentPubSubPushAudience").Required);
        Assert.True(Field(google, "GooglePaymentPubSubPushServiceAccountEmail").Required);
    }

    [Fact]
    public void GetSchema_ReturnsNullForUnknownChannel()
    {
        Assert.Null(_provider.GetSchema("unknown"));
    }

    [Fact]
    public void GetSchema_IsCaseInsensitive()
    {
        var schema = _provider.GetSchema("APPLE");

        Assert.NotNull(schema);
        Assert.Equal("apple", schema.Channel);
    }

    [Fact]
    public void Registry_RegistersAllAnnotatedOptions()
    {
        // 锁死：5 个 Option 类都被 [PaymentChannel] 标注、Name 唯一、类型绑定正确。
        // 加渠道漏标 Attribute 或 Name 拼错都会让本断言失败。
        var names = PaymentChannelRegistry.GetChannelNames()
                                          .Select(n => n.ToLowerInvariant())
                                          .OrderBy(n => n)
                                          .ToList();

        Assert.Equal(new[] { "alipay", "apple", "google", "wechat", "xsolla", }, names);
        Assert.Equal(typeof(ApplePaymentOptionSetting), PaymentChannelRegistry.GetOptionType("apple"));
        Assert.Equal(typeof(GooglePaymentOptionSetting), PaymentChannelRegistry.GetOptionType("google"));
        Assert.Equal(typeof(XsollaPayOptionSetting), PaymentChannelRegistry.GetOptionType("xsolla"));
        Assert.Equal(typeof(AliPayOptionSetting), PaymentChannelRegistry.GetOptionType("alipay"));
        Assert.Equal(typeof(WechatPayOptionSetting), PaymentChannelRegistry.GetOptionType("wechat"));
    }

    [Fact]
    public void Registry_NamesAlignWithHandlerChannelNames()
    {
        // 反射扫描所有 IPaymentChannelHandler 实现，实例化后读 ChannelName，
        // 断言每个 Handler 声明的渠道名都被 Registry 注册——
        // 防止新增 Handler 却漏标 Option 类的 [PaymentChannel]（即绑定漂移）。
        var handlerNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var an = assembly.GetName().Name;
            if (string.IsNullOrEmpty(an) || !an.StartsWith("GameFrameX.Payment", StringComparison.Ordinal))
            {
                continue;
            }

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException)
            {
                continue;
            }

            foreach (var t in types)
            {
                if (t.IsAbstract || t.IsInterface)
                {
                    continue;
                }

                if (!typeof(IPaymentChannelHandler).IsAssignableFrom(t))
                {
                    continue;
                }

                object? instance;
                try
                {
                    instance = Activator.CreateInstance(t);
                }
                catch
                {
                    // 构造失败的 Handler 跳过，不阻断对齐断言
                    continue;
                }

                if (instance == null)
                {
                    // 反射返回 null（罕见场景），跳过不阻断对齐断言
                    continue;
                }

                var name = ((IPaymentChannelHandler)instance).ChannelName;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    handlerNames.Add(name);
                }
            }
        }

        Assert.True(handlerNames.Count >= 1, "未发现任何 IPaymentChannelHandler 实现，反射扫描可能失效");
        foreach (var name in handlerNames)
        {
            Assert.True(PaymentChannelRegistry.TryGet(name, out _),
                        $"Handler 声明渠道 '{name}'，但 Registry 未注册（对应 Option 类漏标 [PaymentChannel] 或 Name 与 Handler.ChannelName 不一致）");
        }
    }

    private static PaymentConfigField Field(PaymentConfigSchema schema, string key)
    {
        return schema.Fields.First(f => f.Key == key);
    }
}