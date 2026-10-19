namespace GameFrameX.Core.Middleware;

/// <summary>
/// 单把签名密钥的查找结果。<see cref="Result"/> 为 <see cref="SignatureKeyLookupResult.Active"/> 时
/// <see cref="Secret"/> 为可用密钥明文，否则为 null（禁用/租约外的密钥不向验签路径提供明文）。
/// </summary>
/// <param name="Result">租约/启用判定结果。</param>
/// <param name="Kid">Key ID。</param>
/// <param name="Secret">密钥明文；仅 Active 时非空。</param>
public sealed record SignatureKeyLookup(SignatureKeyLookupResult Result, string Kid, string? Secret);