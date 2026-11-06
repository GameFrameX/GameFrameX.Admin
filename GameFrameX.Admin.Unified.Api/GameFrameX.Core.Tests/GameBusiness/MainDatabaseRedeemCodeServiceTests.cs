using GameFrameX.Application.CommerceCenter.GameRedeemCodeChannel;
using GameFrameX.Application.CommerceCenter.GameRedeemCodeChannel.Dto;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Business.Game;

namespace GameFrameX.Core.Tests.GameBusiness;

public sealed class MainDatabaseRedeemCodeServiceTests
{
    [Fact]
    public async Task Channel_CreateAndFilter_PreserveUsageLimitWithoutApplication()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitBusinessTables(typeof(GameRedeemCodeChannelEntity));
            var service = new GameRedeemCodeChannelService(new SqlSugarRepository<GameRedeemCodeChannelEntity>
            {
                Context = database.MainDb,
            });
            var created = await service.AddAsync(new AddGameRedeemCodeChannelEntityRequest
            {
                Name = "launch", Description = "launch", RoleUsageLimit = 2,
            });
            var row = Assert.Single((await service.PageAsync(new() { Name = "launch" })).Items);
            Assert.Equal(created.Id, row.Id);
            Assert.Equal(2, row.RoleUsageLimit);
            Assert.Empty((await service.PageAsync(new() { Name = "missing" })).Items);
            await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(new AddGameRedeemCodeChannelEntityRequest
            {
                Name = "invalid", Description = "invalid", RoleUsageLimit = -1,
            }));
            await Assert.ThrowsAsync<BusinessException>(() => service.UpdateAsync(new UpdateGameRedeemCodeChannelEntityRequest
            {
                Id = created.Id + 1, Name = "missing", Description = "missing",
            }));
        }
    }
}
