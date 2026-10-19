// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text;
using System.Text.Json;
using GameFrameX.Payment.Xsolla.Models;
using GameFrameX.Payment.Xsolla.Option;

namespace GameFrameX.Payment.Xsolla.Transaction;

/// <summary>
/// Xsolla交易管理器
/// </summary>
public class TransactionManager
{
    private const string BaseUrl = "https://api.xsolla.com";
    private const string SandboxBaseUrl = "https://sandbox-api.xsolla.com";

    /// <summary>
    /// 根据外部订单ID查询交易信息
    /// </summary>
    /// <param name="httpClient">HTTP客户端</param>
    /// <param name="externalId">外部订单ID</param>
    /// <param name="optionSetting">配置设置</param>
    /// <param name="isSandbox">是否为沙盒环境</param>
    /// <returns>交易查询结果</returns>
    public async Task<XsollaTransactionResponse?> FindTransactionsByExternalIdAsync(HttpClient httpClient, string externalId, XsollaPayOptionSetting optionSetting, bool isSandbox = false)
    {
        try
        {
            var baseUrl = isSandbox ? SandboxBaseUrl : BaseUrl;
            var url = $"{baseUrl}/v2/project/{optionSetting.XsollaPaymentProjectId}/transactions";

            // 构建查询参数
            var queryParams = new List<string>
            {
                $"external_id={Uri.EscapeDataString(externalId)}",
                "limit=10", // 限制返回数量
                "offset=0", // 偏移量
            };

            var fullUrl = $"{url}?{string.Join("&", queryParams)}";

            // 设置请求头
            var request = new HttpRequestMessage(HttpMethod.Get, fullUrl);
            request.Headers.Add("Authorization", $"Basic {GetBasicAuthHeader(optionSetting.XsollaPaymentMerchantId, optionSetting.XsollaPaymentApiKey)}");
            request.Headers.Add("Accept", "application/json");

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var transactionResponse = JsonSerializer.Deserialize<XsollaTransactionResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });

                return transactionResponse;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Xsolla API请求失败: {response.StatusCode}, 内容: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"查询Xsolla交易时发生错误: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 根据交易ID查询单个交易信息
    /// </summary>
    /// <param name="httpClient">HTTP客户端</param>
    /// <param name="transactionId">交易ID</param>
    /// <param name="optionSetting">配置设置</param>
    /// <param name="isSandbox">是否为沙盒环境</param>
    /// <returns>交易信息</returns>
    public async Task<XsollaTransaction?> GetTransactionByIdAsync(HttpClient httpClient, long transactionId, XsollaPayOptionSetting optionSetting, bool isSandbox = false)
    {
        try
        {
            var baseUrl = isSandbox ? SandboxBaseUrl : BaseUrl;
            var url = $"{baseUrl}/v2/project/{optionSetting.XsollaPaymentProjectId}/transactions/{transactionId}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Basic {GetBasicAuthHeader(optionSetting.XsollaPaymentMerchantId, optionSetting.XsollaPaymentApiKey)}");
            request.Headers.Add("Accept", "application/json");

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var transaction = JsonSerializer.Deserialize<XsollaTransaction>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });

                return transaction;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null; // 交易不存在
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Xsolla API请求失败: {response.StatusCode}, 内容: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"查询Xsolla交易详情时发生错误: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 验证交易状态是否为成功
    /// </summary>
    /// <param name="transaction">交易信息</param>
    /// <returns>是否为成功状态</returns>
    public static bool IsTransactionSuccessful(XsollaTransaction transaction)
    {
        if (transaction == null)
        {
            return false;
        }

        // Xsolla的成功状态通常为 "done" 或 "paid"
        return transaction.Status.Equals("done", StringComparison.OrdinalIgnoreCase) ||
               transaction.Status.Equals("paid", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 验证交易是否已完成（包括成功和失败的最终状态）
    /// </summary>
    /// <param name="transaction">交易信息</param>
    /// <returns>是否已完成</returns>
    public static bool IsTransactionCompleted(XsollaTransaction transaction)
    {
        if (transaction == null)
        {
            return false;
        }

        var completedStatuses = new[] { "done", "paid", "canceled", "rejected", "refunded", };
        return completedStatuses.Contains(transaction.Status.ToLowerInvariant());
    }

    /// <summary>
    /// 生成Basic认证头
    /// </summary>
    /// <param name="merchantId">商户ID</param>
    /// <param name="apiKey">API密钥</param>
    /// <returns>Base64编码的认证字符串</returns>
    private static string GetBasicAuthHeader(int merchantId, string apiKey)
    {
        var credentials = $"{merchantId}:{apiKey}";
        var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
        return encodedCredentials;
    }
}