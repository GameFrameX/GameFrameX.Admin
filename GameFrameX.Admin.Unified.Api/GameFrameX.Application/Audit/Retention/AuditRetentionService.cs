using GameFrameX.Core.Service.Business;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Log;
using GameFrameX.Foundation.Orm.Entity;
using Mapster;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace GameFrameX.Application.Audit.Retention;

/// <summary>
/// 审计日志留存服务（C404 / GFX-1216 R3）。把主库超期的访问 / 操作日志按批搬入同库归档表，
/// 再按归档保留天数清理归档表；范围不含异常日志与差异日志（契约 §1.7）。
/// </summary>
public class AuditRetentionService
{
    private const long SecondsPerDay = 24 * 60 * 60;

    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly AuditRetentionOptions _options;
    private readonly ILogger<AuditRetentionService> _logger;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sqlSugarClient">数据库客户端</param>
    /// <param name="options">审计留存配置</param>
    /// <param name="logger">日志</param>
    public AuditRetentionService(ISqlSugarClient sqlSugarClient, IOptions<AuditRetentionOptions> options, ILogger<AuditRetentionService> logger)
    {
        _sqlSugarClient = sqlSugarClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// 执行一轮留存：搬移超期热数据并按保留天数清理归档表。开关关闭时立即返回，不触碰数据。
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>本次执行的汇总结果</returns>
    public async Task<AuditRetentionExecutionResult> ExecuteAsync(CancellationToken cancellationToken)
    {
        var startedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var result = new AuditRetentionExecutionResult { Enabled = _options.Enabled, };
        if (!_options.Enabled)
        {
            result.ElapsedMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startedAt;
            return result;
        }

        var nowSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var batchLimit = Math.Max(1, _options.BatchSize);
        var db = _sqlSugarClient.GetMainDatabaseScope();

        result.Tables.Add(await ArchiveSourceAsync<SystemLogVisitEntity, SystemLogVisitArchiveEntity>(db, "访问日志", nowSeconds - (long)_options.VisitHotDataDays * SecondsPerDay, batchLimit, cancellationToken));
        result.Tables.Add(await ArchiveSourceAsync<SystemLogOperationEntity, SystemLogOperationArchiveEntity>(db, "操作日志", nowSeconds - (long)_options.OperationHotDataDays * SecondsPerDay, batchLimit, cancellationToken));

        if (_options.ArchiveRetentionDays > 0)
        {
            var archiveCutoff = nowSeconds - (long)_options.ArchiveRetentionDays * SecondsPerDay;
            result.Tables[0].PurgedCount = await PurgeArchiveAsync<SystemLogVisitArchiveEntity>(db, archiveCutoff, batchLimit, cancellationToken);
            result.Tables[1].PurgedCount = await PurgeArchiveAsync<SystemLogOperationArchiveEntity>(db, archiveCutoff, batchLimit, cancellationToken);
        }

        result.ElapsedMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - startedAt;
        _logger.LogInformation("审计日志归档完成：{Summary}", System.Text.Json.JsonSerializer.Serialize(result));
        return result;
    }

    /// <summary>
    /// 把单张源表的超期行按批搬入归档表（每批一个事务：先插入归档，再删源行）。
    /// </summary>
    private async Task<AuditRetentionTableResult> ArchiveSourceAsync<TSource, TArchive>(ISqlSugarClient db, string logKind, long cutoffSeconds, int batchLimit, CancellationToken cancellationToken)
        where TSource : LogVisitFields, new()
        where TArchive : EntityBase, IArchiveTimestamped, new()
    {
        var tableResult = new AuditRetentionTableResult { LogKind = logKind, };
        while (!cancellationToken.IsCancellationRequested)
        {
            var sourceRows = await db.Queryable<TSource>()
                                     .Where(u => u.CreatedTime < cutoffSeconds)
                                     .OrderBy(u => u.Id)
                                     .Take(batchLimit)
                                     .ToListAsync(cancellationToken);
            if (sourceRows.Count == 0)
            {
                break;
            }

            var archivedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var archiveRows = sourceRows.Select(row =>
            {
                var archive = row.Adapt<TArchive>();
                archive.ArchivedTime = archivedAt;
                return archive;
            }).ToList();

            var sourceIds = sourceRows.Select(u => u.Id).ToList();
            var moved = await db.Ado.UseTranAsync(async () =>
            {
                await db.Insertable(archiveRows).ExecuteCommandAsync(cancellationToken);
                await db.Deleteable<TSource>().In(sourceIds).ExecuteCommandAsync(cancellationToken);
            });
            if (!moved.IsSuccess)
            {
                throw moved.ErrorException ?? new InvalidOperationException($"审计日志归档事务失败：{logKind}");
            }

            tableResult.ArchivedCount += sourceRows.Count;
            if (sourceRows.Count < batchLimit)
            {
                break;
            }
        }

        return tableResult;
    }

    /// <summary>
    /// 按归档时间清理归档表（分批删除）。
    /// </summary>
    private static async Task<long> PurgeArchiveAsync<TArchive>(ISqlSugarClient db, long cutoffSeconds, int batchLimit, CancellationToken cancellationToken)
        where TArchive : EntityBase, IArchiveTimestamped, new()
    {
        long purgedCount = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            var archiveIds = await db.Queryable<TArchive>()
                                     .Where(u => u.ArchivedTime < cutoffSeconds)
                                     .OrderBy(u => u.Id)
                                     .Take(batchLimit)
                                     .Select(u => u.Id)
                                     .ToListAsync(cancellationToken);
            if (archiveIds.Count == 0)
            {
                break;
            }

            purgedCount += await db.Deleteable<TArchive>().In(archiveIds).ExecuteCommandAsync(cancellationToken);
            if (archiveIds.Count < batchLimit)
            {
                break;
            }
        }

        return purgedCount;
    }
}
