using GameFrameX.Entity.Tenant.Game.App;
using GameFrameX.Application.Game.Order.GamePaymentConfig;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Core.Tests.PayConfig;

public class PaymentConfigScopeTests
{
    public static IEnumerable<object[]> ScopeCases()
    {
#pragma warning disable CS8625 // xUnit MemberData 的 object[] 元素装箱 nullable long? null，理论数据驱动契约允许
        yield return new object[] { null, null, 0L, 0L, };
        yield return new object[] { 100L, null, 100L, 0L, };
#pragma warning restore CS8625
        yield return new object[] { 100L, 200L, 100L, 200L, };
    }

    [Theory]
    [MemberData(nameof(ScopeCases))]
    public void BuildScope_NormalizesNullableScope(long? gameAppId, long? packageId, long expectedGameAppId, long expectedPackageId)
    {
        var scope = TenantGamePaymentConfigEntityService.BuildScope(gameAppId, packageId);

        Assert.Equal(expectedGameAppId, scope.GameAppIdNormalized);
        Assert.Equal(expectedPackageId, scope.PackageIdNormalized);
    }

    [Fact]
    public void BuildScope_RejectsPackageWithoutGameApp()
    {
        var error = TenantGamePaymentConfigEntityService.GetScopeError(null, 200);

        Assert.Contains("分包支付配置必须指定应用", error);
    }

    [Fact]
    public void PickBestConfig_PrefersPackageThenAppThenTenantDefault()
    {
        var configs = new List<TenantGamePaymentConfigEntity>
        {
            new() { Id = 1, AppId = 0, PayChannel = "apple", GameAppIdNormalized = 0, PackageIdNormalized = 0, },
            new() { Id = 2, AppId = 100, PayChannel = "apple", GameAppId = 100, GameAppIdNormalized = 100, PackageIdNormalized = 0, },
            new() { Id = 3, AppId = 100, PayChannel = "apple", GameAppId = 100, PackageId = 200, GameAppIdNormalized = 100, PackageIdNormalized = 200, },
        };

        var best = TenantGamePaymentConfigEntityService.PickBestConfig(configs, 100, 200);

        Assert.NotNull(best);
        Assert.Equal(3, best.Id);
    }

    [Fact]
    public void PickBestConfig_FallsBackToAppDefaultBeforeTenantDefault()
    {
        var configs = new List<TenantGamePaymentConfigEntity>
        {
            new() { Id = 1, AppId = 0, PayChannel = "apple", GameAppIdNormalized = 0, PackageIdNormalized = 0, },
            new() { Id = 2, AppId = 100, PayChannel = "apple", GameAppId = 100, GameAppIdNormalized = 100, PackageIdNormalized = 0, },
        };

        var best = TenantGamePaymentConfigEntityService.PickBestConfig(configs, 100, 200);

        Assert.NotNull(best);
        Assert.Equal(2, best.Id);
    }

    [Fact]
    public void PackageMatchesScope_RejectsCrossAppOrDisabledPackage()
    {
        var package = new TenantGameAppPackageEntity
        {
            TenantId = 1,
            GameAppId = 100,
            IsEnabled = true,
        };

        Assert.True(PaymentOptionSettingProvider.PackageMatchesScope(package, 1, 100));
        Assert.False(PaymentOptionSettingProvider.PackageMatchesScope(package, 1, 200));

        package.IsEnabled = false;
        Assert.False(PaymentOptionSettingProvider.PackageMatchesScope(package, 1, 100));
    }
}