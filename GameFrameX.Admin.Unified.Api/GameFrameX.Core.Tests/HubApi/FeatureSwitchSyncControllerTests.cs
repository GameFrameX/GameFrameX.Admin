using System.Text;
using GameFrameX.Application.Game.App.FeatureSwitch;
using GameFrameX.Core.Const;
using GameFrameX.Core.Middleware;
using GameFrameX.Entity.Enum;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Extension;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Tenant.Game.FeatureSwitch;
using GameFrameX.Hub.Api.Const;
using GameFrameX.Hub.Api.Controllers;
using GameFrameX.Hub.Api.Dto.FeatureSwitchSync;
using GameFrameX.Hub.Api.Middleware;
using GameFrameX.Hub.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using JsonElement = System.Text.Json.JsonElement;

namespace GameFrameX.Core.Tests.HubApi;

/// <summary>
/// C347 功能开关服务器通道（FeatureSwitchSyncController + OpenAccess 鉴权面）端点级测试。
/// <para>覆盖：消费方过滤（client 行不下发，server|both 命中）、四维无上下文仅命中 Default 哨兵行、
/// 服务器通道不做设备灰度、AppId 只取凭证绑定注入、版本协议三态、缺凭证上下文拒绝，
/// 以及 scope = FeatureSwitchSync 的路由放行 / 越权 403 语义与 HMAC v2 签名链路对本路由的适用性。</para>
/// </summary>
public sealed class FeatureSwitchSyncControllerTests : IDisposable
{
    private const long TenantId = 2090001;
    private const long CredentialAppId = 2091001;

