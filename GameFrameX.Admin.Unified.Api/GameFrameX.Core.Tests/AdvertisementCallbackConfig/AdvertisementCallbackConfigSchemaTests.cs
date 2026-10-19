using GameFrameX.Advertisement.Abstract;
using GameFrameX.Advertisement.AdMob;
using GameFrameX.Advertisement.Pangle;
using GameFrameX.Advertisement.TopOn;
using GameFrameX.Advertisement.UnityAds;
using System.Reflection;

namespace GameFrameX.Core.Tests.AdvertisementCallbackConfig;

public class AdvertisementCallbackConfigSchemaTests
{
    private readonly AdvertisementCallbackConfigSchemaProvider _provider = new();

    [Fact]
    public void GetProviders_ReturnsOnlyFirstPublicProviders()
    {
        var values = _provider.GetProviders().Select(item => item.Value).OrderBy(value => value).ToList();

        Assert.Equal(new[] { "GoogleAdMob", "Pangle", "TopOn", "UnityAds", }, values);
    }

    [Fact]
    public void Registry_RegistersOnlyFirstPublicProviders()
    {
        var values = AdvertisementCallbackProviderRegistry.GetProviderNames().OrderBy(value => value).ToList();

        Assert.Equal(new[] { "GoogleAdMob", "Pangle", "TopOn", "UnityAds", }, values);
    }

    [Theory]
    [MemberData(nameof(ProviderOptionTypes))]
    public void Schema_FieldsMatchOptionWritableProperties(string provider, Type optionType)
    {
        var schema = _provider.GetSchema(provider);
        var expected = optionType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(property => property.CanWrite && property.GetIndexParameters().Length == 0)
                                 .Where(property => property.GetCustomAttribute<AdvertisementCallbackFieldAttribute>()?.Ignore != true)
                                 .Select(property => property.Name)
                                 .ToList();

        Assert.NotNull(schema);
        Assert.Equal(expected, schema.Fields.Select(field => field.Key).ToList());
    }

    [Theory]
    [MemberData(nameof(ProviderFieldKeys))]
    public void Schema_PreservesC15FieldKeys(string provider, string[] expectedKeys)
    {
        var schema = _provider.GetSchema(provider);

        Assert.NotNull(schema);
        Assert.Equal(expectedKeys, schema.Fields.Select(field => field.Key).ToArray());
    }

    [Theory]
    [InlineData("TopOn", "userIdParam", "user_id")]
    [InlineData("TopOn", "transactionIdParam", "trans_id")]
    [InlineData("TopOn", "signParam", "sign")]
    [InlineData("GoogleAdMob", "customDataParam", "custom_data")]
    [InlineData("GoogleAdMob", "signatureParam", "signature")]
    [InlineData("Pangle", "transactionIdParam", "TransId")]
    [InlineData("Pangle", "signParam", "Sign")]
    [InlineData("UnityAds", "orderIdParam", "oid")]
    [InlineData("UnityAds", "hmacParam", "hmac")]
    public void Schema_PreservesC15DefaultValues(string provider, string key, string expectedDefault)
    {
        var schema = _provider.GetSchema(provider);
        var field = Field(schema, key);

        Assert.Equal(expectedDefault, field.Default);
    }

    [Fact]
    public void GoogleAdMobVerificationKeys_UsesMultilineSensitiveField()
    {
        var schema = _provider.GetSchema("GoogleAdMob");
        var field = Field(schema, "verificationKeys");

        Assert.True(field.Required);
        Assert.True(field.Sensitive);
        Assert.Equal("multiline", field.Type);
    }

    [Theory]
    [InlineData("Pangle", "signingSecret")]
    [InlineData("TopOn", "signingSecret")]
    [InlineData("UnityAds", "signingSecret")]
    public void SecretFields_AreSensitive(string provider, string key)
    {
        var schema = _provider.GetSchema(provider);
        var field = Field(schema, key);

        Assert.Equal("secret", field.Type);
        Assert.True(field.Sensitive);
    }

    [Fact]
    public void GetSchema_ReturnsNullForUnknownProvider()
    {
        Assert.Null(_provider.GetSchema("UnknownProvider"));
    }

    public static IEnumerable<object[]> ProviderOptionTypes()
    {
        yield return new object[] { "TopOn", typeof(TopOnAdvertisementCallbackOption), };
        yield return new object[] { "GoogleAdMob", typeof(GoogleAdMobAdvertisementCallbackOption), };
        yield return new object[] { "Pangle", typeof(PangleAdvertisementCallbackOption), };
        yield return new object[] { "UnityAds", typeof(UnityAdsAdvertisementCallbackOption), };
    }

    public static IEnumerable<object[]> ProviderFieldKeys()
    {
        yield return new object[]
        {
            "TopOn",
            new[]
            {
                "callbackUrl",
                "signingSecret",
                "userIdParam",
                "extraDataParam",
                "transactionIdParam",
                "rewardNameParam",
                "rewardAmountParam",
                "placementIdParam",
                "adsourceIdParam",
                "networkFirmIdParam",
                "signParam",
            },
        };
        yield return new object[]
        {
            "GoogleAdMob",
            new[]
            {
                "callbackUrl",
                "verificationKeys",
                "userIdParam",
                "customDataParam",
                "transactionIdParam",
                "rewardItemParam",
                "rewardAmountParam",
                "adUnitParam",
                "adNetworkParam",
                "keyIdParam",
                "signatureParam",
            },
        };
        yield return new object[]
        {
            "Pangle",
            new[]
            {
                "callbackUrl",
                "signingSecret",
                "transactionIdParam",
                "signParam",
                "rewardAmountParam",
                "rewardNameParam",
                "extraParam",
            },
        };
        yield return new object[]
        {
            "UnityAds",
            new[]
            {
                "callbackUrl",
                "signingSecret",
                "sidParam",
                "orderIdParam",
                "hmacParam",
            },
        };
    }

    private static AdvertisementCallbackConfigField Field(AdvertisementCallbackConfigSchema? schema, string key)
    {
        Assert.NotNull(schema);
        return Assert.Single(schema.Fields, field => field.Key == key);
    }
}