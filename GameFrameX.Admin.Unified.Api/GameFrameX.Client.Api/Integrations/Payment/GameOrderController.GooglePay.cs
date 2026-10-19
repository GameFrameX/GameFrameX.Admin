using GameFrameX.Client.Api.Services.PayVerification;
using GameFrameX.Client.Api.Services.PublicCallback;
using System.Text;
using GameFrameX.Client.Api.Integrations.Payment.Dto.Google;
using GameFrameX.Client.Api.Services;
using GameFrameX.Entity.Enum;
using GameFrameX.Payment.Google;
using GameFrameX.Payment.Google.Option;

namespace GameFrameX.Client.Api.Integrations.Payment;

/// <summary>
///     程序版本控制器
/// </summary>
public partial class GameOrderController
{
    /// <summary>
    /// 通知发货
    /// 文档地址：https://www.cnblogs.com/zhaoyingjie/p/18000958
    /// </summary>
    /// <returns></returns>
    [PublicCallbackEndpoint("google", "payment")]
    [HttpPost("{tenantId:long}/{appId:long}/{packageId:long?}")]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> NotifyGoogle()
    {
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            var body = await reader.ReadToEndAsync();
            LogHelper.InfoConsole(body);
            try
            {
                var googlePaymentOptionSettings = _paymentChannelFactory.GetOptionSettings("google")
                                                                        .OfType<GooglePaymentOptionSetting>()
                                                                        .ToList();
                if (googlePaymentOptionSettings.Count == 0)
                {
                    LogHelper.Warning("Google支付配置未找到，无法处理支付通知");
                    Response.StatusCode = 500;
                    return GameApiResult.Error<BaseOperationAckResponse>(PaymentErrorCode.PayChannelNotSupported, "Google支付配置未找到");
                }

                var authorizationHeader = Request.Headers.Authorization.FirstOrDefault();
                if (string.IsNullOrEmpty(authorizationHeader) ||
                    !await _googlePubSubPushAuthenticator.ValidateAsync(authorizationHeader, googlePaymentOptionSettings))
                {
                    LogHelper.Warning("Google Pub/Sub push JWT 校验失败，拒绝处理支付通知");
                    Response.StatusCode = 401;
                    return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                }

                var googleWebHookInput = JsonHelper.Deserialize<GoogleWebHookRequest>(body);
                if (googleWebHookInput?.Message == null || string.IsNullOrWhiteSpace(googleWebHookInput.Message.Data))
                {
                    Response.StatusCode = 400;
                    return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                }

                byte[] data;
                try
                {
                    data = Convert.FromBase64String(googleWebHookInput.Message.Data);
                }
                catch (FormatException)
                {
                    Response.StatusCode = 400;
                    return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                }

                var dataJsonStr = Encoding.UTF8.GetString(data);
                var googleWebHookInputGooglePlayNotification = JsonHelper.Deserialize<GoogleWebHookInputGooglePlayNotification>(dataJsonStr);
                if (googleWebHookInputGooglePlayNotification == null)
                {
                    Response.StatusCode = 400;
                    return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                }

                if (googleWebHookInputGooglePlayNotification.VoidedPurchaseNotification != null)
                {
                    await GooglePlayApiManager.InitializeAsync(googlePaymentOptionSettings.First());
                    var voidedPurchase = googleWebHookInputGooglePlayNotification.VoidedPurchaseNotification;
                    LogHelper.Info($"收到Google Play退款通知，订单ID: {voidedPurchase.OrderId}, 退款原因: {voidedPurchase.VoidedReason}, 退款来源: {voidedPurchase.VoidedSource}");
                    var orderId = voidedPurchase.OrderId;

                    var gameOrderEntity = await PRepository.GetSingleAsync(m => m.OrderId == orderId);
                    if (gameOrderEntity != null)
                    {
                        return await _shipService.RefundAsync(gameOrderEntity.Id, $"Google Play refund: reason={voidedPurchase.VoidedReason}, source={voidedPurchase.VoidedSource}");
                    }

                    return HttpJsonResultData<BaseOperationAckResponse>.Success();
                }

                if (googleWebHookInputGooglePlayNotification.OneTimeProductNotification != null)
                {
                    // 处理一次性购买
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                    var oneTimeProductNotification = googleWebHookInputGooglePlayNotification.OneTimeProductNotification;
                    var verificationResult = await _googlePlayPurchaseService.VerifyPurchaseAsync(googlePaymentOptionSettings,
                                                                                                  googleWebHookInputGooglePlayNotification.PackageName,
                                                                                                  oneTimeProductNotification.Sku,
                                                                                                  oneTimeProductNotification.PurchaseToken);
                    var productPurchase = verificationResult?.Purchase;

                    if (productPurchase != null)
                    {
                        LogHelper.Info(" productPurchase :" + JsonHelper.Serialize(productPurchase));
                        var orderId = Convert.ToInt64(productPurchase.ObfuscatedExternalAccountId);

                        var gameOrderEntity = await PRepository.GetSingleAsync(m => m.Id == orderId, cts.Token);
                        if (gameOrderEntity == null)
                        {
                            // 订单信息找不到
                            // Response.StatusCode = 400;
                            LogHelper.InfoConsole($"订单信息找不到 -{orderId}");
                            return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                        }

                        var gameRoleEntity = await _gameRoleService.GetAsync(m => m.Id == long.Parse(gameOrderEntity.RoleId));
                        if (gameRoleEntity == null)
                        {
                            // 用户角色ID不存在
                            // Response.StatusCode = 400;
                            LogHelper.InfoConsole($"用户角色ID不存在 -{productPurchase.ObfuscatedExternalAccountId}");
                            return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                        }

                        gameOrderEntity.OrderId = productPurchase.OrderId;
                        if (gameOrderEntity.Status == OrderStatusEnum.Unpaid)
                        {
                            gameOrderEntity.Status = OrderStatusEnum.NotShipped;
                            gameOrderEntity.ProviderTransactionId = productPurchase.OrderId;
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
                                LogHelper.Info($"Google订单已被并发回调认领，跳过发货 - orderId:{gameOrderEntity.Id}");
                                return HttpJsonResultData<BaseOperationAckResponse>.Success();
                            }
                        }

                        // 检查是否是沙盒模式 
                        if (productPurchase.PurchaseType is 0)
                        {
                            // 沙盒模式
                            // 检查账号是否是沙盒用户
                            if (gameRoleEntity.IsSandbox is true)
                            {
                                // 是沙盒用户
                                var sandboxResult = await SendOutGoods(gameOrderEntity, true, true);
                                return await AcknowledgeGooglePurchaseAfterSuccessfulDelivery(sandboxResult, verificationResult, googleWebHookInputGooglePlayNotification.PackageName, oneTimeProductNotification);
                            }

                            // 不是沙盒用户
                            Response.StatusCode = 400;
                            LogHelper.InfoConsole($"不是沙盒用户 -{productPurchase}");
                            return HttpJsonResultData<BaseOperationAckResponse>.ValidationError();
                        }

                        // 发货
                        var result = await SendOutGoods(gameOrderEntity, true);
                        return await AcknowledgeGooglePurchaseAfterSuccessfulDelivery(result, verificationResult, googleWebHookInputGooglePlayNotification.PackageName, oneTimeProductNotification);
                    }
                    else
                    {
                        LogHelper.Warning("订单验证返回空结果");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Response.StatusCode = 500;
                LogHelper.Warning("订单验证超时");
                return HttpJsonResultData<BaseOperationAckResponse>.Fail("订单验证超时");
            }
            catch (Exception e)
            {
                Response.StatusCode = 500;
                LogHelper.Error(e);
                return HttpJsonResultData<BaseOperationAckResponse>.Fail("Google支付通知处理失败");
            }
        }

        return HttpJsonResultData<BaseOperationAckResponse>.Success();
    }

