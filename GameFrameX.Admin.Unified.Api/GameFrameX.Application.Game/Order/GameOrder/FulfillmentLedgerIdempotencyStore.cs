using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Foundation.Idempotency;
using GameFrameX.Foundation.Utility;
using SqlSugar;

namespace GameFrameX.Application.Game.Order.GameOrder;

/// <summary>
/// 权益发货账本表的 Foundation 幂等存储实现：<see cref="IIdempotencyStore"/> 统一接口承载本站点独立幂等规则。
/// </summary>
/// <remarks>
/// 站点规则（幂等选型决策 2026-09-13，写路径由"事后记账"重排为"占位先行"）：
/// <list type="bullet">
/// <item>业务表即存储：一行账本就是一条幂等记录，占位 = 先建一行 Pending（<see cref="FulfillmentStatusEnum.Pending"/>
/// 为枚举预留的发放意图态），游戏服调用成功 / 失败后落定 Success / Failed；退款作废走服务侧 Voided 终态 + Complete 存在性校验。</item>
/// <item>并发恰一由账本唯一索引仲裁：占位插入输家重读既有行按状态判定；Failed 法定覆盖（条件更新翻回 Pending，复用同一行）。
/// 跨进程重复回调在占位处恰一，游戏服发货与角色累充递增由占位权保证仅执行一次。</item>
/// <item>同键按状态回放：不做内容一致性判定（RequestDigest 为常量，Conflict 不可达）。</item>
/// <item>保留期永久：账本行不过期（ExpiredTime 取 long.MaxValue），<see cref="RemoveExpiredRecordsAsync"/> 为声明式空实现；
/// 崩溃中断遗留的 Pending 行由并发等待超时（Busy）与人工补偿（Compensate 独立键）承接。</item>
/// </list>
/// 职责分工：本类只管判定生命周期（占位 / 状态落定 / 读取映射）；失败原因（业务事实）由服务先行写入账本行。
/// </remarks>
public sealed class FulfillmentLedgerIdempotencyStore : IIdempotencyStore
{
    /// <summary>
    /// 采购发货幂等作用域。
    /// </summary>
    public const string OrderShipIdempotencyScope = "order.ship";

    /// <summary>
    /// 退款扣除幂等作用域。
    /// </summary>
    public const string OrderRefundIdempotencyScope = "order.refund";

    /// <summary>
    /// 人工补偿幂等作用域（键含每次唯一 Guid，保持"每次独立意图、不跨次去重"语义）。
    /// </summary>
    public const string OrderCompensateIdempotencyScope = "order.compensate";

    /// <summary>
    /// 常量请求摘要：本站点按状态回放、不做内容冲突判定，Conflict 分支由此不可达。
    /// </summary>
    public const string RequestDigest = "fulfillment-key-replay";

    private readonly ISqlSugarClient _db;

    /// <summary>
    /// 占位模板：携带订单快照全字段（Type / ItemsJson / OperatorId / 审计列），占位成功即账本行落库。
    /// </summary>
    private readonly TenantGameOrderFulfillmentEntity _ledgerTemplate;

    /// <summary>
    /// 初始化存储。
    /// </summary>
    /// <param name="db">租户路由的数据库客户端（与发货服务同源）。</param>
    /// <param name="ledgerTemplate">占位模板（订单快照字段已就绪，IdempotencyKey 以占位记录为准覆盖）。</param>
    public FulfillmentLedgerIdempotencyStore(ISqlSugarClient db, TenantGameOrderFulfillmentEntity ledgerTemplate)
    {
        _db = db;
        _ledgerTemplate = ledgerTemplate;
    }

    /// <summary>
    /// 本站点协调器选项：失败可重试（Failed 行复用重执行）；并发等待 5 秒（游戏服发货窗口内重复回调可等到首次结果回放）。
    /// </summary>
    public static IdempotencyOptions CreateCoordinatorOptions()
    {
        return new IdempotencyOptions
        {
            FailedReplayPolicy = FailedReplayPolicy.ReExecute,
            ConcurrentWaitTimeoutMilliseconds = 5_000,
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
            _ledgerTemplate.IdempotencyKey = newRecord.IdempotencyKey;
            _ledgerTemplate.Status = FulfillmentStatusEnum.Pending;
            await _db.Insertable(_ledgerTemplate).ExecuteCommandAsync();
            return true;
        }
        catch (Exception)
        {
            // 并发同键由账本唯一索引仲裁：输家重读既有行按状态判定；重读无行 = 非冲突异常，原样上抛
            var existing = await _db.Queryable<TenantGameOrderFulfillmentEntity>()
                                     .FirstAsync(u => u.IdempotencyKey == newRecord.IdempotencyKey);
            if (existing == null)
            {
                throw;
            }

            if (existing.Status != FulfillmentStatusEnum.Failed)
            {
                // Success / Pending / Voided（终态或执行中）：占用中，占位失败
                return false;
            }

            // 法定覆盖：存量 Failed 允许重试复用同一行——条件更新（仅 Failed 可翻回 Pending）保证并发重试恰一
            var rows = await _db.Updateable<TenantGameOrderFulfillmentEntity>()
                                .SetColumns(u => u.Status == FulfillmentStatusEnum.Pending)
                                .SetColumns(u => u.FailureReason == null)
                                .SetColumns(u => u.ItemsJson == _ledgerTemplate.ItemsJson)
                                .SetColumns(u => u.OperatorId == _ledgerTemplate.OperatorId)
                                .SetColumns(u => u.UpdateTime == _ledgerTemplate.CreatedTime)
                                .SetColumns(u => u.UpdatedId == _ledgerTemplate.OperatorId)
                                .SetColumns(u => u.UpdatedName == _ledgerTemplate.CreatedName)
                                .Where(u => u.IdempotencyKey == newRecord.IdempotencyKey && u.Status == FulfillmentStatusEnum.Failed)
                                .ExecuteCommandAsync();
            return rows == 1;
        }
    }

