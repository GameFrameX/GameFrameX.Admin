namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// 协议域认证授权错误码（段 201，见 <see cref="ErrorSegmentRegistry"/>）。
/// </summary>
public enum AuthErrorCode
{
    /// <summary>
    /// 授权无效
    /// </summary>
    [ErrorText("授权无效", "Invalid authorization")]
    AuthorizeInvalid = 20100001,

    /// <summary>
    /// 令牌无效
    /// </summary>
    [ErrorText("令牌无效", "Invalid token")] TokenInvalid = 20100002,

    /// <summary>
    /// 令牌已过期
    /// </summary>
    [ErrorText("令牌已过期", "Token expired")] TokenExpired = 20100003,

    /// <summary>
    /// 未授权
    /// </summary>
    [ErrorText("未授权", "Unauthorized")] Unauthorized = 20100004,

    /// <summary>
    /// 签名验证失败
    /// </summary>
    [ErrorText("签名验证失败", "Signature verification failed")]
    SignatureVerifyFailed = 20100005,
}