// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using GameFrameX.Client.Api.Services;
using GameFrameX.Client.Api.Services.PublicCallback;
using GameFrameX.Entity.Enum;
using GameFrameX.Payment.Wechat;
using SKIT.FlurlHttpClient.Wechat.TenpayV3;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Events;

namespace GameFrameX.Client.Api.Integrations.Payment;

public partial class GameOrderController
{
    /// <summary>
    /// 微信支付回调通知。
    /// <para>应答遵循微信支付规范（非统一信封）：成功返回 200 + <c>{"code":"SUCCESS"}</c>；
    /// 失败返回 500 + <c>{"code":"FAIL","message":"..."}</c>，微信将按退避策略重试，
    /// 重复通知由「Unpaid→NotShipped 原子认领 + 发货幂等」兜底（与支付宝回调同一信任链，C234）。</para>
    /// <para>信任链：先按微信 v3 规范显式验签（<c>Wechatpay-Signature</c> 头，平台证书本地验），
    /// 再解密资源、校验金额，最后原子认领发货；<c>DeserializeEvent</c> 本身不验签，验签绝不可省略。</para>
    /// </summary>
    /// <returns>微信规范 JSON 应答</returns>
    [PublicCallbackEndpoint("wechat", "payment")]
    [HttpPost("{tenantId:long}/{appId:long}/{packageId:long?}")]
    public async Task<IActionResult> NotifyWeChatPay()
    {
        string callbackJson;
        using (var reader = new StreamReader(Request.Body))
        {
            callbackJson = await reader.ReadToEndAsync();
        }

        LogHelper.InfoConsole($"微信支付回调信息 - {callbackJson}");

        var timestamp = Request.Headers["Wechatpay-Timestamp"].FirstOrDefault();
        var nonce = Request.Headers["Wechatpay-Nonce"].FirstOrDefault();
        var signature = Request.Headers["Wechatpay-Signature"].FirstOrDefault();
        var serialNumber = Request.Headers["Wechatpay-Serial"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(timestamp) || string.IsNullOrWhiteSpace(nonce) ||
            string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(serialNumber))
        {
            LogHelper.Warning("微信支付回调缺少验签请求头（Wechatpay-Timestamp/Nonce/Signature/Serial）");
            return WechatPayFail("缺少验签请求头");
        }

        // 多商户配置场景逐候选验签：验签（Wechatpay-Signature）依赖各候选商户的平台证书，命中即锁定该配置
        WechatPayOptionSetting? matchedSetting = null;
        WechatTenpayClient? matchedClient = null;
        WechatTenpayEvent? callbackEvent = null;
        foreach (var setting in _paymentChannelFactory.GetOptionSettings("wechat").OfType<WechatPayOptionSetting>())
        {
            try
            {
                var client = await WechatTenpayClientFactory.GetClientAsync(setting);
                var verifyResult = client.VerifyEventSignature(timestamp, nonce, callbackJson, signature, serialNumber);
                if (!verifyResult.Result)
                {
                    LogHelper.Warning($"微信支付回调验签失败（候选配置） - merchant:{setting.WechatPayMerchantId}, error:{verifyResult.Error?.Message}");
                    continue;
                }

                callbackEvent = client.DeserializeEvent(callbackJson);
                matchedSetting = setting;
                matchedClient = client;
                break;
            }
            catch (Exception ex)
            {
                LogHelper.Warning($"微信支付回调验签异常（候选配置） - merchant:{setting.WechatPayMerchantId}, error:{ex.Message}");
            }
        }

        if (matchedSetting == null || matchedClient == null || callbackEvent == null)
        {
            LogHelper.Warning("微信支付回调验签失败，无可用商户配置");
            return WechatPayFail("签名验证失败");
        }

        // 只处理支付成功通知，其余通知类型（退款等）直接确认，避免微信对可忽略事件持续重试
        if (!"TRANSACTION.SUCCESS".Equals(callbackEvent.EventType))
        {
            return WechatPaySuccess();
        }

        var resource = matchedClient.DecryptEventResource<TransactionResource>(callbackEvent);
        if (resource.TradeState != "SUCCESS" || resource.Amount == null)
        {
            LogHelper.Warning($"微信支付回调交易状态异常 - outTradeNo:{resource.OutTradeNumber}, tradeState:{resource.TradeState}");
            return WechatPayFail("交易状态异常");
        }

        if (!long.TryParse(resource.OutTradeNumber, out var orderId))
        {
            LogHelper.Warning($"微信支付回调订单号无效 - {resource.OutTradeNumber}");
            return WechatPayFail("订单号无效");
        }

        using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
        {
            var gameOrderEntity = await PRepository.GetSingleAsync(m => m.Id == orderId, cts.Token);
            if (gameOrderEntity == null)
            {
                LogHelper.InfoConsole($"订单信息找不到 - {orderId}");
                return WechatPayFail("订单不存在");
            }

            // 金额校验：微信回调 total（分）必须与订单 Amount（分）一致，防止伪造金额
            if ((long)resource.Amount.Total != gameOrderEntity.Amount)
            {
                LogHelper.Warning($"微信支付回调金额不匹配 - orderId:{orderId}, 订单:{gameOrderEntity.Amount}分, 回调:{resource.Amount.Total}分");
                return WechatPayFail("金额不匹配");
            }

            var gameRoleEntity = await _gameRoleService.GetAsync(m => m.Id == long.Parse(gameOrderEntity.RoleId));
            if (gameRoleEntity == null)
            {
                LogHelper.InfoConsole($"用户角色ID不存在 - {orderId}");
                return WechatPayFail("角色不存在");
            }

            gameOrderEntity.OrderId = orderId.ToString();
            if (gameOrderEntity.Status == OrderStatusEnum.Unpaid)
            {
                // 仅首次支付时落库，避免重复回调把状态回写成可重新发货
                gameOrderEntity.Status = OrderStatusEnum.NotShipped;
                gameOrderEntity.ProviderTransactionId = resource.TransactionId;
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
                    LogHelper.Info($"微信支付订单已被并发回调认领，跳过发货 - orderId:{orderId}");
                    return WechatPaySuccess();
                }
            }

            // 发货：失败返回 FAIL 由微信重试，发货链路（ShipAsync）自身幂等
            var shipResult = await SendOutGoods(gameOrderEntity, true);
            return shipResult.IsSuccess ? WechatPaySuccess() : WechatPayFail("发货失败");
        }
    }

    /// <summary>
    /// 微信支付回调成功应答（微信规范 JSON，非统一信封）
    /// </summary>
    private static ContentResult WechatPaySuccess()
    {
        return new ContentResult
        {
            StatusCode = StatusCodes.Status200OK,
            ContentType = "application/json",
            Content = "{\"code\":\"SUCCESS\"}",
        };
    }

    /// <summary>
    /// 微信支付回调失败应答：HTTP 500 + FAIL，微信按退避策略重试
    /// </summary>
    private static ContentResult WechatPayFail(string message)
    {
        return new ContentResult
        {
            StatusCode = StatusCodes.Status500InternalServerError,
            ContentType = "application/json",
            Content = JsonHelper.Serialize(new { code = "FAIL", message, }),
        };
    }
}
