using GameFrameX.Application.System.Tenant.Device.TenantGameAppDevice;
using GameFrameX.Application.System.Tenant.Device.TenantGameAppDevice.Dto;
using GameFrameX.Application.System.Tenant.Device.TenantDeviceGroup;
using GameFrameX.Application.System.Tenant.Device.TenantDeviceGroup.Dto;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Foundation.Orm.Entity;
using GameFrameX.Entity.Tenant.Device;
using GameFrameX.Entity.Tenant.Game;
using SqlSugar;
using GameFrameX.Core.Extension;

namespace GameFrameX.Core.Tests.TenantGameAppDevice;

/// <summary>
/// C211 设备域应用作用域隔离测试。
/// <para>覆盖：跨应用设备加入分组被拒绝、同应用设备加入分组成功、PageAsync/SelectAsync 按当前应用收口、
/// 机器码在同应用内唯一（跨应用允许同名机器码）。</para>
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
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(
            typeof(TenantGameAppEntity),
            typeof(TenantGameAppDeviceEntity),
            typeof(TenantGameAppDeviceGroupEntity),
            typeof(TenantGameAppDeviceGroupRelationEntity));
    }

    /// <summary>
    /// 跨应用设备加入分组直接拒绝（C211 核心隔离验收）。
    /// </summary>
    [Fact]
    public async Task AddGroup_RejectsCrossAppDevice()
    {
        await SeedAsync();
        var service = new TenantDeviceGroupService(
            Repository<TenantGameAppDeviceGroupEntity>(),
            Repository<TenantGameAppEntity>());

        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(new AddTenantDeviceGroupRequest
        {
            AppId = App100,
            Name = "group-app100",
            DeviceIds = new List<long> { DeviceA1, DeviceB1, }, // DeviceB1 属 App200
        }));
    }

    /// <summary>
    /// 同应用设备加入分组成功，关系表统一携带分组所属 AppId。
    /// </summary>
    [Fact]
    public async Task AddGroup_AcceptsSameAppDevices()
    {
        await SeedAsync();
        var service = new TenantDeviceGroupService(
            Repository<TenantGameAppDeviceGroupEntity>(),
            Repository<TenantGameAppEntity>());

        var group = await service.AddAsync(new AddTenantDeviceGroupRequest
        {
            AppId = App100,
            Name = "group-app100",
            DeviceIds = new List<long> { DeviceA1, },
        });

        Assert.Equal(App100, group.AppId);
        var relations = await _db.Queryable<TenantGameAppDeviceGroupRelationEntity>()
                                 .Where(r => r.DeviceGroupId == group.Id)
                                 .ToListAsync();
        Assert.Single(relations);
        Assert.All(relations, r => Assert.Equal(App100, r.AppId));
    }

    /// <summary>
    /// PageAsync 按当前应用收口：应用 100 的设备页不返回应用 200 的设备。
    /// </summary>
    [Fact]
    public async Task DevicePage_ReturnsOnlyCurrentAppDevices()
    {
        await SeedAsync();
        var service = new TenantGameAppDeviceService(
            Repository<TenantGameAppDeviceEntity>(),
            Repository<TenantGameAppEntity>());

        var page = await service.PageAsync(new TenantGameAppDeviceRequest { AppId = App100, });

        var ids = page.Items.Select(u => u.Id).OrderBy(i => i).ToArray();
        Assert.Equal(new[] { DeviceA1, DeviceA2, }, ids);
    }

    /// <summary>
    /// 机器码在同应用内唯一；重复机器码（同应用）新增被拒。
    /// </summary>
    [Fact]
    public async Task DeviceAdd_RejectsDuplicateIdentifierInSameApp()
    {
        await SeedAsync();
        var service = new TenantGameAppDeviceService(
            Repository<TenantGameAppDeviceEntity>(),
            Repository<TenantGameAppEntity>());

        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(new AddTenantGameAppDeviceRequest
        {
            AppId = App100,
            Name = "dup",
            DeviceUniqueIdentifier = "uid-a1", // 已存在于 App100
        }));
    }

    /// <summary>
    /// 不同应用允许相同机器码（应用间设备码独立）。
    /// </summary>
    [Fact]
    public async Task DeviceAdd_AcceptsSameIdentifierAcrossApps()
    {
        await SeedAsync();
        var service = new TenantGameAppDeviceService(
            Repository<TenantGameAppDeviceEntity>(),
            Repository<TenantGameAppEntity>());

        var device = await service.AddAsync(new AddTenantGameAppDeviceRequest
        {
            AppId = App200, // App200 现有 device-b1(uid-b1)；uid-a1 属 App100，在 App200 下应可复用
            Name = "reuse-uid",
            DeviceUniqueIdentifier = "uid-a1",
        });

        Assert.Equal(App200, device.AppId);
        var existsOnApp200 = await _db.Queryable<TenantGameAppDeviceEntity>()
                                      .AnyAsync(u => u.AppId == App200 && u.DeviceUniqueIdentifier == "uid-a1");
        Assert.True(existsOnApp200);
    }

    private const long App100 = 100;
    private const long App200 = 200;
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

        await _db.Insertable(new TenantGameAppEntity
        {
            Id = App100,
            Name = "app-100",
            CreatedTime = now,
            IsDeleted = false,
        }).ExecuteCommandAsync();

        await _db.Insertable(new TenantGameAppEntity
        {
            Id = App200,
            Name = "app-200",
            CreatedTime = now,
            IsDeleted = false,
        }).ExecuteCommandAsync();

        await _db.Insertable(new TenantGameAppDeviceEntity
        {
            Id = DeviceA1,
            AppId = App100,
            Name = "device-a1",
            DeviceUniqueIdentifier = "uid-a1",
            CreatedTime = now,
            IsDeleted = false,
        }).ExecuteCommandAsync();

        await _db.Insertable(new TenantGameAppDeviceEntity
        {
            Id = DeviceA2,
            AppId = App100,
            Name = "device-a2",
            DeviceUniqueIdentifier = "uid-a2",
            CreatedTime = now,
            IsDeleted = false,
        }).ExecuteCommandAsync();

        await _db.Insertable(new TenantGameAppDeviceEntity
        {
            Id = DeviceB1,
            AppId = App200,
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