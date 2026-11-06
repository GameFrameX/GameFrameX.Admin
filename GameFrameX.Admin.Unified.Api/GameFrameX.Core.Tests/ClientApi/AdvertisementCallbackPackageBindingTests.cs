using System.Security.Cryptography;
using System.Text;
using GameFrameX.Advertisement.Abstract;
using GameFrameX.Advertisement.TopOn;
using GameFrameX.Application.ClientDelivery.AdvertisementCallbackConfig;
using GameFrameX.Application.ClientDelivery.AdvertisementCallbackConfig.Dto;
using GameFrameX.Client.Api.Integrations.Advertisement;
using GameFrameX.Client.Api.Services.ClientApiOptions;
using GameFrameX.Core.Enum.Errors;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Middleware;
using GameFrameX.Core.Service.Business;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Business.ClientApiConfig;
using GameFrameX.Entity.Business.Game;
using GameFrameX.Foundation.Http.Normalization;
using GameFrameX.Foundation.Json;
using Microsoft.AspNetCore.Http;

namespace GameFrameX.Core.Tests.ClientApi;

/// <summary>
/// 广告回调配置按 PackageId 绑定（R5）闭环测试：真实 <see cref="ClientApiOptionsProvider"/> + 真实奖励服务。
/// <para>覆盖：分包隔离验签（各包只能用自己的密钥）、跨包密钥拒绝、无配置 fail-closed、
/// 重复 (PackageId, Provider) 保存拒绝（31800001）、直插多条命中解析 fail-closed、账本 PackageId 与所选配置一致。
/// 路由缺 packageId 的 404 由 <see cref="PublicCallbackLegacyRouteRejectionTests"/> 覆盖。</para>
/// </summary>
public sealed class AdvertisementCallbackPackageBindingTests : IDisposable
{
    private const long RoleId = 7301;
    private const long UserId = 7302;
    private const long AreaId = 7303;
    private const long ClassifyId = 7304;
    private const long PackageA = 7311;
    private const long PackageB = 7312;
    private const long PackageWithoutConfig = 7313;
    private const string DispatchPath = "/game/api/req_player_send_advertisement_item";

    private readonly string? _originalKey;

