using GameFrameX.Client.Api.Services.PayVerification;
using System.Globalization;
using GameFrameX.Entity.Tenant.ClientApiConfig;
using GameFrameX.Client.Api.GameUserPay.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Client.Api.Services.ClientApiTenantOptions;
using GameFrameX.Core.Service;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Client.Api.GameUserPay;

/// <summary>
/// 游戏玩家支付控制器（C11 支付域：Create/Verify/Query/Report 四 op 自建）
/// <para>路由：POST /api/GameUserPay/{Action}（继承 BaseController 基类路由）。</para>
/// <para>App 级鉴权：AppId 事实数据取自 <see cref="ClientApiAppContext"/>（中间件上下文优先，
/// 缺失时内联读 AppId 头。本 host 的 AppResolutionMiddleware 仅在 App-Id + App-Secret 双头齐全时才校验拦截，
/// 游戏侧请求仅带 AppId 头、走头回退解析，App 级鉴权由签名中间件兜底）。</para>
/// </summary>
public class GameUserPayController : BaseController
{
    /// <summary>
    /// 对外单号序号（同毫秒并发下单不撞）
    /// </summary>
    private static long _orderSeq;

    private readonly Repository<TenantGameOrderEntity> _payOrderRepository;
    private readonly IClientApiTenantOptionsProvider _optionsProvider;
    private readonly IPayReceiptVerifier _payReceiptVerifier;

    /// <summary>
    ///
    /// </summary>
    /// <param name="payOrderRepository">支付订单仓储</param>
    /// <param name="optionsProvider">租户配置 Provider（沙箱充值发货开关，C378 起读策略合并表）</param>
    /// <param name="payReceiptVerifier">支付收据验真器</param>
    public GameUserPayController(Repository<TenantGameOrderEntity> payOrderRepository,
        IClientApiTenantOptionsProvider optionsProvider,
        IPayReceiptVerifier payReceiptVerifier)
    {
        _payOrderRepository = payOrderRepository;
        _optionsProvider = optionsProvider;
        _payReceiptVerifier = payReceiptVerifier;
    }

    /// <summary>
    /// 支付下单（生成对外单号落库，Status=Unpaid）。
    /// <para>沙箱判定服务端权威（防客户端伪造免支付白嫖）：读租户订单策略表 EnableSandBoxRecharge，
    /// 与 Client.Api 下单尾部同源同优先级（应用级 &gt; 租户默认）；命中 → Status=Shipped + IsNeedPay=false
    /// （沙箱直发闭环，免 Verify）；未命中 → IsNeedPay=true。</para>
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<HttpJsonResultData<GameUserPayCreateResponse>> Create([FromBody] GameUserPayCreateRequest request)
    {
        var appId = ClientApiAppContext.ResolveAppId(HttpContext);
        var enableSandbox = await IsSandboxRechargeEnabledAsync(appId);
        var entity = new TenantGameOrderEntity
        {
            AppId = appId,
            AccountId = request.UserId.ToString(CultureInfo.InvariantCulture),
            RoleId = request.UserId.ToString(CultureInfo.InvariantCulture),
            AreaId = 0,
            Sku = 0,
            ProductId = request.Sku,
            PayChannel = request.PayChannel.ToString(CultureInfo.InvariantCulture),
            Platform = request.Platform.ToString(CultureInfo.InvariantCulture),
            Channel = request.Channel?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            SubChannel = string.Empty,
            Country = request.Country!,
            Amount = ToOrderAmount(request.Amount),
            Content = string.Empty,
            AdventureSuffix = string.Empty,
            IsSandbox = enableSandbox,
            Status = enableSandbox ? OrderStatusEnum.Shipped : OrderStatusEnum.Unpaid,
            OrderId = CreateOrderId(),
        };
        await _payOrderRepository.InsertAsync(entity);
        return HttpJsonResultData<GameUserPayCreateResponse>.Success(new GameUserPayCreateResponse
        {
            OrderId = entity.OrderId,
            IsNeedPay = !enableSandbox,
            Amount = request.Amount,
            Sku = request.Sku,
        });
    }

