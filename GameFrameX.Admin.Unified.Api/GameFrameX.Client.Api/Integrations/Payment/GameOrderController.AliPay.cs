using GameFrameX.Client.Api.Services.PublicCallback;
using Aop.Api.Util;
using GameFrameX.Client.Api.Services;
using GameFrameX.Entity.Enum;
using GameFrameX.Payment.AliPay;

namespace GameFrameX.Client.Api.Integrations.Payment;

public partial class GameOrderController
{
    /// <summary>
    /// 支付宝异步通知
    /// </summary>
    /// <returns></returns>
    [PublicCallbackEndpoint("alipay", "payment")]
    [HttpPost("{tenantId:long}/{appId:long}/{packageId:long?}")]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> NotifyAliPay()
    {
        var data = GetAliPayNotifyParameters();
        LogHelper.InfoConsole($"支付宝回调信息 - {JsonHelper.Serialize(data)}");

        var aliPayOptionSetting = _paymentChannelFactory.GetOptionSettings("alipay")
                                                        .OfType<AliPayOptionSetting>()
                                                        .FirstOrDefault(setting => VerifyAliPayCallbackSign(data, setting));
        if (aliPayOptionSetting == null)
        {
            LogHelper.Warning($"支付宝支付配置未找到或回调签名验证失败 - {JsonHelper.Serialize(data)}");
            return GameApiResult.Error<BaseOperationAckResponse>(AuthErrorCode.SignatureVerifyFailed, "签名验证失败");
        }

        var tradeStatus = GetAliPayNotifyValue(data, "trade_status");
        if (tradeStatus == "TRADE_SUCCESS" || tradeStatus == "TRADE_FINISHED")
        {
            var outTradeNo = GetAliPayNotifyValue(data, "out_trade_no");
            if (!long.TryParse(outTradeNo, out var orderId))
            {
                LogHelper.Warning($"支付宝回调订单号无效 - {outTradeNo}");
                return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var httpJsonResult = HttpJsonResultData<BaseOperationAckResponse>.Success();
            var gameOrderEntity = await PRepository.GetSingleAsync(m => m.Id == orderId, cts.Token);
            if (gameOrderEntity == null)
            {
                LogHelper.InfoConsole($"订单信息找不到 - {orderId}");
                return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
            }

            // 金额校验：AliPay 回调 total_amount(元) 换算为分后必须与订单 Amount(分) 一致，防止伪造金额
            var totalAmountStr = GetAliPayNotifyValue(data, "total_amount");
            if (!decimal.TryParse(totalAmountStr, out var totalAmountYuan))
            {
                LogHelper.Warning($"支付宝回调金额格式无效 - orderId:{orderId}, total_amount:{totalAmountStr}");
                Response.StatusCode = 400;
                return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
            }

            var expectedAmountFen = (int)Math.Round(totalAmountYuan * 100m);
            if (expectedAmountFen != (int)gameOrderEntity.Amount)
            {
                LogHelper.Warning($"支付宝回调金额不匹配 - orderId:{orderId}, 订单:{gameOrderEntity.Amount}分, 回调:{totalAmountStr}元({expectedAmountFen}分)");
                Response.StatusCode = 400;
                return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
            }

            var gameRoleEntity = await _gameRoleService.GetAsync(m => m.Id == long.Parse(gameOrderEntity.RoleId));
            if (gameRoleEntity == null)
            {
                LogHelper.InfoConsole($"用户角色ID不存在 - {orderId}");
                return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
            }

            gameOrderEntity.OrderId = orderId.ToString();
            if (gameOrderEntity.Status == OrderStatusEnum.Unpaid)
            {
                // 仅首次支付时落库，避免重复回调把状态回写成可重新发货
                gameOrderEntity.Status = OrderStatusEnum.NotShipped;
                gameOrderEntity.ProviderTransactionId = GetAliPayNotifyValue(data, "trade_no");
                var affectedRows = await PRepository.AsUpdateable(gameOrderEntity)
                                                    .UpdateColumns(u => new
                                                    {
                                                        u.OrderId,
                                                        u.Status,
                                                        u.ProviderTransactionId,
                                                    })
                                                    .Where(u => u.Id == gameOrderEntity.Id && u.Status == OrderStatusEnum.Unpaid)
                                                    .ExecuteCommandAsync(cts.Token);
                if (affectedRows <= 0)
                {
                    // P0-03 并发回调已被认领（Status 已流转），幂等返回避免重复发货
                    LogHelper.Info($"支付宝订单已被并发回调认领，跳过发货 - orderId:{orderId}");
                    return HttpJsonResultData<BaseOperationAckResponse>.Success();
                }
            }

            // 发货
            httpJsonResult = await SendOutGoods(gameOrderEntity, true);
            return httpJsonResult;
        }

        return HttpJsonResultData<BaseOperationAckResponse>.Success();
    }

    /// <summary>
    /// 验证支付宝回调签名
    /// 使用支付宝官方 RSA/RSA2 签名算法验证回调参数的完整性
    /// </summary>
    private bool VerifyAliPayCallbackSign(IDictionary<string, string> data, AliPayOptionSetting aliPayOptionSetting)
    {
        if (!data.TryGetValue("sign", out var sign) || string.IsNullOrWhiteSpace(sign))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(aliPayOptionSetting.AliPayPublicKey))
        {
            LogHelper.Warning("支付宝公钥未配置，无法验证回调签名");
            return false;
        }

        var signType = GetAliPayNotifyValue(data, "sign_type");
        if (string.IsNullOrWhiteSpace(signType))
        {
            signType = aliPayOptionSetting.AliPaySignType;
        }

        return AlipaySignature.RSACheckV1(
            data,
            aliPayOptionSetting.AliPayPublicKey,
            aliPayOptionSetting.AliPayCharset,
            signType,
            false);
    }

    private Dictionary<string, string> GetAliPayNotifyParameters()
    {
        if (Request.HasFormContentType)
        {
            return Request.Form.ToDictionary(x => x.Key, x => x.Value.ToString());
        }

        return Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString());
    }

    private static string GetAliPayNotifyValue(IDictionary<string, string> data, string key)
    {
        return data.TryGetValue(key, out var value) ? value : string.Empty;
    }
}