using GameFrameX.Client.Api.Services.PublicCallback;
using System.Text;
using GameFrameX.Client.Api.Services;
using GameFrameX.Entity.Enum;
using GameFrameX.Payment.Apple.AppStoreServer;
using GameFrameX.Payment.Apple.AppStoreServer.Models;
using GameFrameX.Payment.Apple.Option;

namespace GameFrameX.Client.Api.Integrations.Payment;

/// <summary>
///  订单控制器 - Apple Pay 相关
/// </summary>
public partial class GameOrderController
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    private static long GuidToLong(Guid guid)
    {
        // 获取 Guid 的字节数组
        var guidBytes = guid.ToByteArray();

        // 创建一个新的字节数组用于存储重新排列后的字节
        var valueBytes = new byte[8];

        // 根据 LongToGuidExact 中的映射关系，反向重新排列字节
        valueBytes[0] = guidBytes[3];
        valueBytes[1] = guidBytes[2];
        valueBytes[2] = guidBytes[1];
        valueBytes[3] = guidBytes[0];
        valueBytes[4] = guidBytes[5];
        valueBytes[5] = guidBytes[4];
        valueBytes[6] = guidBytes[7];
        valueBytes[7] = guidBytes[6];

        // 将重新排列后的字节数组转换为 long 值
        return BitConverter.ToInt64(valueBytes, 0);
    }

    /// <summary>
    /// 通知发货
    /// 文档地址：https://www.cnblogs.com/zhaoyingjie/p/18000958
    /// </summary>
    /// <returns></returns>
    [PublicCallbackEndpoint("apple", "payment")]
    [HttpPost("{tenantId:long}/{appId:long}/{packageId:long?}")]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> NotifyApple()
    {
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            var body = await reader.ReadToEndAsync();
            LogHelper.InfoConsole(body);
            try
            {
                if (string.IsNullOrWhiteSpace(body))
                {
                    Response.StatusCode = 400;
                    return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                }

                var applePayNotification = JsonHelper.Deserialize<ResponseBodyV2>(body);
                if (applePayNotification == null || string.IsNullOrWhiteSpace(applePayNotification.SignedPayload))
                {
                    Response.StatusCode = 400;
                    return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                }

                ApplePaymentOptionSetting? applePaymentOptionSetting = null;
                ResponseBodyV2DecodedPayload? applePayNotificationPayload = null;
                string? transactionPayloadJson = null;
                foreach (var candidate in _paymentChannelFactory.GetOptionSettings("apple").OfType<ApplePaymentOptionSetting>())
                {
                    if (string.IsNullOrWhiteSpace(candidate.ApplePaymentRootCertificate))
                    {
                        continue;
                    }

                    try
                    {
                        var notificationPayloadJson = AppleJwsVerifier.VerifyAndDecode(applePayNotification.SignedPayload, candidate.ApplePaymentRootCertificate);
                        var candidatePayload = JsonHelper.Deserialize<ResponseBodyV2DecodedPayload>(notificationPayloadJson);
                        if (candidatePayload?.Data == null || string.IsNullOrWhiteSpace(candidatePayload.Data.SignedTransactionInfo))
                        {
                            continue;
                        }

                        transactionPayloadJson = AppleJwsVerifier.VerifyAndDecode(candidatePayload.Data.SignedTransactionInfo, candidate.ApplePaymentRootCertificate);
                        applePaymentOptionSetting = candidate;
                        applePayNotificationPayload = candidatePayload;
                        break;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Warning($"Apple JWS 签名验证失败，继续尝试候选配置: {ex.Message}");
                    }
                }

                if (applePaymentOptionSetting == null || applePayNotificationPayload == null || string.IsNullOrWhiteSpace(transactionPayloadJson))
                {
                    LogHelper.Error("Apple支付配置未找到或 JWS 签名验证失败。请检查 tenantId/appId/env 对应的后台支付配置");
                    Response.StatusCode = 400;
                    return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                }

                LogHelper.Info("applePayNotificationPayload: " + JsonHelper.Serialize(applePayNotificationPayload));

                var transactionInfo = JsonHelper.Deserialize<JWSTransactionDecodedPayload>(transactionPayloadJson);
                if (transactionInfo == null)
                {
                    Response.StatusCode = 400;
                    return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                }

                LogHelper.Info("Transaction Info: " + JsonHelper.Serialize(transactionInfo));
                if (applePayNotificationPayload.NotificationType == NotificationTypeV2.ONE_TIME_CHARGE)
                {
                    var httpJsonResult = HttpJsonResultData<BaseOperationAckResponse>.Success();
                    // 处理一次性购买
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

                    if (string.IsNullOrWhiteSpace(transactionInfo.TransactionId) ||
                        string.IsNullOrWhiteSpace(transactionInfo.AppAccountToken) ||
                        string.IsNullOrWhiteSpace(transactionInfo.ProductId))
                    {
                        Response.StatusCode = 400;
                        return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                    }

                    if (!Guid.TryParse(transactionInfo.AppAccountToken, out var uuid))
                    {
                        Response.StatusCode = 400;
                        LogHelper.Warning($"Apple AppAccountToken 无效 - {transactionInfo.AppAccountToken}");
                        return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                    }

                    var orderId = GuidToLong(uuid);

                    var gameOrderEntity = await PRepository.GetSingleAsync(m => m.Id == orderId, cts.Token);
                    if (gameOrderEntity == null)
                    {
                        // 订单信息找不到
                        // Response.StatusCode = 400;
                        LogHelper.InfoConsole($"订单信息找不到 -{orderId}");
                        return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                    }

                    if (!string.Equals(gameOrderEntity.PayChannel, "apple", StringComparison.OrdinalIgnoreCase))
                    {
                        Response.StatusCode = 400;
                        LogHelper.Warning($"Apple交易订单支付渠道不匹配 - orderId:{orderId}, payChannel:{gameOrderEntity.PayChannel}");
                        return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                    }

                    if (gameOrderEntity.Sku.ToString() != transactionInfo.ProductId)
                    {
                        Response.StatusCode = 400;
                        LogHelper.Warning($"Apple交易商品不匹配 - orderId:{orderId}, sku:{gameOrderEntity.Sku}, productId:{transactionInfo.ProductId}");
                        return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                    }

                    var isSandboxReceipt = transactionInfo.Environment == ApplePayEnvironment.Sandbox;
                    if ((gameOrderEntity.IsSandbox ?? false) != isSandboxReceipt)
                    {
                        Response.StatusCode = 400;
                        LogHelper.Warning($"Apple交易环境不匹配 - orderId:{orderId}, orderSandbox:{gameOrderEntity.IsSandbox}, receiptSandbox:{isSandboxReceipt}");
                        return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                    }

                    var existingOrder = await PRepository.GetSingleAsync(m => m.AppleTransactionId == transactionInfo.TransactionId, cts.Token);
                    if (existingOrder != null && existingOrder.Id != gameOrderEntity.Id)
                    {
                        Response.StatusCode = 409;
                        LogHelper.Warning($"Apple交易已绑定其他订单 - transactionId:{transactionInfo.TransactionId}, existsOrderId:{existingOrder.Id}, currentOrderId:{gameOrderEntity.Id}");
                        return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                    }

                    var gameRoleEntity = await _gameRoleService.GetAsync(m => m.Id == long.Parse(gameOrderEntity.RoleId));
                    if (gameRoleEntity == null)
                    {
                        // 用户角色ID不存在
                        // Response.StatusCode = 400;
                        LogHelper.InfoConsole($"用户角色ID不存在 -{orderId}");
                        return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                    }

                    if (gameRoleEntity.IsSandbox != true && isSandboxReceipt)
                    {
                        Response.StatusCode = 400;
                        LogHelper.InfoConsole($"不是沙盒用户 -{orderId}");
                        return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                    }

                    if (gameOrderEntity.Status == OrderStatusEnum.Unpaid)
                    {
                        gameOrderEntity.OrderId = transactionInfo.TransactionId;
                        gameOrderEntity.Status = OrderStatusEnum.NotShipped;
                        gameOrderEntity.AppleTransactionId = transactionInfo.TransactionId;
                        gameOrderEntity.AppleOriginalTransactionId = transactionInfo.OriginalTransactionId;
                        gameOrderEntity.AppleReceiptEnvironment = transactionInfo.Environment?.ToString() ?? string.Empty;
                        gameOrderEntity.AppleVerifiedTime = TimerHelper.UnixTimeSeconds();

                        var affectedRows = await PRepository.AsUpdateable(gameOrderEntity)
                                                            .UpdateColumns(u => new
                                                            {
                                                                u.OrderId,
                                                                u.Status,
                                                                u.AppleTransactionId,
                                                                u.AppleOriginalTransactionId,
                                                                u.AppleReceiptEnvironment,
                                                                u.AppleVerifiedTime,
                                                            })
                                                            .Where(u => u.Id == gameOrderEntity.Id && u.Status == OrderStatusEnum.Unpaid)
                                                            .ExecuteCommandAsync(cts.Token);
                        if (affectedRows <= 0)
                        {
                            Response.StatusCode = 409;
                            LogHelper.Warning($"Apple订单状态更新失败 - orderId:{orderId}, transactionId:{transactionInfo.TransactionId}");
                            return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                        }
                    }
                    else if (!string.Equals(gameOrderEntity.AppleTransactionId, transactionInfo.TransactionId, StringComparison.OrdinalIgnoreCase))
                    {
                        Response.StatusCode = 409;
                        LogHelper.Warning($"Apple订单已处理过其他交易 - orderId:{orderId}, existsTransactionId:{gameOrderEntity.AppleTransactionId}, currentTransactionId:{transactionInfo.TransactionId}");
                        return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                    }

                    // 检查是否是沙盒模式 

                    if (isSandboxReceipt)
                    {
                        httpJsonResult = await SendOutGoods(gameOrderEntity, true, true);
                        await AppleConsumptionHandler(transactionInfo, applePaymentOptionSetting);
                        return httpJsonResult;
                    }


                    // 发货
                    httpJsonResult = await SendOutGoods(gameOrderEntity, true);
                    await AppleConsumptionHandler(transactionInfo, applePaymentOptionSetting);
                    return httpJsonResult;
                }

                if (applePayNotificationPayload.NotificationType == NotificationTypeV2.REFUND)
                {
                    var gameOrderEntity = await FindAppleOrderAsync(transactionInfo);
                    if (gameOrderEntity == null)
                    {
                        LogHelper.Warning($"Apple退款通知未找到订单 - transactionId:{transactionInfo.TransactionId}, originalTransactionId:{transactionInfo.OriginalTransactionId}");
                        return HttpJsonResultData<BaseOperationAckResponse>.Success();
                    }

                    return await _shipService.RefundAsync(gameOrderEntity.Id, $"Apple refund: revocationReason={transactionInfo.RevocationReason}, revocationType={transactionInfo.RevocationType}");
                }

                if (applePayNotificationPayload.NotificationType is NotificationTypeV2.REFUND_DECLINED or NotificationTypeV2.REFUND_REVERSED)
                {
                    LogHelper.Info($"Apple退款状态通知无需扣除 - type:{applePayNotificationPayload.NotificationType}, transactionId:{transactionInfo.TransactionId}");
                    return HttpJsonResultData<BaseOperationAckResponse>.Success();
                }
            }
            catch (OperationCanceledException)
            {
                LogHelper.Warning("订单验证超时");
                // P1-12 超时返回 5xx，触发 Apple App Store Server Notifications 重试，避免订单状态不一致
                Response.StatusCode = 500;
                return HttpJsonResultData<BaseOperationAckResponse>.Success();
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
                // P1-12 未预期异常返回 5xx，触发重试
                Response.StatusCode = 500;
                return HttpJsonResultData<BaseOperationAckResponse>.Success();
            }
        }

        return HttpJsonResultData<BaseOperationAckResponse>.Success();
    }

    private AppStoreServerApiClient GetAppStoreServerApiClientProduction(ApplePaymentOptionSetting applePaymentOptionSetting)
    {
        var keyId = applePaymentOptionSetting.ApplePaymentApiKeyId;
        var issuerId = applePaymentOptionSetting.ApplePaymentIssuerId;
        var bundleId = applePaymentOptionSetting.ApplePaymentBundleId;
        var privateKey = applePaymentOptionSetting.ApplePaymentPrivateKey;
        return new AppStoreServerApiClient(privateKey, keyId, issuerId, bundleId, ApplePayEnvironment.Production, _httpClient);
    }

    private AppStoreServerApiClient GetAppStoreServerApiClientSandbox(ApplePaymentOptionSetting applePaymentOptionSetting)
    {
        var keyId = applePaymentOptionSetting.ApplePaymentApiKeyId;
        var issuerId = applePaymentOptionSetting.ApplePaymentIssuerId;
        var bundleId = applePaymentOptionSetting.ApplePaymentBundleId;
        var privateKey = applePaymentOptionSetting.ApplePaymentPrivateKey;
        return new AppStoreServerApiClient(privateKey, keyId, issuerId, bundleId, ApplePayEnvironment.Sandbox, _httpClient);
    }

    private async Task<TenantGameOrderEntity?> FindAppleOrderAsync(JWSTransactionDecodedPayload transactionInfo)
    {
        TenantGameOrderEntity? gameOrderEntity = null;
        if (!string.IsNullOrWhiteSpace(transactionInfo.TransactionId))
        {
            gameOrderEntity = await PRepository.GetSingleAsync(m => m.AppleTransactionId == transactionInfo.TransactionId);
        }

        if (gameOrderEntity == null && !string.IsNullOrWhiteSpace(transactionInfo.OriginalTransactionId))
        {
            gameOrderEntity = await PRepository.GetSingleAsync(m => m.AppleOriginalTransactionId == transactionInfo.OriginalTransactionId);
        }

        if (gameOrderEntity == null &&
            !string.IsNullOrWhiteSpace(transactionInfo.AppAccountToken) &&
            Guid.TryParse(transactionInfo.AppAccountToken, out var uuid))
        {
            var orderId = GuidToLong(uuid);
            gameOrderEntity = await PRepository.GetSingleAsync(m => m.Id == orderId);
        }

        return gameOrderEntity;
    }

    /// <summary>
    /// 处理Apple Pay交易
    /// </summary>
    /// <param name="transactionInfo">Apple Pay交易信息</param>
    /// <param name="applePaymentOptionSetting">Apple支付配置</param>
    private async Task<bool> AppleConsumptionHandler(JWSTransactionDecodedPayload transactionInfo, ApplePaymentOptionSetting applePaymentOptionSetting)
    {
        if (transactionInfo == null || string.IsNullOrEmpty(transactionInfo.TransactionId))
        {
            return false;
        }

        if (!IsAppleServerApiConfigured(applePaymentOptionSetting))
        {
            LogHelper.Error("Apple Server API 配置不完整，跳过消费信息上报。请在后台配置中设置 ApiKeyId、IssuerId、BundleId、PrivateKey");
            return false;
        }

        var consumptionRequest = new ConsumptionRequest
        {
            CustomerConsented = true,
            DeliveryStatus = DeliveryStatus.DELIVERED,
            RefundPreference = RefundPreference.DECLINE,
            SampleContentProvided = true,
        };

        if (transactionInfo.Environment == ApplePayEnvironment.Sandbox)
        {
            // 沙盒模式
            var appStoreServerApiClientSandbox = GetAppStoreServerApiClientSandbox(applePaymentOptionSetting);
            var isSuccess = await appStoreServerApiClientSandbox.SendConsumptionInformation(transactionInfo.TransactionId, consumptionRequest);
            if (isSuccess is false)
            {
                LogHelper.Error($"Apple Pay 交易处理失败 - {transactionInfo.TransactionId}");
            }

            return isSuccess;
        }
        else
        {
            // 生产模式
            var appStoreServerApiClientProduction = GetAppStoreServerApiClientProduction(applePaymentOptionSetting);
            var isSuccess = await appStoreServerApiClientProduction.SendConsumptionInformation(transactionInfo.TransactionId, consumptionRequest);
            if (isSuccess is false)
            {
                LogHelper.Error($"Apple Pay 交易处理失败 - {transactionInfo.TransactionId}");
            }

            return isSuccess;
        }
    }

    private static bool IsAppleServerApiConfigured(ApplePaymentOptionSetting applePaymentOptionSetting)
    {
        return !string.IsNullOrWhiteSpace(applePaymentOptionSetting.ApplePaymentApiKeyId) &&
               !string.IsNullOrWhiteSpace(applePaymentOptionSetting.ApplePaymentIssuerId) &&
               !string.IsNullOrWhiteSpace(applePaymentOptionSetting.ApplePaymentBundleId) &&
               !string.IsNullOrWhiteSpace(applePaymentOptionSetting.ApplePaymentPrivateKey);
    }
}