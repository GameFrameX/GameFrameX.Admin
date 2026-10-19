namespace GameFrameX.Advertisement.Abstract;

/// <summary>
/// 验签结果 + 已解析的强类型 Option（Option 透传给角色解析，避免二次读配置）。
/// </summary>
public sealed class OptionVerification
{
    public VerifiedPayload Payload { get; set; } = null!;
    public object Option { get; set; } = null!;

#pragma warning disable CS8601 // Option 属性声明非空，但 Invalid 工厂允许 option 为 null（验签前置失败时 Option 未解析）
    public static OptionVerification Invalid(object? option)
    {
        return new OptionVerification { Payload = VerifiedPayload.Invalid(), Option = option, };
    }
#pragma warning restore CS8601

    public static OptionVerification Valid(object option, VerifiedPayload payload)
    {
        return new OptionVerification { Payload = payload, Option = option, };
    }
}