using GameFrameX.Payment.Apple.Option;
using System.Text;
using System.Text.Json;
using GameFrameX.Payment.Apple.Models.Receipt;
using GameFrameX.Payment.Apple.Models.Verification;

namespace GameFrameX.Payment.Apple;

/// <summary>
/// Apple收据验证管理器
/// </summary>
public class AppleReceiptVerificationManager
{
    private readonly ApplePaymentOptionSetting _paymentSettings;
    private const string ProductionUrl = "https://buy.itunes.apple.com/verifyReceipt";
    private const string SandboxUrl = "https://sandbox.itunes.apple.com/verifyReceipt";

    public AppleReceiptVerificationManager(ApplePaymentOptionSetting settings)
    {
        _paymentSettings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// 验证收据
    /// </summary>
    public async Task<AppleReceiptVerificationResponse?> VerifyReceiptAsync(string receiptData, HttpClient httpClient)
    {
        var requestData = new
        {
            receipt_data = receiptData,
            password = _paymentSettings.ApplePaymentSharedSecret,
            exclude_old_transactions = true,
        };

        var json = JsonSerializer.Serialize(requestData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // 首先尝试生产环境
        var url = _paymentSettings.IsSandbox ? SandboxUrl : ProductionUrl;
        var response = await httpClient.PostAsync(url, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        var verificationResponse = JsonSerializer.Deserialize<AppleReceiptVerificationResponse>(responseContent);

        // 如果生产环境返回21007错误（测试收据），则尝试沙盒环境
        if (verificationResponse?.Status == 21007 && !_paymentSettings.IsSandbox)
        {
            var sandboxContent = new StringContent(json, Encoding.UTF8, "application/json");
            var sandboxResponse = await httpClient.PostAsync(SandboxUrl, sandboxContent);
            var sandboxResponseContent = await sandboxResponse.Content.ReadAsStringAsync();
            verificationResponse = JsonSerializer.Deserialize<AppleReceiptVerificationResponse>(sandboxResponseContent);
        }

        return verificationResponse;
    }

    /// <summary>
    /// 检查验证是否成功
    /// </summary>
    public bool IsVerificationSuccessful(AppleReceiptVerificationResponse response)
    {
        return response.Status == 0;
    }

    /// <summary>
    /// 验证Bundle ID
    /// </summary>
    public bool ValidateBundleId(AppleReceiptVerificationResponse response)
    {
        return response.Receipt?.BundleId?.Equals(_paymentSettings.ApplePaymentBundleId, StringComparison.OrdinalIgnoreCase) == true;
    }

    /// <summary>
    /// 提取交易信息
    /// </summary>
    public AppleVerificationTransactionInfo? ExtractTransactionInfo(AppleReceiptVerificationResponse response, string transactionId)
    {
        var inAppPurchases = response.Receipt?.InApp ?? new List<AppleVerificationTransactionInfo>();
        return inAppPurchases.FirstOrDefault(x => x.TransactionId == transactionId);
    }
}