    /// <summary>
    /// 支付验真（按 OrderId+UserId 查单；已 Shipped 幂等直接回 Rcode=0 防重放；订单不存在 Rcode=60）。
    /// <para>验真经 <see cref="IPayReceiptVerifier"/>：成功 → Status=Shipped + 回填 ProviderTransactionId；
    /// pending/可疑 → Rcode=7 不记账；渠道不支持/配置未加载 → 信封错误码拒绝，绝不静默放行。</para>
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<HttpJsonResultData<GameUserPayVerifyResponse>> Verify([FromBody] GameUserPayVerifyRequest request)
    {
        var accountId = request.UserId.ToString(CultureInfo.InvariantCulture);
        var order = await _payOrderRepository.GetFirstAsync(x => x.OrderId == request.OrderId && x.AccountId == accountId);
        if (order == null)
        {
            return HttpJsonResultData<GameUserPayVerifyResponse>.Success(new GameUserPayVerifyResponse
            {
                Rcode = 60,
                OrderId = request.OrderId,
                ProductId = request.ProductId ?? string.Empty,
            });
        }

        // 幂等重放：已到账订单直接回 0，不再跑验真（防重放刷发货）
        if (order.Status >= OrderStatusEnum.Shipped)
        {
            return HttpJsonResultData<GameUserPayVerifyResponse>.Success(new GameUserPayVerifyResponse
            {
                Rcode = 0,
                OrderId = order.OrderId,
                ProductId = GetProductId(order),
            });
        }

        // C271 跨订单幂等防重：同一 Apple 渠道交易号（TransactionId）已绑定其他订单并回填 ProviderTransactionId 时拒绝，
        // 防止同一笔渠道交易经 verifyReceipt / Server API 任一路径重复验真发货
        if (request.PayChannel == 100 && !string.IsNullOrEmpty(request.TransactionId))
        {
            var conflict = await _payOrderRepository.GetFirstAsync(x => x.ProviderTransactionId == request.TransactionId && x.Id != order.Id);
            if (conflict != null)
            {
                return GameApiResult.Error<GameUserPayVerifyResponse>(PaymentErrorCode.PayChannelNotSupported, $"Apple 交易已绑定其他订单，拒绝重复发货: transactionId={request.TransactionId}, boundOrderId={conflict.OrderId}");
            }
        }

        var verify = await _payReceiptVerifier.VerifyAsync(order, request);
        if (verify.State == GamePayVerifyState.Rejected)
        {
            return GameApiResult.Error<GameUserPayVerifyResponse>(PaymentErrorCode.PayChannelNotSupported, verify.Message);
        }

        if (verify.State == GamePayVerifyState.Succeeded)
        {
            order.Status = OrderStatusEnum.Shipped;
            order.ProviderTransactionId = verify.ProviderTransactionId;
            order.ReceiptJson = verify.ReceiptJson;
            await _payOrderRepository.UpdateAsync(order);
            return HttpJsonResultData<GameUserPayVerifyResponse>.Success(new GameUserPayVerifyResponse
            {
                Rcode = 0,
                OrderId = order.OrderId,
                ProductId = verify.ProductId.Length > 0 ? verify.ProductId : GetProductId(order),
            });
        }

        return HttpJsonResultData<GameUserPayVerifyResponse>.Success(new GameUserPayVerifyResponse
        {
            Rcode = 7,
            OrderId = order.OrderId,
            ProductId = request.ProductId ?? GetProductId(order),
        });
    }

