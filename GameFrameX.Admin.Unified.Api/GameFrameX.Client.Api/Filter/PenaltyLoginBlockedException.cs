namespace GameFrameX.Client.Api.Filter;

/// <summary>
/// 登录阻断异常：处罚事实源命中封禁/限制登录时抛出，由 <see cref="PenaltyLoginBlockedExceptionFilter"/> 统一转 HttpJsonResultData<BaseOperationAckResponse>。
/// </summary>
public sealed class PenaltyLoginBlockedException : Exception
{
    /// <summary>
    /// 返回给客户端的消息码（对应 <see cref="GameFrameX.Core.Enum.Errors.AccountErrorCode"/>）。
    /// </summary>
    public int Code { get; }

    public PenaltyLoginBlockedException(int code, string message) : base(message)
    {
        Code = code;
    }
}