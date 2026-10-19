using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Foundation.Idempotency;
using GameFrameX.Foundation.Utility;
using SqlSugar;

namespace GameFrameX.Application.Game.Tenant.AntiCheat.TenantGameAntiCheatEvent;

/// <summary>
/// 反作弊事件表的 Foundation 幂等存储实现：<see cref="IIdempotencyStore"/> 统一接口承载本站点独立幂等规则。
/// </summary>
/// <remarks>
/// 站点规则（与既有领域语义逐条对齐，幂等选型决策 2026-09-13）：
/// <list type="bullet">
/// <item>业务表即存储：一行事件就是一条幂等记录，无并行通用表；业务字段随占位一次性写入（本站点执行 == 持久化，无外部副作用）。</item>
/// <item>并发恰一由 <c>ux_anticheat_idempotency</c> 唯一索引仲裁：占位插入的输家重读既有行，按 Replay 口径返回。</item>
/// <item>同键一律回放：不做内容一致性判定（RequestDigest 为常量，Conflict 不可达），与"命中返回既有事件"的既有语义一致。</item>
/// <item>保留期永久：事件行不过期（ExpiredTime 取 long.MaxValue），<see cref="RemoveExpiredRecordsAsync"/> 为声明式空实现。</item>
/// </list>
/// 职责分工：本类只管判定生命周期（占位 / 存在性 / 撤销）；事件业务字段由模板一次性携带，回放侧由服务重读业务行。
/// </remarks>
public sealed class AntiCheatEventIdempotencyStore : IIdempotencyStore
{
    /// <summary>
    /// 上报幂等作用域。
    /// </summary>
    public const string IdempotencyScope = "anticheat.event.submit";

    /// <summary>
    /// 常量请求摘要：本站点按键回放、不做内容冲突判定，Conflict 分支由此不可达。
    /// </summary>
    public const string RequestDigest = "anti-cheat-key-replay";

    private readonly SqlSugarRepository<TenantGameAntiCheatEventEntity> _repository;

    /// <summary>
    /// 占位模板：携带全部业务字段（含 IdempotencyKey），占位成功即事件入账。
    /// </summary>
    private readonly TenantGameAntiCheatEventEntity _eventTemplate;

    /// <summary>
    /// 初始化存储。
    /// </summary>
    /// <param name="repository">事件仓储（租户路由）。</param>
    /// <param name="eventTemplate">占位模板（业务字段已就绪，IdempotencyKey 以占位记录为准覆盖）。</param>
    public AntiCheatEventIdempotencyStore(SqlSugarRepository<TenantGameAntiCheatEventEntity> repository, TenantGameAntiCheatEventEntity eventTemplate)
    {
        _repository = repository;
        _eventTemplate = eventTemplate;
    }

    /// <summary>
    /// 本站点协调器选项：执行 == 持久化（无 Processing 中间态），并发等待与失败策略均不参与判定，保留期对行为无影响（行不过期）。
    /// </summary>
    public static IdempotencyOptions CreateCoordinatorOptions()
    {
        return new IdempotencyOptions
        {
            FailedReplayPolicy = FailedReplayPolicy.ReExecute,
        };
    }

    /// <inheritdoc />
    public async Task<bool> TryBeginProcessingAsync(IdempotencyRecord newRecord, CancellationToken cancellationToken = default)
    {
        if (newRecord == null)
        {
            throw new ArgumentNullException(nameof(newRecord));
        }

        try
        {
            _eventTemplate.IdempotencyKey = newRecord.IdempotencyKey;
            await _repository.InsertWithDiffLogAsync(_eventTemplate);
            return true;
        }
        catch (Exception)
        {
            // 并发同键由唯一索引仲裁：输家重读既有行即视为已入账（Replay）；重读无行 = 非冲突异常，原样上抛
            var concurrent = await _repository.GetFirstAsync(u => u.IdempotencyKey == newRecord.IdempotencyKey);
            if (concurrent == null)
            {
                throw;
            }

            return false;
        }
    }

    /// <inheritdoc />
    public async Task CompleteAsync(IdempotencyCompletionRequest request, CancellationToken cancellationToken = default)
    {
        var exists = await _repository.GetFirstAsync(u => u.IdempotencyKey == request.Identifier.IdempotencyKey);
        if (exists == null)
        {
            throw new InvalidOperationException($"幂等记录不存在，无法落为完成：scope={request.Identifier.IdempotencyScope} key={request.Identifier.IdempotencyKey}");
        }
    }

    /// <inheritdoc />
    public async Task FailAsync(IdempotencyFailureRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // 执行失败 = 撤销占位行（本站点执行即插入，撤销即删除；调用方保证占位成功后才调用）
        var rows = await _repository.AsDeleteable().Where(u => u.IdempotencyKey == request.Identifier.IdempotencyKey).ExecuteCommandAsync();
        if (rows == 0)
        {
            throw new InvalidOperationException($"幂等记录不存在，无法落为失败：scope={request.Identifier.IdempotencyScope} key={request.Identifier.IdempotencyKey}");
        }
    }

    /// <inheritdoc />
    public async Task<IdempotencyRecord?> TryGetRecordAsync(IdempotencyRecordIdentifier identifier, CancellationToken cancellationToken = default)
    {
        if (identifier == null)
        {
            throw new ArgumentNullException(nameof(identifier));
        }

        var entity = await _repository.GetFirstAsync(u => u.IdempotencyKey == identifier.IdempotencyKey);
        if (entity == null)
        {
            return null;
        }

        // 已入账事件恒为已完成：无 Processing 中间态，占位成功即终态
        return new IdempotencyRecord(
            identifier.IdempotencyScope,
            identifier.IdempotencyKey,
            RequestDigest,
            IdempotencyStatus.Completed,
            null,
            entity.CreatedTime * 1000L,
            long.MaxValue,
            entity.CreatedTime * 1000L);
    }

    /// <inheritdoc />
    public Task<int> RemoveExpiredRecordsAsync(ExpiredRecordRemovalRequest request, CancellationToken cancellationToken = default)
    {
        // 保留期永久：业务行不过期，清理为声明式空实现（接口契约允许的等价机制）
        return Task.FromResult(0);
    }
}
