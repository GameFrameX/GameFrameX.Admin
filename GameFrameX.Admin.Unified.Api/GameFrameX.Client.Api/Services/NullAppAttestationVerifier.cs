namespace GameFrameX.Client.Api.Services;

/// <summary>
/// 应用校验器空实现。
/// <para>首版不真正校验签名 / 设备证明；调用即抛 <see cref="NotImplementedException"/>，避免被误用为"始终通过"的默认实现。</para>
/// <para>完整实施由衍生 change 提供真实实现并替换注册。</para>
/// </summary>
public class NullAppAttestationVerifier : IAppAttestationVerifier
{
    /// <inheritdoc />
    public Task<bool> VerifyAsync(long appId, HttpContext context)
    {
        throw new NotImplementedException("应用签名/设备证明校验由衍生 change 实现");
    }
}