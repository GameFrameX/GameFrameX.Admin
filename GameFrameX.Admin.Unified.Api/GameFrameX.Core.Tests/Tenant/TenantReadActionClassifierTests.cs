using GameFrameX.Core.Service.Tenant;

namespace GameFrameX.Core.Tests.Tenant;

/// <summary>
/// 固化 <see cref="TenantReadActionClassifier"/> 只读前缀集合与判定规则。
/// 扩充只读前缀时必须同步更新本测试，避免「未禁止即放行」的隐性写操作漏判。
/// </summary>
public class TenantReadActionClassifierTests
{
    [Theory]
    [InlineData("Get", true)]
    [InlineData("GetUser", true)]
    [InlineData("Page", true)]
    [InlineData("PageAsync", true)]
    [InlineData("List", true)]
    [InlineData("QueryUsers", true)]
    [InlineData("Find", true)]
    [InlineData("ExportData", true)]
    [InlineData("CheckPermission", true)]
    [InlineData("VerifyCode", true)]
    [InlineData("Info", true)]
    [InlineData("Options", true)]
    [InlineData("Tree", true)]
    [InlineData("Count", true)]
    [InlineData("Exist", true)]
    [InlineData("Add", false)]
    [InlineData("AddTenant", false)]
    [InlineData("Update", false)]
    [InlineData("UpdateTenant", false)]
    [InlineData("Delete", false)]
    [InlineData("Remove", false)]
    [InlineData("CreateOrder", false)]
    [InlineData("SendEmail", false)]
    [InlineData("Publish", false)]
    [InlineData("IssueCoupon", false)] // 关键：Is 已被排除，Issue 不会被误判为读
    [InlineData("HashToken", false)] // 关键：Has 已被排除
    [InlineData("Enable", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsReadAction_MatchesReadOnlyPrefixes(string? actionName, bool expected)
    {
        Assert.Equal(expected, TenantReadActionClassifier.IsReadAction(actionName));
    }

    [Fact]
    public void IsReadAction_IsCaseInsensitive()
    {
        Assert.True(TenantReadActionClassifier.IsReadAction("page"));
        Assert.True(TenantReadActionClassifier.IsReadAction("GETUSER"));
        Assert.False(TenantReadActionClassifier.IsReadAction("DELETE"));
    }

    [Fact]
    public void ReadPrefixes_AreLocked()
    {
        // 锁定集合内容，扩充需同步更新本测试（吸收 strict-change lesson：复合扩展点单测固化）。
        var expected = new[]
        {
            "Get", "Page", "List", "Query", "Find", "Fetch", "Read", "View",
            "Info", "Detail", "Tree", "Select", "Option", "Options",
            "Check", "Verify", "Export", "Count", "Exist",
        };
        Assert.Equal(expected, TenantReadActionClassifier.ReadPrefixes);
    }
}