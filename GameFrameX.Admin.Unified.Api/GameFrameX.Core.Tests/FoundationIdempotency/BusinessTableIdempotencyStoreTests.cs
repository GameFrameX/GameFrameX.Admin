using GameFrameX.Application.Game.Tenant.AntiCheat.TenantGameAntiCheatEvent;
using GameFrameX.Application.Game.Order.GameOrder;
using GameFrameX.Client.Api.Integrations.Advertisement;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Foundation.Idempotency;
using GameFrameX.Foundation.Utility;

namespace GameFrameX.Core.Tests.FoundationIdempotency;

/// <summary>
/// 业务表即 Store 的 Foundation 接口契约测试（真实 SQLite 租户库）：
/// 反作弊事件表（插入即占位即执行）与广告奖励账本表（claim Pending → 落定），
/// 验证各站点独立规则下的 <see cref="IIdempotencyStore"/> 硬性契约——并发恰一（唯一索引仲裁）、
/// 法定覆盖（仅广告账本 Failed 可重试复用）、状态映射回放、落定缺失抛错。仲裁语义被破坏时测试红。
/// </summary>
public sealed class BusinessTableIdempotencyStoreTests : IDisposable
{
    private readonly ClientApiTestDb _db = new();

    [Fact]
    public async Task AntiCheatStore_RepeatedOccupation_ExactlyOneSucceeds()
    {
        _db.InitTenantTables(typeof(TenantGameAntiCheatEventEntity));

        // 顺序重复占位：仅首次成功（并发恰一由 ux_anticheat_idempotency 唯一索引在库层仲裁，
        // SQLite 测试库为单连接不支持多线程并发，真并发验证依赖 PG 环境）
        var results = new List<bool>();
        for (var i = 0; i < 8; i++)
        {
            var store = new AntiCheatEventIdempotencyStore(_db.Repository<TenantGameAntiCheatEventEntity>(), BuildAntiCheatTemplate("anticheat-key-concurrent"));
            results.Add(await store.TryBeginProcessingAsync(BuildRecord(AntiCheatEventIdempotencyStore.IdempotencyScope, "anticheat-key-concurrent")));
        }

        Assert.Single(results, succeeded => succeeded);
        var rows = await _db.TenantDb.Queryable<TenantGameAntiCheatEventEntity>().Where(u => u.IdempotencyKey == "anticheat-key-concurrent").ToListAsync();
        Assert.Single(rows);
    }

    [Fact]
    public async Task AntiCheatStore_ExistingEvent_ReplaysAsCompleted()
    {
        _db.InitTenantTables(typeof(TenantGameAntiCheatEventEntity));

        var store = new AntiCheatEventIdempotencyStore(_db.Repository<TenantGameAntiCheatEventEntity>(), BuildAntiCheatTemplate("anticheat-key-replay"));
        Assert.True(await store.TryBeginProcessingAsync(BuildRecord(AntiCheatEventIdempotencyStore.IdempotencyScope, "anticheat-key-replay")));

        var record = await store.TryGetRecordAsync(new IdempotencyRecordIdentifier(AntiCheatEventIdempotencyStore.IdempotencyScope, "anticheat-key-replay"));
        Assert.NotNull(record);
        Assert.Equal(IdempotencyStatus.Completed, record!.Status);

        // 重复占位：既有事件恒为已完成，占位失败（Replay）
        Assert.False(await store.TryBeginProcessingAsync(BuildRecord(AntiCheatEventIdempotencyStore.IdempotencyScope, "anticheat-key-replay")));
    }

    [Fact]
    public async Task AntiCheatStore_FailAsync_RemovesOccupationRow()
    {
        _db.InitTenantTables(typeof(TenantGameAntiCheatEventEntity));

        var store = new AntiCheatEventIdempotencyStore(_db.Repository<TenantGameAntiCheatEventEntity>(), BuildAntiCheatTemplate("anticheat-key-fail"));
        Assert.True(await store.TryBeginProcessingAsync(BuildRecord(AntiCheatEventIdempotencyStore.IdempotencyScope, "anticheat-key-fail")));

        await store.FailAsync(new IdempotencyFailureRequest(new IdempotencyRecordIdentifier(AntiCheatEventIdempotencyStore.IdempotencyScope, "anticheat-key-fail"), 0));

        Assert.Null(await store.TryGetRecordAsync(new IdempotencyRecordIdentifier(AntiCheatEventIdempotencyStore.IdempotencyScope, "anticheat-key-fail")));
    }

