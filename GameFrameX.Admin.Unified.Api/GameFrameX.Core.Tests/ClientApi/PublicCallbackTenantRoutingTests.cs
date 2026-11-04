using System.Text.Json;
using GameFrameX.Client.Api.Integrations.Payment;
using GameFrameX.Client.Api.Middleware;
using GameFrameX.Client.Api.Services.ClientApiTenantOptions;
using GameFrameX.Client.Api.Services.PublicCallback;
using GameFrameX.Core.Middleware;
using GameFrameX.Entity.Tenant.ClientApiConfig;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Entity.Tenant.Game.App;
using GameFrameX.Payment.AliPay;
using Microsoft.AspNetCore.Http;
using SqlSugar;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class PublicCallbackTenantRoutingTests
{
    [Fact]
    public async Task Middleware_WithPackageRouteParameter_EstablishesCallbackContextBeforeNext()
    {
        const long packageId = 1900000000201;
        using (var fixture = CreateFixture(packageId))
        {
            var context = new DefaultHttpContext();
            SetPublicCallbackEndpoint(context, "alipay", "payment");
            context.Request.RouteValues["packageId"] = packageId;
            var nextCalled = false;
            var middleware = new PublicCallbackTenantResolutionMiddleware(current =>
            {
                nextCalled = true;
                Assert.False(current.Items.ContainsKey("TenantId"));
                Assert.False(current.Items.ContainsKey("AppId"));
                Assert.IsType<PublicCallbackRouteContext>(current.Items[PublicCallbackRouteContext.ItemKey]);
                Assert.Equal(packageId, current.Items[PaymentOptionSettingProvider.PackageIdKey]);
                return Task.CompletedTask;
            });
            await middleware.InvokeAsync(context, new PublicCallbackRouteResolver(fixture.MainDb));
            Assert.True(nextCalled);
            Assert.Equal(200, context.Response.StatusCode);
        }
    }

    [Fact]
    public async Task Middleware_WithoutPackageRouteParameter_EstablishesCallbackContext()
    {
        using (var fixture = CreateFixture())
        {
            var context = new DefaultHttpContext();
            SetPublicCallbackEndpoint(context, "alipay", "payment");
            var middleware = new PublicCallbackTenantResolutionMiddleware(current =>
            {
                Assert.IsType<PublicCallbackRouteContext>(current.Items[PublicCallbackRouteContext.ItemKey]);
                return Task.CompletedTask;
            });
            await middleware.InvokeAsync(context, new PublicCallbackRouteResolver(fixture.MainDb));
            Assert.Equal(200, context.Response.StatusCode);
            Assert.False(context.Items.ContainsKey("TenantId"));
        }
    }

    [Fact]
    public void PaymentProvider_BareQueryScopeDoesNotChangeMainConfiguration()
    {
        using (var fixture = CreateFixture())
        {
            InsertAliPayConfig(fixture.MainDb, false);
            var context = new DefaultHttpContext();
            context.Request.QueryString = new QueryString("?tenantId=999&appId=888&env=prod");
            context.Items[PublicCallbackRouteContext.ItemKey] = new PublicCallbackRouteContext("alipay", "payment", null);
            var provider = new PaymentOptionSettingProvider(fixture.MainDb, new HttpContextAccessor { HttpContext = context });
            var setting = Assert.Single(provider.GetOptionSettings<AliPayOptionSetting>("alipay"));
            Assert.Equal("2026000000000000", setting.AliPayAppId);
        }
    }

    [Fact]
    public void PaymentProvider_AppendsPackageRouteSegmentToConfiguredCallbackUrl()
    {
        const long packageId = 1900000003201;
        using (var fixture = CreateFixture(packageId))
        {
            InsertAliPayConfig(fixture.MainDb, true);
            var context = new DefaultHttpContext();
            context.Items[PaymentOptionSettingProvider.PackageIdKey] = packageId;
            context.Items[PaymentOptionSettingProvider.PaymentIsSandboxKey] = true;
            var provider = new PaymentOptionSettingProvider(fixture.MainDb, new HttpContextAccessor { HttpContext = context });
            var setting = provider.GetOptionSetting<AliPayOptionSetting>("alipay");
            // 完整新路由地址 + 当前分包上下文：回调地址带上分包段，网关回调按分包解析回同一租户
            Assert.NotNull(setting);
            Assert.Equal("https://client.example.com/api/GameOrder/NotifyAliPay/1900000003201", setting.AliPayNotifyUrl);
        }
    }

    [Fact]
    public void PaymentProvider_BuildsCallbackUrlFromSiteRootWithoutPackageContext()
    {
        using (var fixture = CreateFixture())
        {
            InsertAliPayConfig(fixture.MainDb, false, "https://client.example.com");
            var context = new DefaultHttpContext();
            var provider = new PaymentOptionSettingProvider(fixture.MainDb, new HttpContextAccessor { HttpContext = context });
            var setting = provider.GetOptionSetting<AliPayOptionSetting>("alipay");
            // 站点根地址自动补全路由段；无分包上下文时不带分包段（主库配置）
            Assert.NotNull(setting);
            Assert.Equal("https://client.example.com/api/GameOrder/NotifyAliPay", setting.AliPayNotifyUrl);
        }
    }

    [Fact]
    public void PaymentProvider_NormalizesLegacyThreeSegmentCallbackUrl()
    {
        using (var fixture = CreateFixture())
        {
            InsertAliPayConfig(fixture.MainDb, false, "https://client.example.com/api/GameOrder/NotifyAliPay/999/888/777");
            var context = new DefaultHttpContext();
            var provider = new PaymentOptionSettingProvider(fixture.MainDb, new HttpContextAccessor { HttpContext = context });
            var setting = provider.GetOptionSetting<AliPayOptionSetting>("alipay");
            // 旧「租户/应用/分包」三段地址去段重建为新路由（无分包上下文时不带分包段）
            Assert.NotNull(setting);
            Assert.Equal("https://client.example.com/api/GameOrder/NotifyAliPay", setting.AliPayNotifyUrl);
        }
    }

    [Fact]
    public void PaymentProvider_RejectsUnmappableCallbackUrl()
    {
        using (var fixture = CreateFixture())
        {
            InsertAliPayConfig(fixture.MainDb, false, "https://client.example.com/pay/notify");
            var context = new DefaultHttpContext();
            var provider = new PaymentOptionSettingProvider(fixture.MainDb, new HttpContextAccessor { HttpContext = context });
            var setting = provider.GetOptionSetting<AliPayOptionSetting>("alipay");
            // 无法识别的地址置空：处理器按「通知地址未配置」拒绝下单
            Assert.NotNull(setting);
            Assert.Equal(string.Empty, setting.AliPayNotifyUrl);
        }
    }

    [Fact]
    public async Task AdvertisementOptionsProvider_PicksPangleConfigFromPublicCallbackContext()
    {
        using (var fixture = CreateFixture())
        {
            fixture.MainDb.Insertable(new[]
            {
                new TenantAdvertisementCallbackConfigEntity
                {
                    Id = 1900000004201, Provider = "TopOn",
                    ConfigJson = JsonSerializer.Serialize(new { callbackUrl = "https://client.example.com/topon", signingSecret = "topon-secret" }),
                    IsEnabled = true,
                },
                new TenantAdvertisementCallbackConfigEntity
                {
                    Id = 1900000004202, Provider = "Pangle",
                    ConfigJson = JsonSerializer.Serialize(new { callbackUrl = "https://client.example.com/pangle", signingSecret = "pangle-secret" }),
                    IsEnabled = true,
                },
            }).ExecuteCommand();
            var context = new DefaultHttpContext();
            context.Items["TenantId"] = 999L;
            context.Items[PublicCallbackRouteContext.ItemKey] = new PublicCallbackRouteContext("pangle", "ad", null);
            var provider = new ClientApiTenantOptionsProvider(fixture.MainDb, new HttpContextAccessor { HttpContext = context });
            var options = await provider.GetAdvertisementCallbackOptionsAsync();
            Assert.NotNull(options);
            Assert.Equal("Pangle", options.Provider);
            Assert.Equal("pangle-secret", options.PangleSecurityKey);
        }
    }

    private static ClientApiTestDb CreateFixture(long? packageId = null)
    {
        var fixture = new ClientApiTestDb().InitTenantTables(typeof(TenantGameAppPackageEntity), typeof(TenantGamePaymentConfigEntity), typeof(TenantAdvertisementCallbackConfigEntity));
        if (packageId.HasValue)
        {
            fixture.MainDb.Insertable(new TenantGameAppPackageEntity
            {
                Id = packageId.Value, Name = "主库测试分包", PackageName = "com.gameframex.callback",
                PlatformCode = "ios", PlatformName = "iOS", ChannelCode = "default", ChannelName = "Default", IsEnabled = true,
            }).ExecuteCommand();
        }

        return fixture;
    }

    private static void SetPublicCallbackEndpoint(DefaultHttpContext context, string provider, string callbackKind)
    {
        context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(new PublicCallbackEndpointAttribute(provider, callbackKind)), provider + "-" + callbackKind));
    }

    private static void InsertAliPayConfig(ISqlSugarClient database, bool isSandbox, string notifyUrl = "https://client.example.com/api/GameOrder/NotifyAliPay")
    {
        database.Insertable(new TenantGamePaymentConfigEntity
        {
            Id = 1900000004201,
            PackageIdNormalized = 0,
            PayChannel = "alipay",
            IsSandbox = isSandbox,
            IsEnabled = true,
            ConfigJson = JsonSerializer.Serialize(new AliPayOptionSetting
            {
                AliPayAppId = "2026000000000000",
                AliPayMerchantPrivateKey = "private-key",
                AliPayPublicKey = "public-key",
                AliPayNotifyUrl = notifyUrl,
                AliPayReturnUrl = "https://client.example.com/pay/return",
            }),
        }).ExecuteCommand();
    }
}
