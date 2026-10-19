using System.Collections.Concurrent;
using GameFrameX.Application.Game.Order.GameOrder.Dto;
using GameFrameX.Application.Game.ServerManager;
using GameFrameX.Entity.Enum;
using GameFrameX.Foundation.Http.Extension;
using GameFrameX.Foundation.Http.Normalization;
using GameFrameX.Foundation.Idempotency;
using GameFrameX.Foundation.Json;

namespace GameFrameX.Application.Game.Order.GameOrder;

/// <summary>
/// 订单发货服务（Admin API 和 Client API 共享）。
/// <para>幂等判定统一走 Foundation 协调器 + 发货账本表即 Store（<see cref="FulfillmentLedgerIdempotencyStore"/>）：
/// 占位先行——账本行（Pending）先于游戏服调用创建，同一发货/退款意图在并发与重试下恰好执行一次，
/// 重复请求回放首次结果（Success 口径）。订单 Status 前置守卫保留领域完整性语义；进程内订单锁仅作
/// 同订单操作串行化。站点规则见 Store 类备注。</para>
/// </summary>
public class TenantGameOrderShipService
{
    private static readonly ConcurrentDictionary<long, SemaphoreSlim> OrderLocks = new();
    private readonly ISqlSugarClient _db;
    private readonly IHttpClientFactory _httpClientFactory;

