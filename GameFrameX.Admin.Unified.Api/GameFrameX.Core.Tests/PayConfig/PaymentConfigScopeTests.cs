using GameFrameX.Entity.Tenant.Game.App;
using GameFrameX.Application.ClientDelivery.GamePaymentConfig;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Core.Tests.PayConfig;

public class PaymentConfigScopeTests
{
    [Theory]
    [InlineData(null, 0)]
    [InlineData(0L, 0)]
    [InlineData(200L, 200)]
    public void BuildScope_NormalizesPackage(long? packageId, long expectedPackageId)
    {
        var scope = GamePaymentConfigService.BuildScope(packageId);
        Assert.Equal(expectedPackageId, scope.PackageIdNormalized);
    }

    [Fact]
    public void BuildScope_PackageRequiresNoApplication()
    {
        var scope = GamePaymentConfigService.BuildScope(200);
        Assert.Equal(200, scope.PackageId);
    }

    [Fact]
    public void PickBestConfig_PrefersPackage()
    {
        var configs = new[]
        {
            new TenantGamePaymentConfigEntity { Id = 1, PayChannel = "apple", PackageIdNormalized = 0 },
            new TenantGamePaymentConfigEntity { Id = 2, PayChannel = "apple", PackageId = 200, PackageIdNormalized = 200 },
        };
        Assert.Equal(2, GamePaymentConfigService.PickBestConfig(configs, 200)?.Id);
    }

    [Fact]
    public void PickBestConfig_FallsBackToDefaultAndExcludesOtherPackage()
    {
        var configs = new[]
        {
            new TenantGamePaymentConfigEntity { Id = 1, PayChannel = "apple", PackageIdNormalized = 0 },
            new TenantGamePaymentConfigEntity { Id = 2, PayChannel = "apple", PackageId = 300, PackageIdNormalized = 300 },
        };
        Assert.Equal(1, GamePaymentConfigService.PickBestConfig(configs, 200)?.Id);
    }

    [Fact]
    public void PackageMatchesScope_RequiresExistingEnabledPackage()
    {
        var package = new TenantGameAppPackageEntity { IsEnabled = true };
        Assert.True(PaymentOptionSettingProvider.PackageMatchesScope(package));
        Assert.False(PaymentOptionSettingProvider.PackageMatchesScope(null));
        package.IsEnabled = false;
        Assert.False(PaymentOptionSettingProvider.PackageMatchesScope(package));
    }
}
