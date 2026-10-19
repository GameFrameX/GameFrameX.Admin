using GameFrameX.Client.Api.Services.PublicCallback;
using System.Text;
using System.Text.Json;
using GameFrameX.Client.Api.Integrations.Payment.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Entity.Enum;
using GameFrameX.Payment.Xsolla.Option;

namespace GameFrameX.Client.Api.Integrations.Payment;

/// <summary>
///     程序版本控制器
/// </summary>
public partial class GameOrderController
{
    /// <summary>
    /// 通知发货
    /// 文档地址：https://developers.xsolla.com/zh/webhooks/overview/#section/Webhook
    /// </summary>
    /// <returns></returns>
    [PublicCallbackEndpoint("xsolla", "payment")]
    [HttpPost("{tenantId:long}/{appId:long}/{packageId:long?}")]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> NotifyXsolla()
    {
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            var body = await reader.ReadToEndAsync();
            LogHelper.Info("收到Xsolla 支付通知-->" + body);
            var authorization = Request.Headers.Authorization;
            if (authorization.Count <= 0)
            {
                // 签名校验失败
                Response.StatusCode = 400;
                LogHelper.InfoConsole("签名校验失败--没有签名");
                return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
            }

            var signature = authorization[0]?.Replace("Signature ", string.Empty).Trim();

            var xsollaPayOptionSetting = _paymentChannelFactory.GetOptionSettings("xsolla")
                                                               .OfType<XsollaPayOptionSetting>()
                                                               .FirstOrDefault(setting =>
                                                                                   !string.IsNullOrWhiteSpace(setting.XsollaPaymentWebHookSecretKey) &&
                                                                                   Sha1Helper.VerifyHash($"{body}{setting.XsollaPaymentWebHookSecretKey}", signature, Encoding.ASCII));
            if (xsollaPayOptionSetting == null)
            {
                Response.StatusCode = 400;
                LogHelper.InfoConsole("签名校验失败--没有可用的Xsolla支付配置或签名不匹配");
                return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
            }

            var xsollaWebHook = JsonHelper.Deserialize<XsollaWebHookRequest>(body);
            if (xsollaWebHook != null)
            {
                switch (xsollaWebHook.NotificationType)
                {
                    // 用户验证
                    // https://developers.xsolla.com/zh/webhooks/operation/user-validation/
                    case "user_validation":
                    {
                        var userValidationInput = JsonHelper.Deserialize<XsollaWebHookUserValidationRequest>(body);
                        if (userValidationInput != null && long.TryParse(userValidationInput.User.Id, out var userId))
                        {
                            // 用户验证
                            var gameRoleEntity = await _gameRoleService.GetAsync(m => m.Id == userId);
                            if (gameRoleEntity == null)
                            {
                                // 用户角色ID不存在
                                Response.StatusCode = 400;
                                LogHelper.InfoConsole("用户角色ID不存在");
                                return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                            }
                        }
                        else
                        {
                            // 用户角色ID不存在
                            Response.StatusCode = 400;
                            LogHelper.InfoConsole("用户角色ID不存在");
                            return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                        }
                    }
                        break;
                    // 游戏服务 支付成功
                    case "order_paid":
                    {
                        var xsollaWebHookOrderPaidInput = JsonHelper.Deserialize<XsollaWebHookOrderPaidRequest>(body);
                        if (xsollaWebHookOrderPaidInput != null && long.TryParse(xsollaWebHookOrderPaidInput.User.ExternalId, out var userId))
                        {
                            // 用户验证
                            var gameRoleEntity = await _gameRoleService.GetAsync(m => m.Id == userId);
                            if (gameRoleEntity == null)
                            {
                                // 用户角色ID不存在
                                Response.StatusCode = 400;
                                LogHelper.InfoConsole($"用户角色ID不存在 -{userId}");
                                return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                            }

                            var gameOrderEntity = await PRepository.GetSingleAsync(m => m.OrderId == xsollaWebHookOrderPaidInput.Order.Id.ToString());
                            if (gameOrderEntity == null)
                            {
                                // 订单信息找不到
                                Response.StatusCode = 400;
                                LogHelper.InfoConsole($"订单信息找不到 -{xsollaWebHookOrderPaidInput.Order.Id}");
                                return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                            }

                            // 金额校验：Xsolla order.amount(元) 换算为分后必须与订单 Amount(分) 一致，防止伪造金额
                            if (!decimal.TryParse(xsollaWebHookOrderPaidInput.Order.Amount, out var xsollaAmountYuan))
                            {
                                Response.StatusCode = 400;
                                LogHelper.Warning($"Xsolla回调金额格式无效 - orderId:{gameOrderEntity.Id}, amount:{xsollaWebHookOrderPaidInput.Order.Amount}");
                                return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                            }

                            var xsollaExpectedFen = (int)Math.Round(xsollaAmountYuan * 100m);
                            if (xsollaExpectedFen != (int)gameOrderEntity.Amount)
                            {
                                Response.StatusCode = 400;
                                LogHelper.Warning($"Xsolla回调金额不匹配 - orderId:{gameOrderEntity.Id}, 订单:{gameOrderEntity.Amount}分, 回调:{xsollaWebHookOrderPaidInput.Order.Amount}元({xsollaExpectedFen}分)");
                                return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                            }

                            // 先设置为未发货
                            if (gameOrderEntity.Status == OrderStatusEnum.Unpaid)
                            {
                                gameOrderEntity.Status = OrderStatusEnum.NotShipped;
                                gameOrderEntity.ProviderTransactionId = xsollaWebHookOrderPaidInput.Order.Id.ToString();
                                var affectedRows = await PRepository.AsUpdateable(gameOrderEntity)
                                                                    .UpdateColumns(u => new
                                                                    {
                                                                        u.OrderId,
                                                                        u.Status,
                                                                        u.ProviderTransactionId,
                                                                    })
                                                                    .Where(u => u.Id == gameOrderEntity.Id && u.Status == OrderStatusEnum.Unpaid)
                                                                    .ExecuteCommandAsync();
                                if (affectedRows <= 0)
                                {
                                    // P0-03 并发回调已被认领（Status 已流转），幂等返回避免重复发货
                                    LogHelper.InfoConsole($"Xsolla订单已被并发回调认领，跳过发货 - orderId:{xsollaWebHookOrderPaidInput.Order.Id}");
                                    return HttpJsonResultData<BaseOperationAckResponse>.Success();
                                }
                            }

                            // 检查是否是沙盒模式 
                            if (xsollaWebHookOrderPaidInput.Order.Mode != "default")
                            {
                                // 沙盒模式
                                // 检查账号是否是沙盒用户
                                if (gameRoleEntity.IsSandbox is true)
                                {
                                    // 是沙盒用户
                                    return await SendOutGoods(gameOrderEntity, true, true);
                                }

                                // 不是沙盒用户
                                Response.StatusCode = 400;
                                LogHelper.InfoConsole($"不是沙盒用户 -{xsollaWebHookOrderPaidInput.Order.Id}");
                                return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                            }

                            // 发货
                            return await SendOutGoods(gameOrderEntity, true);
                        }

                        // 用户角色ID不存在
                        Response.StatusCode = 400;
                        LogHelper.InfoConsole($"用户角色ID不存在 -{xsollaWebHookOrderPaidInput?.User.ExternalId}");
                        return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                    }
                    // 游戏服务 支付取消
                    case "order_canceled":
                    case "order_cancelled":
                    {
                        return await HandleXsollaCancellationAsync(body, xsollaWebHook.NotificationType);
                    }
                    // 订阅 付款
                    case "payment":
                    // 取消订阅时
                    case "cancel_subscription":
                    // 当用户创建订阅时
                    case "create_subscription":
                    // 当订阅状态设置为非续订时
                    case "non_renewal_subscription":
                    // 如果订阅中的某些参数（plan_id、date_next_charge）发生更改，以及每次订阅续订时
                    case "update_subscription":
                    {
                        return HandleUnsupportedXsollaSubscriptionNotification(body, xsollaWebHook.NotificationType);
                    }
                    // 退款
                    case "refund":
                    {
                        var refundOrderId = TryGetXsollaOrderId(body);
                        var refundExternalId = TryGetXsollaExternalId(body);
                        if (!string.IsNullOrWhiteSpace(refundOrderId))
                        {
                            var gameOrderEntity = await PRepository.GetSingleAsync(m => m.OrderId == refundOrderId);
                            if (gameOrderEntity != null)
                            {
                                return await _shipService.RefundAsync(gameOrderEntity.Id, $"Xsolla refund: externalId={refundExternalId}");
                            }
                        }
                    }
                        break;
                }
            }
        }

