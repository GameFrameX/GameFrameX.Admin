using System.Security.Cryptography;
using GameFrameX.Core.SignatureAuth;
using System.Text;
using GameFrameX.Advertisement.Abstract;
using GameFrameX.Advertisement.AdMob;
using GameFrameX.Advertisement.TopOn;
using GameFrameX.Advertisement.UnityAds;
using GameFrameX.Client.Api.Integrations.Advertisement;
using GameFrameX.Client.Api.Services.ClientApiTenantOptions;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Foundation.Http.Normalization;
using GameFrameX.Foundation.Json;
using Microsoft.AspNetCore.Http;

namespace GameFrameX.Core.Tests.ClientApi;

/// <summary>
/// <see cref="GameAdvertisementRewardService"/> 单测（C114 / GFX-110）。
/// <para>覆盖三 Provider（TopOn / AdMob / UnityAds）验签 fail-closed、幂等重放、schema 驱动参数改名、发放失败→重试→成功。</para>
/// </summary>
public sealed class GameAdvertisementRewardServiceTests
{
    private const long RoleId = 7001;
    private const long UserId = 7002;
    private const long AreaId = 8001;
    private const long ClassifyId = 9001;
    private const string DispatchPath = "/game/api/req_player_send_advertisement_item";

    [Fact]
    public async Task TopOn_ValidSignature_DispatchesOnce_And_WritesSuccessLedger()
    {
        using var db = CreateDb();
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        handler.SetJson(DispatchPath, HttpJsonResultData<string>.Success(JsonHelper.Serialize(new GameAdWebHookController.PlayerSendAdvertisementItemResponse())).ToString());
        var option = new TopOnAdvertisementCallbackOption { signingSecret = "topon-secret", };
        var query = BuildTopOnQuery(option, "TX-1001", RoleId, null);
        SignTopOn(option, query);
        var service = CreateService(db, option, handler, query);

        var result = await service.ReceiveTopOnAsync();

        Assert.True(result.IsValid);
        Assert.Equal(AdvertisementRewardStatusEnum.Success, result.Status);
        Assert.Equal("adreward:topon:TX-1001", result.IdempotencyKey);
        Assert.Equal(1, handler.Count(DispatchPath));
        var rows = await db.TenantDb.Queryable<TenantGameAdvertisementRewardEntity>().ToListAsync();
        Assert.Single(rows);
        Assert.Equal(AdvertisementRewardStatusEnum.Success, rows[0].Status);
        Assert.Equal(db.AppId, rows[0].AppId);
    }

    [Fact]
    public async Task TopOn_InvalidSignature_FailsClosed_NoLedger_NoDispatch()
    {
        using var db = CreateDb();
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        handler.SetJson(DispatchPath, HttpJsonResultData<string>.Success(JsonHelper.Serialize(new GameAdWebHookController.PlayerSendAdvertisementItemResponse())).ToString());
        var option = new TopOnAdvertisementCallbackOption { signingSecret = "topon-secret", };
        var query = BuildTopOnQuery(option, "TX-2002", RoleId, null);
        SignTopOn(option, query);
        query["sign"] = "deadbeef";
        var service = CreateService(db, option, handler, query);

        var result = await service.ReceiveTopOnAsync();

        Assert.False(result.IsValid);
        Assert.Equal(601, result.Code);
        Assert.Equal(0, handler.Count(DispatchPath));
        var rows = await db.TenantDb.Queryable<TenantGameAdvertisementRewardEntity>().ToListAsync();
        Assert.Empty(rows);
    }

    [Fact]
    public async Task TopOn_Replay_IsIdempotent_NoSecondDispatch()
    {
        using var db = CreateDb();
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        handler.SetJson(DispatchPath, HttpJsonResultData<string>.Success(JsonHelper.Serialize(new GameAdWebHookController.PlayerSendAdvertisementItemResponse())).ToString());
        var option = new TopOnAdvertisementCallbackOption { signingSecret = "topon-secret", };
        var query = BuildTopOnQuery(option, "TX-3003", RoleId, null);
        SignTopOn(option, query);

        var first = await CreateService(db, option, handler, query).ReceiveTopOnAsync();
        var second = await CreateService(db, option, handler, query).ReceiveTopOnAsync();

        Assert.True(first.IsValid);
        Assert.True(second.IsValid);
        Assert.Equal(AdvertisementRewardStatusEnum.Success, second.Status);
        Assert.Equal(1, handler.Count(DispatchPath));
        var rows = await db.TenantDb.Queryable<TenantGameAdvertisementRewardEntity>().ToListAsync();
        Assert.Single(rows);
    }

