using GameFrameX.Core.Job;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Log;
using GameFrameX.Entity.System;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace GameFrameX.Core.Tests.System.Job;

/// <summary>
/// C404 裁定 §9.4：LogJob 不再删除 SystemLogVisit / SystemLogOperation（移交审计留存归档），
/// 仍清理 SystemLogDiff 与租户历史日志表。行为级断言，防止有人回退修剪。
/// </summary>
public sealed class LogJobRetentionBoundaryTests : IDisposable
{
    private readonly ClientApiTestDb _database = new();

    public LogJobRetentionBoundaryTests()
    {
        _database.InitTenantTables(typeof(SystemLogVisitEntity), typeof(SystemLogOperationEntity), typeof(SystemLogDiffEntity),
                                   typeof(TenantLogVisitEntity), typeof(TenantLogOperationEntity), typeof(TenantLogDiffEntity));
    }

    public void Dispose()
    {
        _database.Dispose();
    }

    [Fact]
    public async Task LogJob_KeepsSystemVisitAndOperation_DeletesDiffAndTenantHistory()
    {
        var cutoff = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 40 * 24 * 60 * 60;
        foreach (var entity in new object[]
                 {
                     new SystemLogVisitEntity { Id = 501, CreatedTime = cutoff - 100, },
                     new SystemLogOperationEntity { Id = 502, CreatedTime = cutoff - 100, },
                     new SystemLogDiffEntity { Id = 503, CreatedTime = cutoff - 100, },
                     new TenantLogVisitEntity { Id = 504, CreatedTime = cutoff - 100, },
                     new TenantLogOperationEntity { Id = 505, CreatedTime = cutoff - 100, },
                     new TenantLogDiffEntity { Id = 506, CreatedTime = cutoff - 100, },
                 })
        {
            _database.MainDb.InsertableByObject(entity).ExecuteCommand();
        }

        // SqlSugarEntityConfig 的插入钩子会把 CreatedTime 填成当前时刻；超期场景用显式 UPDATE 回填
        //（LogJob 只按 CreatedTime 判超期；UPDATE 不触发插入钩子）。
        BackfillCreatedTime<SystemLogVisitEntity>(501, cutoff);
        BackfillCreatedTime<SystemLogOperationEntity>(502, cutoff);
        BackfillCreatedTime<SystemLogDiffEntity>(503, cutoff);
        BackfillCreatedTime<TenantLogVisitEntity>(504, cutoff);
        BackfillCreatedTime<TenantLogOperationEntity>(505, cutoff);
        BackfillCreatedTime<TenantLogDiffEntity>(506, cutoff);

        var services = new ServiceCollection();
        services.AddSingleton<ISqlSugarClient>(_database.MainDb);
        var job = new LogJob(services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>());
        await job.ExecuteAsync(new JobExecutingContext(), CancellationToken.None);


        // §9.4：两张 System 表保留（审计留存负责归档）。
        Assert.Equal(1, _database.MainDb.Queryable<SystemLogVisitEntity>().Count());
        Assert.Equal(1, _database.MainDb.Queryable<SystemLogOperationEntity>().Count());
        // 差异日志与租户历史日志仍按保留期清理。
        Assert.Equal(0, _database.MainDb.Queryable<SystemLogDiffEntity>().Count());
        Assert.Equal(0, _database.MainDb.Queryable<TenantLogVisitEntity>().Count());
        Assert.Equal(0, _database.MainDb.Queryable<TenantLogOperationEntity>().Count());
        Assert.Equal(0, _database.MainDb.Queryable<TenantLogDiffEntity>().Count());
    }

    private void BackfillCreatedTime<TEntity>(long rowId, long cutoff) where TEntity : class, new()
    {
        _database.MainDb.Updateable<TEntity>()
                 .SetColumns("CreatedTime", cutoff - 100)
                 .Where("Id = @rowId", new { rowId, })
                 .ExecuteCommand();
    }
}
