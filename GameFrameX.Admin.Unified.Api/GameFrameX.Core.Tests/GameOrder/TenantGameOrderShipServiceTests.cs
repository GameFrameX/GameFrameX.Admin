using GameFrameX.Application.Game.Order.GameOrder;
using GameFrameX.Core.Enum;
using GameFrameX.Application.Game.Order.GameOrder.Dto;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Foundation.Http.Normalization;
using GameFrameX.Foundation.Json;
using SqlSugar;

namespace GameFrameX.Core.Tests.GameOrder;

public class TenantGameOrderShipServiceTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"game_order_ship_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;
    private readonly RecordingHttpMessageHandler _handler = new();
    private readonly TenantGameOrderShipService _service;

    public TenantGameOrderShipServiceTests()
    {
        _db = new SqlSugarScope(new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
            InitKeyType = InitKeyType.Attribute,
            IsAutoCloseConnection = true,
        });
        _db.CodeFirst.InitTables<TenantGameAreaEntity, TenantGameRoleEntity, TenantGameOrderEntity, TenantGameOrderFulfillmentEntity>();
        _service = new TenantGameOrderShipService(_db, new TestHttpClientFactory(_handler));
    }

    [Fact]
    public async Task ShipAsync_WhenCalledTwice_SendsItemsOnlyOnce()
    {
        var orderId = await SeedOrderAsync(OrderStatusEnum.NotShipped);

        var first = await _service.ShipAsync(orderId, true);
        var second = await _service.ShipAsync(orderId, true);

        var order = await _db.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.Id == orderId);
        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(OrderStatusEnum.Shipped, order.Status);
        Assert.Equal(1, _handler.Count("/game/api/req_player_send_item"));
        Assert.Equal(0, _handler.Count("/game/api/req_player_refund_order"));

        var fulfillments = await FulfillmentsAsync(orderId);
        Assert.Single(fulfillments);
        Assert.Equal(FulfillmentTypeEnum.Purchase, fulfillments[0].Type);
        Assert.Equal(FulfillmentStatusEnum.Success, fulfillments[0].Status);
        Assert.Null(fulfillments[0].FailureReason);
        Assert.Equal(0, fulfillments[0].OperatorId);
    }

    [Fact]
    public async Task ShipAsync_WhenGameServerFails_RecordsFailedWithReasonAndRecoversOnRetry()
    {
        var orderId = await SeedOrderAsync(OrderStatusEnum.NotShipped);

        _handler.EnqueueFail("/game/api/req_player_send_item");
        var failed = await _service.ShipAsync(orderId, true);
        Assert.False(failed.IsSuccess);

        var afterFail = await _db.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.Id == orderId);
        Assert.Equal(OrderStatusEnum.NotShipped, afterFail.Status);

        var failedRows = await FulfillmentsAsync(orderId);
        Assert.Single(failedRows);
        Assert.Equal(FulfillmentStatusEnum.Failed, failedRows[0].Status);
        Assert.False(string.IsNullOrWhiteSpace(failedRows[0].FailureReason));

        var recovered = await _service.ShipAsync(orderId, true);
        Assert.True(recovered.IsSuccess);

        var order = await _db.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.Id == orderId);
        Assert.Equal(OrderStatusEnum.Shipped, order.Status);

        var finalRows = await FulfillmentsAsync(orderId);
        Assert.Single(finalRows);
        Assert.Equal(FulfillmentStatusEnum.Success, finalRows[0].Status);
        Assert.Null(finalRows[0].FailureReason);
        Assert.Equal(2, _handler.Count("/game/api/req_player_send_item"));
    }

    [Fact]
    public async Task RefundAsync_WhenOrderIsShipped_DeductsOnlyOnce()
    {
        var orderId = await SeedOrderAsync(OrderStatusEnum.Shipped);

        var first = await _service.RefundAsync(orderId, "test");
        var second = await _service.RefundAsync(orderId, "test");

        var order = await _db.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.Id == orderId);
        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.Equal(OrderStatusEnum.Refunded, order.Status);
        Assert.Equal(1, _handler.Count("/game/api/req_player_refund_order"));

        var fulfillments = await FulfillmentsAsync(orderId);
        Assert.Single(fulfillments);
        Assert.Equal(FulfillmentTypeEnum.Refund, fulfillments[0].Type);
        Assert.Equal(FulfillmentStatusEnum.Success, fulfillments[0].Status);
    }

    [Theory]
    [InlineData(OrderStatusEnum.Unpaid)]
    [InlineData(OrderStatusEnum.NotShipped)]
    public async Task RefundAsync_WhenOrderIsNotDelivered_VoidsWithoutCallingGameServer(OrderStatusEnum status)
    {
        var orderId = await SeedOrderAsync(status);

        var result = await _service.RefundAsync(orderId, "test");

        var order = await _db.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.Id == orderId);
        Assert.True(result.IsSuccess);
        Assert.Equal(OrderStatusEnum.Voided, order.Status);
        Assert.Empty(_handler.RequestPaths);

        var fulfillments = await FulfillmentsAsync(orderId);
        Assert.Single(fulfillments);
        Assert.Equal(FulfillmentStatusEnum.Voided, fulfillments[0].Status);
    }

    [Fact]
    public async Task ShipAndRefundAsync_WhenConcurrent_DoesNotDeliverTwice()
    {
        var orderId = await SeedOrderAsync(OrderStatusEnum.NotShipped);

        await Task.WhenAll(
            _service.ShipAsync(orderId, true),
            _service.RefundAsync(orderId, "concurrent"));

        var order = await _db.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.Id == orderId);
        Assert.True(order.Status is OrderStatusEnum.Voided or OrderStatusEnum.Refunded);
        Assert.True(_handler.Count("/game/api/req_player_send_item") <= 1);
        Assert.True(_handler.Count("/game/api/req_player_refund_order") <= 1);
    }

    [Fact]
    public async Task CompensateAsync_WithOperator_RecordsCompensateWithOperatorId()
    {
        var orderId = await SeedOrderAsync(OrderStatusEnum.Shipped);

        var result = await _service.CompensateAsync(orderId, "测试补发", 42);

        var order = await _db.Queryable<TenantGameOrderEntity>().FirstAsync(u => u.Id == orderId);
        Assert.True(result.IsSuccess);
        // 人工补偿不改订单状态
        Assert.Equal(OrderStatusEnum.Shipped, order.Status);
        Assert.Equal(1, _handler.Count("/game/api/req_player_send_item"));

        var fulfillments = await FulfillmentsAsync(orderId);
        Assert.Single(fulfillments);
        Assert.Equal(FulfillmentTypeEnum.Compensate, fulfillments[0].Type);
        Assert.Equal(FulfillmentStatusEnum.Success, fulfillments[0].Status);
        Assert.Equal(42, fulfillments[0].OperatorId);
    }

    [Fact]
    public async Task CompensateAsync_CalledTwice_CreatesTwoDistinctAuditedRows()
    {
        var orderId = await SeedOrderAsync(OrderStatusEnum.Shipped);

        await _service.CompensateAsync(orderId, "测试补发", 42);
        await _service.CompensateAsync(orderId, "测试补发", 42);

        var fulfillments = await FulfillmentsAsync(orderId);
        // 人工补偿每次独立幂等键（含 Guid），每次新增审计行
        Assert.Equal(2, fulfillments.Count);
        Assert.All(fulfillments, f => Assert.Equal(42, f.OperatorId));
        Assert.All(fulfillments, f => Assert.Equal(FulfillmentTypeEnum.Compensate, f.Type));
    }

    [Fact]
    public async Task CompensateAsync_WithoutOperator_RejectsAndWritesNothing()
    {
        var orderId = await SeedOrderAsync(OrderStatusEnum.Shipped);

        var ex = await Record.ExceptionAsync(() => _service.CompensateAsync(orderId, "测试补发", 0));
        Assert.NotNull(ex);

        Assert.Empty(_handler.RequestPaths);
        var fulfillments = await FulfillmentsAsync(orderId);
        Assert.Empty(fulfillments);
    }

    private async Task<List<TenantGameOrderFulfillmentEntity>> FulfillmentsAsync(long orderId)
    {
        return await _db.Queryable<TenantGameOrderFulfillmentEntity>()
                        .Where(u => u.OrderId == orderId)
                        .ToListAsync();
    }

    private async Task<long> SeedOrderAsync(OrderStatusEnum status)
    {
        const long areaId = 10001;
        const long roleId = 20001;
        var orderId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (long)status;
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await _db.Insertable(new TenantGameAreaEntity
        {
            Id = areaId,
            TenantId = 1,
            CreatedId = 1,
            CreatedTime = now,
            CreatedName = "test",
            UpdateCount = 0,
            UpdateTime = now,
            UpdatedId = 1,
            UpdatedName = "test",
            IsDeleted = false,
            IsEnabled = true,
            DeleteTime = 0,
            DeletedId = 0,
            DeletedName = string.Empty,
            RowVersion = 0,
            Name = "test-area",
            Description = "test-area",
            HttpManageUrl = "https://game-server.test",
            TcpSocketUrl = "tcp://game-server.test",
            WebSocketUrl = "wss://game-server.test",
            CurrentLoad = 0,
            CreationTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            LastMaintenanceTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ServerVersion = "test",
            ServerType = "test",
            LocalizationKey = "test",
            AllowedPlayerProfile = "test",
            ServerEvents = "test",
            AccessControls = "test",
            ServerContainerId = "test",
            ServerParams = "test",
            AppVersion = "test",
        }).ExecuteCommandAsync();

        await _db.Insertable(new TenantGameRoleEntity
        {
            Id = roleId,
            TenantId = 1,
            CreatedId = 1,
            CreatedTime = now,
            CreatedName = "test",
            UpdateCount = 0,
            UpdateTime = now,
            UpdatedId = 1,
            UpdatedName = "test",
            IsDeleted = false,
            IsEnabled = true,
            DeleteTime = 0,
            DeletedId = 0,
            DeletedName = string.Empty,
            RowVersion = 0,
            Name = "test-role",
            Description = "test-role",
            UserId = 1,
            AreaId = areaId,
            Level = 1,
            IsSandbox = false,
            LastLoginTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            State = 0,
            Avatar = 0,
            CurrentExp = 0,
            CurrentLevelConfigId = 0,
            Gender = false,
            AvatarFrame = 0,
            AllRecharge = 0,
            OnlineTime = 0,
        }).ExecuteCommandAsync();

        await _db.Insertable(new TenantGameOrderEntity
        {
            Id = orderId,
            TenantId = 1,
            CreatedId = 1,
            CreatedTime = now,
            CreatedName = "test",
            UpdateCount = 0,
            UpdateTime = now,
            UpdatedId = 1,
            UpdatedName = "test",
            IsDeleted = false,
            IsEnabled = true,
            DeleteTime = 0,
            DeletedId = 0,
            DeletedName = string.Empty,
            RowVersion = 0,
            AreaId = areaId,
            RoleId = roleId.ToString(),
            Status = status,
            Amount = 100,
            Sku = 30001,
            OrderId = $"provider-{orderId}",
            AccountId = "account-1",
            Channel = "channel",
            PayChannel = "google",
            Platform = "android",
            IsSandbox = false,
            AdventureSuffix = string.Empty,
            RoleRegisterTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AppleTransactionId = string.Empty,
            AppleOriginalTransactionId = string.Empty,
            AppleReceiptEnvironment = string.Empty,
            AppleVerifiedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Content = JsonHelper.Serialize(new PlayerCreateOrderResponse
            {
                RoleId = roleId,
                Sku = 30001,
                Amount = 100,
                IsNeedPay = true,
                RewardsItem = new List<PropertyItemResponse>
                {
                    new() { Id = 1, ItemId = 1, Count = 10, },
                },
            }),
        }).ExecuteCommandAsync();

        return orderId;
    }

    public void Dispose()
    {
        _db.Dispose();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }

    private sealed class TestHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpMessageHandler _handler;

        public TestHttpClientFactory(HttpMessageHandler handler)
        {
            _handler = handler;
        }

        public HttpClient CreateClient(string name)
        {
            return new HttpClient(_handler, false);
        }
    }

    private sealed class RecordingHttpMessageHandler : HttpMessageHandler
    {
        private readonly List<string> _requestPaths = new();
        private readonly Queue<string> _forceFailQueue = new();

        public IReadOnlyList<string> RequestPaths
        {
            get { return _requestPaths; }
        }

        public void EnqueueFail(string path)
        {
            _forceFailQueue.Enqueue(path);
        }

        public int Count(string path)
        {
            return _requestPaths.Count(u => u == path);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            _requestPaths.Add(path);
            var forceFail = _forceFailQueue.Count > 0 && _forceFailQueue.Peek() == path && _forceFailQueue.Dequeue() == path;
            var payload = (path, forceFail) switch
            {
                ("/game/api/req_player_send_item", false)    => HttpJsonResultData<string>.Success(JsonHelper.Serialize(new PlayerSendItemResponse())).ToString(),
                ("/game/api/req_player_send_item", true)     => HttpJsonResultData<string>.Fail("simulated send failure").ToString(),
                ("/game/api/req_player_refund_order", false) => HttpJsonResultData<string>.Success(JsonHelper.Serialize(new PlayerRefundOrderResponse())).ToString(),
                ("/game/api/req_player_refund_order", true)  => HttpJsonResultData<string>.Fail("simulated refund failure").ToString(),
                _                                            => HttpJsonResultData<string>.NotFound().ToString(),
            };

            return Task.FromResult(new HttpResponseMessage(global::System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(payload),
            });
        }
    }
}