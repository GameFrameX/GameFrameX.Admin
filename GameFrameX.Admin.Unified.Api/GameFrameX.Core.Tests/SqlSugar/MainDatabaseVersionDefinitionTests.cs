using GameFrameX.Application.ClientDelivery.GameAppVersionDefinition;
using GameFrameX.Application.ClientDelivery.GameAppVersionDefinition.Dto;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Tenant.Game.Release;

namespace GameFrameX.Core.Tests.SqlSugar;

public sealed class MainDatabaseVersionDefinitionTests
{
    [Fact]
    public async Task Page_FiltersNameWithoutApplication()
    {
        using (var database = new ClientApiTestDb())
        {
            var service = await CreateServiceAsync(database);
            Assert.Single((await service.PageAsync(new() { SearchKey = " 1.1 " })).Items);
        }
    }

    [Fact]
    public async Task Page_WithoutFilterReturnsMainDatabaseDefinitions()
    {
        using (var database = new ClientApiTestDb())
        {
            var service = await CreateServiceAsync(database);
            Assert.Equal(2, (await service.PageAsync(new())).Items.Count());
        }
    }

    [Fact]
    public async Task Select_FiltersNameWithoutApplication()
    {
        using (var database = new ClientApiTestDb())
        {
            var service = await CreateServiceAsync(database);
            Assert.Equal("1.0.0", Assert.Single(await service.SelectAsync(new() { SearchKey = "1.0.0" })).Value);
        }
    }

    [Fact]
    public async Task Add_RejectsDuplicateNameInMainDatabase()
    {
        using (var database = new ClientApiTestDb())
        {
            var service = await CreateServiceAsync(database);
            await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(new() { Name = "1.0.0" }));
        }
    }

    [Fact]
    public async Task Add_AcceptsNewNameWithoutApplication()
    {
        using (var database = new ClientApiTestDb())
        {
            var service = await CreateServiceAsync(database);
            Assert.Equal("2.0.0", (await service.AddAsync(new() { Name = "2.0.0", Description = "next" })).Name);
        }
    }

    [Fact]
    public async Task Update_ExcludesOwnIdFromNameUniqueness()
    {
        using (var database = new ClientApiTestDb())
        {
            var service = await CreateServiceAsync(database);
            var row = Assert.Single((await service.PageAsync(new() { SearchKey = "1.0.0" })).Items);
            await service.UpdateAsync(new UpdateGameAppVersionDefinitionRequest
            {
                Id = row.Id, RowVersion = row.RowVersion, Name = "1.0.0", Description = "changed",
            });
            Assert.Equal("changed", Assert.Single((await service.PageAsync(new() { SearchKey = "1.0.0" })).Items).Description);
        }
    }

    private static async Task<TenantGameAppVersionDefinitionService> CreateServiceAsync(ClientApiTestDb database)
    {
        database.InitTenantTables(typeof(TenantGameAppVersionDefinitionEntity));
        var service = new TenantGameAppVersionDefinitionService(new SqlSugarRepository<TenantGameAppVersionDefinitionEntity>
        {
            Context = database.MainDb,
        });
        await service.AddAsync(new() { Name = "1.0.0", Description = "initial" });
        await service.AddAsync(new() { Name = "1.1.0", Description = "next" });
        return service;
    }
}
