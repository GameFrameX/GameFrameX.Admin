using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Foundation.Idempotency;
using SqlSugar;

namespace GameFrameX.Client.Api.Integrations.Advertisement;

/// <summary>
/// 广告奖励账本表的 Foundation 幂等存储实现：<see cref="IIdempotencyStore"/> 统一接口承载本站点独立幂等规则。
/// </summary>
/// <remarks>
/// 站点规则（与 C114 既有领域语义逐条对齐，幂等选型决策 2026-09-13）：
/// <list type="bullet">
/// <item>业务表即存储：一行账本就是一条幂等记录（占位 = claim 一行 Pending），无并行通用表。</item>
/// <item>状态映射：Pending=Processing（执行中，并发回调等待后回忙碌）/ Success=Completed（回放"奖励已发放"）/ Failed=Failed（法定覆盖，重试复用同一行）。</item>
/// <item>并发恰一由 <c>ux_ad_reward_idempotency</c> 唯一索引仲裁：占位插入输家重读既有行按状态判定；Failed 复用走条件更新（Status=Failed 才允许翻回 Pending）。</item>
/// <item>同键一律按状态回放：不做内容一致性判定（RequestDigest 为常量，Conflict 不可达），与既有"同交易号不重复发奖"语义一致。</item>
/// <item>保留期永久：账本行不过期（ExpiredTime 取 long.MaxValue），<see cref="RemoveExpiredRecordsAsync"/> 为声明式空实现。</item>
/// </list>
/// 职责分工：本类只管判定生命周期（占位 / 状态落定 / 读取映射）；业务事实（角色四字段、失败原因）由服务在 Execute 分支内写入账本行。
/// </remarks>
public sealed class AdvertisementRewardLedgerIdempotencyStore : IIdempotencyStore
{
    /// <summary>
    /// 广告奖励回调幂等作用域。
    /// </summary>
    public const string IdempotencyScope = "ad.reward.receive";

    /// <summary>
    /// 常量请求摘要：本站点按状态回放、不做内容冲突判定，Conflict 分支由此不可达。
    /// </summary>
    public const string RequestDigest = "ad-reward-key-replay";

    private readonly SqlSugarRepository<TenantGameAdvertisementRewardEntity> _repository;

    /// <summary>
    /// 占位模板：携带回调 payload 侧字段（Provider / TransactionId / AdUnit / 奖励项 / 沙盒标记），角色侧字段由服务在 Execute 分支补写。
    /// </summary>
    private readonly TenantGameAdvertisementRewardEntity _ledgerTemplate;

    /// <summary>
    /// 初始化存储。
    /// </summary>
    /// <param name="repository">账本仓储（租户路由）。</param>
    /// <param name="ledgerTemplate">占位模板（payload 字段已就绪，IdempotencyKey 以占位记录为准覆盖）。</param>
    public AdvertisementRewardLedgerIdempotencyStore(SqlSugarRepository<TenantGameAdvertisementRewardEntity> repository, TenantGameAdvertisementRewardEntity ledgerTemplate)
    {
        _repository = repository;
        _ledgerTemplate = ledgerTemplate;
    }

    /// <summary>
    /// 本站点协调器选项：失败可重试（Failed 行复用重执行）；并发等待 1 秒（回调时效敏感，等待后按忙碌返回，与既有"处理中"口径一致）。
    /// </summary>
    public static IdempotencyOptions CreateCoordinatorOptions()
    {
        return new IdempotencyOptions
        {
            FailedReplayPolicy = FailedReplayPolicy.ReExecute,
            ConcurrentWaitTimeoutMilliseconds = 1_000,
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
            _ledgerTemplate.Status = AdvertisementRewardStatusEnum.Pending;
            await _repository.InsertAsync(_ledgerTemplate);
            return true;
        }
        catch (Exception)
        {
            // 并发同键由唯一索引仲裁：输家重读既有行按状态判定；重读无行 = 非冲突异常，原样上抛
            var existing = await _repository.GetFirstAsync(u => u.IdempotencyKey == newRecord.IdempotencyKey);
            if (existing == null)
            {
                throw;
            }

            if (existing.Status != AdvertisementRewardStatusEnum.Failed)
            {
                // Success（已发放）/ Pending（发放中）：占用中，占位失败
                return false;
            }

            // 法定覆盖：存量 Failed 允许重试复用同一行——条件更新（仅 Failed 可翻回 Pending）保证并发重试恰一
            var rows = await _repository.AsUpdateable()
                                        .SetColumns(u => u.Status == AdvertisementRewardStatusEnum.Pending)
                                        .SetColumns(u => u.FailureReason == null)
                                        .SetColumns(u => u.Provider == _ledgerTemplate.Provider)
                                        .SetColumns(u => u.TransactionId == _ledgerTemplate.TransactionId)
                                        .SetColumns(u => u.AdvertisementUnit == _ledgerTemplate.AdvertisementUnit)
                                        .SetColumns(u => u.RewardItem == _ledgerTemplate.RewardItem)
                                        .SetColumns(u => u.RewardAmount == _ledgerTemplate.RewardAmount)
                                        .SetColumns(u => u.IsSandbox == _ledgerTemplate.IsSandbox)
                                        .Where(u => u.IdempotencyKey == newRecord.IdempotencyKey && u.Status == AdvertisementRewardStatusEnum.Failed)
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

        // 落定成功：状态列由本类收口（失败原因一并清空）；首次响应由服务按行状态合成，无需载体
        var rows = await _repository.AsUpdateable()
                                    .SetColumns(u => u.Status == AdvertisementRewardStatusEnum.Success)
                                    .SetColumns(u => u.FailureReason == null)
                                    .Where(u => u.IdempotencyKey == request.Identifier.IdempotencyKey)
                                    .ExecuteCommandAsync();
        if (rows == 0)
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

        // 落定失败：只翻状态列；失败原因（业务事实）由服务先行写入账本行
        var rows = await _repository.AsUpdateable()
                                    .SetColumns(u => u.Status == AdvertisementRewardStatusEnum.Failed)
                                    .Where(u => u.IdempotencyKey == request.Identifier.IdempotencyKey)
                                    .ExecuteCommandAsync();
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

        return new IdempotencyRecord(
            identifier.IdempotencyScope,
            identifier.IdempotencyKey,
            RequestDigest,
            MapStatus(entity.Status),
            null,
            entity.CreatedTime * 1000L,
            long.MaxValue,
            entity.Status == AdvertisementRewardStatusEnum.Pending ? null : entity.UpdateTime * 1000L);
    }

    /// <inheritdoc />
    public Task<int> RemoveExpiredRecordsAsync(ExpiredRecordRemovalRequest request, CancellationToken cancellationToken = default)
    {
        // 保留期永久：账本行不过期，清理为声明式空实现（接口契约允许的等价机制）
        return Task.FromResult(0);
    }

    private static IdempotencyStatus MapStatus(AdvertisementRewardStatusEnum status)
    {
        return status switch
        {
            AdvertisementRewardStatusEnum.Pending => IdempotencyStatus.Processing,
            AdvertisementRewardStatusEnum.Success => IdempotencyStatus.Completed,
            AdvertisementRewardStatusEnum.Failed  => IdempotencyStatus.Failed,
            _                                     => throw new ArgumentOutOfRangeException(nameof(status), status, "不支持的状态映射"),
        };
    }
}