    [Fact]
    public async Task TopOn_SchemaDrivenTransactionParamRename_VerifiesAndDispatches()
    {
        using var db = CreateDb();
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        handler.SetJson(DispatchPath, HttpJsonResultData<string>.Success(JsonHelper.Serialize(new GameAdWebHookController.PlayerSendAdvertisementItemResponse())).ToString());
        // schema 改名：交易参数从默认 trans_id 改为 tx_id（验签串字面量仍按 TopOn 协议固定）
        var option = new TopOnAdvertisementCallbackOption { signingSecret = "topon-secret", transactionIdParam = "tx_id", };
        var query = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["tx_id"] = "TX-4004",
            ["placement_id"] = "p-1",
            ["adsource_id"] = "a-1",
            ["reward_amount"] = "10",
            ["reward_name"] = "coin",
            ["user_id"] = RoleId.ToString(),
            ["is_test"] = "0",
        };
        SignTopOn(option, query, "TX-4004");
        var service = CreateService(db, option, handler, query);

        var result = await service.ReceiveTopOnAsync();

        Assert.True(result.IsValid);
        Assert.Equal(AdvertisementRewardStatusEnum.Success, result.Status);
        Assert.Equal("adreward:topon:TX-4004", result.IdempotencyKey);
        Assert.Equal(1, handler.Count(DispatchPath));
    }

    [Fact]
    public async Task AdMob_ValidSignature_Dispatches_And_WritesSuccessLedger()
    {
        using var db = CreateDb();
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        handler.SetJson(DispatchPath, HttpJsonResultData<string>.Success(JsonHelper.Serialize(new GameAdWebHookController.PlayerSendAdvertisementItemResponse())).ToString());
        var keyBytes = RandomNumberGenerator.GetBytes(32);
        var base64Key = Convert.ToBase64String(keyBytes);
        const string keyId = "12345";
        var option = new GoogleAdMobAdvertisementCallbackOption { verificationKeys = $"{keyId}={base64Key}", };
        var query = BuildAdMobQuery(option, "AD-5005", BuildAdMobCustomData(RoleId, UserId), keyId);
        SignAdMob(keyBytes, option, query);
        var advertisementOptions = BuildAdvertisementOptions("GoogleAdMob", option, new Dictionary<string, string>(StringComparer.Ordinal) { [keyId] = base64Key, });
        var service = CreateService(db, advertisementOptions, handler, query);

        var result = await service.ReceiveAdMobAsync();

        Assert.True(result.IsValid);
        Assert.Equal(AdvertisementRewardStatusEnum.Success, result.Status);
        Assert.Equal("adreward:admob:AD-5005", result.IdempotencyKey);
        Assert.Equal(1, handler.Count(DispatchPath));
        var rows = await db.TenantDb.Queryable<TenantGameAdvertisementRewardEntity>().ToListAsync();
        Assert.Single(rows);
        Assert.Equal(AdvertisementRewardStatusEnum.Success, rows[0].Status);
    }

    [Fact]
    public async Task AdMob_InvalidSignature_FailsClosed_NoLedger()
    {
        using var db = CreateDb();
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        handler.SetJson(DispatchPath, HttpJsonResultData<string>.Success(JsonHelper.Serialize(new GameAdWebHookController.PlayerSendAdvertisementItemResponse())).ToString());
        var keyBytes = RandomNumberGenerator.GetBytes(32);
        var base64Key = Convert.ToBase64String(keyBytes);
        const string keyId = "12345";
        var option = new GoogleAdMobAdvertisementCallbackOption { verificationKeys = $"{keyId}={base64Key}", };
        var query = BuildAdMobQuery(option, "AD-6006", BuildAdMobCustomData(RoleId, UserId), keyId);
        SignAdMob(keyBytes, option, query);
        query["signature"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var advertisementOptions = BuildAdvertisementOptions("GoogleAdMob", option, new Dictionary<string, string>(StringComparer.Ordinal) { [keyId] = base64Key, });
        var service = CreateService(db, advertisementOptions, handler, query);

        var result = await service.ReceiveAdMobAsync();

        Assert.False(result.IsValid);
        Assert.Equal(601, result.Code);
        Assert.Equal(0, handler.Count(DispatchPath));
        var rows = await db.TenantDb.Queryable<TenantGameAdvertisementRewardEntity>().ToListAsync();
        Assert.Empty(rows);
    }

    [Fact]
    public async Task UnityAds_ValidSignature_Dispatches_And_WritesSuccessLedger()
    {
        using var db = CreateDb();
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        handler.SetJson(DispatchPath, HttpJsonResultData<string>.Success(JsonHelper.Serialize(new GameAdWebHookController.PlayerSendAdvertisementItemResponse())).ToString());
        var option = new UnityAdsAdvertisementCallbackOption { signingSecret = "unity-secret", };
        var query = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["sid"] = RoleId.ToString(),
            ["oid"] = "OID-7007",
        };
        query["hmac"] = SignUnityAds(option.signingSecret, query["sid"], query["oid"]);
        var service = CreateService(db, BuildAdvertisementOptions("UnityAds", option), handler, query);

        var result = await service.ReceiveUnityAdsAsync();

        Assert.True(result.IsValid);
        Assert.Equal(AdvertisementRewardStatusEnum.Success, result.Status);
        Assert.Equal("adreward:unityads:OID-7007", result.IdempotencyKey);
        Assert.Equal(1, handler.Count(DispatchPath));
        var rows = await db.TenantDb.Queryable<TenantGameAdvertisementRewardEntity>().ToListAsync();
        Assert.Single(rows);
        Assert.Equal(AdvertisementRewardStatusEnum.Success, rows[0].Status);
    }

    [Fact]
    public async Task UnityAds_InvalidSignature_FailsClosed_NoLedger()
    {
        using var db = CreateDb();
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        handler.SetJson(DispatchPath, HttpJsonResultData<string>.Success(JsonHelper.Serialize(new GameAdWebHookController.PlayerSendAdvertisementItemResponse())).ToString());
        var option = new UnityAdsAdvertisementCallbackOption { signingSecret = "unity-secret", };
        var query = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["sid"] = RoleId.ToString(),
            ["oid"] = "OID-8008",
            ["hmac"] = "deadbeef",
        };
        var service = CreateService(db, BuildAdvertisementOptions("UnityAds", option), handler, query);

        var result = await service.ReceiveUnityAdsAsync();

        Assert.False(result.IsValid);
        Assert.Equal(601, result.Code);
        Assert.Equal(0, handler.Count(DispatchPath));
        var rows = await db.TenantDb.Queryable<TenantGameAdvertisementRewardEntity>().ToListAsync();
        Assert.Empty(rows);
    }

    [Fact]
    public async Task TopOn_DispatchFailure_MarksFailed_Then_Retry_MarksSuccess_ReusingLedgerRow()
    {
        using var db = CreateDb();
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        var option = new TopOnAdvertisementCallbackOption { signingSecret = "topon-secret", };
        var query = BuildTopOnQuery(option, "TX-9009", RoleId, null);
        SignTopOn(option, query);

        // 第一次：dispatch 路径未注册 → 游戏服返回 NotFound → Failed
        var first = await CreateService(db, option, handler, query).ReceiveTopOnAsync();
        Assert.False(first.IsValid);
        Assert.Equal(AdvertisementRewardStatusEnum.Failed, first.Status);
        Assert.NotNull(first.Message);
        var rowsAfterFirst = await db.TenantDb.Queryable<TenantGameAdvertisementRewardEntity>().ToListAsync();
        Assert.Single(rowsAfterFirst);
        Assert.Equal(AdvertisementRewardStatusEnum.Failed, rowsAfterFirst[0].Status);
        var firstLedgerId = rowsAfterFirst[0].Id;

        // 第二次：注册 dispatch 路径 → 同 IdempotencyKey 复用同一行 → Success
        handler.SetJson(DispatchPath, HttpJsonResultData<string>.Success(JsonHelper.Serialize(new GameAdWebHookController.PlayerSendAdvertisementItemResponse())).ToString());
        var second = await CreateService(db, option, handler, query).ReceiveTopOnAsync();
        Assert.True(second.IsValid);
        Assert.Equal(AdvertisementRewardStatusEnum.Success, second.Status);
        var rowsAfterSecond = await db.TenantDb.Queryable<TenantGameAdvertisementRewardEntity>().ToListAsync();
        Assert.Single(rowsAfterSecond);
        Assert.Equal(firstLedgerId, rowsAfterSecond[0].Id);
        Assert.Equal(AdvertisementRewardStatusEnum.Success, rowsAfterSecond[0].Status);
        Assert.Null(rowsAfterSecond[0].FailureReason);
    }

    private static ClientApiTestDb CreateDb()
    {
        var db = new ClientApiTestDb();
        db.InitTenantTables(typeof(TenantGameAdvertisementRewardEntity), typeof(TenantGameRoleEntity), typeof(TenantGameAreaEntity));
        db.SeedAreaClassify(ClassifyId);
        db.SeedArea(AreaId, ClassifyId);
        db.SeedRole(RoleId, UserId, AreaId, db.AppId);
        return db;
    }

    private static GameAdvertisementRewardService CreateService(ClientApiTestDb db, TopOnAdvertisementCallbackOption option, ClientApiRecordingHttpHandler handler, Dictionary<string, string> query)
    {
        return CreateService(db, BuildAdvertisementOptions("TopOn", option), handler, query);
    }

    private static GameAdvertisementRewardService CreateService(ClientApiTestDb db, TenantAdvertisementCallbackOptions advertisementOptions, ClientApiRecordingHttpHandler handler, Dictionary<string, string> query)
    {
        var httpContext = db.CreateHttpContext();
#pragma warning disable CS8620 // 测试输入 Dictionary<string,string> 与 QueryString.Create 期望的 string? 仅可空性差异，运行时等价
        httpContext.Request.QueryString = QueryString.Create(query);
#pragma warning restore CS8620
        var accessor = new HttpContextAccessor { HttpContext = httpContext, };
        var optionsProvider = new FakeAdvertisementOptionsProvider(advertisementOptions);
        var roleRepository = db.Repository<TenantGameRoleEntity>();
        var factory = new AdvertisementCallbackFactory(new IAdvertisementCallbackHandler[]
        {
            new TopOnAdvertisementCallbackHandler(roleRepository),
            new AdMobAdvertisementCallbackHandler(roleRepository),
            new UnityAdsAdvertisementCallbackHandler(roleRepository),
        });
        return new GameAdvertisementRewardService(
            optionsProvider,
            db.Repository<TenantGameAdvertisementRewardEntity>(),
            db.Repository<TenantGameAreaEntity>(),
            new HttpClient(handler, false),
            accessor,
            factory);
    }

    private static TenantAdvertisementCallbackOptions BuildAdvertisementOptions(string provider, IAdvertisementCallbackOptionSetting option, IReadOnlyDictionary<string, string>? adMobKeys = null)
    {
        return new TenantAdvertisementCallbackOptions
        {
            Provider = provider,
            ConfigJson = JsonHelper.Serialize(option),
            AdMobKeys = adMobKeys ?? new Dictionary<string, string>(StringComparer.Ordinal),
        };
    }

    private static Dictionary<string, string> BuildTopOnQuery(TopOnAdvertisementCallbackOption option, string transId, long roleId, string? signOverride)
    {
        var query = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [option.transactionIdParam] = transId,
            [option.placementIdParam] = "p-1",
            [option.adsourceIdParam] = "a-1",
            [option.rewardAmountParam] = "10",
            [option.rewardNameParam] = "coin",
            [option.userIdParam] = roleId.ToString(),
            ["is_test"] = "0",
            [option.signParam] = signOverride ?? string.Empty,
        };
        return query;
    }

    private static Dictionary<string, string> BuildAdMobQuery(GoogleAdMobAdvertisementCallbackOption option, string transactionId, string customData, string keyId)
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [option.transactionIdParam] = transactionId,
            [option.adUnitParam] = "ca-app-pub-1",
            [option.rewardItemParam] = "coin",
            [option.rewardAmountParam] = "10",
            [option.customDataParam] = customData,
            [option.keyIdParam] = keyId,
            [option.signatureParam] = string.Empty,
        };
    }

    private static string BuildAdMobCustomData(long playerId, long userId)
    {
        return JsonHelper.Serialize(new AdMobCallbackCustomDataRequest
        {
            PlayerId = playerId.ToString(),
            UserId = userId.ToString(),
            IsSandbox = "false",
        });
    }

    /// <summary>
    /// TopOn 签名：MD5(trans_id=..&placement_id=..&adsource_id=..&reward_amount=..&reward_name=..&sec_key=..) 小写 hex。
    /// 签名串字面量按 TopOn 协议固定；参数值来自 <paramref name="query"/>（schema 取值后回填）。
    /// </summary>
    private static void SignTopOn(TopOnAdvertisementCallbackOption option, Dictionary<string, string> query, string? transactionId = null)
    {
        var tx = transactionId ?? query[option.transactionIdParam];
        var raw = "trans_id=" + tx +
                  "&placement_id=" + query[option.placementIdParam] +
                  "&adsource_id=" + query[option.adsourceIdParam] +
                  "&reward_amount=" + query[option.rewardAmountParam] +
                  "&reward_name=" + query[option.rewardNameParam] +
                  "&sec_key=" + option.signingSecret;
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(raw));
        query[option.signParam] = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
    }

    /// <summary>
    /// AdMob 签名：HMAC-SHA256(base64key, "k1=v1&k2=v2..."（剔除 signature/key_id，按 key 字典序）) base64。
    /// </summary>
    private static void SignAdMob(byte[] keyBytes, GoogleAdMobAdvertisementCallbackOption option, Dictionary<string, string> query)
    {
        var stringToSign = string.Join("&", query
                                            .Where(p => p.Key != option.signatureParam && p.Key != option.keyIdParam)
                                            .OrderBy(p => p.Key, StringComparer.Ordinal)
                                            .Select(p => p.Key + "=" + p.Value));
        var expected = CanonicalRequestBuilder.HmacSha256(keyBytes, stringToSign);
        query[option.signatureParam] = Convert.ToBase64String(expected);
    }

    /// <summary>
    /// Unity Ads 签名：HMAC-SHA256(secret, "sid,oid") 小写 hex。
    /// </summary>
    private static string SignUnityAds(string secret, string sid, string oid)
    {
        var raw = sid + "," + oid;
        var expected = CanonicalRequestBuilder.HmacSha256(Encoding.UTF8.GetBytes(secret), raw);
        return BitConverter.ToString(expected).Replace("-", string.Empty).ToLowerInvariant();
    }

    /// <summary>
    /// 仅广告回调配置可注入的 <see cref="IClientApiTenantOptionsProvider"/> 桩。
    /// </summary>
    private sealed class FakeAdvertisementOptionsProvider : IClientApiTenantOptionsProvider
    {
        private readonly TenantAdvertisementCallbackOptions _adOptions;

        public FakeAdvertisementOptionsProvider(TenantAdvertisementCallbackOptions advertisementOptions)
        {
            _adOptions = advertisementOptions;
        }

        public ClientApiTenantResolution ResolveContext()
        {
            return new ClientApiTenantResolution();
        }

        public Task<TenantEmailOptions> GetEmailOptionsAsync()
        {
#pragma warning disable CS8625 // 测试桩：本场景只测广告回调，邮件策略返回 null 表示未配置
            return Task.FromResult<TenantEmailOptions>(null);
#pragma warning restore CS8625
        }

        public Task<TenantAdvertisementCallbackOptions> GetAdvertisementCallbackOptionsAsync()
        {
            return Task.FromResult(_adOptions);
        }

        public Task<TenantClientApiTokenPolicyOptions> GetTokenPolicyAsync()
        {
#pragma warning disable CS8625 // 测试桩：本场景只测广告回调，Token 策略返回 null 表示未配置
            return Task.FromResult<TenantClientApiTokenPolicyOptions>(null);
#pragma warning restore CS8625
        }

        public Task<TenantGameLoginTicketPolicyOptions> GetGameLoginTicketPolicyAsync()
        {
            return Task.FromResult(new TenantGameLoginTicketPolicyOptions());
        }

        public Task<TenantOrderPolicyOptions> GetOrderPolicyAsync()
        {
            return Task.FromResult(new TenantOrderPolicyOptions());
        }

        public Task<TenantAreaPolicyOptions> GetAreaPolicyAsync()
        {
            return Task.FromResult(new TenantAreaPolicyOptions());
        }
    }
}