    /// <summary>
    /// 支付订单查询（只读；OrderId 形态查单条 / ProductId 形态查该 UserId+商品最新一条；查无 data 全空 + code 0）
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<HttpJsonResultData<GameUserPayQueryResponse>> Query([FromBody] GameUserPayQueryRequest request)
    {
        TenantGameOrderEntity? order = null;
        var accountId = request.UserId.ToString(CultureInfo.InvariantCulture);
        if (!string.IsNullOrEmpty(request.OrderId))
        {
            order = await _payOrderRepository.GetFirstAsync(x => x.OrderId == request.OrderId && x.AccountId == accountId);
        }
        else if (!string.IsNullOrEmpty(request.ProductId))
        {
            var rows = await _payOrderRepository.GetListAsync(x => x.AccountId == accountId && x.ProductId == request.ProductId);
            order = rows.OrderByDescending(x => x.Id).FirstOrDefault();
        }

        if (order == null)
        {
            return HttpJsonResultData<GameUserPayQueryResponse>.Success(new GameUserPayQueryResponse());
        }

        return HttpJsonResultData<GameUserPayQueryResponse>.Success(new GameUserPayQueryResponse
        {
            OrderId = order.OrderId,
            ProductId = GetProductId(order),
            Status = (int)order.Status,
            Paid = order.Status >= OrderStatusEnum.Shipped,
            Amount = order.Amount,
        });
    }

    /// <summary>
    /// 支付流水上报（纯记账对账，不推进订单状态；订单不存在仍记录（LogWarning 容错）；
    /// 同 OrderId 重复上报幂等覆盖流水字段——最新数据胜）。响应 data 为 null。
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<HttpJsonResultData<object>> Report([FromBody] GameUserPayReportRequest request)
    {
        var order = await _payOrderRepository.GetFirstAsync(x => x.OrderId == request.OrderId);
        if (order == null)
        {
            LogHelper.Warning($"GameUserPay.Report 订单不存在，按流水记账 - OrderId:{request.OrderId}, UserId:{request.UserId}");
            order = new TenantGameOrderEntity
            {
                AppId = ClientApiAppContext.ResolveAppId(HttpContext),
                AccountId = request.UserId.ToString(CultureInfo.InvariantCulture),
                RoleId = request.UserId.ToString(CultureInfo.InvariantCulture),
                AreaId = 0,
                Sku = 0,
                ProductId = request.ProductId,
                PayChannel = request.PayChannel.ToString(CultureInfo.InvariantCulture),
                Platform = string.Empty,
                Channel = request.Channel?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                SubChannel = string.Empty,
                Country = string.Empty,
                Amount = ToOrderAmount(request.RealMoney),
                Content = string.Empty,
                AdventureSuffix = string.Empty,
                Status = OrderStatusEnum.Unpaid,
                OrderId = request.OrderId,
            };
            ApplyReport(order, request);
            await _payOrderRepository.InsertAsync(order);
        }
        else
        {
            ApplyReport(order, request);
            await _payOrderRepository.UpdateAsync(order);
        }

        return HttpJsonResultData<object>.Success(null!);
    }

    /// <summary>
    /// 沙箱充值发货开关（服务端权威）：经租户配置 Provider 读取策略合并表输出节，
    /// 作用域优先级 应用级（TenantId+AppId）&gt; 租户默认（AppId=0），与 Client.Api 下单尾部同源（C378 收敛）。
    /// </summary>
    private async Task<bool> IsSandboxRechargeEnabledAsync(long appId)
    {
        var policy = await _optionsProvider.GetOrderPolicyAsync();
        return policy.EnableSandBoxRecharge;
    }

    /// <summary>
    /// Report 侧流水字段回填（RealMoney/Currency/Level/StrScene）
    /// </summary>
    private static void ApplyReport(TenantGameOrderEntity order, GameUserPayReportRequest request)
    {
        order.RealMoney = request.RealMoney;
        order.Currency = request.Currency;
        order.Level = request.Level;
        order.StrScene = request.StrScene;
    }

    private static string CreateOrderId()
    {
        return "pay" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + Interlocked.Increment(ref _orderSeq);
    }

    private static uint ToOrderAmount(long? amount)
    {
        if (!amount.HasValue || amount.Value <= 0)
        {
            return 0;
        }

        return amount.Value > uint.MaxValue ? uint.MaxValue : (uint)amount.Value;
    }

    private static string GetProductId(TenantGameOrderEntity order)
    {
        if (!string.IsNullOrWhiteSpace(order.ProductId))
        {
            return order.ProductId;
        }

        return order.Sku > 0 ? order.Sku.ToString(CultureInfo.InvariantCulture) : string.Empty;
    }
}