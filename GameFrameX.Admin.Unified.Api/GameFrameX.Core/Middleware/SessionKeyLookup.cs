namespace GameFrameX.Core.Middleware;

/// <summary>
/// 单把会话密钥的查找结果。<see cref="Result"/> 为 <see cref="SessionKeyLookupResult.Active"/> 时
/// <see cref="Secret"/> 为可用密钥明文（64 位 hex），否则为 null（禁用/租约外的密钥不向解密路径提供明文）。
/// </summary>
/// <param name="Result">租约/启用判定结果。</param>
/// <param name="Kid">Key ID（信封 kid 字段）。</param>
/// <param name="Secret">会话密钥明文（64 位 hex）；仅 Active 时非空。</param>
public sealed record SessionKeyLookup(SessionKeyLookupResult Result, string Kid, string? Secret);