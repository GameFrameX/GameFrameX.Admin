using GameFrameX.Core.SqlSugar;
using GameFrameX.Application.PlayerCenter.GameUser;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Application.PlayerCenter.GameUser.Dto;
using GameFrameX.Entity.Business.Game;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Tests.PlayerCenter;

/// <summary>
/// 游戏账号新增回归（D-29：绑定列已可空，未携带的可选字段以 null 入库），
/// 密码列落 PHC 哈希而非明文；通用 AddAsync 委托邮箱新增流程。
/// </summary>
public sealed class GameUserServiceAddTests
{
    private static GameUserService CreateService(ClientApiTestDb db)
    {
        var repository = new SqlSugarRepository<GameUserEntity> { Context = db.MainDb, };
        return new GameUserService(repository, new GameUserPasswordService());
    }

    [Fact]
    public async Task AddByMail_WithoutOptionalFields_StillInserts()
    {
        using var db = new ClientApiTestDb();
        db.InitBusinessTables(typeof(GameUserEntity));
        var service = CreateService(db);

        var response = await service.AddByMailAsync(new AddGameUserEntityByMailRequest
        {
            Email = "regression@example.com",
            EmailPassword = "mail-plain",
            Password = "login-plain",
            Name = "regression",
            Status = GameUserStatus.Active,
        });

        var row = db.MainDb.Queryable<GameUserEntity>().Single(x => x.Email == "regression@example.com");
        Assert.NotEqual(0L, row.Id);
        Assert.Null(row.Phone);
        Assert.Null(row.Avatar);
        Assert.Null(row.DeviceId);
        Assert.Equal("regression@example.com", row.UserName);
        Assert.StartsWith("$argon2id$", row.Password);
        Assert.StartsWith("$argon2id$", row.EmailPassword);
        Assert.Equal(row.Id, response.Id);
    }

    [Fact]
    public async Task GenericAdd_DelegatesToMailFlow_NoNullRequiredColumns()
    {
        using var db = new ClientApiTestDb();
        db.InitBusinessTables(typeof(GameUserEntity));
        var service = CreateService(db);

        await service.AddAsync(new AddGameUserEntityByMailRequest
        {
            Email = "generic-add@example.com",
            EmailPassword = "mail-plain",
            Password = "login-plain",
            Status = GameUserStatus.Active,
        });

        var row = db.MainDb.Queryable<GameUserEntity>().Single(x => x.Email == "generic-add@example.com");
        Assert.Null(row.Phone);
        Assert.StartsWith("$argon2id$", row.Password);
    }

    [Fact]
    public async Task AddByMail_DuplicateEmail_Rejected()
    {
        using var db = new ClientApiTestDb();
        db.InitBusinessTables(typeof(GameUserEntity));
        var service = CreateService(db);
        var request = new AddGameUserEntityByMailRequest
        {
            Email = "dup@example.com",
            EmailPassword = "mail-plain",
            Password = "login-plain",
            Status = GameUserStatus.Active,
        };

        await service.AddByMailAsync(request);
        await Assert.ThrowsAsync<Exception>(() => service.AddByMailAsync(request));
    }
}
