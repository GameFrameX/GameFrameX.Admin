using GameFrameX.Client.Api.Services.Auth;
using GameFrameX.Client.Api.GameUser;
using GameFrameX.Client.Api.GameUser.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Application.Game.User.GameUser;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Core.Tests.ClientApi;

/// <summary>
/// GameUser.DeviceLogin 端点级行为测试（GFX-729 / C320）：匿名设备登录按 DeviceId 找号或建号，
/// 覆盖首登建号 IsNewRegister=true、二次登录找号 IsNewRegister=false、AccessToken PascalCase 契约、
/// 账号删除/禁用态拒绝四场景。
/// </summary>
public sealed class ClientApiGameUserDeviceLoginEndpointTests
{
    [Fact]
    public async Task GameUser_DeviceLogin_FirstLoginCreatesAccountWithIsNewRegister()
    {
        using (var db = new ClientApiTestDb())
        {
            var controller = CreateController(db);
            db.AttachContext(controller);

            var result = await controller.DeviceLogin(new LoginDeviceRequest
            {
                DeviceId = "device-c320-a1",
            });

            var root = ClientApiResultReader.Read(result);
            var data = ClientApiResultReader.Data(root);
            var saved = await db.TenantDb.Queryable<TenantGameUserEntity>().FirstAsync(u => u.DeviceId == "device-c320-a1");
            Assert.Equal(0, ClientApiResultReader.Code(root));
            Assert.True(data.GetProperty("IsNewRegister").GetBoolean());
            Assert.Equal("Device", data.GetProperty("Provider").GetString());
            // 建号命名约定：UserName = "device:" + DeviceId，初始状态 Active
            Assert.Equal("device:device-c320-a1", saved.UserName);
            Assert.Equal(GameUserStatus.Active, saved.Status);
        }
    }

    [Fact]
    public async Task GameUser_DeviceLogin_SecondLoginFindsAccountWithoutReRegister()
    {
        using (var db = new ClientApiTestDb())
        {
            var controller = CreateController(db);
            db.AttachContext(controller);

            var first = await controller.DeviceLogin(new LoginDeviceRequest
            {
                DeviceId = "device-c320-a2",
            });
            var second = await controller.DeviceLogin(new LoginDeviceRequest
            {
                DeviceId = "device-c320-a2",
            });

            var firstRoot = ClientApiResultReader.Read(first);
            var secondRoot = ClientApiResultReader.Read(second);
            var firstData = ClientApiResultReader.Data(firstRoot);
            var secondData = ClientApiResultReader.Data(secondRoot);
            var count = await db.TenantDb.Queryable<TenantGameUserEntity>().CountAsync(u => u.DeviceId == "device-c320-a2");
            Assert.Equal(0, ClientApiResultReader.Code(secondRoot));
            // 二次登录找号复用既有账号：IsNewRegister=false 且不重复建号
            Assert.True(firstData.GetProperty("IsNewRegister").GetBoolean());
            Assert.False(secondData.GetProperty("IsNewRegister").GetBoolean());
            Assert.Equal(1, count);
            Assert.Equal(firstData.GetProperty("Id").GetInt64(), secondData.GetProperty("Id").GetInt64());
        }
    }

    [Fact]
    public async Task GameUser_DeviceLogin_ReturnsPascalCaseAccessTokenAndSessionKey()
    {
        using (var db = new ClientApiTestDb())
        {
            var controller = CreateController(db);
            db.AttachContext(controller);

            var result = await controller.DeviceLogin(new LoginDeviceRequest
            {
                DeviceId = "device-c320-a3",
            });

            var root = ClientApiResultReader.Read(result);
            var data = ClientApiResultReader.Data(root);
            // 登录响应契约：AccessToken 以 PascalCase 命名输出且非空
            Assert.True(data.TryGetProperty("AccessToken", out var accessToken), "AccessToken 字段必须是 PascalCase");
            Assert.False(string.IsNullOrWhiteSpace(accessToken.GetString()));
            // C7: 登录响应随 token 一起回传 per-session 会话密钥 kid/secret
            Assert.False(string.IsNullOrWhiteSpace(data.GetProperty("SessionKeyId").GetString()));
            Assert.Matches("^[0-9a-f]{64}$", data.GetProperty("SessionKeySecret").GetString());
        }
    }

    [Fact]
    public async Task GameUser_DeviceLogin_RejectsDeletedAndDisabledAccount()
    {
        using (var db = new ClientApiTestDb())
        {
            var controller = CreateController(db);
            db.AttachContext(controller);

            // 已删除账号 -> 注销状态拒绝
            db.SeedUser(3001, "device:device-c320-del", status: GameUserStatus.Deleted, deviceId: "device-c320-del");
            var deleted = await controller.DeviceLogin(new LoginDeviceRequest
            {
                DeviceId = "device-c320-del",
            });
            var deletedRoot = ClientApiResultReader.Read(deleted);
            Assert.Equal((int)AccountErrorCode.AccountDeleting, ClientApiResultReader.Code(deletedRoot));
            Assert.Equal("账号处于注销状态", ClientApiResultReader.Message(deletedRoot));

            // 非 Active 非 Deleted（Banned 封禁）-> 禁用拒绝
            db.SeedUser(3002, "device:device-c320-ban", status: GameUserStatus.Banned, deviceId: "device-c320-ban");
            var banned = await controller.DeviceLogin(new LoginDeviceRequest
            {
                DeviceId = "device-c320-ban",
            });
            var bannedRoot = ClientApiResultReader.Read(banned);
            Assert.Equal((int)AccountErrorCode.AccountDisabled, ClientApiResultReader.Code(bannedRoot));
            Assert.Equal("账号已被禁用", ClientApiResultReader.Message(bannedRoot));
        }
    }

    private static GameUserController CreateController(ClientApiTestDb db)
    {
        db.InitTenantTables(typeof(TenantGameUserEntity), typeof(TenantGameUserEmailEntity), typeof(TenantSessionKeyEntity));
        return new GameUserController(
            db.Repository<TenantGameUserEntity>(),
            CreateTokenIssuer(),
            new ClientApiSessionKeyIssuer(db.Repository<TenantSessionKeyEntity>()),
            db.Repository<TenantGameUserEmailEntity>(),
            new FakeTenantGamePenaltyEnforcementService(),
            new TenantGameUserPasswordService());
    }

    private static ClientApiTokenIssuer CreateTokenIssuer()
    {
        return new ClientApiTokenIssuer(FakeOptionsProvider.WithTokenPolicy("unit-test-signing-key", 60));
    }
}
