using GameFrameX.Application.ClientDelivery.Device.GameAppDevice;
using GameFrameX.Application.ClientDelivery.Device.GameAppDevice.Dto;
using GameFrameX.Application.ClientDelivery.Device.DeviceGroup;
using GameFrameX.Application.ClientDelivery.Device.DeviceGroup.Dto;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Foundation.Orm.Entity;
using GameFrameX.Entity.Tenant.Device;
using SqlSugar;
using GameFrameX.Core.Extension;

namespace GameFrameX.Core.Tests.TenantGameAppDevice;

/// <summary>
/// 主库设备与分组行为测试。
/// </summary>
public sealed class TenantGameAppDeviceScopeIsolationTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"tenant_device_scope_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;

    public TenantGameAppDeviceScopeIsolationTests()
    {
        var config = new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _db = new SqlSugarScope(config);
        _db.Aop.DataExecuting = (oldValue, entityInfo) => SqlSugarAopHelper.HandleBaseAudit(oldValue, entityInfo);
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(
            typeof(TenantGameAppDeviceEntity),
            typeof(TenantGameAppDeviceGroupEntity),
            typeof(TenantGameAppDeviceGroupRelationEntity));
    }

    [Fact]
    public async Task AddGroup_RejectsMissingDevice()
    {
        await SeedAsync();
        var service = new DeviceGroupService(
            Repository<TenantGameAppDeviceGroupEntity>());

        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(new AddTenantDeviceGroupRequest
        {

            Name = "group-app100",
            DeviceIds = new List<long> { DeviceA1, 404, }
        }));
    }

    [Fact]
    public async Task AddGroup_AcceptsExistingDevices()
    {
        await SeedAsync();
        var service = new DeviceGroupService(
            Repository<TenantGameAppDeviceGroupEntity>());

        var group = await service.AddAsync(new AddTenantDeviceGroupRequest
        {

            Name = "group-app100",
            DeviceIds = new List<long> { DeviceA1, DeviceB1, },
        });

        var relations = await _db.Queryable<TenantGameAppDeviceGroupRelationEntity>()
                                 .Where(r => r.DeviceGroupId == group.Id)
                                 .ToListAsync();
        Assert.Equal(2, relations.Count);
        var detail = await service.GetDetailAsync(new GameFrameX.Core.BaseInputOutput.BaseIdRequest { Id = group.Id });
        Assert.Equal(new[] { DeviceA1, DeviceB1 }, detail.DeviceIds!.OrderBy(id => id));
    }

    [Fact]
    public async Task DevicePage_ReturnsMainDatabaseDevices()
    {
        await SeedAsync();
        var service = new GameAppDeviceService(
            Repository<TenantGameAppDeviceEntity>());

        var page = await service.PageAsync(new GameAppDeviceRequest());

        var ids = page.Items.Select(u => u.Id).OrderBy(i => i).ToArray();
        Assert.Equal(new[] { DeviceA1, DeviceA2, DeviceB1, }, ids);
    }

    [Fact]
    public async Task DeviceAdd_RejectsDuplicateIdentifier()
    {
        await SeedAsync();
        var service = new GameAppDeviceService(
            Repository<TenantGameAppDeviceEntity>());

        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(new AddTenantGameAppDeviceRequest
        {

            Name = "dup",
            DeviceUniqueIdentifier = "uid-a1",
        }));
    }

    [Fact]
    public async Task DeviceAdd_AcceptsUniqueIdentifier()
    {
        await SeedAsync();
        var service = new GameAppDeviceService(
            Repository<TenantGameAppDeviceEntity>());

        var device = await service.AddAsync(new AddTenantGameAppDeviceRequest
        {
            Name = "unique-device",
            DeviceUniqueIdentifier = "uid-new",
        });

        Assert.True(await _db.Queryable<TenantGameAppDeviceEntity>()
            .AnyAsync(u => u.Id == device.Id && u.DeviceUniqueIdentifier == "uid-new"));
    }

    private const long DeviceA1 = 1001;
    private const long DeviceA2 = 1002;
    private const long DeviceB1 = 2001;

    private SqlSugarRepository<T> Repository<T>() where T : EntityBase, new()
    {
        return new SqlSugarRepository<T> { Context = _db, };
    }

    private async Task SeedAsync()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await _db.Insertable(new TenantGameAppDeviceEntity
        {
            Id = DeviceA1,

            Name = "device-a1",
            DeviceUniqueIdentifier = "uid-a1",
            CreatedTime = now,
            IsDeleted = false,
        }).ExecuteCommandAsync();

        await _db.Insertable(new TenantGameAppDeviceEntity
        {
            Id = DeviceA2,

            Name = "device-a2",
            DeviceUniqueIdentifier = "uid-a2",
            CreatedTime = now,
            IsDeleted = false,
        }).ExecuteCommandAsync();

        await _db.Insertable(new TenantGameAppDeviceEntity
        {
            Id = DeviceB1,

            Name = "device-b1",
            DeviceUniqueIdentifier = "uid-b1",
            CreatedTime = now,
            IsDeleted = false,
        }).ExecuteCommandAsync();
    }

    public void Dispose()
    {
        _db.Dispose();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}