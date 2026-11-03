using System.Reflection;
using GameFrameX.Client.Api.Integrations.Payment;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Payment.Apple.Option;
using Microsoft.AspNetCore.Http;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class PaymentOptionSettingSingleMainTests : IDisposable
{
    private readonly ClientApiTestDb _database = new();

    public PaymentOptionSettingSingleMainTests()
    {
        _ = Assembly.Load("GameFrameX.Payment.Apple");
        _database.InitTenantTables(typeof(TenantGamePaymentConfigEntity));
    }

    [Fact]
    public void NoHttpContext_ReadsBothEnvironmentsFromMainDatabase()
    {
        Seed(1, false, 0, "production");
        Seed(2, true, 0, "sandbox");
        var settings = Provider(null).GetOptionSettings<ApplePaymentOptionSetting>("apple").ToList();
        Assert.Equal(2, settings.Count);
    }

    [Theory]
    [InlineData("sandbox", true)]
    [InlineData("production", false)]
    public void Environment_SelectsOnlyRequestedConfiguration(string environment, bool sandbox)
    {
        Seed(1, false, 0, "production");
        Seed(2, true, 0, "sandbox");
        var context = new DefaultHttpContext();
        context.Request.Headers["GameFrameX-Payment-Env"] = environment;
        var setting = Assert.Single(Provider(context).GetOptionSettings<ApplePaymentOptionSetting>("apple"));
        Assert.Equal(sandbox ? "sandbox" : "production", setting.ApplePaymentSharedSecret);
    }

    [Fact]
    public void ValidPackage_OverridesDefaultAndDisabledPackageFallsBack()
    {
        var package = _database.SeedPackage(20, "com.example.package");
        Seed(1, false, 0, "default");
        Seed(2, false, 20, "package");
        Seed(3, false, 30, "other");
        var context = new DefaultHttpContext();
        context.Request.Headers[PaymentOptionSettingProvider.PackageIdHeader] = "20";
        Assert.Equal("package", Provider(context).GetOptionSetting<ApplePaymentOptionSetting>("apple").ApplePaymentSharedSecret);

        package.IsEnabled = false;
        _database.MainDb.Updateable(package).ExecuteCommand();
        Assert.Equal("default", Provider(context).GetOptionSetting<ApplePaymentOptionSetting>("apple").ApplePaymentSharedSecret);
    }

    [Fact]
    public void DisabledConfiguration_IsNeverReturned()
    {
        Seed(1, false, 0, "disabled");
        _database.MainDb.Updateable<TenantGamePaymentConfigEntity>().SetColumns(row => row.IsEnabled == false).Where(row => row.Id == 1).ExecuteCommand();
        Assert.Empty(Provider(null).GetOptionSettings("apple"));
    }

    private PaymentOptionSettingProvider Provider(HttpContext? context)
    {
        return new PaymentOptionSettingProvider(_database.MainDb, new HttpContextAccessor { HttpContext = context });
    }

    private void Seed(long id, bool sandbox, long packageId, string secret)
    {
        _database.MainDb.Insertable(new TenantGamePaymentConfigEntity
        {
            Id = id, PayChannel = "apple", IsSandbox = sandbox, PackageIdNormalized = packageId,
            ConfigJson = "{\"ApplePaymentSharedSecret\":\"" + secret + "\"}", IsEnabled = true,
        }).ExecuteCommand();
    }

    public void Dispose()
    {
        _database.Dispose();
    }
}
