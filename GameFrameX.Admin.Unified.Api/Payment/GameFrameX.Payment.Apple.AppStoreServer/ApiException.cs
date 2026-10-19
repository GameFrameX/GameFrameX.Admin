namespace GameFrameX.Payment.Apple.AppStoreServer;

/// <summary>
/// App Store Server API 异常类
/// 用于封装与 App Store Server API 交互过程中发生的错误信息
/// </summary>
public sealed class ApiException : Exception
{
    /// <summary>
    /// HTTP 响应状态码
    /// </summary>
    public int HttpStatusCode { get; }

    /// <summary>
    /// API 返回的错误码（可选）
    /// </summary>
    public int? ApiError { get; }

    /// <summary>
    /// 错误描述信息（可能为 null，如未从响应体解析到 errorMessage 时）
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// 初始化 ApiException 实例
    /// </summary>
    /// <param name="httpStatusCode">HTTP 状态码</param>
    /// <param name="apiError">API 错误码（可选）</param>
    /// <param name="errorMessage">错误描述信息（可选）</param>
    public ApiException(int httpStatusCode, int? apiError = null, string? errorMessage = null) : base(errorMessage ?? $"HTTP {httpStatusCode}" + (apiError.HasValue ? $", API Error: {apiError}" : ""))
    {
        HttpStatusCode = httpStatusCode;
        ApiError = apiError;
        ErrorMessage = errorMessage;
    }
}