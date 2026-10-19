using GameFrameX.Client.Api.Middleware;
using GameFrameX.Core.Middleware;
using GameFrameX.Client.Api.Services;
using GameFrameX.Core.Enum;
using GameFrameX.Client.Api.GameWebShop.Dto;
using GameFrameX.Foundation.Hash;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Base;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Tenant.Device;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Entity.Tenant.Game.Announcement;
using GameFrameX.Entity.Tenant.Game.App;
using GameFrameX.Entity.Tenant.Game.RemoteConfig;
using GameFrameX.Entity.Tenant.Game.FeatureSwitch;
using GameFrameX.Entity.Tenant.Game.Release;
using GameFrameX.Foundation.Json;
using GameFrameX.Foundation.Orm.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
using GameFrameX.Core.Extension;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Tests.ClientApi;

internal sealed class ClientApiTestDb : IDisposable
{
    private readonly List<string> _dbPaths = new();

    public ClientApiTestDb(long tenantId = 1080001, long appId = 1081001)
    {
        TenantId = tenantId;
        AppId = appId;
        MainDb = CreateScope("main");
        TenantDbPath = CreateDatabasePath("tenant");
        MainDb.CodeFirst.InitTables(typeof(SystemTenantEntity));
        MainDb.Insertable(new SystemTenantEntity
        {
            Id = TenantId,
            TenantType = TenantTypeEnum.Db,
            DbType = DbType.Sqlite,
            Connection = $"DataSource={TenantDbPath}",
            ConfigId = TenantId.ToString(),
            IsEnabled = true,
        }).ExecuteCommand();

        TenantIdResolver = new DelegateSqlSugarTenantIdResolver(() => TenantId);
        TenantDbProvider = new ClientApiTenantDbProvider(MainDb, TenantIdResolver);
        TenantDb = TenantDbProvider.GetTenantDb(TenantId);
    }

    public long TenantId { get; }

    public long AppId { get; }

    public SqlSugarScope MainDb { get; }

    public ISqlSugarClient TenantDb { get; }

    public string TenantDbPath { get; }

    public ClientApiTenantDbProvider TenantDbProvider { get; }

    public ISqlSugarTenantIdResolver TenantIdResolver { get; }

    public Repository<T> Repository<T>() where T : EntityBase, new()
    {
        return new Repository<T>(MainDb, TenantDbProvider, TenantIdResolver);
    }

    public ClientApiTestDb InitTenantTables(params Type[] tableTypes)
    {
        TenantDb.CodeFirst.InitTables(tableTypes);
        return this;
    }

    public DefaultHttpContext CreateHttpContext(bool includeTenant = true, bool includeApp = true)
    {
        return CreateHttpContext(includeTenant, includeApp, null);
    }

    public DefaultHttpContext CreateHttpContext(bool includeTenant, bool includeApp, long? packageId)
    {
        var context = new DefaultHttpContext();
        if (includeTenant)
        {
            context.Items[TenantContextAccessor.TenantIdKey] = TenantId;
        }

        if (includeApp)
        {
            context.Items[AppResolutionMiddleware.AppIdKey] = AppId;
        }

        if (packageId.HasValue)
        {
            context.Items[PaymentOptionSettingProvider.PackageIdKey] = packageId.Value;
        }

        return context;
    }

    public void AttachContext(Controller controller, bool includeTenant = true, bool includeApp = true)
    {
        controller.ControllerContext = new ControllerContext { HttpContext = CreateHttpContext(includeTenant, includeApp), };
    }

