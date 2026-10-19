namespace GameFrameX.Core.Exceptions;

/// <summary>
/// Admin 业务异常创建入口。
/// </summary>
public static class BusinessError
{
    /// <summary>
    /// 以模块错误码创建业务异常，消息由 ErrorLocalizer 按请求语言解析（单一事实来源）。
    /// </summary>
    public static BusinessException Oh(global::System.Enum code)
    {
        return new BusinessException(code);
    }

    /// <summary>
    /// 以模块错误码和附加错误信息创建业务异常（本地化文案 + 换行 + errors JSON 文本）。
    /// </summary>
    public static BusinessException Oh(global::System.Enum code, object? errors)
    {
        var exception = new BusinessException(code);
        if (errors is not null)
        {
            return new BusinessException(code, $"{exception.Message}{Environment.NewLine}{Json.Serialize(errors)}");
        }

        return exception;
    }

    /// <summary>
    /// 以用户消息创建业务异常（无稳定错误码场景；Code=0，出站时回退为 HTTP 状态语义）。
    /// </summary>
    public static BusinessException Oh(string message)
    {
        return new BusinessException(message);
    }
}