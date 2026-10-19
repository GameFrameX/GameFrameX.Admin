using GameFrameX.Entity.Enum;

namespace GameFrameX.Client.Api.Integrations.Advertisement;

/// <summary>
/// 广告奖励回调处理结果。
/// <para><c>Code</c> 约定：验签失败 <c>601</c>，其他失败 <c>0</c> 且 <see cref="IsValid"/>=false，成功 <c>0</c> 且 <see cref="IsValid"/>=true。</para>
/// <para>注：change.md §3 以 record 形式描述，实现遵循项目 C# 8.0 语法上限改为 sealed class，字段集合（5 项）与类型保持一致。</para>
/// </summary>
public sealed class AdvertisementRewardReceiveResult
{
    /// <summary>
    /// </summary>
    public AdvertisementRewardReceiveResult(bool isValid, int code, string message, string? idempotencyKey, AdvertisementRewardStatusEnum? status)
    {
        IsValid = isValid;
        Code = code;
        Message = message;
        IdempotencyKey = idempotencyKey;
        Status = status;
    }

    /// <summary>
    /// 处理是否有效（成功）。
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Code：验签失败 601，其他 0。
    /// </summary>
    public int Code { get; }

    /// <summary>
    /// 消息。
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// 幂等键。
    /// </summary>
    public string? IdempotencyKey { get; }

    /// <summary>
    /// 账本状态。
    /// </summary>
    public AdvertisementRewardStatusEnum? Status { get; }

    /// <summary>
    /// 序列化为既有回调 JSON 形状（code / isValid / msg / idempotencyKey / status）。
    /// </summary>
    public object ToAnonymous()
    {
        return new
        {
            code = Code,
            isValid = IsValid,
            msg = Message,
            idempotencyKey = IdempotencyKey,
            status = Status?.ToString(),
        };
    }
}