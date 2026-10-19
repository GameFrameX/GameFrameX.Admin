using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using GameFrameX.Foundation.Extensions;
using GameFrameX.Foundation.Json;
using GameFrameX.Payment.Apple.AppStoreServer.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace GameFrameX.Payment.Apple.AppStoreServer;

/// <summary>
/// Apple App Store Server API 客户端封装类
/// 用于与 Apple 官方服务器进行安全通信，完成订阅、订单、退款、通知等相关业务
/// </summary>
public sealed class AppStoreServerApiClient
{
    /// <summary>
    /// 正式环境接口基地址
    /// </summary>
    private static readonly string ProductionUrl = "https://api.storekit.itunes.apple.com";

    /// <summary>
    /// 沙箱环境接口基地址
    /// </summary>
    private static readonly string SandboxUrl = "https://api.storekit-sandbox.itunes.apple.com";

    /// <summary>
    /// 本地测试环境接口基地址（可自定义）
    /// </summary>
    private static readonly string LocalTestingUrl = "https://local-testing-base-url";

    /// <summary>
    /// 默认 User-Agent 标识
    /// </summary>
    private static readonly string UserAgent = "app-store-server-library/csharp/1.0.0";

    /// <summary>
    /// App Store Connect 生成的 Issuer ID
    /// </summary>
    private readonly string _issuerId;

    /// <summary>
    /// 私钥对应的 Key ID
    /// </summary>
    private readonly string _keyId;

    /// <summary>
    /// ECDSA 私钥内容（PEM 格式）
    /// </summary>
    private readonly string _signingKey;

    /// <summary>
    /// App Bundle ID
    /// </summary>
    private readonly string _bundleId;

    /// <summary>
    /// 当前环境对应的接口基地址
    /// </summary>
    private readonly string _urlBase;

    /// <summary>
    /// 内部复用的 HttpClient 实例
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="signingKey">ECDSA 私钥（PEM 格式）。支持完整 PEM、纯 Base64 以及自动补全 PEM 头尾三种形式</param>
    /// <param name="keyId">私钥对应的 Key ID，可在 App Store Connect 的“用户和访问”-“密钥”页面找到</param>
    /// <param name="issuerId">App Store Connect 提供的 Issuer ID，可在“用户和访问”-“密钥”页面获取</param>
    /// <param name="bundleId">App Bundle ID，例如 com.example.app</param>
    /// <param name="environment">运行环境（Production / Sandbox / LocalTesting）。Xcode 环境不被支持</param>
    /// <param name="httpClient">由 <see cref="IHttpClientFactory"/> 创建的 HttpClient 实例，负责底层连接复用与超时策略；不可为 null</param>
    /// <exception cref="ArgumentException">当传入 Xcode 或未知环境时抛出</exception>
    /// <exception cref="ArgumentNullException">当 <paramref name="httpClient"/> 为 null 时抛出</exception>
    public AppStoreServerApiClient(string signingKey, string keyId, string issuerId, string bundleId, ApplePayEnvironment environment, HttpClient httpClient)
    {
        _issuerId = issuerId;
        _keyId = keyId;
        _bundleId = bundleId;
        _signingKey = signingKey;
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        switch (environment)
        {
            case ApplePayEnvironment.Xcode:
                throw new ArgumentException("Xcode is not a supported environment for an AppStoreServerApiClient");
            case ApplePayEnvironment.Production:
                _urlBase = ProductionUrl;
                break;
            case ApplePayEnvironment.LocalTesting:
                _urlBase = LocalTestingUrl;
                break;
            case ApplePayEnvironment.Sandbox:
                _urlBase = SandboxUrl;
                break;
            default:
                throw new ArgumentException("Unknown environment");
        }
    }

