// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using GameFrameX.Payment.Google.Option;
using GameFrameX.Foundation.Logger;

namespace GameFrameX.Payment.Google;

/// <summary>
/// Google Play API 管理器，用于验证应用内购买
/// </summary>
public static partial class GooglePlayApiManager
{
    /// <summary>
    /// Google Play 服务，用于与 Android Publisher API 进行交互。
    /// </summary>
    private static AndroidPublisherService _androidPublisherService = null!;

    private static GooglePaymentOptionSetting _settings = null!;

    /// <summary>
    /// 自动初始化 <see cref="AndroidPublisherService"/>。
    /// </summary>
    /// <exception cref="InvalidOperationException">当初始化失败时抛出。</exception>
    public static async Task InitializeAsync(GooglePaymentOptionSetting settings)
    {
        if (_androidPublisherService != null)
        {
            return;
        }

        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        if (_androidPublisherService == null)
        {
            try
            {
                GoogleCredential credential;
                var serviceAccountKeyJson = _settings.GooglePaymentServiceAccountKey;
                // 凭证仅从支付渠道运行时配置注入；缺失时 fail-closed，禁止回退到产物目录明文凭证文件（C199）。
                if (string.IsNullOrWhiteSpace(serviceAccountKeyJson))
                {
                    throw new InvalidOperationException("Google Play 服务账号凭证缺失：请在支付渠道配置中填入服务账号 JSON（GooglePaymentServiceAccountKey）。已移除从产物目录读取明文凭证文件的回退路径。");
                }

                // 从支付渠道配置注入的 JSON 内容加载服务账号密钥
                credential = CredentialFactory
                             .FromJson<ServiceAccountCredential>(serviceAccountKeyJson)
                             .ToGoogleCredential()
                             .CreateScoped(AndroidPublisherService.Scope.Androidpublisher);

                _androidPublisherService = new AndroidPublisherService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "GameFrameX Payment System",
                });

                LogHelper.Info("初始化 AndroidPublisherService 成功（凭证来自支付渠道运行时配置注入）。");
            }
            catch (Exception ex)
            {
                LogHelper.Error("初始化 AndroidPublisherService 失败。", ex);
                throw;
            }

            if (_androidPublisherService == null)
            {
                throw new InvalidOperationException("自动初始化 AndroidPublisherService 失败");
            }
        }
    }

    /// <summary>
    /// 验证应用内购买
    /// </summary>
    /// <param name="packageName">应用包名</param>
    /// <param name="productId">产品ID</param>
    /// <param name="purchaseToken">购买令牌</param>
    /// <returns>购买信息</returns>
    public static async Task<ProductPurchase> VerifyPurchaseAsync(string packageName, string productId, string purchaseToken)
    {
        try
        {
            var request = _androidPublisherService.Purchases.Products.Get(packageName, productId, purchaseToken);
            var purchase = await request.ExecuteAsync();

            return purchase;
        }
        catch (Exception ex)
        {
            HandleGoogleApiException(ex, purchaseToken);
#pragma warning disable CS8603 // 异常路径返回 null，调用方应判空
            return null;
#pragma warning restore CS8603
        }
    }

    /// <summary>
    /// 确认购买
    /// </summary>
    /// <param name="packageName">应用包名</param>
    /// <param name="productId">产品ID</param>
    /// <param name="purchaseToken">购买令牌</param>
    /// <param name="developerPayload">开发者载荷（可选）</param>
    public static async Task<bool> AcknowledgePurchaseAsync(string packageName, string productId, string purchaseToken, string? developerPayload = null)
    {
        try
        {
            var acknowledgeRequest = new ProductPurchasesAcknowledgeRequest
            {
                DeveloperPayload = developerPayload,
            };

            var request = _androidPublisherService.Purchases.Products.Acknowledge(acknowledgeRequest, packageName, productId, purchaseToken);
            await request.ExecuteAsync();
            return true;
        }
        catch (Exception ex)
        {
            HandleGoogleApiException(ex, purchaseToken);

            return false;
        }
    }

    /// <summary>
    /// 消耗购买（对于消耗性商品）
    /// </summary>
    /// <param name="packageName">应用包名</param>
    /// <param name="productId">产品ID</param>
    /// <param name="purchaseToken">购买令牌</param>
    public static async Task<bool> ConsumePurchaseAsync(string packageName, string productId, string purchaseToken)
    {
        try
        {
            // 注意：Google Play API v3 中消耗购买需要通过客户端SDK完成
            // 这里只是验证购买状态
            var purchase = await VerifyPurchaseAsync(packageName, productId, purchaseToken);

            if (purchase.ConsumptionState != 1) // 1 表示已消耗
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            HandleGoogleApiException(ex, purchaseToken);
            return false;
        }
    }

    /// <summary>
    /// 验证购买状态是否有效
    /// </summary>
    /// <param name="purchase">购买信息</param>
    /// <returns>是否有效</returns>
    public static bool IsPurchaseValid(ProductPurchase purchase)
    {
        if (purchase == null)
        {
            return false;
        }

        // 检查购买状态：0=已购买，1=已取消，2=待处理
        return purchase.PurchaseState == 0;
    }

    /// <summary>
    /// 检查购买是否需要确认
    /// </summary>
    /// <param name="purchase">购买信息</param>
    /// <returns>是否需要确认</returns>
    public static bool RequiresAcknowledgement(ProductPurchase purchase)
    {
        if (purchase == null)
        {
            return false;
        }

        // 确认状态：0=尚未确认，1=已确认
        return purchase.AcknowledgementState == 0;
    }

    /// <summary>
    /// 检查购买是否已被消耗
    /// </summary>
    /// <param name="purchase">购买信息</param>
    /// <returns>是否已消耗</returns>
    public static bool IsConsumed(ProductPurchase purchase)
    {
        if (purchase == null)
        {
            return false;
        }

        // 消耗状态：0=尚未消耗，1=已消耗
        return purchase.ConsumptionState == 1;
    }

    /// <summary>
    /// 处理 Google API 异常，记录详细的错误信息和解决方案提示。
    /// </summary>
    /// <param name="ex">捕获到的 <see cref="Exception"/> 异常。</param>
    /// <param name="purchaseToken">发生异常时所操作的购买令牌。</param>
    private static void HandleGoogleApiException(Exception ex, string purchaseToken)
    {
        LogHelper.Error($"验证 Google Play 购买失败，令牌: {purchaseToken}. 错误: {ex.Message}");
        LogHelper.Error("这通常是由于服务账号权限不足导致的。请检查您的 Google Play Console 设置:");
        LogHelper.Error("1. 登录到您的 Google Play Console.");
        LogHelper.Error("2. 转到 '用户和权限'.");
        LogHelper.Error("3. 找到您的服务账号 (通常以 '...iam.gserviceaccount.com' 结尾) 并点击 '查看权限'.");
        LogHelper.Error("4. 在 '应用权限' 标签页下, 确保您的应用已被选中.");
        LogHelper.Error("5. 在 '账号权限' 标签页下, 确保已授予 '查看财务数据'、'管理订单' 和 '管理应用内商品' 等权限。特别是 '查看财务数据' 权限对于购买验证至关重要，但默认是关闭的。");
        LogHelper.Error("6. 保存更改后，权限更新可能需要一些时间（最多48小时）才能生效。您可以尝试在Play Console中编辑一下任何一个应用内商品（例如，修改描述），这有时可以强制刷新权限。");
    }
}