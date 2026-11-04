using System.Text.Json;
using GameFrameX.Foundation.Http.Extension;
using GameFrameX.Foundation.Http.Normalization;

namespace GameFrameX.Application.ServerCenter.Online;

/// <summary>
/// Online 管理客户端，按区服解析地址并注入区服标识，保留信封解包和协议错误映射。
/// </summary>
public sealed class OnlineServerClient : IOnlineServerClient
{
    private readonly HttpClient _httpClient;
    private readonly IOnlineServerEndpointResolver _endpointResolver;
    private readonly SqlSugarRepository<TenantGameAreaEntity> _gameAreaRepository;

    public OnlineServerClient(HttpClient httpClient,
        IOnlineServerEndpointResolver endpointResolver,
        SqlSugarRepository<TenantGameAreaEntity> gameAreaRepository)
    {
        _httpClient = httpClient;
        _endpointResolver = endpointResolver;
        _gameAreaRepository = gameAreaRepository;
    }

    /// <inheritdoc />
    public async Task<TResponse> PostAsync<TResponse>(string action, object request, OnlineScopeContext scope) where TResponse : class
    {
        var url = await ResolveUrlAsync(action, scope);
        var payload = MergeScopeIntoPayload(request, scope);

        var content = await _httpClient.PostJsonToStringAsync(url, payload);
        return Unwrap<TResponse>(content, url);
    }

    /// <inheritdoc />
    public async Task<TResponse> GetAsync<TResponse>(string action, OnlineScopeContext scope) where TResponse : class
    {
        var url = await ResolveUrlAsync(action, scope);
        // S3/C401 断链修复点
        var urlWithScope = url + (url.Contains('?') ? "&" : "?")
            + "serverId=" + scope.ServerId;

        var content = await _httpClient.GetToStringAsync(urlWithScope);
        return Unwrap<TResponse>(content, url);
    }

    /// <summary>
    /// 按 <c>scope.ServerId</c> 取区服并解析 Online 管理 URL。区服查不到（已被删除等）按作用域非法拒绝，不落到裸构造。
    /// </summary>
    private async Task<string> ResolveUrlAsync(string action, OnlineScopeContext scope)
    {
        var gameArea = await _gameAreaRepository.GetFirstAsync(u => u.Id == scope.ServerId);
        if (gameArea == null)
        {
            throw BusinessError.Oh(OnlineErrorCode.ServerScopeDenied, new { serverId = scope.ServerId, });
        }

        return _endpointResolver.Resolve(gameArea, action);
    }

    /// <summary>
    /// 双层解包（Foundation <see cref="HttpJsonResultHelper.ToHttpJsonResult{T}"/> 一步完成，同 MailCampaignServerClient 范式）：
    /// 传输信封（失败抛 InvalidOperationException）→ 信封 data 字符串反序列化为 Online 业务响应体
    /// （<see cref="OnlineApiResponse{T}"/>，Code 非 0 映射镜像码抛 BusinessError）。
    /// </summary>
    private static TResponse Unwrap<TResponse>(string content, string url) where TResponse : class
    {
        var envelope = content.ToHttpJsonResult<OnlineApiResponse<TResponse>>();
        if (!envelope.IsSuccess)
        {
            throw new InvalidOperationException($"调用 Online 管理接口失败（{url}）：{envelope.Message}");
        }

        var response = envelope.Data;
        if (response == null)
        {
            throw new InvalidOperationException($"Online 管理接口响应体为空或不可解析（{url}）");
        }

        if (response.Code != 0)
        {
            // Online 非 0 协议码 → 3xx 镜像段按段级映射抛 BusinessError；原始协议码与 MessageKey 随 errors 透出供前端按数字码展示/排查。
            throw BusinessError.Oh(MapToMirrorCode(response.Code),
                new { onlineCode = response.Code, messageKey = response.MessageKey, requestId = response.RequestId, });
        }

        if (response.Data == null)
        {
            throw new InvalidOperationException($"Online 管理接口成功响应缺少 Data 载荷（{url}）");
        }

        return response.Data;
    }

    /// <summary>区服标识并入请求体，业务载荷不能覆盖已验证的区服。</summary>
    private static Dictionary<string, object?> MergeScopeIntoPayload(object request, OnlineScopeContext scope)
    {
        var merged = new Dictionary<string, object?>(StringComparer.Ordinal);
        var payload = JsonSerializer.SerializeToElement(request);
        if (payload.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in payload.EnumerateObject())
            {
                if (string.Equals(property.Name, "TenantId", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(property.Name, "AppId", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(property.Name, "ServerId", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                merged[property.Name] = property.Value;
            }
        }

        // S3/C401 断链修复点
        merged["ServerId"] = scope.ServerId;
        return merged;
    }

    /// <summary>
    /// 「Online 协议码 ↔ Admin 镜像码」段级映射：协议段 Nxxx ↔ 镜像段 32(N-1)（vault 八段一一镜像，见 <see cref="OnlineErrorCode"/> 注释）。
    /// 段内未知详表成员按段级代表成员映射；协议段外未知码兜底系统段（VC-1.10 未知码不崩溃、不误判成功）。
    /// 镜像数值不可手改，调整须回 vault 评审后三端同步。
    /// </summary>
    private static OnlineErrorCode MapToMirrorCode(int onlineCode)
    {
        if (onlineCode >= 1000 && onlineCode < 2000)
        {
            return OnlineErrorCode.InternalError;
        }

        if (onlineCode >= 2000 && onlineCode < 3000)
        {
            return OnlineErrorCode.SessionInvalid;
        }

        if (onlineCode >= 3000 && onlineCode < 4000)
        {
            return OnlineErrorCode.ScopeDenied;
        }

        if (onlineCode >= 4000 && onlineCode < 5000)
        {
            return OnlineErrorCode.ParameterInvalid;
        }

        if (onlineCode >= 5000 && onlineCode < 6000)
        {
            return OnlineErrorCode.StateOperationForbidden;
        }

        if (onlineCode >= 6000 && onlineCode < 7000)
        {
            return OnlineErrorCode.DuplicateRequest;
        }

        if (onlineCode >= 7000 && onlineCode < 8000)
        {
            return OnlineErrorCode.RateLimitExceeded;
        }

        if (onlineCode >= 8000 && onlineCode < 9000)
        {
            return OnlineErrorCode.ServiceBusy;
        }

        return OnlineErrorCode.InternalError;
    }
}