        return HttpJsonResultData<BaseOperationAckResponse>.Success();
    }

    private async Task<HttpJsonResultData<BaseOperationAckResponse>> HandleXsollaCancellationAsync(string body, string notificationType)
    {
        var orderId = TryGetXsollaOrderId(body);
        var externalId = TryGetXsollaExternalId(body);
        if (string.IsNullOrWhiteSpace(orderId))
        {
            LogHelper.Warning($"Xsolla取消通知缺少订单号 - type:{notificationType}, externalId:{externalId}");
            return HttpJsonResultData<BaseOperationAckResponse>.Success();
        }

        var gameOrderEntity = await PRepository.GetSingleAsync(m => m.OrderId == orderId);
        if (gameOrderEntity == null)
        {
            LogHelper.Warning($"Xsolla取消通知未找到订单 - type:{notificationType}, orderId:{orderId}, externalId:{externalId}");
            return HttpJsonResultData<BaseOperationAckResponse>.Success();
        }

        return await _shipService.RefundAsync(gameOrderEntity.Id, $"Xsolla {notificationType}: externalId={externalId}");
    }

    private static HttpJsonResultData<BaseOperationAckResponse> HandleUnsupportedXsollaSubscriptionNotification(string body, string notificationType)
    {
        if (!PaymentWebhookPolicy.IsXsollaSubscriptionNotification(notificationType))
        {
            return HttpJsonResultData<BaseOperationAckResponse>.Success();
        }

        var orderId = TryGetXsollaOrderId(body);
        var externalId = TryGetXsollaExternalId(body);
        LogHelper.Warning($"Xsolla订阅通知暂未接入权益模型，已显式跳过 - type:{notificationType}, orderId:{orderId}, externalId:{externalId}");
        return HttpJsonResultData<BaseOperationAckResponse>.Success();
    }

    private static string TryGetXsollaOrderId(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("order", out var orderElement) &&
                orderElement.TryGetProperty("id", out var idElement))
            {
                return idElement.ValueKind switch
                {
                    JsonValueKind.Number when idElement.TryGetInt64(out var longValue) => longValue.ToString(),
                    JsonValueKind.String                                               => idElement.GetString() ?? string.Empty,
                    _                                                                  => string.Empty,
                };
            }
        }
        catch
        {
            // ignore
        }

        return string.Empty;
    }

    private static string TryGetXsollaExternalId(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("transaction", out var transactionElement) &&
                transactionElement.TryGetProperty("external_id", out var externalIdElement))
            {
                return externalIdElement.GetString() ?? string.Empty;
            }
        }
        catch
        {
            // ignore
        }

        return string.Empty;
    }
}