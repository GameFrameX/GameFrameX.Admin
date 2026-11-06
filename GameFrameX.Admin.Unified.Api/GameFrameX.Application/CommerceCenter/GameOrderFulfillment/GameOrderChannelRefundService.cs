using System.Text.Json;
using GameFrameX.Core.Enum.Errors;
using GameFrameX.Core.Exceptions;
using GameFrameX.Entity.Business.Game.App;
using GameFrameX.Foundation.Logger;
using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.Abstract.Models;

namespace GameFrameX.Application.CommerceCenter.GameOrderFulfillment;

/// <summary>
/// 渠道退款路由服务：按订单支付渠道解析 <see cref="IRefundManager"/> 与主库支付配置，执行渠道侧退款。
/// <para>渠道退款必须先于本地账本扣除（<c>GameOrderShipService.RefundAsync</c>）成功执行：
/// 渠道失败 / 渠道不支持主动退款 / 渠道配置缺失时抛 <see cref="BusinessError"/>（错误码进 PaymentErrorCode 段 704），本地账本不动。</para>
/// <para>渠道配置来源：主库 GamePaymentConfigEntity 中对应支付渠道的基础分包配置，
/// IsSandbox 与订单匹配者优先；ConfigJson 经 <see cref="PaymentChannelRegistry"/> 反序列化为渠道 Option 类型。</para>
/// </summary>
public class GameOrderChannelRefundService
{
    private static readonly JsonSerializerOptions ConfigSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly IReadOnlyDictionary<string, IRefundManager> _refundManagers;
    private readonly ISqlSugarClient _db;
    private readonly IHttpClientFactory _httpClientFactory;

    public GameOrderChannelRefundService(IEnumerable<IRefundManager> refundManagers, ISqlSugarClient db, IHttpClientFactory httpClientFactory)
    {
        _refundManagers = refundManagers.ToDictionary(m => m.ChannelName, m => m, StringComparer.OrdinalIgnoreCase);
        _db = db;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// 执行渠道退款（全额）。订单已支付（非 Unpaid）时必须先于本地账本扣除调用。
    /// </summary>
    /// <param name="order">订单实体（PayChannel / ProviderTransactionId / Amount / IsSandbox 等路由与上下文来源）</param>
    /// <param name="reason">退款原因（管理员填写，透传渠道描述字段并落账本 FailureReason）</param>
    /// <exception cref="BusinessException">渠道未注册（PayChannelNotSupported）/ 配置缺失（RefundConfigMissing）/ 不支持主动退款（RefundNotSupported）/ 渠道返回失败（RefundChannelFailed）</exception>
    public async Task RefundByOrderAsync(Entity.Business.Game.GameOrderEntity order, string reason)
    {
        var channel = (order.PayChannel ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(channel) || !_refundManagers.TryGetValue(channel, out var refundManager))
        {
            throw BusinessError.Oh(PaymentErrorCode.PayChannelNotSupported, $"channel={order.PayChannel}");
        }

        var optionSetting = ResolveOptionSetting(channel, order);
        if (optionSetting == null)
        {
            throw BusinessError.Oh(PaymentErrorCode.RefundConfigMissing, $"channel={channel}, isSandbox={order.IsSandbox ?? false}");
        }

        if (!refundManager.SupportsProactiveRefund)
        {
            throw BusinessError.Oh(PaymentErrorCode.RefundNotSupported, $"channel={channel}");
        }

        var context = BuildContext(channel, order, reason);
        var result = await refundManager.RefundAsync(context, optionSetting, _httpClientFactory.CreateClient());
        if (!result.IsSuccess)
        {
            throw BusinessError.Oh(PaymentErrorCode.RefundChannelFailed, result.ErrorMessage);
        }
    }

    /// <summary>
    /// 查询渠道侧退款状态（按订单渠道路由，供管理员核对渠道退款进展）。
    /// </summary>
    public async Task<PaymentChannelRefundQueryResponse> QueryByOrderAsync(Entity.Business.Game.GameOrderEntity order)
    {
        var channel = (order.PayChannel ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(channel) || !_refundManagers.TryGetValue(channel, out var refundManager))
        {
            throw BusinessError.Oh(PaymentErrorCode.PayChannelNotSupported, $"channel={order.PayChannel}");
        }

        var optionSetting = ResolveOptionSetting(channel, order);
        if (optionSetting == null)
        {
            throw BusinessError.Oh(PaymentErrorCode.RefundConfigMissing, $"channel={channel}, isSandbox={order.IsSandbox ?? false}");
        }

        var context = BuildContext(channel, order, string.Empty);
        return await refundManager.QueryRefundAsync(context, optionSetting, _httpClientFactory.CreateClient());
    }

    /// <summary>
    /// 从主库解析渠道基础分包支付配置，IsSandbox 与订单匹配者优先。
    /// </summary>
    private IPaymentOptionSetting? ResolveOptionSetting(string channel, Entity.Business.Game.GameOrderEntity order)
    {
        var isSandbox = order.IsSandbox ?? false;
        var configs = _db.Queryable<GamePaymentConfigEntity>()
                         .Where(u => u.PayChannel == channel && u.IsEnabled == true)
                         .Where(u => u.PackageIdNormalized == 0)
                         .OrderByDescending(u => u.Id)
                         .ToList();

        GamePaymentConfigEntity? sandboxMatched = null;
        GamePaymentConfigEntity? environmentFallback = null;
        foreach (var config in configs)
        {
            if (string.IsNullOrWhiteSpace(config.ConfigJson))
            {
                continue;
            }

            if (config.IsSandbox == isSandbox)
            {
                sandboxMatched ??= config;
            }
            else
            {
                environmentFallback ??= config;
            }
        }

        var best = sandboxMatched ?? environmentFallback;
        if (best == null)
        {
            return null;
        }

        var optionType = PaymentChannelRegistry.GetOptionType(channel);
        if (optionType == null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize(best.ConfigJson, optionType, ConfigSerializerOptions) as IPaymentOptionSetting;
        }
        catch (JsonException ex)
        {
            LogHelper.Error($"渠道支付配置JSON解析失败 - channel:{channel}, error:{ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 构造渠道退款上下文：渠道交易号按渠道挑选（Apple 优先 originalTransactionId，其余用 ProviderTransactionId）。
    /// </summary>
    private static RefundOrderContext BuildContext(string channel, Entity.Business.Game.GameOrderEntity order, string reason)
    {
        var transactionId = order.ProviderTransactionId ?? string.Empty;
        if (string.Equals(channel, "apple", StringComparison.Ordinal))
        {
            var appleTransactionId = string.IsNullOrWhiteSpace(order.AppleOriginalTransactionId) ? order.AppleTransactionId : order.AppleOriginalTransactionId;
            transactionId = string.IsNullOrWhiteSpace(appleTransactionId) ? transactionId : appleTransactionId;
        }

        return new RefundOrderContext
        {
            TransactionId = transactionId,
            OutTradeNo = string.IsNullOrWhiteSpace(order.OrderId) ? order.Id.ToString() : order.OrderId,
            Amount = order.Amount,
            Currency = order.Currency ?? string.Empty,
            Description = string.IsNullOrWhiteSpace(reason) ? "管理后台发起退款" : reason,
            IsSandbox = order.IsSandbox ?? false,
        };
    }
}