    public TenantGameAppEntity SeedApp(string appSecret = "secret")
    {
        InitTenantTables(typeof(TenantGameAppEntity));
        var entity = WithBasic(new TenantGameAppEntity
        {
            Id = AppId,
            Name = "C108 Test App",
            AppSecret = appSecret,
            IsEnabled = true,
            IsDeleted = false,
            UpdateTime = 1,
        });
        TenantDb.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public TenantGameAppPackageEntity SeedPackage(long packageId, string packageName)
    {
        InitTenantTables(typeof(TenantGameAppPackageEntity));
        var entity = WithSelect(new TenantGameAppPackageEntity
        {
            Id = packageId,
            Name = "C108 Test Package",
            GameAppId = AppId,
            PackageName = packageName,
            PlatformCode = "ios",
            PlatformName = "iOS",
            ChannelCode = "official",
            ChannelName = "Official",
            IsEnabled = true,
            IsDeleted = false,
        });
        TenantDb.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public TenantGameAreaClassifyEntity SeedAreaClassify(long classifyId)
    {
        InitTenantTables(typeof(TenantGameAreaClassifyEntity));
        var entity = WithSelect(new TenantGameAreaClassifyEntity
        {
            Id = classifyId,
            Name = "Asia",
            AppId = AppId,
            LocalizationKey = "area_asia",
            DockerRegister = "registry.test",
            ImageName = "game-server",
            DockerUserName = "user",
            DockerPassword = "password",
            ContainerNetName = "game-net",
            ContainerSubnet = "10.1.0.0/16",
            ContainerGateway = "10.1.0.1",
            IsEnabled = true,
            IsDeleted = false,
        });
        TenantDb.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public TenantGameAreaEntity SeedArea(long areaId, long classifyId, bool isEnabled = true, GameServerStatus status = GameServerStatus.Online)
    {
        InitTenantTables(typeof(TenantGameAreaEntity));
        var entity = WithSelect(new TenantGameAreaEntity
        {
            Id = areaId,
            Name = $"Area-{areaId}",
            ClassifyId = classifyId,
            GameAppId = AppId,
            IsEnabled = isEnabled,
            IsDeleted = false,
            ServerStatus = status,
            HttpManageUrl = "https://game.example.test",
            TcpSocketUrl = "tcp://game.example.test:7000",
            WebSocketUrl = "wss://game.example.test/ws",
            Capacity = 1000,
            CurrentLoad = 10,
            CreationTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            LastMaintenanceTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ServerVersion = "1.0.0",
            ServerType = "Normal",
            LocalizationKey = "area_test",
            Language = GameLanguage.ChineseSimplified,
            AllowedPlayerProfile = "all",
            ServerEvents = "none",
            AccessControls = "public",
            ServerContainerId = "container",
            ServerParams = "{}",
            AppVersion = "1.0.0",
        });
        entity.IsEnabled = isEnabled;
        TenantDb.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public TenantGameAppFeatureSwitchEntity SeedFeatureSwitch(TenantGameAppFeatureSwitchEntity entity)
    {
        InitTenantTables(typeof(TenantGameAppFeatureSwitchEntity));
        WithBasic(entity);
        TenantDb.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public TenantGameAppRemoteConfigEntity SeedRemoteConfig(long id, string packageName, string content)
    {
        InitTenantTables(typeof(TenantGameAppRemoteConfigEntity));
        var entity = WithBasic(new TenantGameAppRemoteConfigEntity
        {
            Id = id,
            AppId = AppId,
            Platform = "ios",
            Package = packageName,
            AppVersion = "1.0.0",
            Language = "zh-CN",
            Channel = "official",
            CheckResourceUrl = "https://client.example.test/resource",
            CheckAppVersionUrl = "https://client.example.test/app",
            Content = content,
            AotCodeList = "[]",
            Priority = 10,
            RolloutPercent = 100,
            Description = "C108 global info",
        });
        TenantDb.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public TenantGameAppReleaseVersionEntity SeedAppVersion(long id, string packageName)
    {
        InitTenantTables(typeof(TenantGameAppReleaseVersionEntity));
        var entity = WithBasic(new TenantGameAppReleaseVersionEntity
        {
            Id = id,
            AppId = AppId,
            Platform = "ios",
            Package = packageName,
            OldVersion = "1.0.0",
            AppVersion = "1.1.0",
            Language = "zh-CN",
            Channel = "official",
            DownloadUrl = "https://client.example.test/app.ipa",
            IsForce = true,
            Title = "Update",
            Announcement = "Upgrade now",
            Description = "C108 app version",
        });
        TenantDb.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public TenantGameAppAssetPackageVersionEntity SeedAssetPackageVersion(long id, string packageName)
    {
        InitTenantTables(typeof(TenantGameAppAssetPackageVersionEntity), typeof(TenantGameAppDeviceEntity));
        var entity = WithBasic(new TenantGameAppAssetPackageVersionEntity
        {
            Id = id,
            AppId = AppId,
            Platform = "ios",
            Package = packageName,
            AppVersion = "1.0.0",
            Language = "zh-CN",
            Channel = "official",
            AssetPackageName = "main",
            RootPath = "https://cdn.example.test/assets",
            ResourceVersion = "2026.07.18",
            Description = "C108 asset package",
        });
        TenantDb.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public TenantGameAnnouncementEntity SeedAnnouncement(long id, long? areaId, string language, string title, bool isActive = true)
    {
        InitTenantTables(typeof(TenantGameAnnouncementEntity));
        var entity = WithBasic(new TenantGameAnnouncementEntity
        {
            Id = id,
            AppId = AppId,
            AreaId = areaId,
            Language = language,
            Title = title,
            Content = $"{title} content",
            StartTime = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds(),
            EndTime = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds(),
            Priority = 10,
            IsActive = isActive,
            IsEnabled = true,
            IsDeleted = false,
        });
        TenantDb.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public TenantGameUserEntity SeedUser(
        long userId,
        string userName,
        string password = "password",
        GameUserStatus status = GameUserStatus.Active,
        string email = "",
        string deviceId = "")
    {
        InitTenantTables(typeof(TenantGameUserEntity));
        var entity = WithSelect(new TenantGameUserEntity
        {
            Id = userId,
            Name = userName,
            UserName = userName,
            Password = PasswordHashHelper.Hash(PasswordHashAlgorithmKind.Argon2id, password),
            Email = email,
            EmailPassword = string.IsNullOrWhiteSpace(email) ? string.Empty : PasswordHashHelper.Hash(PasswordHashAlgorithmKind.Argon2id, password),
            Status = status,
            Phone = string.Empty,
            Avatar = string.Empty,
            GoogleId = string.Empty,
            AppleId = string.Empty,
            FacebookId = string.Empty,
            TwitterId = string.Empty,
            DiscordId = string.Empty,
            QqId = string.Empty,
            WeChatId = string.Empty,
            DeviceId = deviceId,
            IsEnabled = true,
            IsDeleted = false,
        });
        TenantDb.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public TenantGameRoleEntity SeedRole(long roleId, long userId, long areaId, long appId = 0, string name = "Role", bool isSandbox = false)
    {
        InitTenantTables(typeof(TenantGameRoleEntity));
        var entity = WithSelect(new TenantGameRoleEntity
        {
            Id = roleId,
            AppId = appId > 0 ? appId : AppId,
            UserId = userId,
            AreaId = areaId,
            Name = name,
            Description = $"{name} description",
            Level = 1,
            IsSandbox = isSandbox,
            LastLoginTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            State = 0,
            Avatar = 0,
            CurrentExp = 0,
            CurrentLevelConfigId = 0,
            Gender = false,
            AvatarFrame = 0,
            AllRecharge = 0,
            OnlineTime = 0,
        });
        TenantDb.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public TenantGameGoodsEntity SeedGoods(long goodsId, long classifyId = 1, int configId = 10001, bool isEnabled = true)
    {
        InitTenantTables(typeof(TenantGameGoodsEntity));
        var entity = WithSelect(new TenantGameGoodsEntity
        {
            Id = goodsId,
            AppId = AppId,
            Name = $"Goods-{goodsId}",
            Description = "C108 goods",
            ClassifyId = classifyId,
            ConfigId = configId,
            Star = false,
            IsEnabled = isEnabled,
            IsDeleted = false,
        });
        TenantDb.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public TenantGameWebShopEntity SeedWebShop(long shopId, string realPrice, string itemJson, bool isUp = true, int weight = 10)
    {
        InitTenantTables(typeof(TenantGameWebShopEntity));
        var entity = WithSelect(new TenantGameWebShopEntity
        {
            Id = shopId,
            AppId = AppId,
            Name = $"Shop-{shopId}",
            Logo = "https://client.example.test/logo.png",
            Description = "C108 webshop",
            KeyWords = "gift",
            Price = realPrice,
            RealPrice = realPrice,
            ItemJson = itemJson,
            IsUp = isUp,
            Sales = 0,
            Weight = weight,
        });
        TenantDb.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public TenantGameOrderEntity SeedOrder(long orderId, long roleId, long areaId, string payChannel = "google", OrderStatusEnum status = OrderStatusEnum.Unpaid, decimal amount = 100m, long sku = 1, string? providerTransactionId = null, string? orderProviderId = null, bool isSandbox = false)
    {
        InitTenantTables(typeof(TenantGameOrderEntity));
        var entity = WithBasic(new TenantGameOrderEntity
        {
            Id = orderId,
            AppId = AppId,
            Status = status,
            RoleId = roleId.ToString(),
            Amount = (uint)amount,
            Content = JsonHelper.Serialize(new PlayerCreateOrderResponse
            {
                RoleId = roleId,
                Sku = sku,
                Amount = (uint)amount,
                IsNeedPay = true,
                RewardsItem = new List<PropertyItemResponse>
                {
                    new() { Id = 1, ItemId = 1, Count = 1, },
                },
            }),
            AreaId = areaId,
            OrderId = orderProviderId ?? $"provider-{orderId}",
            Sku = sku,
            PayChannel = payChannel,
            Channel = "channel",
            Platform = "ios",
            IsSandbox = isSandbox,
            AdventureSuffix = string.Empty,
            AccountId = $"account-{roleId}",
            RoleRegisterTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AppleTransactionId = string.Empty,
            AppleOriginalTransactionId = string.Empty,
            AppleReceiptEnvironment = string.Empty,
            AppleVerifiedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ProviderTransactionId = providerTransactionId ?? string.Empty,
        });
        TenantDb.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public TenantGameRedeemCodeEntity SeedRedeemCode(long codeId, string code, long areaId, string contents, bool isActive = true, int usageLimit = 1, int usedCount = 0, DateTime? startTime = null, DateTime? endTime = null, long? channelId = null)
    {
        InitTenantTables(typeof(TenantGameRedeemCodeEntity));
        var entity = WithBasic(new TenantGameRedeemCodeEntity
        {
            Id = codeId,
            AppId = AppId,
            AreaId = areaId,
            ChannelId = channelId,
            Code = code,
            Description = "C108 redeem code",
            IsActive = isActive,
            UsageLimit = usageLimit,
            UsedCount = usedCount,
            StartTime = startTime != null ? new DateTimeOffset(startTime.Value).ToUnixTimeSeconds() : DateTimeOffset.Now.AddHours(-1).ToUnixTimeSeconds(),
            EndTime = endTime != null ? new DateTimeOffset(endTime.Value).ToUnixTimeSeconds() : DateTimeOffset.Now.AddHours(1).ToUnixTimeSeconds(),
            Contents = contents,
        });
        TenantDb.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public TenantGameRedeemCodeChannelEntity SeedChannel(long channelId, string name, int roleUsageLimit = 0)
    {
        InitTenantTables(typeof(TenantGameRedeemCodeChannelEntity));
        var entity = WithSelect(new TenantGameRedeemCodeChannelEntity
        {
            Id = channelId,
            AppId = AppId,
            Name = name,
            RoleUsageLimit = roleUsageLimit,
        });
        TenantDb.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public TenantGameWebShopOrderEntity SeedWebShopOrder(long orderId, long roleId, long areaId, long shopId, string reason = "", bool orderStatus = false)
    {
        InitTenantTables(typeof(TenantGameWebShopOrderEntity));
        var entity = WithSelect(new TenantGameWebShopOrderEntity
        {
            Id = orderId,
            AppId = AppId,
            GameShopId = shopId,
            RoleId = roleId,
            AreaId = areaId,
            Reason = reason,
            OrderStatus = orderStatus,
            OrderId = $"webshop-{orderId}",
        });
        TenantDb.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public SystemConfigEntity SeedSystemConfig(string code, string value, string name = "C108 system config")
    {
        MainDb.CodeFirst.InitTables(typeof(SystemConfigEntity));
        var entity = WithBasic(new SystemConfigEntity
        {
            Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Name = name,
            Code = code,
            Value = value,
            SysFlag = YesNoEnum.Y,
            GroupCode = "c108",
            OrderNo = 100,
            Remark = "C108 test config",
        });
        MainDb.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public TenantGameUserEmailEntity SeedEmailCode(long id, string email, string code, bool expired = false)
    {
        InitTenantTables(typeof(TenantGameUserEmailEntity));
        var entity = WithBasic(new TenantGameUserEmailEntity
        {
            Id = id,
            AppId = AppId,
            Email = email,
            UserName = email,
            Language = "zh-CN",
            TemplateId = "145225",
            Code = code,
            ExpirationTime = expired ? DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeSeconds() : DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds(),
            IsEnabled = true,
            IsDeleted = false,
        });
        TenantDb.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public void Dispose()
    {
        MainDb.Dispose();
        foreach (var dbPath in _dbPaths)
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
    }

    private SqlSugarScope CreateScope(string name)
    {
        var dbPath = CreateDatabasePath(name);
        var config = new ConnectionConfig
        {
            ConfigId = name,
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={dbPath}",
            IsAutoCloseConnection = true,
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        return new SqlSugarScope(config);
    }

    private string CreateDatabasePath(string name)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"gfx_c108_{name}_{Guid.NewGuid():N}.db");
        _dbPaths.Add(dbPath);
        return dbPath;
    }

    private T WithSelect<T>(T entity) where T : EntityBasicSelect
    {
        entity.Description ??= "C108 test data";
        return WithBasic(entity);
    }

    private T WithBasic<T>(T entity) where T : EntityBasic
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        entity.TenantId = TenantId;
        entity.CreatedId = 1;
        entity.CreatedName = "c108";
        entity.CreatedTime = now;
        entity.UpdatedId = 1;
        entity.UpdatedName = "c108";
        entity.UpdateTime = now;
        entity.UpdateCount = 0;
        entity.DeletedId = 0;
        entity.DeletedName = string.Empty;
        entity.DeleteTime = 0;
        entity.RowVersion = 0;
        entity.IsDeleted = false;
        entity.IsEnabled = true;
        return entity;
    }
}