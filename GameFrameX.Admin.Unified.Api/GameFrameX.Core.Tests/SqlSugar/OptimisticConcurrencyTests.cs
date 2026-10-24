using System.ComponentModel.DataAnnotations;
using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Service.Base;
using GameFrameX.Core.Extension;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using Microsoft.AspNetCore.Http;
using SqlSugar;
using ClientRepository = GameFrameX.Core.SqlSugar.Repository<GameFrameX.Core.Tests.SqlSugar.OptimisticTestEntity>;
using HubRepository = GameFrameX.Core.SqlSugar.Repository<GameFrameX.Core.Tests.SqlSugar.OptimisticTestEntity>;

namespace GameFrameX.Core.Tests.SqlSugar;

public sealed class OptimisticConcurrencyTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"optimistic_concurrency_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;

    public OptimisticConcurrencyTests()
    {
        var config = new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _db = new SqlSugarScope(config);
        _db.CodeFirst.InitTables(typeof(TenantGameAppEntity), typeof(OptimisticTestEntity));
    }

    [Fact]
    public void VersionColumn_UsesSqlSugarOptimisticLockMetadata()
    {
        var versionColumn = _db.EntityMaintenance.GetEntityInfo<TenantGameAppEntity>().Columns
                               .Single(column => column.PropertyName == nameof(TenantGameAppEntity.RowVersion));

        Assert.True(versionColumn.IsEnableUpdateVersionValidation);
    }

    [Fact]
    public void BaseUpdateInput_RequiresVersion()
    {
        var input = new BaseUpdateRequest { Id = 1, };

        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(input, new ValidationContext(input), validationResults, true);

        Assert.False(isValid);
        Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(BaseUpdateRequest.RowVersion)));
    }

    [Fact]
    public void BaseDeleteInput_RequiresVersion()
    {
        var versionProperty = typeof(BaseDeleteRequest).GetProperty(nameof(BaseDeleteRequest.RowVersion));

        Assert.NotNull(versionProperty);
        Assert.NotNull(versionProperty.GetCustomAttributes(typeof(RequiredAttribute), true).SingleOrDefault());
    }

    [Fact]
    public async Task Middleware_WhenVersionConflicts_ReturnsStableHttp409Body()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new OptimisticConcurrencyMiddleware(_ => throw new VersionExceptions("stale"));

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
        Assert.Contains(OptimisticConcurrencyMiddleware.ConflictMessage, body);
    }

    [Fact]
    public async Task OptimisticUpdate_WhenSnapshotsHaveSameVersion_RejectsSecondWrite()
    {
        var entity = new TenantGameAppEntity
        {
            Id = 1,
            TenantId = 1,
            Name = "initial",
            Description = "initial",
            AppSecret = "secret",
            CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            RowVersion = 0,
        };
        await _db.Insertable(entity).ExecuteCommandAsync();

        var firstSnapshot = await _db.Queryable<TenantGameAppEntity>().InSingleAsync(entity.Id);
        var secondSnapshot = await _db.Queryable<TenantGameAppEntity>().InSingleAsync(entity.Id);
        firstSnapshot.Name = "first";
        secondSnapshot.Name = "second";

        var firstRows = await _db.Updateable(firstSnapshot).ExecuteCommandWithOptLockAsync(true);
        await Assert.ThrowsAsync<VersionExceptions>(() =>
                                                        _db.Updateable(secondSnapshot).ExecuteCommandWithOptLockAsync(true));

        var persisted = await _db.Queryable<TenantGameAppEntity>().InSingleAsync(entity.Id);
        Assert.Equal(1, firstRows);
        Assert.Equal("first", persisted.Name);
        Assert.NotEqual(0, persisted.RowVersion);
    }

    [Fact]
    public async Task ClientApiRepository_WhenSnapshotsConflict_RejectsSecondWrite()
    {
        await SeedTestEntityAsync(2);
        var repository = new ClientRepository(
            _db,
            null!,
            new DelegateSqlSugarTenantIdResolver(() => null));
        var firstSnapshot = await _db.Queryable<OptimisticTestEntity>().InSingleAsync(2);
        var secondSnapshot = await _db.Queryable<OptimisticTestEntity>().InSingleAsync(2);
        firstSnapshot.Value = "client-first";
        secondSnapshot.Value = "client-second";

        await repository.InnerUpdateAsync(firstSnapshot);
        await Assert.ThrowsAsync<VersionExceptions>(() => repository.InnerUpdateAsync(secondSnapshot));

        Assert.Equal("client-first", (await _db.Queryable<OptimisticTestEntity>().InSingleAsync(2)).Value);
    }

    [Fact]
    public async Task HubApiRepository_WhenSnapshotsConflict_RejectsSecondWrite()
    {
        await SeedTestEntityAsync(3);
        var repository = new HubRepository(
            _db,
            null!,
            new DelegateSqlSugarTenantIdResolver(() => null));
        var firstSnapshot = await _db.Queryable<OptimisticTestEntity>().InSingleAsync(3);
        var secondSnapshot = await _db.Queryable<OptimisticTestEntity>().InSingleAsync(3);
        firstSnapshot.Value = "hub-first";
        secondSnapshot.Value = "hub-second";

        await repository.UpdateWithOptimisticLockAsync(firstSnapshot);
        await Assert.ThrowsAsync<VersionExceptions>(() => repository.UpdateWithOptimisticLockAsync(secondSnapshot));

        Assert.Equal("hub-first", (await _db.Queryable<OptimisticTestEntity>().InSingleAsync(3)).Value);
    }

    [Fact]
    public async Task DeleteAsync_WithoutRowVersion_SoftDeletesUsingCurrentVersion()
    {
        await SeedTestEntityAsync(4);
        var service = new DeleteTestService(new SqlSugarRepository<OptimisticTestEntity> { Context = _db, });

        await service.DeleteAsync(new BaseDeleteRequest { Id = 4, });

        var persisted = await _db.Queryable<OptimisticTestEntity>().InSingleAsync(4);
        Assert.True(persisted.IsDeleted);
    }

    [Fact]
    public async Task DeleteAsync_WithStaleRowVersion_ThrowsVersionConflict()
    {
        await SeedTestEntityAsync(5);
        var service = new DeleteTestService(new SqlSugarRepository<OptimisticTestEntity> { Context = _db, });

        await Assert.ThrowsAsync<VersionExceptions>(() =>
                                                         service.DeleteAsync(new BaseDeleteRequest { Id = 5, RowVersion = long.MaxValue, }));
    }

    [Fact]
    public async Task UpdateAsync_WithoutRowVersion_UpdatesUsingCurrentVersion()
    {
        await SeedTestEntityAsync(6);
        var service = new CrudTestService(new SqlSugarRepository<OptimisticTestEntity> { Context = _db, });

        await service.UpdateAsync(new CrudTestUpdateRequest { Id = 6, Value = "updated-without-version", });

        Assert.Equal("updated-without-version", (await _db.Queryable<OptimisticTestEntity>().InSingleAsync(6)).Value);
    }

    [Fact]
    public async Task UpdateAsync_MissingRecord_ThrowsRecordNotFound()
    {
        var service = new CrudTestService(new SqlSugarRepository<OptimisticTestEntity> { Context = _db, });

        await Assert.ThrowsAsync<BusinessException>(() =>
                                                        service.UpdateAsync(new CrudTestUpdateRequest { Id = 999, Value = "missing", }));
    }

    private async Task SeedTestEntityAsync(long id)
    {
        await _db.Insertable(new OptimisticTestEntity
        {
            Id = id,
            Value = "initial",
            CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            RowVersion = 0,
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

    private sealed class DeleteTestService : BaseService<OptimisticTestEntity>
    {
        public DeleteTestService(SqlSugarRepository<OptimisticTestEntity> repository) : base(repository)
        {
        }
    }

    private sealed class CrudTestUpdateRequest : ICrudUpdateRequest
    {
        public long Id { get; set; }

        public long? RowVersion { get; set; }

        public string Value { get; set; }
    }

    private sealed class CrudTestAddRequest : BaseAddRequest
    {
        public string Value { get; set; }
    }

    private sealed class CrudTestPageRequest : BasePageRequest
    {
    }

    private sealed class CrudTestResponse
    {
        public long Id { get; set; }

        public string Value { get; set; }
    }

    private sealed class CrudTestService : GenericCrudService<OptimisticTestEntity, CrudTestAddRequest, CrudTestUpdateRequest, CrudTestResponse, CrudTestPageRequest>
    {
        public CrudTestService(SqlSugarRepository<OptimisticTestEntity> repository) : base(repository)
        {
        }
    }
}