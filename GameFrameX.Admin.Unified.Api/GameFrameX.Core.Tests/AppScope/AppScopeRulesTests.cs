using GameFrameX.Application.Game.App.AppScope;

namespace GameFrameX.Core.Tests.AppScope;

public class AppScopeRulesTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void EnsureFactAppId_RejectsZeroAndNegativeForFactData(long appId)
    {
        // 事实数据（角色/订单/账单/邮件/兑换码/兑换记录/商城订单）禁止 AppId<=0 表达应用归属。
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => AppScopeRules.EnsureFactAppId(appId));
        Assert.Contains("应用Id必须大于0", ex.Message);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(long.MaxValue)]
    public void EnsureFactAppId_AcceptsPositiveForFactData(long appId)
    {
        Assert.Equal(appId, AppScopeRules.EnsureFactAppId(appId));
    }

    [Fact]
    public void EnsureFactAppId_RejectsNullForFactData()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => AppScopeRules.EnsureFactAppId((long?)null, "游戏订单"));
        Assert.Contains("游戏订单", ex.Message);
        Assert.Contains("应用Id不能为空", ex.Message);
    }

    [Fact]
    public void EnsureFactAppId_AcceptsNullablePositiveForFactData()
    {
        Assert.Equal(1, AppScopeRules.EnsureFactAppId((long?)1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(long.MinValue)]
    public void EnsureConfigAppId_RejectsNegativeForConfig(long appId)
    {
        // 配置类允许 AppId=0 租户默认，但不允许负数。
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => AppScopeRules.EnsureConfigAppId(appId));
        Assert.Contains("应用Id必须大于等于0", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void EnsureConfigAppId_AcceptsZeroAsTenantDefaultAndPositiveAsOverride(long appId)
    {
        Assert.Equal(appId, AppScopeRules.EnsureConfigAppId(appId));
    }

    [Fact]
    public void EnsureFactAppId_IncludesEntityNameInMessage()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => AppScopeRules.EnsureFactAppId(0, "游戏角色"));
        Assert.Contains("游戏角色", ex.Message);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(-1, false)]
    public void IsTenantDefaultAppId_OnlyTrueForZero(long appId, bool expected)
    {
        Assert.Equal(expected, AppScopeRules.IsTenantDefaultAppId(appId));
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(long.MaxValue, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    public void IsValidFactAppId_TrueOnlyForPositive(long appId, bool expected)
    {
        // 事实数据合法性 = AppId > 0，0 与负数均非法。
        Assert.Equal(expected, AppScopeRules.IsValidFactAppId(appId));
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, true)]
    [InlineData(-1, false)]
    public void IsValidConfigAppId_TrueForZeroAndPositive(long appId, bool expected)
    {
        // 配置类合法性 = AppId >= 0，负数非法。
        Assert.Equal(expected, AppScopeRules.IsValidConfigAppId(appId));
    }

    [Fact]
    public void TenantDefaultAppId_ConstantIsZero()
    {
        // AppId=0 是租户默认占位的唯一语义，禁止漂移到其它魔数。
        Assert.Equal(0, AppScopeRules.TenantDefaultAppId);
    }
}