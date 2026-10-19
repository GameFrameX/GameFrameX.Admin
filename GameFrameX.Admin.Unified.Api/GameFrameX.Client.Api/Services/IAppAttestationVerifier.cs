namespace GameFrameX.Client.Api.Services;

/// <summary>
/// 应用校验器（请求签名 / 设备证明）。
/// <para>首版不真正校验签名 / 设备证明，仅为 <c>SignatureRequired</c> / <c>DeviceProofRequired</c> 开关提供后续灰度启用的接口锚点。</para>
/// <para>完整签名 / 设备证明密码学实施由衍生 change 承载。</para>
/// </summary>
public interface IAppAttestationVerifier
{
    /// <summary>
    /// 校验当前请求是否满足应用签名 / 设备证明要求。
    /// </summary>
    /// <param name="appId">当前应用 ID</param>
    /// <param name="context">当前 HTTP 上下文</param>
    /// <returns>校验通过返回 true，否则 false</returns>
    Task<bool> VerifyAsync(long appId, HttpContext context);
}