using GameFrameX.Core.Authorization;

namespace GameFrameX.Core.Tests.Authorization;

public sealed class ApiEndpointIdentityTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public void NormalizeRoutePath_NullOrWhitespace_ReturnsEmpty(string? raw, string expected)
    {
        Assert.Equal(expected, ApiEndpointIdentity.NormalizeRoutePath(raw));
    }

    [Theory]
    [InlineData("/api/systemUser/page", "api/systemuser/page")]
    [InlineData("api/systemUser/page", "api/systemuser/page")]
    [InlineData("/API/SystemUser/Page", "api/systemuser/page")]
    [InlineData("systemUser/page", "api/systemuser/page")]
    [InlineData("/systemUser/page/", "api/systemuser/page")]
    [InlineData("///api///systemUser/page///", "api/systemuser/page")]
    public void NormalizeRoutePath_CanonicalizesToLowerWithApiPrefix(string raw, string expected)
    {
        Assert.Equal(expected, ApiEndpointIdentity.NormalizeRoutePath(raw));
    }

    [Theory]
    [InlineData("systemUser:page", "api/systemuser/page")]
    [InlineData("tenantRole:list", "api/tenantrole/list")]
    [InlineData("SYSTEMUSER:PAGE", "api/systemuser/page")]
    public void RouteNameToRoutePath_ColonBecomesSlashAndLowercased(string routeName, string expected)
    {
        Assert.Equal(expected, ApiEndpointIdentity.RouteNameToRoutePath(routeName));
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public void RouteNameToRoutePath_NullOrWhitespace_ReturnsEmpty(string? routeName, string expected)
    {
        Assert.Equal(expected, ApiEndpointIdentity.RouteNameToRoutePath(routeName));
    }

    [Theory]
    [InlineData("/api/systemUser/page", "api/systemuser/page")]
    [InlineData(null, "")]
    [InlineData("/api/tenantRole/list", "api/tenantrole/list")]
    public void FromRequestPath_EqualsNormalize(string? requestPath, string expected)
    {
        Assert.Equal(expected, ApiEndpointIdentity.FromRequestPath(requestPath));
    }

    [Theory]
    [InlineData("get", "/api/systemUser/page", "GET api/systemuser/page")]
    [InlineData("POST", "systemUser/page", "POST api/systemuser/page")]
    public void ToEndpointKey_NormalizesMethodAndRoute(string httpMethod, string routePath, string expected)
    {
        Assert.Equal(expected, ApiEndpointIdentity.ToEndpointKey(httpMethod, routePath));
    }

    [Fact]
    public void ToEndpointKey_DistinguishesSameRouteDifferentMethods()
    {
        var getKey = ApiEndpointIdentity.ToEndpointKey("GET", "/api/shared/resource");
        var postKey = ApiEndpointIdentity.ToEndpointKey("POST", "/api/shared/resource");

        Assert.NotEqual(getKey, postKey);
    }

    [Theory]
    [InlineData("get", "/api/systemUser/page", "GET api/systemuser/page")]
    [InlineData("post", "/api/systemUser/page", "POST api/systemuser/page")]
    public void FromRequestEndpointKey_UsesMethodAndPath(string httpMethod, string requestPath, string expected)
    {
        Assert.Equal(expected, ApiEndpointIdentity.FromRequestEndpointKey(httpMethod, requestPath));
    }

    [Fact]
    public void FromRequestEndpointKey_UsesEndpointTemplateBeforeLiteralRequestPath()
    {
        var key = ApiEndpointIdentity.FromRequestEndpointKey(
            "get",
            "/api/systemUser/ownRoleList/1300000000111",
            "api/systemUser/ownRoleList/{userId}");

        Assert.Equal("GET api/systemuser/ownrolelist/{userid}", key);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromRequestEndpointKey_FallsBackToRequestPathWhenTemplateMissing(string? endpointTemplate)
    {
        Assert.Equal(
            "GET api/systemuser/ownrolelist/1300000000111",
            ApiEndpointIdentity.FromRequestEndpointKey("get", "/api/systemUser/ownRoleList/1300000000111", endpointTemplate));
    }
}