    [Fact]
    public async Task AdvertisementRewardStore_ClaimsPendingRow_AndBlocksReOccupation()
    {
        _db.InitTenantTables(typeof(TenantGameAdvertisementRewardEntity));

        var store = new AdvertisementRewardLedgerIdempotencyStore(_db.Repository<TenantGameAdvertisementRewardEntity>(), BuildAdvertisementRewardTemplate("adreward-key-claim"));
        Assert.True(await store.TryBeginProcessingAsync(BuildRecord(AdvertisementRewardLedgerIdempotencyStore.IdempotencyScope, "adreward-key-claim")));

        // Pending 占用中：占位失败 → 协调器判 Busy / 等待
        Assert.False(await store.TryBeginProcessingAsync(BuildRecord(AdvertisementRewardLedgerIdempotencyStore.IdempotencyScope, "adreward-key-claim")));

        var record = await store.TryGetRecordAsync(new IdempotencyRecordIdentifier(AdvertisementRewardLedgerIdempotencyStore.IdempotencyScope, "adreward-key-claim"));
        Assert.Equal(IdempotencyStatus.Processing, record!.Status);
    }

    [Fact]
    public async Task AdvertisementRewardStore_FailedRow_AllowsLegalReOccupation()
    {
        _db.InitTenantTables(typeof(TenantGameAdvertisementRewardEntity));

        var store = new AdvertisementRewardLedgerIdempotencyStore(_db.Repository<TenantGameAdvertisementRewardEntity>(), BuildAdvertisementRewardTemplate("adreward-key-retry"));
        Assert.True(await store.TryBeginProcessingAsync(BuildRecord(AdvertisementRewardLedgerIdempotencyStore.IdempotencyScope, "adreward-key-retry")));
        await store.FailAsync(new IdempotencyFailureRequest(new IdempotencyRecordIdentifier(AdvertisementRewardLedgerIdempotencyStore.IdempotencyScope, "adreward-key-retry"), 0));

        // 法定覆盖：存量 Failed 重试复用同一行（翻回 Pending）
        Assert.True(await store.TryBeginProcessingAsync(BuildRecord(AdvertisementRewardLedgerIdempotencyStore.IdempotencyScope, "adreward-key-retry")));

        var rows = await _db.TenantDb.Queryable<TenantGameAdvertisementRewardEntity>().Where(u => u.IdempotencyKey == "adreward-key-retry").ToListAsync();
        Assert.Single(rows);
        Assert.Equal(AdvertisementRewardStatusEnum.Pending, rows[0].Status);
    }

    [Fact]
    public async Task AdvertisementRewardStore_CompleteSettlesSuccess_AndReplaysAsCompleted()
    {
        _db.InitTenantTables(typeof(TenantGameAdvertisementRewardEntity));
        var identifier = new IdempotencyRecordIdentifier(AdvertisementRewardLedgerIdempotencyStore.IdempotencyScope, "adreward-key-complete");

        var store = new AdvertisementRewardLedgerIdempotencyStore(_db.Repository<TenantGameAdvertisementRewardEntity>(), BuildAdvertisementRewardTemplate("adreward-key-complete"));
        Assert.True(await store.TryBeginProcessingAsync(BuildRecord(identifier.IdempotencyScope, identifier.IdempotencyKey)));
        await store.CompleteAsync(new IdempotencyCompletionRequest(identifier, ReadOnlyMemory<byte>.Empty, 0));

        var record = await store.TryGetRecordAsync(identifier);
        Assert.Equal(IdempotencyStatus.Completed, record!.Status);

        // 已发放：占位失败（Replay 口径）
        Assert.False(await store.TryBeginProcessingAsync(BuildRecord(identifier.IdempotencyScope, identifier.IdempotencyKey)));
    }

