using GameFrameX.Application.PlayerCenter.GameUser;
using GameFrameX.Application.PlayerCenter.GameRole;
using GameFrameX.Application.PlayerCenter.GameRole.Dto;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Core.Tests.GameBusiness;

public sealed class MainDatabasePlayerServiceTests
{
    [Fact]
    public async Task AccountWithoutRole_CanBeListedBannedAndUnsealed()
    {
        using (var database = new ClientApiTestDb())
        {
            var user = database.SeedUser(100, userName: "player");
            var service = new GameUserService(new SqlSugarRepository<TenantGameUserEntity>
            {
                Context = database.MainDb,
            }, new GameUserPasswordService());
            Assert.Equal(user.Id, Assert.Single((await service.PageAsync(new() { UserName = "player" })).Items).Id);
            await service.BanAsync(new() { Id = user.Id });
            Assert.Equal(GameUserStatus.Banned, (await database.MainDb.Queryable<TenantGameUserEntity>().InSingleAsync(user.Id)).Status);
            await service.UnsealAsync(new() { Id = user.Id });
            Assert.Equal(GameUserStatus.Active, (await database.MainDb.Queryable<TenantGameUserEntity>().InSingleAsync(user.Id)).Status);
            await Assert.ThrowsAsync<BusinessException>(() => service.BanAsync(new() { Id = 999 }));
        }
    }

    [Fact]
    public async Task RolePage_FiltersAreaAndUpdatePreservesConcurrencyCheck()
    {
        using (var database = new ClientApiTestDb())
        using (var client = new HttpClient())
        {
            var role = database.SeedRole(201, 100, 10, name: "one");
            database.SeedRole(202, 100, 11, name: "two");
            var service = new GameRoleService(new SqlSugarRepository<TenantGameRoleEntity>
            {
                Context = database.MainDb,
            }, client);
            var row = Assert.Single((await service.PageAsync(new() { AreaId = 10 })).Items);
            Assert.Equal(role.Id.ToString(), row.RoleId);
            var update = new UpdateTenantGameRoleEntityRequest
            {
                RoleId = row.RoleId, Name = "changed", UserId = 100, AreaId = 10, RowVersion = row.RowVersion,
            };
            await service.UpdateAsync(update);
            Assert.Equal("changed", (await database.MainDb.Queryable<TenantGameRoleEntity>().InSingleAsync(role.Id)).Name);
            await Assert.ThrowsAnyAsync<Exception>(() => service.UpdateAsync(update));
            await Assert.ThrowsAsync<BusinessException>(() => service.UpdateAsync(new() { RoleId = "999" }));
        }
    }
}