    private readonly string _mainDbPath = Path.Combine(Path.GetTempPath(), $"gfx_hub_switch_main_{Guid.NewGuid():N}.db");
    private readonly string _tenantDbPath = Path.Combine(Path.GetTempPath(), $"gfx_hub_switch_tenant_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _mainDb;

    public FeatureSwitchSyncControllerTests()
    {
        var config = new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_mainDbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _mainDb = new SqlSugarScope(config);
        _mainDb.CodeFirst.InitTables(typeof(SystemTenantEntity));
        _mainDb.Insertable(new SystemTenantEntity
        {
            Id = TenantId,
            TenantType = TenantTypeEnum.Db,
            DbType = DbType.Sqlite,
            Connection = $"DataSource={_tenantDbPath}",
            ConfigId = TenantId.ToString(),
            IsEnabled = true,
        }).ExecuteCommand();

        var provider = new HubTenantDbProvider(_mainDb);
        provider.GetTenantDb(TenantId).CodeFirst.InitTables(typeof(TenantGameAppFeatureSwitchEntity));
    }

    private FeatureSwitchSyncController CreateController(out ISqlSugarClient tenantDb, bool includeTenant = true, bool includeApp = true)
    {
        var provider = new HubTenantDbProvider(_mainDb);
        var tenantIdResolver = new DelegateSqlSugarTenantIdResolver(() => TenantId);
        var repository = new Repository<TenantGameAppFeatureSwitchEntity>(_mainDb, provider, tenantIdResolver);
        tenantDb = provider.GetTenantDb(TenantId);
        var controller = new FeatureSwitchSyncController(new FeatureSwitchDeliveryResolver(repository));
        var context = new DefaultHttpContext();
        if (includeTenant)
        {
            context.Items[TenantContextAccessor.TenantIdKey] = TenantId;
        }

        if (includeApp)
        {
            context.Items[TenantGameAppOpenAccessAuthMiddleware.AppIdItemsKey] = CredentialAppId;
        }

        controller.ControllerContext = new ControllerContext { HttpContext = context, };
        return controller;
    }

    private static TenantGameAppFeatureSwitchEntity Row(
        long id,
        string switchKey,
        string valueJson = "{}",
        string audience = FeatureSwitchAudienceValues.Both,
        long? appId = CredentialAppId,
        string platform = GameDimensionConst.PlatformDefault,
        int rolloutPercent = 100)
    {
        return new TenantGameAppFeatureSwitchEntity
        {
            Id = id,
            AppId = appId,
            SwitchKey = switchKey,
            DisplayName = switchKey,
            Audience = audience,
            ValueJson = valueJson,
            Platform = platform,
            Package = GameDimensionConst.PackageNameDefault,
            Language = GameDimensionConst.LanguageDefault,
            Channel = GameDimensionConst.ChannelNameDefault,
            Priority = 0,
            RolloutPercent = rolloutPercent,
            IsEnabled = true,
            IsDeleted = false,
            TenantId = TenantId,
        };
    }

    /// <summary>
    /// 服务器通道消费方过滤：client 行不下发（server|both 命中），且 AppId 只取凭证绑定（他应用行不泄漏）。
    /// </summary>
    [Fact]
    public async Task Sync_FiltersByAudienceAndCredentialBoundApp()
    {
        var controller = CreateController(out var tenantDb);
        tenantDb.Insertable(new[]
        {
            Row(1, "economy.drop_rate", "{\"rate\":2}", audience: FeatureSwitchAudienceValues.Server),
            Row(2, "login.banner", "{\"v\":1}", audience: FeatureSwitchAudienceValues.Both),
            Row(3, "client.only", "{\"v\":3}", audience: FeatureSwitchAudienceValues.Client),
            Row(4, "other.app", "{\"v\":4}", audience: FeatureSwitchAudienceValues.Server, appId: CredentialAppId + 1),
        }).ExecuteCommand();

        var root = ClientApiResultReader.Read(await controller.Sync(new FeatureSwitchSyncRequest()));
        Assert.Equal(0, ClientApiResultReader.Code(root));
        var data = ClientApiResultReader.Data(root);

        var switches = data.GetProperty("Switches");
        Assert.Equal(2, switches.GetArrayLength());
        Assert.Equal("economy.drop_rate", switches[0].GetProperty("SwitchKey").GetString());
        Assert.Equal("login.banner", switches[1].GetProperty("SwitchKey").GetString());
        Assert.Matches("^[0-9a-f]{32}$", data.GetProperty("Version").GetString());
        Assert.True(data.GetProperty("ServerTime").GetInt64() > 0);
    }

    /// <summary>
    /// 四维（platform/package/language/channel）无上下文：仅 Default 哨兵行命中，具体平台行不下发。
    /// </summary>
    [Fact]
    public async Task Sync_WithoutDimensionContext_OnlyDefaultSentinelRowsDelivered()
    {
        var controller = CreateController(out var tenantDb);
        tenantDb.Insertable(new[]
        {
            Row(1, "activity.android_only", "{\"v\":1}", platform: "android"),
            Row(2, "activity.everyone", "{\"v\":2}"),
        }).ExecuteCommand();

        var result = await controller.Sync(new FeatureSwitchSyncRequest());

        var switches = ClientApiResultReader.Data(ClientApiResultReader.Read(result)).GetProperty("Switches");
        Assert.Equal(1, switches.GetArrayLength());
        Assert.Equal("activity.everyone", switches[0].GetProperty("SwitchKey").GetString());
    }

    /// <summary>
    /// 服务器通道无设备身份，不做设备灰度：灰度比例 0 的行仍照常下发（灰度是客户端设备维度语义）。
    /// </summary>
    [Fact]
    public async Task Sync_ServerChannel_IgnoresDeviceRollout()
    {
        var controller = CreateController(out var tenantDb);
        tenantDb.Insertable(new[]
        {
            Row(1, "economy.zero_rollout", "{\"rate\":5}", rolloutPercent: 0),
        }).ExecuteCommand();

        var result = await controller.Sync(new FeatureSwitchSyncRequest());

        var switches = ClientApiResultReader.Data(ClientApiResultReader.Read(result)).GetProperty("Switches");
        Assert.Equal(1, switches.GetArrayLength());
        Assert.Equal("economy.zero_rollout", switches[0].GetProperty("SwitchKey").GetString());
    }

    /// <summary>
    /// 版本协议三态：首拉全量 → 同版本 Unchanged 短路（空数组 + Version 回传）→ 变更后旧版本拿全量新列表。
    /// </summary>
    [Fact]
    public async Task Sync_VersionProtocol_FirstPullUnchangedShortCircuitAndChange()
    {
        var controller = CreateController(out var tenantDb);
        var row = Row(1, "economy.drop_rate", "{\"rate\":1}");
        tenantDb.Insertable(row).ExecuteCommand();

        var firstResult = await controller.Sync(new FeatureSwitchSyncRequest());
        var first = ClientApiResultReader.Data(ClientApiResultReader.Read(firstResult));
        Assert.False(first.GetProperty("Unchanged").GetBoolean());
        Assert.Equal(1, first.GetProperty("Switches").GetArrayLength());
        var version = first.GetProperty("Version").GetString();

        var unchangedController = CreateController(out _);
        var unchangedResult = await unchangedController.Sync(new FeatureSwitchSyncRequest { Version = version, });
        var unchanged = ClientApiResultReader.Data(ClientApiResultReader.Read(unchangedResult));
        Assert.True(unchanged.GetProperty("Unchanged").GetBoolean());
        Assert.Equal(version, unchanged.GetProperty("Version").GetString());
        Assert.Equal(0, unchanged.GetProperty("Switches").GetArrayLength());

        row.ValueJson = "{\"rate\":9}";
        tenantDb.Updateable(row).ExecuteCommand();
        var changedResult = await controller.Sync(new FeatureSwitchSyncRequest { Version = version, });
        var changed = ClientApiResultReader.Data(ClientApiResultReader.Read(changedResult));
        Assert.False(changed.GetProperty("Unchanged").GetBoolean());
        Assert.NotEqual(version, changed.GetProperty("Version").GetString());
        Assert.Equal("{\"rate\":9}", changed.GetProperty("Switches")[0].GetProperty("ValueJson").GetString());
    }

    /// <summary>
    /// 缺凭证租户 / 应用上下文（中间件未注入）→ 拒绝，不回落任何默认作用域。
    /// </summary>
    [Fact]
    public async Task Sync_MissingCredentialContext_Rejected()
    {
        var controller = CreateController(out _, includeApp: false);

        var result = await controller.Sync(new FeatureSwitchSyncRequest());

        var root = ClientApiResultReader.Read(result);
        Assert.NotEqual(0, ClientApiResultReader.Code(root));
    }

    /// <summary>
    /// scope 放行语义：scope 含 FeatureSwitchSync 时 /api/FeatureSwitchSync 路由命中；
    /// 仅含其它 scope（如打包上报）时越权拒绝（中间件返回 403）。
    /// </summary>
    [Fact]
    public void OpenAccessScope_FeatureSwitchSyncRouteMatchesOnlyItsScope()
    {
        var path = new PathString("/api/FeatureSwitchSync/Sync");

        Assert.True(TenantGameAppOpenAccessAuthMiddleware.IsRouteInScopes(path, "[\"" + OpenAccessScopeConst.FeatureSwitchSync + "\"]"));
        Assert.False(TenantGameAppOpenAccessAuthMiddleware.IsRouteInScopes(path, "[\"PackageBuildReport\"]"));
    }

    /// <summary>
    /// HMAC v2 签名链路对本路由的适用性：按 C267 canonical 消息串（body 入签 + canonical query）构造的签名通过校验，
    /// 篡改 body 后原签名失效（FeatureSwitchSync 与 PackageBuildReport 共用同一验签实现）。
    /// </summary>
    [Fact]
    public void OpenAccessSignature_V2CanonicalMessage_VerifiesForFeatureSwitchSync()
    {
        var credential = new GameFrameX.Entity.Tenant.Game.App.TenantGameAppOpenAccessEntity
        {
            AccessKey = "ak-feature-switch-001",
            AccessSecret = "GFX-C347-FEATURE-SWITCH-SECRET",
            IsEnabled = true,
        };
        var body = Encoding.UTF8.GetBytes("{\"version\":\"abc123\"}");
        var provided = GameFrameX.Core.SignatureAuth.CanonicalRequestBuilder.HmacSha256(
            Encoding.UTF8.GetBytes(credential.AccessSecret),
            GameFrameX.Core.SignatureAuth.CanonicalRequestBuilder.BuildV2Message("POST", "/api/FeatureSwitchSync/Sync", string.Empty, credential.AccessKey, 1755244800, "n-c347", body));

        Assert.True(TenantGameAppOpenAccessAuthMiddleware.VerifySignature(credential, "POST", "/api/FeatureSwitchSync/Sync", string.Empty, 1755244800, "n-c347", body, provided));
        Assert.False(TenantGameAppOpenAccessAuthMiddleware.VerifySignature(credential, "POST", "/api/FeatureSwitchSync/Sync", string.Empty, 1755244800, "n-c347",
            Encoding.UTF8.GetBytes("{\"version\":\"tampered\"}"), provided));
    }

    public void Dispose()
    {
        _mainDb.Dispose();
        foreach (var path in new[] { _mainDbPath, _tenantDbPath, })
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        GC.SuppressFinalize(this);
    }
}
