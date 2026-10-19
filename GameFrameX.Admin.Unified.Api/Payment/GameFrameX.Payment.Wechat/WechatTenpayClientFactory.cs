// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using GameFrameX.Foundation.Logger;
using SKIT.FlurlHttpClient.Wechat.TenpayV3;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Models;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Settings;
using SKIT.FlurlHttpClient.Wechat.TenpayV3.Utilities;

namespace GameFrameX.Payment.Wechat;

/// <summary>
/// 微信支付 TenpayV3 客户端工厂。按商户配置签名缓存客户端实例：
/// 平台证书（回调验签依赖）下载一次后随缓存的 <see cref="InMemoryCertificateManager"/> 常驻进程，
/// 避免每次下单 / 回调都触发证书下载。配置变更（商户号 / 证书序列号 / 应用 ID 变化）自然切换到新缓存键。
/// </summary>
public static class WechatTenpayClientFactory
{
    /// <summary>
    /// 客户端缓存：键为商户配置签名（商户号|应用ID|证书序列号）。
    /// ponytail: 并发首启同一商户可能重复下载平台证书，AddOrUpdate 幂等，无害；证书轮换依赖配置序列号变化或进程重启。
    /// </summary>
    private static readonly ConcurrentDictionary<string, WechatTenpayClient> ClientCache = new(StringComparer.Ordinal);

    /// <summary>
    /// 获取（必要时创建并初始化平台证书的）微信支付客户端
    /// </summary>
    /// <param name="settings">微信支付配置</param>
    /// <returns>已就绪的 TenpayV3 客户端</returns>
    public static async Task<WechatTenpayClient> GetClientAsync(WechatPayOptionSetting settings)
    {
        var cacheKey = $"{settings.WechatPayMerchantId}|{settings.WechatPayAppId}|{settings.WechatPayCertificateSerialNumber}";
        var client = ClientCache.GetOrAdd(cacheKey, _ => CreateClient(settings));

        await EnsurePlatformCertificatesAsync(cacheKey, client, settings.WechatPayApiV3Secret);
        return client;
    }

    private static WechatTenpayClient CreateClient(WechatPayOptionSetting settings)
    {
        var options = new WechatTenpayClientOptions
        {
            MerchantId = settings.WechatPayMerchantId,
            MerchantV3Secret = settings.WechatPayApiV3Secret,
            MerchantCertificateSerialNumber = settings.WechatPayCertificateSerialNumber,
            MerchantCertificatePrivateKey = NormalizePrivateKey(settings.WechatPayCertificatePrivateKey),
            PlatformCertificateManager = new InMemoryCertificateManager(),
            // 平台证书引导 / 下单 / 查单的 HTTP 超时（毫秒）：默认值过长，失败场景（无网、DNS 挂起）会拖垮下单主链路
            Timeout = 15_000,
        };
        return new WechatTenpayClient(options);
    }

    /// <summary>
    /// 平台证书引导：回调验签（Wechatpay-Signature）依赖平台证书，InMemoryCertificateManager 初始为空，
    /// 首次使用时下载证书列表，用 APIv3 密钥 AES-256-GCM 解密出证书 PEM 后登记进管理器。
    /// </summary>
    private static async Task EnsurePlatformCertificatesAsync(string cacheKey, WechatTenpayClient client, string apiV3Secret)
    {
        if (client.PlatformCertificateManager is not InMemoryCertificateManager manager || manager.AllEntries().Any())
        {
            return;
        }

        try
        {
            await DownloadAndRegisterCertificatesAsync(cacheKey, client, apiV3Secret, manager);
        }
        catch (Exception ex)
        {
            // 平台证书下载异常（网络不可达等）：下单仍可用（请求签名走商户证书），但回调验签将失败——记录错误并放行，回调端点给出 FAIL 应答后由微信重试触发再下载
            LogHelper.Error($"微信支付平台证书下载异常 - config:{cacheKey}, error:{ex.Message}");
        }
    }

    private static async Task DownloadAndRegisterCertificatesAsync(string cacheKey, WechatTenpayClient client, string apiV3Secret, InMemoryCertificateManager manager)
    {
        var response = await client.ExecuteQueryCertificatesAsync(new QueryCertificatesRequest { AlgorithmType = "RSA", });
        if (!response.IsSuccessful())
        {
            // 平台证书下载失败：下单仍可用（请求签名走商户证书），但回调验签将失败——记录错误并放行，由回调端点给出 FAIL 应答
            LogHelper.Error($"微信支付平台证书下载失败 - config:{cacheKey}, error:{response.ErrorMessage}");
            return;
        }

        var keyBytes = Encoding.UTF8.GetBytes(apiV3Secret);
        foreach (var certificate in response.CertificateList)
        {
            var plainBytes = AESUtility.DecryptWithGCM(
                keyBytes,
                Encoding.UTF8.GetBytes(certificate.EncryptCertificate.Nonce ?? string.Empty),
                Encoding.UTF8.GetBytes(certificate.EncryptCertificate.AssociatedData ?? string.Empty),
                Convert.FromBase64String(certificate.EncryptCertificate.CipherText ?? string.Empty),
                string.Empty);
            var entry = new CertificateEntry(
                certificate.EncryptCertificate.Algorithm,
                certificate.SerialNumber,
                Encoding.UTF8.GetString(plainBytes),
                certificate.EffectiveTime,
                certificate.ExpireTime);
            manager.AddEntry(entry);
        }
    }

    /// <summary>
    /// 规范化商户私钥：剥离 PEM 头尾与空白（与 AliPay 私钥处理同模式），SDK 两种形态均接受
    /// </summary>
    private static string NormalizePrivateKey(string privateKey)
    {
        return privateKey
               .Replace("-----BEGIN PRIVATE KEY-----", "")
               .Replace("-----END PRIVATE KEY-----", "")
               .Replace("\r", "")
               .Replace("\n", "")
               .Replace(" ", "");
    }
}
