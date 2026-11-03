using GameFrameX.Application.System.GlobalLanguage;
using GameFrameX.Application.System.GlobalPlatform;
using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Global;
using SqlSugar;

namespace GameFrameX.Core.Tests.System;

public sealed class GlobalDefinitionSelectorTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"global_definition_selector_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _database;

    public GlobalDefinitionSelectorTests()
    {
        var configuration = new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_databasePath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(configuration);
        _database = new SqlSugarScope(configuration);
        _database.CodeFirst.InitTables(typeof(GlobalLanguageEntity), typeof(GlobalPlatformEntity));
        // 仅重建测试库的平台表以允许描述为空，生产实体映射保持不变。
        var platformTable = _database.EntityMaintenance.GetTableName<GlobalPlatformEntity>();
        var tableDefinition = _database.Ado.GetString($"SELECT sql FROM sqlite_master WHERE name = '{platformTable}'");
        _database.Ado.ExecuteCommand($"DROP TABLE {platformTable}");
        _database.Ado.ExecuteCommand(tableDefinition.Replace("NOT NULL", string.Empty, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task LanguageStringSelector_PreservesNameValueAndNullableDescription()
    {
        await _database.Insertable(new GlobalLanguageEntity
        {
            Id = 101, Name = "English", Code = "en", Description = null!, IsDeleted = false,
        }).ExecuteCommandAsync();
        var service = CreateLanguageService();

        var result = Assert.Single(await service.SelectStringAsync(new BaseSelectPageRequest()));

        Assert.Equal("101", result.Id);
        Assert.Equal("English", result.Value);
        Assert.Null(result.Label);
    }

    [Fact]
    public async Task LanguageStringSelector_UsesNameFilterAndPageSize()
    {
        await _database.Insertable(new[]
        {
            new GlobalLanguageEntity { Id = 101, Name = "English", Code = "en", Description = "Latin", IsDeleted = false, },
            new GlobalLanguageEntity { Id = 102, Name = "English alternative", Code = "en-alt", Description = "Latin", IsDeleted = false, },
            new GlobalLanguageEntity { Id = 103, Name = "Deleted English", Code = "deleted", Description = "Latin", IsDeleted = true, },
        }).ExecuteCommandAsync();
        var service = CreateLanguageService();

        Assert.Empty(await service.SelectStringAsync(new BaseSelectPageRequest { SearchKey = " Latin ", }));
        Assert.Single(await service.SelectStringAsync(new BaseSelectPageRequest { SearchKey = " English ", PageSize = 1, }));
        Assert.Equal(2, (await service.SelectStringAsync(new BaseSelectPageRequest { SearchKey = "English", })).Count);
    }

    [Fact]
    public async Task LanguageNumericSelector_KeepsLanguageCodeMapping()
    {
        await _database.Insertable(new GlobalLanguageEntity
        {
            Id = 101, Name = "English", Code = "en", Description = "Latin", IsDeleted = false,
        }).ExecuteCommandAsync();

        var result = Assert.Single(await CreateLanguageService().SelectAsync(new BaseSelectPageRequest { SearchKey = "Latin", }));

        Assert.Equal(101, result.Id);
        Assert.Equal("en", result.Value);
        Assert.Equal("Latin", result.Label);
    }

    [Fact]
    public async Task PlatformStringSelector_PreservesNameMappingAndNullableDescription()
    {
        await SeedPlatformsAsync();
        var service = CreatePlatformService();

        var result = await service.SelectStringAsync(new BaseSelectPageRequest { SearchKey = " Android ", });

        Assert.Equal(new[] { "201", "202", }, result.Select(item => item.Id));
        Assert.Equal(new[] { "Android", "Android alternative", }, result.Select(item => item.Value));
        Assert.Null(result[0].Label);
        Assert.Equal("Mobile", result[1].Label);
        Assert.Empty(await service.SelectStringAsync(new BaseSelectPageRequest { SearchKey = "Mobile", }));
        Assert.Single(await service.SelectStringAsync(new BaseSelectPageRequest { SearchKey = "Android", PageSize = 1, }));
    }

    [Fact]
    public async Task PlatformNumericSelector_FiltersDeletedRowsAndKeepsNameMapping()
    {
        await SeedPlatformsAsync();
        var service = CreatePlatformService();

        var result = await service.SelectAsync(new BaseSelectPageRequest { SearchKey = " Android ", });

        Assert.Equal(new long[] { 201, 202, }, result.Select(item => item.Id));
        Assert.Equal(new[] { "Android", "Android alternative", }, result.Select(item => item.Value));
        Assert.Equal(string.Empty, result[0].Label);
        Assert.Equal("Mobile", result[1].Label);
        Assert.Empty(await service.SelectAsync(new BaseSelectPageRequest { SearchKey = "Mobile", }));
        Assert.Single(await service.SelectAsync(new BaseSelectPageRequest { SearchKey = "Android", PageSize = 1, }));
    }

    [Fact]
    public async Task LanguageNumericSelector_PutsAllDefaultFormsFirstThenOrdersById()
    {
        await _database.Insertable(new[]
        {
            new GlobalLanguageEntity { Id = 101, Name = "English", Code = "en", Description = "Latin", },
            new GlobalLanguageEntity { Id = 104, Name = "Flagged", Code = "flag", Description = "Flag", IsDefault = true, },
            new GlobalLanguageEntity { Id = 103, Name = "default", Code = "name-default", Description = "Name default", },
            new GlobalLanguageEntity { Id = 102, Name = "Code default", Code = "default", Description = "Code default", },
        }).ExecuteCommandAsync();

        var result = await CreateLanguageService().SelectAsync(new BaseSelectPageRequest());

        Assert.Equal(new long[] { 102, 103, 104, 101, }, result.Select(item => item.Id));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("  ", "  ")]
    public async Task LanguageNumericSelector_FallsBackToName(string? code, string? description)
    {
        await _database.Insertable(new GlobalLanguageEntity
        {
            Id = 101, Name = "Fallback", Code = code, Description = description!,
        }).ExecuteCommandAsync();

        var result = Assert.Single(await CreateLanguageService().SelectAsync(new BaseSelectPageRequest()));

        Assert.Equal("Fallback", result.Value);
        Assert.Equal("Fallback", result.Label);
    }

    [Theory]
    [InlineData(" English ")]
    [InlineData(" Latin ")]
    [InlineData(" en ")]
    public async Task LanguageNumericSelector_SearchesNameDescriptionAndCode(string searchKey)
    {
        await _database.Insertable(new[]
        {
            new GlobalLanguageEntity { Id = 101, Name = "English", Code = " en ", Description = "Latin", },
            new GlobalLanguageEntity { Id = 102, Name = "Other", Code = "other", Description = "Other", },
        }).ExecuteCommandAsync();

        var result = Assert.Single(await CreateLanguageService().SelectAsync(new BaseSelectPageRequest { SearchKey = searchKey, }));

        Assert.Equal(101, result.Id);
        Assert.Equal("en", result.Value);
        Assert.Equal("Latin", result.Label);
    }

    private async Task SeedPlatformsAsync()
    {
        await _database.Insertable(new[]
        {
            new GlobalPlatformEntity { Id = 201, Name = "Android", Description = null!, IsDeleted = false, },
            new GlobalPlatformEntity { Id = 202, Name = "Android alternative", Description = "Mobile", IsDeleted = false, },
            new GlobalPlatformEntity { Id = 203, Name = "Deleted Android", Description = "Mobile", IsDeleted = true, },
            new GlobalPlatformEntity { Id = 204, Name = "Other", Description = "Mobile", IsDeleted = false, },
        }).ExecuteCommandAsync();
    }

    private GlobalPlatformService CreatePlatformService()
    {
        return new GlobalPlatformService(new SqlSugarRepository<GlobalPlatformEntity> { Context = _database, });
    }

    private GlobalLanguageService CreateLanguageService()
    {
        return new GlobalLanguageService(new SqlSugarRepository<GlobalLanguageEntity> { Context = _database, });
    }

    public void Dispose()
    {
        _database.Dispose();
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}
