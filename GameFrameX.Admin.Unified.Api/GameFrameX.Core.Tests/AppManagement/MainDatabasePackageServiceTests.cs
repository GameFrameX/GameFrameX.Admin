using GameFrameX.Application.ClientDelivery.GameAppPackage;
using GameFrameX.Application.ClientDelivery.GameAppPackage.Dto;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Tenant.Game.App;

namespace GameFrameX.Core.Tests.AppManagement;

public sealed class MainDatabasePackageServiceTests
{
    [Fact]
    public async Task Create_DefaultAndInheritance_UseMainDatabaseRules()
    {
        using (var database = new ClientApiTestDb())
        {
            var service = CreateService(database);
            var first = await service.AddAsync(Input("first", isDefault: true));
            var second = await service.AddAsync(Input("second", isDefault: true));
            Assert.False((await database.MainDb.Queryable<TenantGameAppPackageEntity>().InSingleAsync(first.Id)).IsDefault);
            Assert.True((await database.MainDb.Queryable<TenantGameAppPackageEntity>().InSingleAsync(second.Id)).IsDefault);
            var childInput = Input("child");
            childInput.ParentPackageId = first.Id;
            var child = await service.AddAsync(childInput);
            Assert.Equal(PackageKindEnum.Derived, child.PackageKind);
            var invalid = Input("grandchild");
            invalid.ParentPackageId = child.Id;
            await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(invalid));
            await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(Input("first")));
            Assert.Equal(3, (await service.ListAsync()).Count);
            Assert.Single(await service.SelectAsync(new() { SearchKey = "child" }));
        }
    }

    [Fact]
    public async Task Create_RejectsMissingPlatformAndChannel()
    {
        using (var database = new ClientApiTestDb())
        {
            var service = CreateService(database);
            var missingPlatform = Input("missing-platform");
            missingPlatform.PlatformCode = "missing";
            await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(missingPlatform));
            var missingChannel = Input("missing-channel");
            missingChannel.ChannelId = 999;
            await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(missingChannel));
        }
    }

    private static AddGameAppPackageRequest Input(string name, bool isDefault = false)
    {
        return new AddGameAppPackageRequest
        {
            Name = name, PackageName = name, PlatformCode = "android", ChannelId = 10,
            PackageVersion = "1.0.0", Remark = "test", IsDefault = isDefault,
        };
    }

    private static GameAppPackageService CreateService(ClientApiTestDb database)
    {
        database.InitTenantTables(typeof(TenantGameAppPackageEntity), typeof(GlobalPlatformEntity),
            typeof(GlobalChannelDefinitionEntity), typeof(TenantChannelDefinitionEntity));
        database.MainDb.Insertable(new GlobalPlatformEntity
        {
            Id = 1, Name = "android", Description = "Android", IsEnabled = true, IsDeleted = false,
        }).ExecuteCommand();
        database.MainDb.Insertable(new GlobalChannelDefinitionEntity
        {
            Id = 10, Name = "official", Description = "Official", Scope = ChannelScopeEnum.System,
            IsEnabled = true, IsDeleted = false,
        }).ExecuteCommand();
        return new GameAppPackageService(
            new SqlSugarRepository<TenantGameAppPackageEntity> { Context = database.MainDb },
            new SqlSugarRepository<GlobalPlatformEntity> { Context = database.MainDb },
            new SqlSugarRepository<GlobalChannelDefinitionEntity> { Context = database.MainDb },
            new SqlSugarRepository<TenantChannelDefinitionEntity> { Context = database.MainDb });
    }
}
