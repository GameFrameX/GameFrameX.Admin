using System.Globalization;

namespace GameFrameX.Core.Exceptions;

/// <summary>
/// Admin 业务异常，承载统一数字错误码、用户消息和建议 HTTP 状态。
/// </summary>
public sealed class BusinessException : Exception
{
    /// <summary>
    /// 以模块错误码枚举创建业务异常（统一错误码体系唯一入口，见 ErrorSegmentRegistry）。
    /// message 缺省时由 ErrorLocalizer 按请求语言解析文案（游戏生态默认中文）。
    /// </summary>
    public BusinessException(global::System.Enum code, string? message = null, int statusCode = (int)global::System.Net.HttpStatusCode.BadRequest, Exception? innerException = null)
        : base(message ?? Localization.Errors.ErrorLocalizer.Localize(code), innerException)
    {
        ErrorCode = code;
        Code = Convert.ToInt32(code, CultureInfo.InvariantCulture);
        HttpStatusCode = statusCode;
    }

    /// <summary>
    /// 以纯文本消息创建业务异常（无稳定错误码场景；Code=0，出站时回退为 HTTP 状态语义）。
    /// </summary>
    public BusinessException(string message, int statusCode = (int)global::System.Net.HttpStatusCode.BadRequest, Exception? innerException = null)
        : base(message, innerException)
    {
        HttpStatusCode = statusCode;
    }

    /// <summary>
    /// 业务错误码枚举（见 GameFrameX.Core.Enum.Errors 命名空间）；纯文本构造路径为 null。
    /// </summary>
    public global::System.Enum? ErrorCode { get; }

    /// <summary>
    /// 统一数字错误码（模块枚举的 8 位数值）；纯文本构造路径为 0，出站时回退为 HTTP 状态语义。
    /// </summary>
    public int Code { get; }

    /// <summary>
    /// Admin 统一响应应使用的 HTTP 状态码。
    /// </summary>
    public int HttpStatusCode { get; private set; }

    /// <summary>
    /// 设置建议 HTTP 状态码，并返回当前异常以兼容历史链式调用。
    /// </summary>
    public BusinessException StatusCode(int statusCode)
    {
        HttpStatusCode = statusCode;
        return this;
    }
}