    [Fact]
    public async Task AdvertisementRewardStore_SettleWithoutRecord_Throws()
    {
        _db.InitTenantTables(typeof(TenantGameAdvertisementRewardEntity));

        var store = new AdvertisementRewardLedgerIdempotencyStore(_db.Repository<TenantGameAdvertisementRewardEntity>(), BuildAdvertisementRewardTemplate("adreward-key-missing"));
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => store.CompleteAsync(new IdempotencyCompletionRequest(new IdempotencyRecordIdentifier(AdvertisementRewardLedgerIdempotencyStore.IdempotencyScope, "adreward-key-missing"), ReadOnlyMemory<byte>.Empty, 0)));
    }

    [Fact]
    public async Task FulfillmentStore_ClaimsPendingRow_FailedReOccupation_AndVoidedMapsCompleted()
    {
        _db.InitTenantTables(typeof(TenantGameOrderFulfillmentEntity));

        var store = new FulfillmentLedgerIdempotencyStore(_db.TenantDb, BuildFulfillmentTemplate("fulfillment-key-claim"));
        Assert.True(await store.TryBeginProcessingAsync(BuildRecord(FulfillmentLedgerIdempotencyStore.OrderShipIdempotencyScope, "fulfillment-key-claim")));

        // Pending 占用中：占位失败（协调器等待后回忙碌）
        Assert.False(await store.TryBeginProcessingAsync(BuildRecord(FulfillmentLedgerIdempotencyStore.OrderShipIdempotencyScope, "fulfillment-key-claim")));

        // 落定失败 → 法定覆盖：Failed 行复用翻回 Pending（单行）
        var identifier = new IdempotencyRecordIdentifier(FulfillmentLedgerIdempotencyStore.OrderShipIdempotencyScope, "fulfillment-key-claim");
        await store.FailAsync(new IdempotencyFailureRequest(identifier, 0));
        Assert.True(await store.TryBeginProcessingAsync(BuildRecord(identifier.IdempotencyScope, identifier.IdempotencyKey)));
        var rows = await _db.TenantDb.Queryable<TenantGameOrderFulfillmentEntity>().Where(u => u.IdempotencyKey == "fulfillment-key-claim").ToListAsync();
        Assert.Single(rows);
        Assert.Equal(FulfillmentStatusEnum.Pending, rows[0].Status);

        // 落定成功 → Completed 映射 + 回放占位失败
        await store.CompleteAsync(new IdempotencyCompletionRequest(identifier, ReadOnlyMemory<byte>.Empty, 0));
        var record = await store.TryGetRecordAsync(identifier);
        Assert.Equal(IdempotencyStatus.Completed, record!.Status);
        Assert.False(await store.TryBeginProcessingAsync(BuildRecord(identifier.IdempotencyScope, identifier.IdempotencyKey)));
    }

    [Fact]
    public async Task FulfillmentStore_VoidedTerminal_MapsCompleted_AndBlocksReOccupation()
    {
        _db.InitTenantTables(typeof(TenantGameOrderFulfillmentEntity));

        var store = new FulfillmentLedgerIdempotencyStore(_db.TenantDb, BuildFulfillmentTemplate("fulfillment-key-voided"));
        Assert.True(await store.TryBeginProcessingAsync(BuildRecord(FulfillmentLedgerIdempotencyStore.OrderRefundIdempotencyScope, "fulfillment-key-voided")));

        // 作废为业务终态：服务侧落 Voided 后 Complete 做存在性校验（不抛错）
        await _db.TenantDb.Updateable<TenantGameOrderFulfillmentEntity>()
                 .SetColumns(u => u.Status == FulfillmentStatusEnum.Voided)
                 .Where(u => u.IdempotencyKey == "fulfillment-key-voided")
                 .ExecuteCommandAsync();
        await store.CompleteAsync(new IdempotencyCompletionRequest(new IdempotencyRecordIdentifier(FulfillmentLedgerIdempotencyStore.OrderRefundIdempotencyScope, "fulfillment-key-voided"), ReadOnlyMemory<byte>.Empty, 0));

        var record = await store.TryGetRecordAsync(new IdempotencyRecordIdentifier(FulfillmentLedgerIdempotencyStore.OrderRefundIdempotencyScope, "fulfillment-key-voided"));
        Assert.Equal(IdempotencyStatus.Completed, record!.Status);
        Assert.False(await store.TryBeginProcessingAsync(BuildRecord(FulfillmentLedgerIdempotencyStore.OrderRefundIdempotencyScope, "fulfillment-key-voided")));
    }

    private static IdempotencyRecord BuildRecord(string scope, string key)
    {
        var now = TimerHelper.UnixTimeMilliseconds();
        return new IdempotencyRecord(scope, key, "digest-" + key, IdempotencyStatus.Processing, null, now, now + 86_400_000L, null);
    }

    private static TenantGameAntiCheatEventEntity BuildAntiCheatTemplate(string idempotencyKey)
    {
        return new TenantGameAntiCheatEventEntity
        {
            AppId = 1081001,
            Provider = AntiCheatProviderEnum.Custom,
            ProviderName = "test",
            ProviderEventId = "event-1",
            TargetType = AntiCheatTargetTypeEnum.Role,
            TargetId = "role-1",
            RiskLevel = RiskLevelEnum.Medium,
            EvidenceSummary = "test evidence",
            SuggestedAction = AntiCheatSuggestedActionEnum.None,
            SuggestedActionNote = string.Empty,
            OccurredTime = TimerHelper.UnixTimeMilliseconds(),
            IdempotencyKey = idempotencyKey,
            CreatedName = "test",
            ReviewDecision = string.Empty,
            ReviewNote = string.Empty,
        };
    }

    private static TenantGameAdvertisementRewardEntity BuildAdvertisementRewardTemplate(string idempotencyKey)
    {
        return new TenantGameAdvertisementRewardEntity
        {
            AppId = 1081001,
            Provider = "TopOn",
            TransactionId = "tx-" + idempotencyKey,
            AdvertisementUnit = "unit-1",
            RoleId = string.Empty,
            UserId = 0,
            AreaId = 0,
            RewardItem = "coin",
            RewardAmount = "10",
            Status = AdvertisementRewardStatusEnum.Pending,
            IdempotencyKey = idempotencyKey,
#pragma warning disable CS8625 // 实体 FailureReason 声明非空但 DB 列允许 NULL；测试模板默认 NULL
            FailureReason = null,
#pragma warning restore CS8625
            IsSandbox = false,
            CreatedTime = TimerHelper.UnixTimeSeconds(),
        };
    }

    private static TenantGameOrderFulfillmentEntity BuildFulfillmentTemplate(string idempotencyKey)
    {
        var now = TimerHelper.UnixTimeSeconds();
        return new TenantGameOrderFulfillmentEntity
        {
            TenantId = 1080001,
            AppId = 1081001,
            OrderId = 9001,
            PayChannel = "google",
            ProviderTransactionId = "provider-tx-1",
            Sku = 30001,
            RoleId = "20001",
            AreaId = 10001,
            AccountId = "account-1",
            Amount = 100,
            ItemsJson = "[{\"itemId\":1,\"count\":10}]",
            Type = FulfillmentTypeEnum.Purchase,
            Status = FulfillmentStatusEnum.Pending,
            IdempotencyKey = idempotencyKey,
#pragma warning disable CS8601 // 实体 FailureReason 字段在 DB schema 实际允许 null；测试模板默认 NULL
            FailureReason = null,
#pragma warning restore CS8601
            OperatorId = 0,
            CreatedId = 0,
            CreatedTime = now,
            CreatedName = "system",
            UpdateCount = 0,
            UpdateTime = now,
            UpdatedId = 0,
            UpdatedName = "system",
            IsDeleted = false,
            IsEnabled = true,
            DeleteTime = 0,
            DeletedId = 0,
            DeletedName = string.Empty,
            RowVersion = 0,
        };
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
