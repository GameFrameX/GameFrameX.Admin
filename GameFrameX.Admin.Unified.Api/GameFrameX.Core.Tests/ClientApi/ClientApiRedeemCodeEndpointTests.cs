using GameFrameX.Client.Api.GameRedeemCode;
using GameFrameX.Client.Api.GameRedeemCode.Dto;
using GameFrameX.Client.Api.GameWebShop.Dto;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Foundation.Json;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class ClientApiRedeemCodeEndpointTests
{
    [Fact]
    public async Task GameRedeemCode_Consume_AppliesRewardsAndWritesUseRecord()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.SeedRedeemCode(4001, "C108-REDEEM", 2001, JsonHelper.Serialize(new[]
        {
            new PropertyItemResponse { Id = 11, ItemId = 11, Count = 2, },
        }));
        var controller = CreateController(db, ClientApiRecordingHttpHandler.WithGameServerDefaults());
        db.AttachContext(controller);

        var result = await controller.Consume(new GameRedeemCodeConsumeEntityRequest
        {
            AreaId = 2001,
            Code = "C108-REDEEM",
            RoleId = 3001,
        });

        var root = ClientApiResultReader.Read(result);
        var usedRecordCount = await db.TenantDb.Queryable<TenantGameRedeemCodeUseRecordEntity>().CountAsync();
        var code = await db.TenantDb.Queryable<TenantGameRedeemCodeEntity>().FirstAsync(u => u.Code == "C108-REDEEM");
        Assert.Equal(0, ClientApiResultReader.Code(root));
        Assert.Equal(1, usedRecordCount);
        Assert.Equal(1, code.UsedCount);
    }

    [Theory]
    [InlineData(false, false, 0, (int)RedeemCodeErrorCode.RedeemCodeNotActive, "兑换码未激活")]
    [InlineData(true, true, 0, (int)RedeemCodeErrorCode.RedeemCodeExpired, "兑换码已过期")]
    [InlineData(true, false, 1, (int)RedeemCodeErrorCode.RedeemCodeLimit, "兑换码次数已用完")]
    public async Task GameRedeemCode_Consume_RejectsInvalidCodeState(
        bool isActive,
        bool expired,
        int usedCount,
        int expectedCode,
        string expectedMessage)
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.SeedRedeemCode(
            4001,
            "C108-REDEEM",
            2001,
            JsonHelper.Serialize(new[] { new PropertyItemResponse { Id = 11, ItemId = 11, Count = 2, }, }),
            isActive,
            1,
            usedCount,
            expired ? DateTime.UtcNow.AddHours(-2) : DateTime.UtcNow.AddHours(-1),
            expired ? DateTime.UtcNow.AddMinutes(-1) : DateTime.UtcNow.AddHours(1));
        var controller = CreateController(db);
        db.AttachContext(controller);

        var result = await controller.Consume(new GameRedeemCodeConsumeEntityRequest
        {
            AreaId = 2001,
            Code = "C108-REDEEM",
            RoleId = 3001,
        });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal(expectedCode, ClientApiResultReader.Code(root));
        Assert.Equal(expectedMessage, ClientApiResultReader.Message(root));
    }

    [Fact]
    public async Task GameRedeemCode_Consume_RejectsAlreadyUsedCode()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.SeedRedeemCode(4001, "C108-REDEEM", 2001, JsonHelper.Serialize(new[]
        {
            new PropertyItemResponse { Id = 11, ItemId = 11, Count = 2, },
        }));
        db.InitTenantTables(typeof(TenantGameRedeemCodeUseRecordEntity));
        await db.TenantDb.Insertable(new TenantGameRedeemCodeUseRecordEntity
        {
            Id = 9001,
            AppId = db.AppId,
            AreaId = 2001,
            Code = "C108-REDEEM",
            PlayerId = 3001,
        }).ExecuteCommandAsync();
        var controller = CreateController(db);
        db.AttachContext(controller);

        var result = await controller.Consume(new GameRedeemCodeConsumeEntityRequest
        {
            AreaId = 2001,
            Code = "C108-REDEEM",
            RoleId = 3001,
        });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal(70300004, ClientApiResultReader.Code(root));
        Assert.Equal("兑换码已使用", ClientApiResultReader.Message(root));
    }

    [Fact]
    public async Task GameRedeemCode_Consume_RejectsMissingAppContext()
    {
        using var db = new ClientApiTestDb();
        var controller = CreateController(db);
        db.AttachContext(controller, includeApp: false);

        var result = await controller.Consume(new GameRedeemCodeConsumeEntityRequest
        {
            AreaId = 2001,
            Code = "C108-REDEEM",
            RoleId = 3001,
        });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal((int)AuthErrorCode.AuthorizeInvalid, ClientApiResultReader.Code(root));
        Assert.Equal("应用未认证", ClientApiResultReader.Message(root));
    }

    [Fact]
    public async Task GameRedeemCode_Consume_AllowsWithoutChannelLimit()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        // 兑换码无 ChannelId：保持现有行为，不受组级限制影响
        db.SeedRedeemCode(4001, "C108-NO-CHANNEL", 2001, JsonHelper.Serialize(new[]
        {
            new PropertyItemResponse { Id = 11, ItemId = 11, Count = 2, },
        }));
        var controller = CreateController(db);
        db.AttachContext(controller);

        var result = await controller.Consume(new GameRedeemCodeConsumeEntityRequest
        {
            AreaId = 2001,
            Code = "C108-NO-CHANNEL",
            RoleId = 3001,
        });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        var usageCount = await db.TenantDb.Queryable<TenantGameRedeemCodeChannelRoleUsageEntity>().CountAsync();
        Assert.Equal(0, usageCount);
    }

    [Fact]
    public async Task GameRedeemCode_Consume_AllowsWhenChannelRoleUsageLimitZero()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.SeedChannel(5001, "channel-zero", 0);
        db.SeedRedeemCode(4001, "C108-LIMIT0", 2001, JsonHelper.Serialize(new[]
        {
            new PropertyItemResponse { Id = 11, ItemId = 11, Count = 2, },
        }), channelId: 5001);
        var controller = CreateController(db);
        db.AttachContext(controller);

        var result = await controller.Consume(new GameRedeemCodeConsumeEntityRequest
        {
            AreaId = 2001,
            Code = "C108-LIMIT0",
            RoleId = 3001,
        });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        var usageCount = await db.TenantDb.Queryable<TenantGameRedeemCodeChannelRoleUsageEntity>().CountAsync();
        Assert.Equal(0, usageCount);
    }

    [Fact]
    public async Task GameRedeemCode_Consume_RejectsWhenChannelRoleLimitReached()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        // 每角色可用 1 次，并预置已达上限的累计行
        db.SeedChannel(5001, "channel-one", 1);
        db.SeedRedeemCode(4001, "C108-LIMIT1-A", 2001, JsonHelper.Serialize(new[]
        {
            new PropertyItemResponse { Id = 11, ItemId = 11, Count = 2, },
        }), channelId: 5001);
        db.SeedRedeemCode(4002, "C108-LIMIT1-B", 2001, JsonHelper.Serialize(new[]
        {
            new PropertyItemResponse { Id = 12, ItemId = 12, Count = 1, },
        }), channelId: 5001);
        db.InitTenantTables(typeof(TenantGameRedeemCodeChannelRoleUsageEntity));
        await db.TenantDb.Insertable(new TenantGameRedeemCodeChannelRoleUsageEntity
        {
            Id = 9001,
            AppId = db.AppId,
            ChannelId = 5001,
            RoleId = 3001,
            UsedCount = 1,
            LastCode = "C108-LIMIT1-A",
            LastUseTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        }).ExecuteCommandAsync();
        var controller = CreateController(db);
        db.AttachContext(controller);

        var result = await controller.Consume(new GameRedeemCodeConsumeEntityRequest
        {
            AreaId = 2001,
            Code = "C108-LIMIT1-B",
            RoleId = 3001,
        });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal(70300007, ClientApiResultReader.Code(root));
        Assert.Equal("兑换码分组次数已用完", ClientApiResultReader.Message(root));
    }

    [Fact]
    public async Task GameRedeemCode_Consume_AccumulatesChannelRoleUsageOnSuccess()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        // 每角色可用 2 次，首次兑换应成功并写入累计
        db.SeedChannel(5001, "channel-two", 2);
        db.SeedRedeemCode(4001, "C108-ACC-A", 2001, JsonHelper.Serialize(new[]
        {
            new PropertyItemResponse { Id = 11, ItemId = 11, Count = 2, },
        }), channelId: 5001);
        var controller = CreateController(db);
        db.AttachContext(controller);

        var result = await controller.Consume(new GameRedeemCodeConsumeEntityRequest
        {
            AreaId = 2001,
            Code = "C108-ACC-A",
            RoleId = 3001,
        });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        var usage = await db.TenantDb.Queryable<TenantGameRedeemCodeChannelRoleUsageEntity>()
                            .Where(u => u.ChannelId == 5001 && u.RoleId == 3001).FirstAsync();
        Assert.NotNull(usage);
        Assert.Equal(1, usage.UsedCount);
        Assert.Equal("C108-ACC-A", usage.LastCode);
    }

    [Fact]
    public async Task GameRedeemCode_Consume_IncrementsExistingChannelRoleUsage()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        // 每角色可用 2 次，预置已用 1 次：本次兑换应成功并经 UPDATE 条件递增到 2
        db.SeedChannel(5001, "channel-inc", 2);
        db.SeedRedeemCode(4001, "C108-INC-A", 2001, JsonHelper.Serialize(new[]
        {
            new PropertyItemResponse { Id = 11, ItemId = 11, Count = 2, },
        }), channelId: 5001);
        db.SeedRedeemCode(4002, "C108-INC-B", 2001, JsonHelper.Serialize(new[]
        {
            new PropertyItemResponse { Id = 12, ItemId = 12, Count = 1, },
        }), channelId: 5001);
        db.InitTenantTables(typeof(TenantGameRedeemCodeChannelRoleUsageEntity));
        await db.TenantDb.Insertable(new TenantGameRedeemCodeChannelRoleUsageEntity
        {
            Id = 9001,
            AppId = db.AppId,
            ChannelId = 5001,
            RoleId = 3001,
            UsedCount = 1,
            LastCode = "C108-INC-A",
            LastUseTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        }).ExecuteCommandAsync();
        var controller = CreateController(db);
        db.AttachContext(controller);

        var result = await controller.Consume(new GameRedeemCodeConsumeEntityRequest
        {
            AreaId = 2001,
            Code = "C108-INC-B",
            RoleId = 3001,
        });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        var usage = await db.TenantDb.Queryable<TenantGameRedeemCodeChannelRoleUsageEntity>()
                            .Where(u => u.ChannelId == 5001 && u.RoleId == 3001).FirstAsync();
        Assert.Equal(2, usage.UsedCount);
        Assert.Equal("C108-INC-B", usage.LastCode);
    }

    [Fact]
    public async Task GameRedeemCode_Consume_DifferentChannelsDoNotShareQuota()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.SeedChannel(5001, "channel-a", 1);
        db.SeedChannel(5002, "channel-b", 1);
        db.SeedRedeemCode(4001, "C108-CROSS-A", 2001, JsonHelper.Serialize(new[]
        {
            new PropertyItemResponse { Id = 11, ItemId = 11, Count = 2, },
        }), channelId: 5001);
        db.SeedRedeemCode(4002, "C108-CROSS-B", 2001, JsonHelper.Serialize(new[]
        {
            new PropertyItemResponse { Id = 12, ItemId = 12, Count = 1, },
        }), channelId: 5002);
        var controller = CreateController(db);
        db.AttachContext(controller);

        var first = await controller.Consume(new GameRedeemCodeConsumeEntityRequest
        {
            AreaId = 2001,
            Code = "C108-CROSS-A",
            RoleId = 3001,
        });
        var second = await controller.Consume(new GameRedeemCodeConsumeEntityRequest
        {
            AreaId = 2001,
            Code = "C108-CROSS-B",
            RoleId = 3001,
        });

        Assert.Equal(0, ClientApiResultReader.Code(ClientApiResultReader.Read(first)));
        Assert.Equal(0, ClientApiResultReader.Code(ClientApiResultReader.Read(second)));
        var countA = await db.TenantDb.Queryable<TenantGameRedeemCodeChannelRoleUsageEntity>()
                             .Where(u => u.ChannelId == 5001 && u.RoleId == 3001).CountAsync();
        var countB = await db.TenantDb.Queryable<TenantGameRedeemCodeChannelRoleUsageEntity>()
                             .Where(u => u.ChannelId == 5002 && u.RoleId == 3001).CountAsync();
        Assert.Equal(1, countA);
        Assert.Equal(1, countB);
    }

    [Fact]
    public async Task GameRedeemCode_Consume_DifferentRolesDoNotShareQuota()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        db.SeedRole(3002, 1002, 2001);
        db.SeedChannel(5001, "channel-shared", 1);
        db.SeedRedeemCode(4001, "C108-ROLE-A", 2001, JsonHelper.Serialize(new[]
        {
            new PropertyItemResponse { Id = 11, ItemId = 11, Count = 2, },
        }), channelId: 5001);
        db.SeedRedeemCode(4002, "C108-ROLE-B", 2001, JsonHelper.Serialize(new[]
        {
            new PropertyItemResponse { Id = 12, ItemId = 12, Count = 1, },
        }), channelId: 5001);
        var controller = CreateController(db);
        db.AttachContext(controller);

        var first = await controller.Consume(new GameRedeemCodeConsumeEntityRequest
        {
            AreaId = 2001,
            Code = "C108-ROLE-A",
            RoleId = 3001,
        });
        var second = await controller.Consume(new GameRedeemCodeConsumeEntityRequest
        {
            AreaId = 2001,
            Code = "C108-ROLE-B",
            RoleId = 3002,
        });

        Assert.Equal(0, ClientApiResultReader.Code(ClientApiResultReader.Read(first)));
        Assert.Equal(0, ClientApiResultReader.Code(ClientApiResultReader.Read(second)));
    }

    [Fact]
    public async Task GameRedeemCode_Consume_RejectsWhenChannelMissing()
    {
        using var db = new ClientApiTestDb();
        db.SeedAreaClassify(10);
        db.SeedArea(2001, 10);
        db.SeedRole(3001, 1001, 2001);
        // 兑换码指向不存在的渠道（未 seed）：安全拒绝为"兑换码不存在"
        db.SeedRedeemCode(4001, "C108-GHOST-CHANNEL", 2001, JsonHelper.Serialize(new[]
        {
            new PropertyItemResponse { Id = 11, ItemId = 11, Count = 2, },
        }), channelId: 5999);
        var controller = CreateController(db);
        db.AttachContext(controller);

        var result = await controller.Consume(new GameRedeemCodeConsumeEntityRequest
        {
            AreaId = 2001,
            Code = "C108-GHOST-CHANNEL",
            RoleId = 3001,
        });

        var root = ClientApiResultReader.Read(result);
        Assert.Equal(70300005, ClientApiResultReader.Code(root));
        Assert.Equal("兑换码不存在", ClientApiResultReader.Message(root));
    }

    private static GameRedeemCodeController CreateController(ClientApiTestDb db, ClientApiRecordingHttpHandler? handler = null)
    {
        db.InitTenantTables(
            typeof(TenantGameRedeemCodeEntity),
            typeof(TenantGameRedeemCodeUseRecordEntity),
            typeof(TenantGameRedeemCodeChannelEntity),
            typeof(TenantGameRedeemCodeChannelRoleUsageEntity),
            typeof(TenantGameAreaEntity));
        return new GameRedeemCodeController(
            db.Repository<TenantGameRedeemCodeEntity>(),
            db.Repository<TenantGameRedeemCodeUseRecordEntity>(),
            db.Repository<TenantGameRedeemCodeChannelEntity>(),
            db.Repository<TenantGameRedeemCodeChannelRoleUsageEntity>(),
            db.Repository<TenantGameAreaEntity>(),
            new HttpClient(handler ?? ClientApiRecordingHttpHandler.WithGameServerDefaults(), false));
    }
}