    public TenantGameOrderShipService(ISqlSugarClient db, IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// 执行发货
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="isRecharge">是否充值发货</param>
    /// <param name="isSandBox">是否沙盒发货</param>
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> ShipAsync(long orderId, bool isRecharge, bool isSandBox = false)
    {
        var orderLock = OrderLocks.GetOrAdd(orderId, _ => new SemaphoreSlim(1, 1));
        await orderLock.WaitAsync();
        try
        {
            var gameOrderEntity = await GetOrderAsync(orderId);

            // 领域前置守卫：未支付抛错；已到终态（含存量订单无占位行）按既定口径返回成功，不建占位行
            if (gameOrderEntity.Status == OrderStatusEnum.Unpaid)
            {
                throw new Exception("订单未支付");
            }

            if (gameOrderEntity.Status != OrderStatusEnum.NotShipped)
            {
                return HttpJsonResultData<BaseOperationAckResponse>.Success();
            }

            var idempotencyContext = CreateFulfillmentCoordinator(gameOrderEntity, FulfillmentTypeEnum.Purchase, 0, FulfillmentLedgerIdempotencyStore.OrderShipIdempotencyScope);
            var decision = await idempotencyContext.Coordinator.BeginAsync(new IdempotencyBeginRequest(FulfillmentLedgerIdempotencyStore.OrderShipIdempotencyScope, idempotencyContext.IdempotencyKey, FulfillmentLedgerIdempotencyStore.RequestDigest));
            if (decision.DecisionKind == IdempotencyDecisionKind.Replay)
            {
                // 同一发货意图已执行（含跨进程并发输家等到首次结果）：回放首次结果，不再发货
                return HttpJsonResultData<BaseOperationAckResponse>.Success();
            }

            if (decision.DecisionKind == IdempotencyDecisionKind.Busy)
            {
                return HttpJsonResultData<BaseOperationAckResponse>.Fail("订单处理中，请稍后重试");
            }

            try
            {
                var sendResult = await SendItemsAsync(gameOrderEntity, isRecharge, isSandBox);
                if (sendResult.IsSuccess)
                {
                    await UpdateStatusAsync(gameOrderEntity.Id, OrderStatusEnum.Shipped);
                    // 发货成功（=支付确认）后累加角色累充：恰一递增由占位权保证（同意图仅执行一次到此处）
                    await AddRoleAllRechargeAsync(sendResult.RoleId, gameOrderEntity.Amount);
                    await idempotencyContext.Coordinator.CompleteAsync(new IdempotencyCompletionRequest(idempotencyContext.Identifier, ReadOnlyMemory<byte>.Empty, 0));
                    return HttpJsonResultData<BaseOperationAckResponse>.Success();
                }

                await WriteFailureReasonAsync(idempotencyContext.IdempotencyKey, sendResult.Message);
                await idempotencyContext.Coordinator.FailAsync(new IdempotencyFailureRequest(idempotencyContext.Identifier, 0));
                return HttpJsonResultData<BaseOperationAckResponse>.Fail($"发货失败: {sendResult.Message}");
            }
            catch (Exception)
            {
                // 执行异常：占位行落定失败（ReExecute 允许重试），异常原样上抛
                await idempotencyContext.Coordinator.FailAsync(new IdempotencyFailureRequest(idempotencyContext.Identifier, 0));
                throw;
            }
        }
        finally
        {
            orderLock.Release();
        }
    }

    /// <summary>
    /// 执行退款后的游戏服扣除
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="reason">退款原因</param>
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> RefundAsync(long orderId, string reason = "")
    {
        var orderLock = OrderLocks.GetOrAdd(orderId, _ => new SemaphoreSlim(1, 1));
        await orderLock.WaitAsync();
        try
        {
            var gameOrderEntity = await GetOrderAsync(orderId);

            // 领域前置守卫：已退款/已作废按既定口径返回成功
            if (gameOrderEntity.Status is OrderStatusEnum.Refunded or OrderStatusEnum.Voided)
            {
                return HttpJsonResultData<BaseOperationAckResponse>.Success();
            }

            var idempotencyContext = CreateFulfillmentCoordinator(gameOrderEntity, FulfillmentTypeEnum.Refund, 0, FulfillmentLedgerIdempotencyStore.OrderRefundIdempotencyScope);
            var decision = await idempotencyContext.Coordinator.BeginAsync(new IdempotencyBeginRequest(FulfillmentLedgerIdempotencyStore.OrderRefundIdempotencyScope, idempotencyContext.IdempotencyKey, FulfillmentLedgerIdempotencyStore.RequestDigest));
            if (decision.DecisionKind == IdempotencyDecisionKind.Replay)
            {
                return HttpJsonResultData<BaseOperationAckResponse>.Success();
            }

            if (decision.DecisionKind == IdempotencyDecisionKind.Busy)
            {
                return HttpJsonResultData<BaseOperationAckResponse>.Fail("订单处理中，请稍后重试");
            }

            try
            {
                if (gameOrderEntity.Status is OrderStatusEnum.Unpaid or OrderStatusEnum.NotShipped)
                {
                    await UpdateStatusAsync(gameOrderEntity.Id, OrderStatusEnum.Voided);
                    // 作废为业务终态（无实际扣除）：服务先行落 Voided，Complete 做存在性校验
                    await SettleVoidedAsync(idempotencyContext.IdempotencyKey);
                    await idempotencyContext.Coordinator.CompleteAsync(new IdempotencyCompletionRequest(idempotencyContext.Identifier, ReadOnlyMemory<byte>.Empty, 0));
                    return HttpJsonResultData<BaseOperationAckResponse>.Success();
                }

                var gameAreaEntity = await GetGameAreaAsync(gameOrderEntity);
                var roleId = await GetRoleIdAsync(gameOrderEntity);
                var playerCreateOrderResult = GetPlayerCreateOrderResult(gameOrderEntity);
                var itemsJson = JsonHelper.Serialize(playerCreateOrderResult.RewardsItem, EmbeddedJsonOptions.Instance);

                var request = new PlayerRefundOrderRequest
                {
                    AccountId = gameOrderEntity.AccountId ?? string.Empty,
                    ProductId = gameOrderEntity.Sku,
                    OrderId = gameOrderEntity.OrderId ?? gameOrderEntity.Id.ToString(),
                    RoleId = roleId,
                    IsRecharge = true,
                    IsSandbox = gameOrderEntity.IsSandbox ?? false,
                    Items = playerCreateOrderResult.RewardsItem,
                    Amount = gameOrderEntity.Amount,
                    Channel = gameOrderEntity.Channel ?? string.Empty,
                    PayChannel = gameOrderEntity.PayChannel ?? string.Empty,
                    Platform = gameOrderEntity.Platform ?? string.Empty,
                    Reason = reason ?? string.Empty,
                };

                var httpClient = _httpClientFactory.CreateClient();
                var areaAddress = GameServerEndpointUrlBuilder.Build(gameAreaEntity, GameServerEndpointKindEnum.Fulfillment, "req_player_refund_order");
                var content = await httpClient.PostJsonToStringAsync(areaAddress, request);
                var respPlayerRefund = content.ToHttpJsonResult<PlayerRefundOrderResponse>();
                if (respPlayerRefund.IsSuccess)
                {
                    await UpdateStatusAsync(gameOrderEntity.Id, OrderStatusEnum.Refunded);
                    await idempotencyContext.Coordinator.CompleteAsync(new IdempotencyCompletionRequest(idempotencyContext.Identifier, ReadOnlyMemory<byte>.Empty, 0));
                    return HttpJsonResultData<BaseOperationAckResponse>.Success();
                }

                // 失败原因同时并入游戏服返回 message 与退款原因，便于审计
                var failureReason = string.IsNullOrWhiteSpace(reason) ? respPlayerRefund.Message : $"{respPlayerRefund.Message} | reason={reason}";
                await WriteFailureReasonAsync(idempotencyContext.IdempotencyKey, failureReason);
                await idempotencyContext.Coordinator.FailAsync(new IdempotencyFailureRequest(idempotencyContext.Identifier, 0));
                return HttpJsonResultData<BaseOperationAckResponse>.Fail($"退款扣除失败: {respPlayerRefund.Message}");
            }
            catch (Exception)
            {
                await idempotencyContext.Coordinator.FailAsync(new IdempotencyFailureRequest(idempotencyContext.Identifier, 0));
                throw;
            }
        }
        finally
        {
            orderLock.Release();
        }
    }

    /// <summary>
    /// 人工补偿/重发。高风险人工操作入口，必须传入操作人 Id 用于审计。
    /// <para>按 Purchase 发货链路重新调一次游戏服发货；CompensateAsync 幂等键每次不同（含 Guid），
    /// 每次调用是独立意图、不做跨次去重，但每次都有独立审计记录；经协调器走统一机制（占位先行）并落定。</para>
    /// 不改订单 Status（订单可能已 Shipped，人工补发是额外发）。
    /// </summary>
    /// <param name="orderId">订单ID</param>
    /// <param name="reason">补偿原因（落账本 FailureReason 仅在失败时）</param>
    /// <param name="operatorId">操作人 Id，必须 &gt; 0；0 表示系统自动调用，被拒绝</param>
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> CompensateAsync(long orderId, string reason, long operatorId)
    {
        if (operatorId <= 0)
        {
            throw new Exception("人工补偿必须指定操作人");
        }

        var orderLock = OrderLocks.GetOrAdd(orderId, _ => new SemaphoreSlim(1, 1));
        await orderLock.WaitAsync();
        try
        {
            var gameOrderEntity = await GetOrderAsync(orderId);

            if (gameOrderEntity.Status == OrderStatusEnum.Unpaid)
            {
                throw new Exception("订单未支付");
            }

            var idempotencyContext = CreateFulfillmentCoordinator(gameOrderEntity, FulfillmentTypeEnum.Compensate, operatorId, FulfillmentLedgerIdempotencyStore.OrderCompensateIdempotencyScope);
            await idempotencyContext.Coordinator.BeginAsync(new IdempotencyBeginRequest(FulfillmentLedgerIdempotencyStore.OrderCompensateIdempotencyScope, idempotencyContext.IdempotencyKey, FulfillmentLedgerIdempotencyStore.RequestDigest));

            var sendResult = await SendItemsAsync(gameOrderEntity, true, false);
            if (sendResult.IsSuccess)
            {
                await idempotencyContext.Coordinator.CompleteAsync(new IdempotencyCompletionRequest(idempotencyContext.Identifier, ReadOnlyMemory<byte>.Empty, 0));
                return HttpJsonResultData<BaseOperationAckResponse>.Success();
            }

            var failureReason = string.IsNullOrWhiteSpace(reason) ? sendResult.Message : $"{sendResult.Message} | reason={reason}";
            await WriteFailureReasonAsync(idempotencyContext.IdempotencyKey, failureReason);
            await idempotencyContext.Coordinator.FailAsync(new IdempotencyFailureRequest(idempotencyContext.Identifier, 0));
            return HttpJsonResultData<BaseOperationAckResponse>.Fail($"人工补偿失败: {sendResult.Message}");
        }
        finally
        {
            orderLock.Release();
        }
    }

    /// <summary>
    /// 创建发货链路的协调器 + 账本表即 Store（占位模板携带订单快照全字段）。
    /// </summary>
    private FulfillmentIdempotencyContext CreateFulfillmentCoordinator(
        Entity.Tenant.Game.TenantGameOrderEntity order, FulfillmentTypeEnum type, long operatorId, string idempotencyScope)
    {
        var idempotencyKey = BuildFulfillmentIdempotencyKey(type, order, operatorId);
        var template = BuildFulfillmentTemplate(order, type, GetItemsJson(order), operatorId);
        var store = new FulfillmentLedgerIdempotencyStore(_db, template);
        return new FulfillmentIdempotencyContext
        {
            Coordinator = new IdempotencyCoordinator(store, SystemClock.Instance, FulfillmentLedgerIdempotencyStore.CreateCoordinatorOptions()),
            Identifier = new IdempotencyRecordIdentifier(idempotencyScope, idempotencyKey),
            IdempotencyKey = idempotencyKey,
        };
    }

    /// <summary>
    /// 构造占位账本模板（订单快照全字段；并发恰一由账本唯一索引在库层仲裁）。
    /// </summary>
    private static TenantGameOrderFulfillmentEntity BuildFulfillmentTemplate(
        Entity.Tenant.Game.TenantGameOrderEntity order, FulfillmentTypeEnum type, string itemsJson, long operatorId)
    {
        var now = TimerHelper.UnixTimeSeconds();
        var operatorName = operatorId > 0 ? "admin" : "system";

        return new TenantGameOrderFulfillmentEntity
        {
            TenantId = order.TenantId,
            AppId = order.AppId,
            OrderId = order.Id,
            PayChannel = order.PayChannel ?? string.Empty,
            ProviderTransactionId = order.ProviderTransactionId,
            Sku = order.Sku,
            RoleId = order.RoleId,
            AreaId = order.AreaId,
            AccountId = order.AccountId ?? string.Empty,
            Amount = order.Amount,
            ItemsJson = itemsJson ?? string.Empty,
            Type = type,
            Status = FulfillmentStatusEnum.Pending,
            IdempotencyKey = string.Empty,
#pragma warning disable CS8601 // 实体 FailureReason 字段在 DB schema 实际允许 null（= null! 仅满足 CS8618），运行时接受 null
            FailureReason = null,
#pragma warning restore CS8601
            OperatorId = operatorId,
            CreatedId = operatorId,
            CreatedTime = now,
            CreatedName = operatorName,
            UpdateCount = 0,
            UpdateTime = now,
            UpdatedId = operatorId,
            UpdatedName = operatorName,
            IsDeleted = false,
            IsEnabled = true,
            DeleteTime = 0,
            DeletedId = 0,
            DeletedName = string.Empty,
            RowVersion = 0,
        };
    }

    /// <summary>
    /// 写入账本失败原因（业务事实，服务侧职责；状态列翻转由 Store 落定）。
    /// </summary>
    private async Task WriteFailureReasonAsync(string idempotencyKey, string failureReason)
    {
        await _db.Updateable<Entity.Tenant.Game.TenantGameOrderFulfillmentEntity>()
                 .SetColumns(u => u.FailureReason == failureReason)
                 .Where(u => u.IdempotencyKey == idempotencyKey)
                 .ExecuteCommandAsync();
    }

    /// <summary>
    /// 落定作废终态（未实际发货的退款：无实际权益发放或扣除）。
    /// </summary>
    private async Task SettleVoidedAsync(string idempotencyKey)
    {
        await _db.Updateable<Entity.Tenant.Game.TenantGameOrderFulfillmentEntity>()
                 .SetColumns(u => u.Status == FulfillmentStatusEnum.Voided)
                 .SetColumns(u => u.FailureReason == null)
                 .Where(u => u.IdempotencyKey == idempotencyKey)
                 .ExecuteCommandAsync();
    }

    /// <summary>
    /// 调游戏服 req_player_send_item 发货。ShipAsync 与 CompensateAsync 共用。
    /// </summary>
    private async Task<SendItemOutcome> SendItemsAsync(Entity.Tenant.Game.TenantGameOrderEntity gameOrderEntity, bool isRecharge, bool forceSandbox)
    {
        var gameAreaEntity = await GetGameAreaAsync(gameOrderEntity);
        var roleId = await GetRoleIdAsync(gameOrderEntity);
        var playerCreateOrderResult = GetPlayerCreateOrderResult(gameOrderEntity);
        var itemsJson = JsonHelper.Serialize(playerCreateOrderResult.RewardsItem, EmbeddedJsonOptions.Instance);

        var request = new PlayerSendItemRequest
        {
            AccountId = gameOrderEntity.AccountId ?? string.Empty,
            ProductId = gameOrderEntity.Sku,
            PlayerId = roleId,
            IsRecharge = isRecharge,
            IsSandbox = forceSandbox || (gameOrderEntity.IsSandbox ?? false),
            Items = playerCreateOrderResult.RewardsItem,
            Amount = gameOrderEntity.Amount,
            Channel = gameOrderEntity.Channel ?? string.Empty,
            PayChannel = gameOrderEntity.PayChannel ?? string.Empty,
            Platform = gameOrderEntity.Platform ?? string.Empty,
        };

        var httpClient = _httpClientFactory.CreateClient();
        var areaAddress = GameServerEndpointUrlBuilder.Build(gameAreaEntity, GameServerEndpointKindEnum.Fulfillment, "req_player_send_item");
        var content = await httpClient.PostJsonToStringAsync(areaAddress, request);
        var respPlayerCreate = content.ToHttpJsonResult<PlayerSendItemResponse>();

        return new SendItemOutcome
        {
            IsSuccess = respPlayerCreate.IsSuccess,
            Message = respPlayerCreate.Message,
            ItemsJson = itemsJson,
            RoleId = roleId,
        };
    }

    private static string GetItemsJson(Entity.Tenant.Game.TenantGameOrderEntity gameOrderEntity)
    {
        var playerCreateOrderResult = GetPlayerCreateOrderResult(gameOrderEntity);
        return JsonHelper.Serialize(playerCreateOrderResult.RewardsItem, EmbeddedJsonOptions.Instance);
    }

    /// <summary>
    /// 构造权益发货幂等键。规则见 change C113 §3。
    /// </summary>
    private static string BuildFulfillmentIdempotencyKey(FulfillmentTypeEnum type, Entity.Tenant.Game.TenantGameOrderEntity order, long operatorId)
    {
        var channel = order.PayChannel ?? string.Empty;
        var providerTx = !string.IsNullOrWhiteSpace(order.ProviderTransactionId)
                             ? order.ProviderTransactionId
                             : $"order{order.Id}";

        switch (type)
        {
            case FulfillmentTypeEnum.Purchase:
                return $"purchase:{channel}:{providerTx}";
            case FulfillmentTypeEnum.Refund:
                return $"refund:{channel}:{providerTx}";
            case FulfillmentTypeEnum.Compensate:
            {
                var guidPart = Guid.NewGuid().ToString("N").Substring(0, 12);
                return $"compensate:{order.Id}:{operatorId}:{guidPart}";
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, "不支持的发放类型");
        }
    }

    private async Task<Entity.Tenant.Game.TenantGameOrderEntity> GetOrderAsync(long orderId)
    {
        return await _db.Queryable<Entity.Tenant.Game.TenantGameOrderEntity>().FirstAsync(u => u.Id == orderId)
               ?? throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
    }

    private async Task<Entity.Tenant.Game.TenantGameAreaEntity> GetGameAreaAsync(Entity.Tenant.Game.TenantGameOrderEntity gameOrderEntity)
    {
        return await _db.Queryable<Entity.Tenant.Game.TenantGameAreaEntity>().Filter(null, true).Where(u => u.IsDeleted == false).FirstAsync(u => u.Id == gameOrderEntity.AreaId)
               ?? throw new Exception("区服不存在");
    }

    private async Task<long> GetRoleIdAsync(Entity.Tenant.Game.TenantGameOrderEntity gameOrderEntity)
    {
        if (!long.TryParse(gameOrderEntity.RoleId, out var roleId))
        {
            throw new Exception("角色ID无效");
        }

        var gameRoleEntity = await _db.Queryable<Entity.Tenant.Game.TenantGameRoleEntity>().FirstAsync(u => u.Id == roleId);
        if (gameRoleEntity == null)
        {
            throw new Exception("角色不存在");
        }

        return roleId;
    }

    private static PlayerCreateOrderResponse GetPlayerCreateOrderResult(Entity.Tenant.Game.TenantGameOrderEntity gameOrderEntity)
    {
        if (string.IsNullOrWhiteSpace(gameOrderEntity.Content))
        {
            throw new Exception("订单内容为空");
        }

        return JsonHelper.Deserialize<PlayerCreateOrderResponse>(gameOrderEntity.Content)
               ?? throw new Exception("订单内容解析失败");
    }

    private async Task UpdateStatusAsync(long orderId, OrderStatusEnum status)
    {
        await _db.Updateable<Entity.Tenant.Game.TenantGameOrderEntity>()
                 .SetColumns(u => u.Status == status)
                 .Where(u => u.Id == orderId)
                 .ExecuteCommandAsync();
    }

    /// <summary>
    /// 发货成功后累加角色累充（AllRecharge）。使用数据库侧累加并显式限定角色 Id，
    /// 避免依赖 SqlSugar 自动识别实体主键，也避免同角色多订单并发时读改写丢失。
    /// </summary>
    private async Task AddRoleAllRechargeAsync(long roleId, uint amount)
    {
        var amountInt = (int)amount;
        if (amountInt <= 0)
        {
            return;
        }

        await _db.Updateable<Entity.Tenant.Game.TenantGameRoleEntity>()
                 .SetColumns(u => u.AllRecharge == (u.AllRecharge ?? 0) + amountInt)
                 .Where(u => u.Id == roleId)
                 .ExecuteCommandAsync();
    }

    /// <summary>
    /// 发货链路幂等上下文（协调器 + 标识 + 键）。
    /// </summary>
    private sealed class FulfillmentIdempotencyContext
    {
        public IdempotencyCoordinator Coordinator { get; init; } = null!;

        public IdempotencyRecordIdentifier Identifier { get; init; } = null!;

        public string IdempotencyKey { get; init; } = null!;
    }

    private sealed class SendItemOutcome
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; } = null!;

        public string ItemsJson { get; set; } = null!;

        public long RoleId { get; set; }
    }
}
