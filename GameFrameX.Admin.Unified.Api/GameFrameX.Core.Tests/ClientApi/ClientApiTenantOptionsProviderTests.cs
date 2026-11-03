using GameFrameX.Client.Api.Services.ClientApiTenantOptions;

namespace GameFrameX.Core.Tests.ClientApi;

public class ClientApiTenantOptionsProviderTests
{
    [Theory]
    [InlineData("", 0)]
    [InlineData("k1=base64a", 1)]
    [InlineData("k1=base64a,k2=base64b", 2)]
    [InlineData("k1=base64a,,k2=base64b", 2)]
    [InlineData("k1=, =base64b", 0)]
    public void ParseAdMobKeys_HandlesFormats(string raw, int expectedCount)
    {
        var keys = ClientApiTenantOptionsProvider.ParseAdMobKeys(raw);

        Assert.Equal(expectedCount, keys.Count);
    }

    [Fact]
    public void ParseAdMobKeys_PreservesKeyIdAndValue()
    {
        var keys = ClientApiTenantOptionsProvider.ParseAdMobKeys("123=abc==,456=xyz");

        Assert.Equal("abc==", keys["123"]);
        Assert.Equal("xyz", keys["456"]);
    }

}
