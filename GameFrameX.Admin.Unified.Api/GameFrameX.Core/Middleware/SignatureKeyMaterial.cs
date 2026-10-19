namespace GameFrameX.Core.Middleware;

/// <summary>
/// 参与验签的密钥材料（仅 Kid + 明文密钥，不含租约等元数据——调用方只对生效 key 请求此类型）。
/// </summary>
/// <param name="Kid">Key ID。</param>
/// <param name="Secret">HMAC 密钥明文。</param>
public sealed record SignatureKeyMaterial(string Kid, string Secret);