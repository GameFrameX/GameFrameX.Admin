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
    public async Task PlatformStringSelector_PreservesNameMappingAndPageSize()
    {
        await _database.Insertable(new GlobalPlatformEntity
        {
            Id = 201, Name = "Android", Description = "Mobile", IsDeleted = false,
        }).ExecuteCommandAsync();
        var service = new GlobalPlatformService(new SqlSugarRepository<GlobalPlatformEntity> { Context = _database, });

        var result = Assert.Single(await service.SelectStringAsync(new BaseSelectPageRequest { SearchKey = " Android ", PageSize = 1, }));

        Assert.Equal("201", result.Id);
        Assert.Equal("Android", result.Value);
        Assert.Equal("Mobile", result.Label);
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
