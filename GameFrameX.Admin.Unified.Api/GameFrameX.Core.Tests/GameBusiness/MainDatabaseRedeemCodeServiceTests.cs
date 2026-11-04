using GameFrameX.Application.CommerceCenter.GameRedeemCodeChannel;
using GameFrameX.Application.CommerceCenter.GameRedeemCodeChannel.Dto;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Core.Tests.GameBusiness;

public sealed class MainDatabaseRedeemCodeServiceTests
{
    [Fact]
    public async Task Channel_CreateAndFilter_PreserveUsageLimitWithoutApplication()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitTenantTables(typeof(TenantGameRedeemCodeChannelEntity));
            var service = new TenantGameRedeemCodeChannelEntityService(new SqlSugarRepository<TenantGameRedeemCodeChannelEntity>
            {
                Context = database.MainDb,
            });
            var created = await service.AddAsync(new AddTenantGameRedeemCodeChannelEntityRequest
            {
                Name = "launch", Description = "launch", RoleUsageLimit = 2,
            });
            var row = Assert.Single((await service.PageAsync(new() { Name = "launch" })).Items);
            Assert.Equal(created.Id, row.Id);
            Assert.Equal(2, row.RoleUsageLimit);
            Assert.Empty((await service.PageAsync(new() { Name = "missing" })).Items);
            await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(new AddTenantGameRedeemCodeChannelEntityRequest
            {
                Name = "invalid", Description = "invalid", RoleUsageLimit = -1,
            }));
            await Assert.ThrowsAsync<BusinessException>(() => service.UpdateAsync(new UpdateTenantGameRedeemCodeChannelEntityRequest
            {
                Id = created.Id + 1, Name = "missing", Description = "missing",
            }));
        }
    }
}
