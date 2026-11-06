using GameFrameX.Application.Platform.Email;
using GameFrameX.Entity.Business.ClientApiConfig;
using GameFrameX.Entity.System;
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
        _db.CodeFirst.InitTables<SystemEmailProviderEntity, EmailConfigEntity>();
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
    public async Task TenantSelector_ExactPurposeBeatsDefaultPriority()
    {
        await InsertAsync(1, 0, 0, 999, 40, true);
        await InsertAsync(2, 0, 7, 1, 5, true);
        await InsertAsync(3, 200, 0, 999, 20, true);
        await InsertAsync(4, 200, 7, 1, 10, true);

        var selected = await EmailConfigSelector.SelectAsync(_db, 7);

        Assert.NotNull(selected);
        Assert.Equal(4, selected.Id);
    }

    [Fact]
    public async Task TenantSelector_UsesPriorityUpdateTimeAndIdTieBreakAndIgnoresDisabled()
    {
        await InsertAsync(1, 200, 7, 300, 30, false);
        await InsertAsync(2, 200, 7, 100, 10, true);
        await InsertAsync(3, 200, 7, 200, 20, true);
        await InsertAsync(4, 200, 7, 200, 20, true);

        var selected = await EmailConfigSelector.SelectAsync(_db, 7);

        Assert.NotNull(selected);
        Assert.Equal(4, selected.Id);
    }

    [Fact]
    public async Task TenantSelector_DefaultPurposeFallbackWorksWithoutRequestContext()
    {
        await InsertAsync(1, 200, 0, 100, 10, true);
        var selected = await EmailConfigSelector.SelectAsync(_db, 7);

        Assert.Equal(1, selected?.Id);
    }

    [Fact]
    public async Task TenantSelector_ReturnsNullWhenNoEnabledProvider()
    {
        await InsertAsync(1, 200, 7, 100, 10, false);
        await InsertAsync(2, 200, 7, 100, 10, false);

        var selected = await EmailConfigSelector.SelectAsync(_db, 7);

        Assert.Null(selected);
    }

    private async Task InsertSystemAsync(long id, int purpose, int priority, long updateTime, bool enabled)
    {
        await _db.Insertable(new SystemEmailProviderEntity
        {
            Id = id,
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

    private async Task InsertAsync(long id, long appId, int purpose, int priority, long updateTime, bool enabled)
    {
        await _db.Insertable(new EmailConfigEntity
        {
            Id = id,
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