    /// <inheritdoc />
    public async Task CompleteAsync(IdempotencyCompletionRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // 落定成功：仅 Pending 可翻 Success（退款作废等业务终态由服务先行落定，此处做存在性校验）
        var rows = await _db.Updateable<TenantGameOrderFulfillmentEntity>()
                            .SetColumns(u => u.Status == FulfillmentStatusEnum.Success)
                            .SetColumns(u => u.FailureReason == null)
                            .Where(u => u.IdempotencyKey == request.Identifier.IdempotencyKey && u.Status == FulfillmentStatusEnum.Pending)
                            .ExecuteCommandAsync();
        if (rows == 0)
        {
            await EnsureRecordExistsAsync(request.Identifier);
        }
    }

    /// <inheritdoc />
    public async Task FailAsync(IdempotencyFailureRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // 落定失败：仅 Pending 可翻 Failed；失败原因（业务事实）由服务先行写入
        var rows = await _db.Updateable<TenantGameOrderFulfillmentEntity>()
                            .SetColumns(u => u.Status == FulfillmentStatusEnum.Failed)
                            .Where(u => u.IdempotencyKey == request.Identifier.IdempotencyKey && u.Status == FulfillmentStatusEnum.Pending)
                            .ExecuteCommandAsync();
        if (rows == 0)
        {
            await EnsureRecordExistsAsync(request.Identifier);
        }
    }

    /// <inheritdoc />
    public async Task<IdempotencyRecord?> TryGetRecordAsync(IdempotencyRecordIdentifier identifier, CancellationToken cancellationToken = default)
    {
        if (identifier == null)
        {
            throw new ArgumentNullException(nameof(identifier));
        }

        var entity = await _db.Queryable<TenantGameOrderFulfillmentEntity>()
                              .FirstAsync(u => u.IdempotencyKey == identifier.IdempotencyKey);
        if (entity == null)
        {
            return null;
        }

        return new IdempotencyRecord(
            identifier.IdempotencyScope,
            identifier.IdempotencyKey,
            RequestDigest,
            MapStatus(entity.Status),
            null,
            entity.CreatedTime * 1000L,
            long.MaxValue,
            entity.Status == FulfillmentStatusEnum.Pending ? null : entity.UpdateTime * 1000L);
    }

    /// <inheritdoc />
    public Task<int> RemoveExpiredRecordsAsync(ExpiredRecordRemovalRequest request, CancellationToken cancellationToken = default)
    {
        // 保留期永久：账本行不过期，清理为声明式空实现（接口契约允许的等价机制）
        return Task.FromResult(0);
    }

    private async Task EnsureRecordExistsAsync(IdempotencyRecordIdentifier identifier)
    {
        var exists = await _db.Queryable<TenantGameOrderFulfillmentEntity>()
                              .AnyAsync(u => u.IdempotencyKey == identifier.IdempotencyKey);
        if (!exists)
        {
            throw new InvalidOperationException($"幂等记录不存在，无法落定：scope={identifier.IdempotencyScope} key={identifier.IdempotencyKey}");
        }
    }

    private static IdempotencyStatus MapStatus(FulfillmentStatusEnum status)
    {
        return status switch
        {
            FulfillmentStatusEnum.Pending => IdempotencyStatus.Processing,
            FulfillmentStatusEnum.Success => IdempotencyStatus.Completed,
            FulfillmentStatusEnum.Failed  => IdempotencyStatus.Failed,
            // 作废为业务终态（未实际发货的退款），映射已完成：同键回放
            FulfillmentStatusEnum.Voided  => IdempotencyStatus.Completed,
            _                             => throw new ArgumentOutOfRangeException(nameof(status), status, "不支持的状态映射"),
        };
    }
}