    private async Task<HttpJsonResultData<BaseOperationAckResponse>> AcknowledgeGooglePurchaseAfterSuccessfulDelivery(
        HttpJsonResultData<BaseOperationAckResponse> deliveryResult,
        GooglePlayPurchaseVerificationResponse? verificationResult,
        string packageName,
        GoogleWebHookInputOneTimeProductNotification notification)
    {
        if (deliveryResult.IsSuccess != true)
        {
            return deliveryResult;
        }

        if (verificationResult == null ||
            verificationResult.Purchase == null ||
            !GooglePlayApiManager.RequiresAcknowledgement(verificationResult.Purchase))
        {
            return deliveryResult;
        }

        var acknowledged = await _googlePlayPurchaseService.AcknowledgePurchaseAsync(verificationResult.Setting,
                                                                                     packageName,
                                                                                     notification.Sku,
                                                                                     notification.PurchaseToken,
                                                                                     verificationResult.Purchase.OrderId);
        if (acknowledged)
        {
            return deliveryResult;
        }

        Response.StatusCode = 500;
        LogHelper.Warning($"Google Play购买确认失败，将触发Pub/Sub重试 - orderId:{verificationResult?.Purchase?.OrderId}");
        return HttpJsonResultData<BaseOperationAckResponse>.Fail("Google Play购买确认失败");
    }
}