    /// <summary>
    /// 无返回值的通用请求方法
    /// </summary>
    /// <param name="path">API 路径，例如 /inApps/v1/subscriptions/extend/mass</param>
    /// <param name="method">HTTP 方法，例如 GET、POST、PUT、DELETE</param>
    /// <param name="queryParameters">查询参数，可为 null；内部自动进行 URL 编码</param>
    /// <param name="body">请求体，可为 null；支持对象或 byte[]</param>
    /// <param name="contentType">Content-Type，默认 application/json；当 body 为 byte[] 时须显式指定</param>
    /// <exception cref="ApiException">当响应状态码非 2xx 时抛出，内含 HTTP 状态码、Apple 错误码及错误信息</exception>
    protected async Task MakeRequest(string path, string method, Dictionary<string, List<string>>? queryParameters, object? body, string contentType = "application/json")
    {
        var response = await SendRequest(path, method, queryParameters, body, contentType);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        await ThrowApiException(response);
    }

    /// <summary>
    /// 带返回值的通用请求方法
    /// </summary>
    /// <typeparam name="T">返回的 DTO 类型，须有无参构造函数</typeparam>
    /// <param name="path">API 路径，例如 /inApps/v1/subscriptions/extend/mass</param>
    /// <param name="method">HTTP 方法，例如 GET、POST、PUT、DELETE</param>
    /// <param name="queryParameters">查询参数，可为 null；内部自动进行 URL 编码</param>
    /// <param name="body">请求体，可为 null；支持对象或 byte[]</param>
    /// <param name="contentType">Content-Type，默认 application/json；当 body 为 byte[] 时须显式指定</param>
    /// <returns>反序列化后的响应对象</returns>
    /// <exception cref="ApiException">当响应状态码非 2xx 时抛出，内含 HTTP 状态码、Apple 错误码及错误信息</exception>
    protected async Task<T> MakeRequest<T>(string path, string method, Dictionary<string, List<string>>? queryParameters, object? body, string contentType = "application/json") where T : class, new()
    {
        var response = await SendRequest(path, method, queryParameters, body, contentType);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            if (responseBody.IsNullOrEmptyOrWhiteSpace())
            {
                return new T();
            }

            return (T)JsonHelper.Deserialize(responseBody, typeof(T));
        }

