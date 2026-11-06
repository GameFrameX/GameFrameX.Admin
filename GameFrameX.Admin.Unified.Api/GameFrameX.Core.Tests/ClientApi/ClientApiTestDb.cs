using GameFrameX.Client.Api.Middleware;
using GameFrameX.Core.Middleware;
using GameFrameX.Client.Api.Services;
using GameFrameX.Core.Enum;
using GameFrameX.Client.Api.GameWebShop.Dto;
using GameFrameX.Foundation.Hash;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Base;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Business.Device;
using GameFrameX.Entity.Business.Game;
using GameFrameX.Entity.Business.Game.Announcement;
using GameFrameX.Entity.Business.Game.App;
using GameFrameX.Entity.Business.Game.RemoteConfig;
using GameFrameX.Entity.Business.Game.FeatureSwitch;
using GameFrameX.Entity.Business.Game.Release;
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
        DbPath = CreateDatabasePath("tenant");


        DbProvider = MainDb;
    }

    public long TenantId { get; }

    public long AppId { get; }

    public SqlSugarScope MainDb { get; }

    public string DbPath { get; }

    public ISqlSugarClient DbProvider { get; }


    public Repository<T> Repository<T>() where T : EntityBase, new()
    {
        return new Repository<T>(MainDb);
    }

    public ClientApiTestDb InitBusinessTables(params Type[] tableTypes)
    {
        DbProvider.CodeFirst.InitTables(tableTypes);
        return this;
    }

    public DefaultHttpContext CreateHttpContext(bool include = true, bool includeApp = true)
    {
        return CreateHttpContext(include, includeApp, null);
    }

    public DefaultHttpContext CreateHttpContext(bool include, bool includeApp, long? packageId)
    {
        var context = new DefaultHttpContext();
        if (include)
        {
            context.Items["TenantId"] = TenantId;
        }

        if (packageId.HasValue)
        {
            context.Items[PaymentOptionSettingProvider.PackageIdKey] = packageId.Value;
        }

        return context;
    }

    public void AttachContext(Controller controller, bool include = true, bool includeApp = true)
    {
        controller.ControllerContext = new ControllerContext { HttpContext = CreateHttpContext(include, includeApp), };
    }

    public GameAppPackageEntity SeedPackage(long packageId, string packageName)
    {
        InitBusinessTables(typeof(GameAppPackageEntity));
        var entity = WithSelect(new GameAppPackageEntity
        {
            Id = packageId,
            Name = "C108 Test Package",
            PackageName = packageName,
            PlatformCode = "ios",
            PlatformName = "iOS",
            ChannelCode = "official",
            ChannelName = "Official",
            IsEnabled = true,
            IsDeleted = false,
        });
        DbProvider.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public GameAreaClassifyEntity SeedAreaClassify(long classifyId)
    {
        InitBusinessTables(typeof(GameAreaClassifyEntity));
        var entity = WithSelect(new GameAreaClassifyEntity
        {
            Id = classifyId,
            Name = "Asia",
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
        DbProvider.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public GameAreaEntity SeedArea(long areaId, long classifyId, bool isEnabled = true, GameServerStatus status = GameServerStatus.Online)
    {
        InitBusinessTables(typeof(GameAreaEntity));
        var entity = WithSelect(new GameAreaEntity
        {
            Id = areaId,
            Name = $"Area-{areaId}",
            ClassifyId = classifyId,
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
        DbProvider.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public GameAppFeatureSwitchEntity SeedFeatureSwitch(GameAppFeatureSwitchEntity entity)
    {
        InitBusinessTables(typeof(GameAppFeatureSwitchEntity));
        WithBasic(entity);
        DbProvider.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public GameAppRemoteConfigEntity SeedRemoteConfig(long id, string packageName, string content)
    {
        InitBusinessTables(typeof(GameAppRemoteConfigEntity));
        var entity = WithBasic(new GameAppRemoteConfigEntity
        {
            Id = id,
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
        DbProvider.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public GameAppReleaseVersionEntity SeedAppVersion(long id, string packageName)
    {
        InitBusinessTables(typeof(GameAppReleaseVersionEntity));
        var entity = WithBasic(new GameAppReleaseVersionEntity
        {
            Id = id,
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
        DbProvider.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public GameAppAssetPackageVersionEntity SeedAssetPackageVersion(long id, string packageName)
    {
        InitBusinessTables(typeof(GameAppAssetPackageVersionEntity), typeof(GameAppDeviceEntity));
        var entity = WithBasic(new GameAppAssetPackageVersionEntity
        {
            Id = id,
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
        DbProvider.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public GameAnnouncementEntity SeedAnnouncement(long id, long? areaId, string language, string title, bool isActive = true)
    {
        InitBusinessTables(typeof(GameAnnouncementEntity));
        var entity = WithBasic(new GameAnnouncementEntity
        {
            Id = id,
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
        DbProvider.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public GameUserEntity SeedUser(
        long userId,
        string userName,
        string password = "password",
        GameUserStatus status = GameUserStatus.Active,
        string email = "",
        string deviceId = "")
    {
        InitBusinessTables(typeof(GameUserEntity));
        var entity = WithSelect(new GameUserEntity
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
        DbProvider.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public GameRoleEntity SeedRole(long roleId, long userId, long areaId, long appId = 0, string name = "Role", bool isSandbox = false)
    {
        InitBusinessTables(typeof(GameRoleEntity));
        var entity = WithSelect(new GameRoleEntity
        {
            Id = roleId,
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
        DbProvider.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public GameGoodsEntity SeedGoods(long goodsId, long classifyId = 1, int configId = 10001, bool isEnabled = true)
    {
        InitBusinessTables(typeof(GameGoodsEntity));
        var entity = WithSelect(new GameGoodsEntity
        {
            Id = goodsId,
            Name = $"Goods-{goodsId}",
            Description = "C108 goods",
            ClassifyId = classifyId,
            ConfigId = configId,
            Star = false,
            IsEnabled = isEnabled,
            IsDeleted = false,
        });
        DbProvider.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public GameWebShopEntity SeedWebShop(long shopId, string realPrice, string itemJson, bool isUp = true, int weight = 10)
    {
        InitBusinessTables(typeof(GameWebShopEntity));
        var entity = WithSelect(new GameWebShopEntity
        {
            Id = shopId,
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
        DbProvider.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public GameOrderEntity SeedOrder(long orderId, long roleId, long areaId, string payChannel = "google", OrderStatusEnum status = OrderStatusEnum.Unpaid, decimal amount = 100m, long sku = 1, string? providerTransactionId = null, string? orderProviderId = null, bool isSandbox = false)
    {
        InitBusinessTables(typeof(GameOrderEntity));
        var entity = WithBasic(new GameOrderEntity
        {
            Id = orderId,
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
        DbProvider.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public GameRedeemCodeEntity SeedRedeemCode(long codeId, string code, long areaId, string contents, bool isActive = true, int usageLimit = 1, int usedCount = 0, DateTime? startTime = null, DateTime? endTime = null, long? channelId = null)
    {
        InitBusinessTables(typeof(GameRedeemCodeEntity));
        var entity = WithBasic(new GameRedeemCodeEntity
        {
            Id = codeId,
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
        DbProvider.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public GameRedeemCodeChannelEntity SeedChannel(long channelId, string name, int roleUsageLimit = 0)
    {
        InitBusinessTables(typeof(GameRedeemCodeChannelEntity));
        var entity = WithSelect(new GameRedeemCodeChannelEntity
        {
            Id = channelId,
            Name = name,
            RoleUsageLimit = roleUsageLimit,
        });
        DbProvider.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public GameWebShopOrderEntity SeedWebShopOrder(long orderId, long roleId, long areaId, long shopId, string reason = "", bool orderStatus = false)
    {
        InitBusinessTables(typeof(GameWebShopOrderEntity));
        var entity = WithSelect(new GameWebShopOrderEntity
        {
            Id = orderId,
            GameShopId = shopId,
            RoleId = roleId,
            AreaId = areaId,
            Reason = reason,
            OrderStatus = orderStatus,
            OrderId = $"webshop-{orderId}",
        });
        DbProvider.Insertable(entity).ExecuteCommand();
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
        DbProvider.Insertable(entity).ExecuteCommand();
        return entity;
    }

    public GameUserEmailEntity SeedEmailCode(long id, string email, string code, bool expired = false)
    {
        InitBusinessTables(typeof(GameUserEmailEntity));
        var entity = WithBasic(new GameUserEmailEntity
        {
            Id = id,
            Email = email,
            UserName = email,
            Language = "zh-CN",
            TemplateId = "145225",
            Code = code,
            ExpirationTime = expired ? DateTimeOffset.UtcNow.AddMinutes(-1).ToUnixTimeSeconds() : DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds(),
            IsEnabled = true,
            IsDeleted = false,
        });
        DbProvider.Insertable(entity).ExecuteCommand();
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
        return SqlSugarClientConfig.AddSqlSugarClientService(config);
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