    public AdvertisementCallbackPackageBindingTests()
    {
        _originalKey = Environment.GetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable);
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, "unit-test-secret-key");
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(SecretCrypto.KeyEnvironmentVariable, _originalKey);
    }

    [Fact]
    public async Task TwoPackages_SameProvider_EachVerifiesOnlyWithOwnSecret_AndLedgerCarriesPackageId()
    {
        using var db = CreateDb();
        InsertConfig(db, 7401, PackageA, "secret-alpha");
        InsertConfig(db, 7402, PackageB, "secret-beta");

        var alpha = await ReceiveTopOnAsync(db, PackageA, "TX-PKG-A", "secret-alpha");
        Assert.True(alpha.IsValid, alpha.Message);
        Assert.Equal(PackageA, (await SingleLedgerAsync(db)).PackageId);

        db.MainDb.Deleteable<GameAdvertisementRewardEntity>().ExecuteCommand();
        var beta = await ReceiveTopOnAsync(db, PackageB, "TX-PKG-B", "secret-beta");
        Assert.True(beta.IsValid, beta.Message);
        Assert.Equal(PackageB, (await SingleLedgerAsync(db)).PackageId);
    }

    [Fact]
    public async Task CrossPackageSecret_IsRejected_NoLedger()
    {
        using var db = CreateDb();
        InsertConfig(db, 7401, PackageA, "secret-alpha");
        InsertConfig(db, 7402, PackageB, "secret-beta");

        // 以包 B 的密钥签名，却走包 A 的回调路由：必须验签失败且不写账本
        var result = await ReceiveTopOnAsync(db, PackageA, "TX-CROSS", "secret-beta");

        Assert.False(result.IsValid);
        Assert.Equal(601, result.Code);
        Assert.Empty(await db.MainDb.Queryable<GameAdvertisementRewardEntity>().ToListAsync());
    }

    [Fact]
    public async Task PackageWithoutConfig_FailsClosed()
    {
        using var db = CreateDb();
        InsertConfig(db, 7401, PackageA, "secret-alpha");

        var result = await ReceiveTopOnAsync(db, PackageWithoutConfig, "TX-NOCONF", "secret-alpha");

        Assert.False(result.IsValid);
        Assert.Empty(await db.MainDb.Queryable<GameAdvertisementRewardEntity>().ToListAsync());
    }

    [Fact]
    public async Task DuplicatePackageProvider_SaveIsRejected()
    {
        using var db = CreateDb();
        var service = CreateConfigService(db);

        await service.AddAsync(BuildAddRequest(PackageA, "secret-alpha"));

        var exception = await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(BuildAddRequest(PackageA, "secret-alpha-2")));

        Assert.Equal((int)AdvertisementCallbackConfigErrorCode.PackageProviderAlreadyExists, exception.Code);
    }

    [Fact]
    public async Task MultipleMatchingRows_DirectInsert_ResolutionFailsClosed()
    {
        using var db = CreateDb();
        // 绕过服务层直接插两条同 (PackageId, Provider) 启用行：解析必须 fail-closed（不回退最新一条）
        InsertConfig(db, 7401, PackageA, "secret-alpha");
        InsertConfig(db, 7402, PackageA, "secret-alpha-dup");

        var provider = CreateOptionsProvider(db, PackageA);
        Assert.Null(await provider.GetAdvertisementCallbackOptionsAsync());

        var result = await ReceiveTopOnAsync(db, PackageA, "TX-DUPCONF", "secret-alpha");
        Assert.False(result.IsValid);
        Assert.Empty(await db.MainDb.Queryable<GameAdvertisementRewardEntity>().ToListAsync());
    }

    private static AddAdvertisementCallbackConfigRequest BuildAddRequest(long packageId, string signingSecret)
    {
        return new AddAdvertisementCallbackConfigRequest
        {
            PackageId = packageId,
            Provider = "TopOn",
            CallbackUrl = "https://cb.example.test/hook",
            SigningSecret = signingSecret,
            VerificationKeys = string.Empty,
            ConfigJson = JsonHelper.Serialize(new TopOnAdvertisementCallbackOption
            {
                callbackUrl = "https://cb.example.test/hook",
                signingSecret = signingSecret,
            }),
            IsEnabled = true,
        };
    }

    private static AdvertisementCallbackConfigService CreateConfigService(ClientApiTestDb db)
    {
        return new AdvertisementCallbackConfigService(
            new Repository<AdvertisementCallbackConfigEntity>(db.MainDb),
            new AdvertisementCallbackConfigSchemaProvider(),
            new Repository<Entity.Business.Game.App.GameAppPackageEntity>(db.MainDb));
    }

    private static void InsertConfig(ClientApiTestDb db, long id, long packageId, string signingSecret)
    {
        db.MainDb.Insertable(new AdvertisementCallbackConfigEntity
        {
            Id = id,
            PackageId = packageId,
            Provider = "TopOn",
            CallbackUrl = "https://cb.example.test/hook",
            SigningSecret = SecretCrypto.Encrypt(signingSecret) ?? signingSecret,
            VerificationKeys = string.Empty,
            ConfigJson = JsonHelper.Serialize(new TopOnAdvertisementCallbackOption
            {
                callbackUrl = "https://cb.example.test/hook",
                signingSecret = signingSecret,
            }),
            IsEnabled = true,
        }).ExecuteCommand();
    }

    private static async Task<GameAdvertisementRewardEntity> SingleLedgerAsync(ClientApiTestDb db)
    {
        var rows = await db.MainDb.Queryable<GameAdvertisementRewardEntity>().ToListAsync();
        return Assert.Single(rows);
    }

    private static async Task<AdvertisementRewardReceiveResult> ReceiveTopOnAsync(ClientApiTestDb db, long packageId, string transId, string signingSecret)
    {
        var handler = ClientApiRecordingHttpHandler.WithGameServerDefaults();
        handler.SetJson(DispatchPath, HttpJsonResultData<string>.Success(JsonHelper.Serialize(new GameAdWebHookController.PlayerSendAdvertisementItemResponse())).ToString());
        var option = new TopOnAdvertisementCallbackOption { signingSecret = signingSecret, };
        var query = BuildTopOnQuery(option, transId);
        SignTopOn(signingSecret, option, query);

        var httpContext = new DefaultHttpContext();
        httpContext.Items[PublicCallbackRouteContext.ItemKey] = new PublicCallbackRouteContext("topon", "ad", packageId);
#pragma warning disable CS8620 // 测试输入 Dictionary<string,string> 与 QueryString.Create 期望的 string? 仅可空性差异，运行时等价
        httpContext.Request.QueryString = QueryString.Create(query);
#pragma warning restore CS8620

        var roleRepository = db.Repository<GameRoleEntity>();
        var factory = new AdvertisementCallbackFactory(new IAdvertisementCallbackHandler[]
        {
            new TopOnAdvertisementCallbackHandler(roleRepository),
        });
        var service = new GameAdvertisementRewardService(
            CreateOptionsProvider(db, packageId),
            db.Repository<GameAdvertisementRewardEntity>(),
            db.Repository<GameAreaEntity>(),
            new HttpClient(handler, false),
            new HttpContextAccessor { HttpContext = httpContext, },
            factory);
        return await service.ReceiveTopOnAsync();
    }

    private static ClientApiOptionsProvider CreateOptionsProvider(ClientApiTestDb db, long packageId)
    {
        var context = new DefaultHttpContext();
        context.Items[PublicCallbackRouteContext.ItemKey] = new PublicCallbackRouteContext("topon", "ad", packageId);
        return new ClientApiOptionsProvider(db.MainDb, new HttpContextAccessor { HttpContext = context, });
    }

    private static ClientApiTestDb CreateDb()
    {
        var db = new ClientApiTestDb();
        db.InitBusinessTables(typeof(GameAdvertisementRewardEntity), typeof(GameRoleEntity), typeof(GameAreaEntity), typeof(AdvertisementCallbackConfigEntity));
        db.SeedAreaClassify(ClassifyId);
        db.SeedArea(AreaId, ClassifyId);
        db.SeedRole(RoleId, UserId, AreaId);
        db.SeedPackage(PackageA, "com.example.alpha");
        db.SeedPackage(PackageB, "com.example.beta");
        db.SeedPackage(PackageWithoutConfig, "com.example.noconf");
        return db;
    }

    private static Dictionary<string, string> BuildTopOnQuery(TopOnAdvertisementCallbackOption option, string transId)
    {
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [option.transactionIdParam] = transId,
            [option.placementIdParam] = "p-1",
            [option.adsourceIdParam] = "a-1",
            [option.rewardAmountParam] = "10",
            [option.rewardNameParam] = "coin",
            [option.userIdParam] = RoleId.ToString(),
            ["is_test"] = "0",
            [option.signParam] = string.Empty,
        };
    }

    /// <summary>
    /// TopOn 签名：MD5(trans_id=..&amp;placement_id=..&amp;adsource_id=..&amp;reward_amount=..&amp;reward_name=..&amp;sec_key=..) 小写 hex
    /// （与 <c>GameAdvertisementRewardServiceTests</c> 同一协议口径）。
    /// </summary>
    private static void SignTopOn(string signingSecret, TopOnAdvertisementCallbackOption option, Dictionary<string, string> query)
    {
        var raw = "trans_id=" + query[option.transactionIdParam] +
                  "&placement_id=" + query[option.placementIdParam] +
                  "&adsource_id=" + query[option.adsourceIdParam] +
                  "&reward_amount=" + query[option.rewardAmountParam] +
                  "&reward_name=" + query[option.rewardNameParam] +
                  "&sec_key=" + signingSecret;
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(raw));
        query[option.signParam] = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
    }
}