        await ThrowApiException(response);
        // ThrowApiException 所有分支均抛出 ApiException，正常情况下不会执行到这里
        throw new InvalidOperationException("ThrowApiException 应始终抛出异常，此处不可达");
    }

    /// <summary>
    /// 内部统一发送 HTTP 请求
    /// </summary>
    /// <param name="path">API 路径，须以 / 开头</param>
    /// <param name="method">HTTP 方法</param>
    /// <param name="queryParameters">查询参数，可为 null</param>
    /// <param name="body">请求体，可为 null</param>
    /// <param name="contentType">Content-Type，默认为 application/json</param>
    /// <returns>原始 HttpResponseMessage，由调用方负责确保成功状态码</returns>
    private async Task<HttpResponseMessage> SendRequest(string path, string method, Dictionary<string, List<string>>? queryParameters, object? body, string contentType)
    {
        // 拼接完整 URL
        var requestUrl = _urlBase + path;
        if (queryParameters != null && queryParameters.Count > 0)
        {
            var queryString = new StringBuilder();
            foreach (var param in queryParameters)
            {
                foreach (var val in param.Value)
                {
                    if (queryString.Length > 0)
                    {
                        queryString.Append("&");
                    }

                    queryString.Append($"{Uri.EscapeDataString(param.Key)}={Uri.EscapeDataString(val)}");
                }
            }

            requestUrl += "?" + queryString;
        }

        // 构造请求消息
        var request = new HttpRequestMessage(new HttpMethod(method), requestUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateBearerToken());
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // 写入 Body
        if (body != null)
        {
            if (body is byte[] byteBody)
            {
                var content = new ByteArrayContent(byteBody);
                if (!string.IsNullOrEmpty(contentType))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                }

                request.Content = content;
            }
            else
            {
                var json = JsonHelper.Serialize(body);
                request.Content = new StringContent(json, Encoding.UTF8, contentType);
            }
        }

        return await _httpClient.SendAsync(request);
    }

    /// <summary>
    /// 将非成功状态码包装为 ApiException
    /// </summary>
    /// <param name="response">待检查的 HttpResponseMessage</param>
    /// <exception cref="ApiException">始终抛出，内含尽可能详细的错误信息</exception>
    private async Task ThrowApiException(HttpResponseMessage response)
    {
        try
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonHelper.Deserialize<Dictionary<string, object>>(errorBody);
            int? errorCode = null;
            string? errorMessage = null;

            if (errorResponse != null)
            {
                if (errorResponse.ContainsKey("errorCode") && errorResponse["errorCode"] is int code)
                {
                    errorCode = code;
                }
                else if (errorResponse.ContainsKey("errorCode") && int.TryParse(errorResponse["errorCode"]?.ToString(), out var parsedCode))
                {
                    errorCode = parsedCode;
                }

                if (errorResponse.ContainsKey("errorMessage"))
                {
                    errorMessage = errorResponse["errorMessage"]?.ToString();
                }
            }

            if (errorCode.HasValue)
            {
                throw new ApiException((int)response.StatusCode, errorCode.Value, errorMessage);
            }

            throw new ApiException((int)response.StatusCode, null, errorMessage);
        }
        catch (Exception e) when (!(e is ApiException))
        {
            throw new ApiException((int)response.StatusCode);
        }
    }

    /// <summary>
    /// 批量延长所有活跃订阅者的续期日期
    /// </summary>
    /// <param name="massExtendRenewalDateRequest">包含延长天数、产品 ID 等信息的请求体</param>
    /// <returns>批量延长结果，含成功与失败数量</returns>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    public async Task<MassExtendRenewalDateResponse> ExtendRenewalDateForAllActiveSubscribers(MassExtendRenewalDateRequest massExtendRenewalDateRequest)
    {
        return await MakeRequest<MassExtendRenewalDateResponse>("/inApps/v1/subscriptions/extend/mass", "POST", null, massExtendRenewalDateRequest);
    }

    /// <summary>
    /// 延长单个订阅的续期日期
    /// </summary>
    /// <param name="originalTransactionId">原始交易 ID，可在收据中获取</param>
    /// <param name="extendRenewalDateRequest">包含延长天数、原因等信息的请求体</param>
    /// <returns>延长结果，含新的续期日期</returns>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    public async Task<ExtendRenewalDateResponse> ExtendSubscriptionRenewalDate(string originalTransactionId, ExtendRenewalDateRequest extendRenewalDateRequest)
    {
        return await MakeRequest<ExtendRenewalDateResponse>("/inApps/v1/subscriptions/extend/" + originalTransactionId, "PUT", null, extendRenewalDateRequest);
    }

    /// <summary>
    /// 查询订阅状态列表
    /// </summary>
    /// <param name="transactionId">原始交易 ID 或任何后续交易 ID</param>
    /// <param name="status">可选状态过滤列表，例如只查看活跃订阅</param>
    /// <returns>该交易关联的所有订阅状态</returns>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    public async Task<StatusResponse> GetAllSubscriptionStatuses(string transactionId, List<Status>? status = null)
    {
        var queryParameters = new Dictionary<string, List<string>>();
        if (status != null)
        {
            var statusList = new List<string>();
            foreach (var s in status)
            {
                statusList.Add(((int)s).ToString());
            }

            queryParameters.Add("status", statusList);
        }

        return await MakeRequest<StatusResponse>("/inApps/v1/subscriptions/" + transactionId, "GET", queryParameters, null);
    }

    /// <summary>
    /// 获取退款历史
    /// </summary>
    /// <param name="transactionId">原始交易 ID</param>
    /// <param name="revision">分页令牌，首次调用可传 null</param>
    /// <returns>退款记录列表及下一页令牌</returns>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    public async Task<RefundHistoryResponse> GetRefundHistory(string transactionId, string revision)
    {
        var queryParameters = new Dictionary<string, List<string>>();
        if (revision != null)
        {
            queryParameters.Add("revision", new List<string> { revision, });
        }

        return await MakeRequest<RefundHistoryResponse>("/inApps/v2/refund/lookup/" + transactionId, "GET", queryParameters, null);
    }

    /// <summary>
    /// 查询批量续期延长状态
    /// </summary>
    /// <param name="requestIdentifier">批量延长请求唯一标识符，由 ExtendRenewalDateForAllActiveSubscribers 返回</param>
    /// <param name="productId">App 内购买产品 ID</param>
    /// <returns>当前批次处理进度及明细</returns>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    public async Task<MassExtendRenewalDateStatusResponse> GetStatusOfSubscriptionRenewalDateExtensions(string requestIdentifier, string productId)
    {
        return await MakeRequest<MassExtendRenewalDateStatusResponse>("/inApps/v1/subscriptions/extend/mass/" + productId + "/" + requestIdentifier, "GET", null, null);
    }

    /// <summary>
    /// 查询测试通知状态
    /// </summary>
    /// <param name="testNotificationToken">RequestTestNotification 返回的测试令牌</param>
    /// <returns>测试通知当前状态及 payload 解析结果</returns>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    public async Task<CheckTestNotificationResponse> GetTestNotificationStatus(string testNotificationToken)
    {
        return await MakeRequest<CheckTestNotificationResponse>("/inApps/v1/notifications/test/" + testNotificationToken, "GET", null, null);
    }

    /// <summary>
    /// 获取通知历史
    /// </summary>
    /// <param name="paginationToken">分页令牌，首次调用可传 null</param>
    /// <param name="notificationHistoryRequest">查询条件，如时间范围、通知类型等</param>
    /// <returns>通知列表及下一页令牌</returns>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    public async Task<NotificationHistoryResponse> GetNotificationHistory(string paginationToken, NotificationHistoryRequest notificationHistoryRequest)
    {
        var queryParameters = new Dictionary<string, List<string>>();
        if (paginationToken != null)
        {
            queryParameters.Add("paginationToken", new List<string> { paginationToken, });
        }

        return await MakeRequest<NotificationHistoryResponse>("/inApps/v1/notifications/history", "POST", queryParameters, notificationHistoryRequest);
    }

    /// <summary>
    /// 获取交易历史
    /// </summary>
    /// <param name="transactionId">原始交易 ID 或任何后续交易 ID</param>
    /// <param name="revision">分页令牌，首次调用可传 null</param>
    /// <param name="transactionHistoryRequest">查询条件，如时间范围、产品 ID、排序方式等</param>
    /// <param name="version">API 版本，默认 V1；如需使用 V2 可传入 GetTransactionHistoryVersion.V2</param>
    /// <returns>交易记录列表及下一页令牌</returns>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    public async Task<HistoryResponse> GetTransactionHistory(string transactionId, string revision, TransactionHistoryRequest transactionHistoryRequest, GetTransactionHistoryVersion version = GetTransactionHistoryVersion.V1)
    {
        var queryParameters = new Dictionary<string, List<string>>();
        if (revision != null)
        {
            queryParameters.Add("revision", new List<string> { revision, });
        }

        var startDate = transactionHistoryRequest.StartDate;
        if (startDate != null)
        {
            queryParameters.Add("startDate", new List<string> { startDate.ToString() ?? string.Empty, });
        }

        var endDate = transactionHistoryRequest.EndDate;
        if (endDate != null)
        {
            queryParameters.Add("endDate", new List<string> { endDate.ToString() ?? string.Empty, });
        }

        if (transactionHistoryRequest.ProductIds != null)
        {
            queryParameters.Add("productId", transactionHistoryRequest.ProductIds);
        }

        if (transactionHistoryRequest.ProductTypes != null)
        {
            var types = new List<string>();
            foreach (var t in transactionHistoryRequest.ProductTypes)
            {
                types.Add(t.ToString());
            }

            queryParameters.Add("productType", types);
        }

        var sort = transactionHistoryRequest.Sort;
        if (sort != null)
        {
            queryParameters.Add("sort", new List<string> { sort.ToString() ?? string.Empty, });
        }

        if (transactionHistoryRequest.SubscriptionGroupIdentifiers != null)
        {
            queryParameters.Add("subscriptionGroupIdentifier", transactionHistoryRequest.SubscriptionGroupIdentifiers);
        }

        var inAppOwnershipType = transactionHistoryRequest.InAppOwnershipType;
        if (inAppOwnershipType != null)
        {
            queryParameters.Add("inAppOwnershipType", new List<string> { inAppOwnershipType.ToString() ?? string.Empty, });
        }

        var revoked = transactionHistoryRequest.Revoked;
        if (revoked != null)
        {
            queryParameters.Add("revoked", new List<string> { revoked.ToString() ?? string.Empty, });
        }

        var versionPath = version == GetTransactionHistoryVersion.V1 ? "v1" : "v2";
        return await MakeRequest<HistoryResponse>("/inApps/" + versionPath + "/history/" + transactionId, "GET", queryParameters, null);
    }

    /// <summary>
    /// 获取交易详情
    /// </summary>
    /// <param name="transactionId">交易 ID，可在收据或历史接口中获取</param>
    /// <returns>交易详细信息，含产品 ID、购买时间、过期时间等</returns>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    public async Task<TransactionInfoResponse> GetTransactionInfo(string transactionId)
    {
        return await MakeRequest<TransactionInfoResponse>("/inApps/v1/transactions/" + transactionId, "GET", null, null);
    }

    /// <summary>
    /// 根据订单 ID 查询订单
    /// </summary>
    /// <param name="orderId">Apple 订单 ID，由客户端购买成功后返回</param>
    /// <returns>订单关联的交易列表及状态</returns>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    public async Task<OrderLookupResponse> LookUpOrderId(string orderId)
    {
        return await MakeRequest<OrderLookupResponse>("/inApps/v1/lookup/" + orderId, "GET", null, null);
    }

    /// <summary>
    /// 请求发送测试通知
    /// </summary>
    /// <returns>测试通知令牌，可用于 GetTestNotificationStatus 查询状态</returns>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    public async Task<SendTestNotificationResponse> RequestTestNotification()
    {
        return await MakeRequest<SendTestNotificationResponse>("/inApps/v1/notifications/test", "POST", null, null);
    }

    /// <summary>
    /// 【已废弃】发送消耗型数据（V1 版本）
    /// 请使用 SendConsumptionInformation 替代
    /// </summary>
    /// <param name="transactionId">原始交易 ID</param>
    /// <param name="consumptionRequest">V1 版本消耗型数据请求体</param>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    [Obsolete("Use SendConsumptionInformation instead")]
    public async Task SendConsumptionData(string transactionId, ConsumptionRequestV1 consumptionRequest)
    {
        await MakeRequest<object>("/inApps/v1/transactions/consumption/" + transactionId, "PUT", null, consumptionRequest);
    }

    /// <summary>
    /// 发送消耗型数据（V2 版本）
    /// </summary>
    /// <param name="transactionId">原始交易 ID</param>
    /// <param name="consumptionRequest">V2 版本消耗型数据请求体，含账号状态、消耗时间等</param>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    public async Task<bool> SendConsumptionInformation(string transactionId, ConsumptionRequest consumptionRequest)
    {
        var result = await MakeRequest<object>("/inApps/v2/transactions/consumption/" + transactionId, "PUT", null, consumptionRequest);
        return result != null;
    }

    /// <summary>
    /// 设置 App 账号令牌
    /// </summary>
    /// <param name="originalTransactionId">原始交易 ID</param>
    /// <param name="updateAppAccountTokenRequest">包含新 appAccountToken 的请求体</param>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    public async Task SetAppAccountToken(string originalTransactionId, UpdateAppAccountTokenRequest updateAppAccountTokenRequest)
    {
        await MakeRequest<object>("/inApps/v1/transactions/" + originalTransactionId + "/appAccountToken", "PUT", null, updateAppAccountTokenRequest);
    }

    /// <summary>
    /// 上传图片资源
    /// </summary>
    /// <param name="imageIdentifier">图片唯一标识符，可由业务方自定义</param>
    /// <param name="image">PNG 图片二进制数据</param>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    public async Task UploadImage(string imageIdentifier, byte[] image)
    {
        await MakeRequest<object>("/inApps/v1/messaging/image/" + imageIdentifier, "PUT", null, image, "image/png");
    }

    /// <summary>
    /// 删除图片资源
    /// </summary>
    /// <param name="imageIdentifier">图片唯一标识符</param>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    public async Task DeleteImage(string imageIdentifier)
    {
        await MakeRequest<object>("/inApps/v1/messaging/image/" + imageIdentifier, "DELETE", null, null);
    }

    /// <summary>
    /// 获取图片列表
    /// </summary>
    /// <returns>已上传图片标识符列表</returns>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    public async Task<GetImageListResponse> GetImageList()
    {
        return await MakeRequest<GetImageListResponse>("/inApps/v1/messaging/image/list", "GET", null, null);
    }

    /// <summary>
    /// 上传消息
    /// </summary>
    /// <param name="messageIdentifier">消息唯一标识符，可由业务方自定义</param>
    /// <param name="uploadMessageRequestBody">消息内容，含标题、描述、本地化信息等</param>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    public async Task UploadMessage(string messageIdentifier, UploadMessageRequestBody uploadMessageRequestBody)
    {
        await MakeRequest<object>("/inApps/v1/messaging/message/" + messageIdentifier, "PUT", null, uploadMessageRequestBody);
    }

    /// <summary>
    /// 删除消息
    /// </summary>
    /// <param name="messageIdentifier">消息唯一标识符</param>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    public async Task DeleteMessage(string messageIdentifier)
    {
        await MakeRequest<object>("/inApps/v1/messaging/message/" + messageIdentifier, "DELETE", null, null);
    }

    /// <summary>
    /// 获取消息列表
    /// </summary>
    /// <returns>已上传消息标识符列表</returns>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    public async Task<GetMessageListResponse> GetMessageList()
    {
        return await MakeRequest<GetMessageListResponse>("/inApps/v1/messaging/message/list", "GET", null, null);
    }

    /// <summary>
    /// 配置默认消息
    /// </summary>
    /// <param name="productId">App 内购买产品 ID</param>
    /// <param name="locale">语言地区代码，例如 zh-Hans、en-US</param>
    /// <param name="defaultConfigurationRequest">默认消息配置，含消息标识符、展示优先级等</param>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    public async Task ConfigureDefaultMessage(string productId, string locale, DefaultConfigurationRequest defaultConfigurationRequest)
    {
        await MakeRequest<object>("/inApps/v1/messaging/default/" + productId + "/" + locale, "PUT", null, defaultConfigurationRequest);
    }

    /// <summary>
    /// 删除默认消息
    /// </summary>
    /// <param name="productId">App 内购买产品 ID</param>
    /// <param name="locale">语言地区代码</param>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    public async Task DeleteDefaultMessage(string productId, string locale)
    {
        await MakeRequest<object>("/inApps/v1/messaging/default/" + productId + "/" + locale, "DELETE", null, null);
    }

    /// <summary>
    /// 获取 App 交易信息
    /// </summary>
    /// <param name="transactionId">交易 ID</param>
    /// <returns>App 级交易信息，含签名、票据等</returns>
    /// <exception cref="ApiException">当 Apple 返回错误时抛出</exception>
    public async Task<AppTransactionInfoResponse> GetAppTransactionInfo(string transactionId)
    {
        return await MakeRequest<AppTransactionInfoResponse>("/inApps/v1/transactions/appTransactions/" + transactionId, "GET", null, null);
    }

    /// <summary>
    /// 创建 JWT Bearer 令牌（ECDSA 签名）
    /// 注意：私钥需为 PEM 格式；若仅提供 Base64 内容，将自动补全 PEM 头尾
    /// </summary>
    /// <returns>可用于 Authorization: Bearer 的 JWT 字符串，有效期 20 分钟（Apple 官方规定的 JWT 有效期上限）</returns>
    /// <exception cref="CryptographicException">当私钥格式非法时抛出</exception>
    public string CreateBearerToken()
    {
        using (var prvKey = ECDsa.Create())
        {
            prvKey.ImportFromPem(_signingKey);

            var securityDescriptor = new SecurityTokenDescriptor
            {
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(20),
                Claims = new Dictionary<string, object>
                {
                    { "iss", _issuerId },
                    { "aud", "appstoreconnect-v1" },
                    { "bid", _bundleId },
                },
                TokenType = "JWT",
            };

            var securityKey = new ECDsaSecurityKey(prvKey) { KeyId = _keyId, };

            securityDescriptor.SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256);

            return new JsonWebTokenHandler().CreateToken(securityDescriptor);
        }
    }
}