using GameFrameX.Client.Api.RemoteConfig;
using GameFrameX.Client.Api.Services.RemoteConfig;
using GameFrameX.Client.Api.Services.PackageVersion;
using GameFrameX.Client.Api.Services.GameLoginTicket;
using GameFrameX.Client.Api.GameAppVersion;
using GameFrameX.Client.Api.GameAssetPackageVersion;
using GameFrameX.Client.Api.GameArea;
using GameFrameX.Client.Api.GameWebShop;
using GameFrameX.Client.Api.GameAnnouncement;
using GameFrameX.Client.Api.GameAnnouncement.Dto;
using GameFrameX.Client.Api.GameLoginTicketKey;
using GameFrameX.Client.Api.GameLoginTicketKey.Dto;
using GameFrameX.Client.Api.GameAppVersion.Dto;
using GameFrameX.Client.Api.GameArea.Dto;
using GameFrameX.Client.Api.GameAssetPackageVersion.Dto;
using GameFrameX.Client.Api.RemoteConfig.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Entity.Tenant.Device;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Entity.Tenant.Game.Announcement;
using GameFrameX.Entity.Tenant.Game.App;
using GameFrameX.Entity.Tenant.Game.RemoteConfig;
using GameFrameX.Entity.Tenant.Game.Release;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class ClientApiConfigEndpointTests
{
    [Fact]
    public async Task GameAppVersion_GetInfo_UsesTrustedAppContextAndReturnsPascalCasePayload()
    {
        using var db = new ClientApiTestDb();
        db.SeedAppVersion(1, "com.gameframex.c108");
        var resolver = new GameAppPackageResolver(db.TenantDbProvider, db.TenantIdResolver);
        var matcher = new AppVersionFallbackMatcher(db.Repository<TenantGameAppReleaseVersionEntity>(), resolver);
        var controller = new GameAppVersionController(db.Repository<TenantGameAppReleaseVersionEntity>(), matcher);
        db.AttachContext(controller);

        var result = await controller.GetInfo(new GameAppVersionRequest
        {
            AppId = 999999,
            Platform = "ios",
            PackageName = "com.gameframex.c108",
            AppVersion = "1.0.0",
            Language = "zh-CN",
            Channel = "official",
        });

        var root = ClientApiResultReader.Read(result);
        var data = ClientApiResultReader.Data(root);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.True(data.GetProperty("IsForce").GetBoolean());
        Assert.True(data.GetProperty("IsUpgrade").GetBoolean());
        Assert.Equal("https://client.example.test/app.ipa", data.GetProperty("AppDownloadUrl").GetString());
    }

    [Fact]
    public async Task GameAssetPackageVersion_GetInfo_BuildsPascalCaseAssetPathWithoutSideEffect()
    {
        using var db = new ClientApiTestDb();
        db.SeedAssetPackageVersion(1, "com.gameframex.c108");
        var resolver = new GameAppPackageResolver(db.TenantDbProvider, db.TenantIdResolver);
        var matcher = new AssetPackageVersionFallbackMatcher(db.Repository<TenantGameAppAssetPackageVersionEntity>(), resolver);
        var controller = new GameAssetPackageVersionController(
            db.Repository<TenantGameAppAssetPackageVersionEntity>(),
            db.Repository<TenantGameAppDeviceEntity>(),
            matcher);
        db.AttachContext(controller);

        var result = await controller.GetInfo(new AssetPackageVersionRequest
        {
            AssetPackageName = "main",
            DeviceUniqueIdentifier = "device-c108",
            Platform = "ios",
            PackageName = "com.gameframex.c108",
            AppVersion = "1.0.0",
            Language = "zh-CN",
            Channel = "official",
        });

        var root = ClientApiResultReader.Read(result);
        var data = ClientApiResultReader.Data(root);
        // GFX-706：GetInfo 纯只读——设备注册副作用已拆至 RegisterDevice，携带标识调用也不写设备表
        var deviceCount = await db.TenantDb.Queryable<TenantGameAppDeviceEntity>().CountAsync();
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Equal(0, deviceCount);
        Assert.Equal("2026.07.18", data.GetProperty("Version").GetString());
        Assert.EndsWith("/com.gameframex.c108/ios/1.0.0/main/2026.07.18/", data.GetProperty("AssetPackagePath").GetString());
    }

    [Fact]
    public async Task GameAssetPackageVersion_RegisterDevice_IdempotentByAppAndIdentifier()
    {
        using var db = new ClientApiTestDb();
        db.SeedAssetPackageVersion(1, "com.gameframex.c108");
        var resolver = new GameAppPackageResolver(db.TenantDbProvider, db.TenantIdResolver);
        var matcher = new AssetPackageVersionFallbackMatcher(db.Repository<TenantGameAppAssetPackageVersionEntity>(), resolver);
        var controller = new GameAssetPackageVersionController(
            db.Repository<TenantGameAppAssetPackageVersionEntity>(),
            db.Repository<TenantGameAppDeviceEntity>(),
            matcher);
        db.AttachContext(controller);

        var first = await controller.RegisterDevice(new DeviceRegisterRequest
        {
            DeviceUniqueIdentifier = "device-c108",
        });
        var second = await controller.RegisterDevice(new DeviceRegisterRequest
        {
            DeviceUniqueIdentifier = "device-c108",
        });

        var firstRoot = ClientApiResultReader.Read(first);
        var secondRoot = ClientApiResultReader.Read(second);
        var deviceCount = await db.TenantDb.Queryable<TenantGameAppDeviceEntity>().CountAsync();
        Assert.Equal(0, ClientApiResultReader.Code(firstRoot));
        Assert.Equal(0, ClientApiResultReader.Code(secondRoot));
        // 幂等：两次注册仅落一行，DeviceId 一致
        Assert.Equal(1, deviceCount);
        Assert.Equal(
            ClientApiResultReader.Data(firstRoot).GetProperty("DeviceId").GetInt64(),
            ClientApiResultReader.Data(secondRoot).GetProperty("DeviceId").GetInt64());
        Assert.True(ClientApiResultReader.Data(firstRoot).GetProperty("DeviceId").GetInt64() > 0);
    }

    [Fact]
    public async Task GameRemoteConfig_GetInfo_ReturnsAppScopedContentAndPascalCasePayload()
    {
        using var db = new ClientApiTestDb();
        db.InitTenantTables(typeof(TenantGameAppPackageEntity));
        db.SeedRemoteConfig(1, "com.gameframex.c108", "{\"feature\":\"on\"}");
        var resolver = new GameAppPackageResolver(db.TenantDbProvider, db.TenantIdResolver);
        var delivery = new RemoteConfigDeliveryService(db.Repository<TenantGameAppRemoteConfigEntity>(), resolver);
        var controller = new GameRemoteConfigController(db.Repository<TenantGameAppRemoteConfigEntity>(), delivery);
        db.AttachContext(controller);

        var result = await controller.GetInfo(new RemoteConfigRequest
        {
            AppId = 999999,
            Platform = "ios",
            PackageName = "com.gameframex.c108",
            AppVersion = "1.0.0",
            Language = "zh-CN",
            Channel = "official",
            DeviceId = "device-c108",
        });

        var root = ClientApiResultReader.Read(result);
        var data = ClientApiResultReader.Data(root);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Equal("{\"feature\":\"on\"}", data.GetProperty("Content").GetString());
        Assert.Equal("https://client.example.test/app", data.GetProperty("CheckAppVersionUrl").GetString());
        Assert.True(data.TryGetProperty("AotCodeList", out _));
    }

    [Fact]
    public async Task GameArea_GetAreaList_FiltersDisabledAreaAndSuppressesManagerUrlByPolicy()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(100, 10);
        db.SeedArea(101, 10, false);
        var controller = new GameAreaController(db.Repository<TenantGameAreaEntity>(), FakeOptionsProvider.WithAreaPolicy(false));
        db.AttachContext(controller);

        var result = await controller.GetAreaList(new GameAreaGetAreaListRequest());

        var root = ClientApiResultReader.Read(result);
        var areas = ClientApiResultReader.Data(root)[0].GetProperty("GameAreaList");
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Equal(1, areas.GetArrayLength());
        Assert.Equal(100, areas[0].GetProperty("Id").GetInt64());
        Assert.Equal(string.Empty, areas[0].GetProperty("HttpManageUrl").GetString());
    }

    [Fact]
    public async Task Announcement_ListByAreaId_FallsBackToDefaultLanguageAndSkipsInactiveRows()
    {
        using var db = new ClientApiTestDb();
        db.SeedAnnouncement(1, 100, "default", "Area default");
        db.SeedAnnouncement(2, 100, "fr-FR", "Inactive localized", false);
        db.SeedAnnouncement(3, null, "default", "Global default");
        var controller = new GameAnnouncementController(db.Repository<TenantGameAnnouncementEntity>());
        db.AttachContext(controller);

        var result = await controller.ListByAreaId(new GameAnnouncementListRequest { Id = 100, Language = "fr-FR", });

        var root = ClientApiResultReader.Read(result);
        var data = ClientApiResultReader.Data(root);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Equal(2, data.GetArrayLength());
        Assert.Equal("Area default", data[0].GetProperty("Title").GetString());
        Assert.Equal("Global default", data[1].GetProperty("Title").GetString());
    }

    [Fact]
    public async Task GameLoginTicketKey_PublicKeys_ReturnsPublicKeysWithoutPrivateMaterial()
    {
        var resolver = new StaticGameLoginTicketKeyResolver();
        var controller = new GameLoginTicketKeyController(resolver);
        using var db = new ClientApiTestDb(10, 20);
        db.AttachContext(controller);

        var result = await controller.PublicKeys();

        var root = ClientApiResultReader.Read(result);
        var key = ClientApiResultReader.Data(root)[0];
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Equal("tenant-10-app-20-test", key.GetProperty("Kid").GetString());
        Assert.Equal("public-key", key.GetProperty("PublicKeyPem").GetString());
        Assert.False(key.TryGetProperty("PrivateKeyPem", out _));
    }

    [Fact]
    public async Task GameLoginTicketKey_PublicKeys_RejectsMissingAppContext()
    {
        var resolver = new StaticGameLoginTicketKeyResolver();
        var controller = new GameLoginTicketKeyController(resolver);
        using var db = new ClientApiTestDb(10, 20);
        db.AttachContext(controller, includeApp: false);

        var result = await controller.PublicKeys();

        var root = ClientApiResultReader.Read(result);
        Assert.Equal((int)AuthErrorCode.AuthorizeInvalid, ClientApiResultReader.Code(root));
        Assert.Equal("应用未认证", ClientApiResultReader.Message(root));
    }

    [Fact]
    public async Task GameLoginTicketKeyResolver_PublicKeys_DoesNotAutoCreateSigningKey()
    {
        using var db = new ClientApiTestDb(10, 20);
        db.InitTenantTables(typeof(TenantGameLoginTicketKeyEntity));
        var resolver = new GameLoginTicketKeyResolver(db.TenantDbProvider, FakeOptionsProvider.WithTicketPolicy(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60));

        var publicKeys = await resolver.GetPublicKeysAsync(10, 20);

        Assert.Empty(publicKeys);
        Assert.Equal(0, await db.TenantDb.Queryable<TenantGameLoginTicketKeyEntity>().CountAsync());

        var signingKey = await resolver.ResolveSigningKeyAsync(10, 20);

        Assert.False(string.IsNullOrWhiteSpace(signingKey.Kid));
        Assert.Equal(1, await db.TenantDb.Queryable<TenantGameLoginTicketKeyEntity>().CountAsync());
        Assert.Single(await resolver.GetPublicKeysAsync(10, 20));
    }

    private sealed class StaticGameLoginTicketKeyResolver : IGameLoginTicketKeyResolver
    {
        public Task<GameLoginTicketSigningKey> ResolveSigningKeyAsync(long tenantId, long appId)
        {
            throw new NotSupportedException();
        }

        public Task<IReadOnlyCollection<GameLoginTicketPublicKey>> GetPublicKeysAsync(long tenantId, long appId)
        {
            IReadOnlyCollection<GameLoginTicketPublicKey> keys = new[]
            {
                new GameLoginTicketPublicKey(
                    $"tenant-{tenantId}-app-{appId}-test",
                    GameLoginTicketSigningKey.AlgorithmRs256,
                    "public-key",
                    DateTimeOffset.UtcNow.AddMinutes(-5).ToUnixTimeSeconds(),
                    DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds()),
            };
            return Task.FromResult(keys);
        }
    }
}