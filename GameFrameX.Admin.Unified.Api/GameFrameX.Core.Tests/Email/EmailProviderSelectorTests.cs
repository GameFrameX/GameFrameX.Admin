using GameFrameX.Application.Game.System.Email;
using GameFrameX.Entity.Tenant.ClientApiConfig;
using GameFrameX.Entity.System;
using GameFrameX.Client.Api.Services.ClientApiTenantOptions;
using GameFrameX.Client.Api.Services.Email;
using SqlSugar;

namespace GameFrameX.Core.Tests.Email;

public class EmailProviderSelectorTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"email_provider_selector_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;

    public EmailProviderSelectorTests()
    {
        _db = new SqlSugarScope(new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
            InitKeyType = InitKeyType.Attribute,
            IsAutoCloseConnection = true,
        });
        _db.CodeFirst.InitTables<SystemEmailProviderEntity, TenantEmailConfigEntity>();
    }

    [Fact]
    public async Task SystemSelector_ExactPurposeBeatsDefaultAndIgnoresDisabled()
    {
        await InsertSystemAsync(1, 0, 999, 30, true);
        await InsertSystemAsync(2, 10, 10, 20, false);
        await InsertSystemAsync(3, 10, 1, 10, true);

        var selected = await SystemEmailProviderSelector.SelectAsync(_db, 10);

        Assert.NotNull(selected);
        Assert.Equal(3, selected.Id);
    }

    [Fact]
    public async Task SystemSelector_DefaultLayerUsesPriorityUpdateTimeAndIdTieBreak()
    {
        await InsertSystemAsync(1, 0, 100, 10, true);
        await InsertSystemAsync(2, 0, 200, 10, true);
        await InsertSystemAsync(3, 0, 200, 20, true);
        await InsertSystemAsync(4, 0, 200, 20, true);

        var selected = await SystemEmailProviderSelector.SelectAsync(_db, 99);

        Assert.NotNull(selected);
        Assert.Equal(4, selected.Id);
    }

    [Fact]
    public async Task SystemSelector_ReturnsNullWhenNoEnabledProviderExists()
    {
        await InsertSystemAsync(1, 0, 100, 10, false);
        await InsertSystemAsync(2, 10, 100, 10, false);

        var selected = await SystemEmailProviderSelector.SelectAsync(_db, 10);

        Assert.Null(selected);
    }

    [Fact]
    public async Task TenantSelector_ScopeLayerBeatsLowerScopePriority()
    {
        var context = new ClientApiTenantResolution { TenantId = 100, AppId = 200, };
        await InsertTenantAsync(1, 100, 0, 0, 999, 40, true);
        await InsertTenantAsync(2, 100, 0, 7, 999, 30, true);
        await InsertTenantAsync(3, 100, 200, 0, 999, 20, true);
        await InsertTenantAsync(4, 100, 200, 7, 1, 10, true);

        var selected = await TenantEmailConfigSelector.SelectAsync(_db, context, 7);

        Assert.NotNull(selected);
        Assert.Equal(4, selected.Id);
    }

    [Fact]
    public async Task TenantSelector_SameScopeUsesPriorityUpdateTimeAndIdTieBreakAndIgnoresDisabled()
    {
        var context = new ClientApiTenantResolution { TenantId = 100, AppId = 200, };
        await InsertTenantAsync(1, 100, 200, 7, 300, 30, false);
        await InsertTenantAsync(2, 100, 200, 7, 100, 10, true);
        await InsertTenantAsync(3, 100, 200, 7, 200, 20, true);
        await InsertTenantAsync(4, 100, 200, 7, 200, 20, true);

        var selected = await TenantEmailConfigSelector.SelectAsync(_db, context, 7);

        Assert.NotNull(selected);
        Assert.Equal(4, selected.Id);
    }

    [Fact]
    public async Task TenantSelector_ReturnsNullWithoutTenantContext()
    {
        var selected = await TenantEmailConfigSelector.SelectAsync(_db, new ClientApiTenantResolution { TenantId = 0, AppId = 200, }, 7);

        Assert.Null(selected);
    }

    [Fact]
    public async Task TenantSelector_ReturnsNullWhenTenantHasNoEnabledProvider()
    {
        var context = new ClientApiTenantResolution { TenantId = 100, AppId = 200, };
        await InsertTenantAsync(1, 999, 200, 7, 100, 10, true);
        await InsertTenantAsync(2, 100, 200, 7, 100, 10, false);

        var selected = await TenantEmailConfigSelector.SelectAsync(_db, context, 7);

        Assert.Null(selected);
    }

    private async Task InsertSystemAsync(long id, int purpose, int priority, long updateTime, bool enabled)
    {
        await _db.Insertable(new SystemEmailProviderEntity
        {
            Id = id,
            TenantId = 0,
            CreatedId = 1,
            CreatedTime = 1,
            CreatedName = "test",
            UpdateCount = 0,
            UpdateTime = updateTime,
            UpdatedId = 1,
            UpdatedName = "test",
            IsDeleted = false,
            IsEnabled = enabled,
            DeleteTime = 0,
            DeletedId = 0,
            DeletedName = string.Empty,
            RowVersion = 0,
            Purpose = purpose,
            Provider = "TencentCloudSes",
            DefaultFromEmail = "system@example.test",
            SecretId = "sid",
            SecretKey = "skey",
            TemplateId = 1,
            Priority = priority,
        }).ExecuteCommandAsync();
    }

    private async Task InsertTenantAsync(long id, long tenantId, long appId, int purpose, int priority, long updateTime, bool enabled)
    {
        await _db.Insertable(new TenantEmailConfigEntity
        {
            Id = id,
            TenantId = tenantId,
            CreatedId = 1,
            CreatedTime = 1,
            CreatedName = "test",
            UpdateCount = 0,
            UpdateTime = updateTime,
            UpdatedId = 1,
            UpdatedName = "test",
            IsDeleted = false,
            IsEnabled = enabled,
            DeleteTime = 0,
            DeletedId = 0,
            DeletedName = string.Empty,
            RowVersion = 0,
            AppId = appId,
            Purpose = purpose,
            Provider = "TencentCloudSes",
            FromEmailAddress = "tenant@example.test",
            FromEmailSecretId = "sid",
            FromEmailSecretKey = "skey",
            Priority = priority,
            Remark = "test",
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