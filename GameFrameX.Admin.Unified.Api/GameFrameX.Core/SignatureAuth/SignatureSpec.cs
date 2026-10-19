namespace GameFrameX.Core.SignatureAuth;

/// <summary>
/// 传输签名规范常量（C267 单一权威）。消息串格式、字段顺序与分隔符的唯一代码事实源，
/// 服务端验签（<c>SignatureValidationMiddleware</c> / <c>TenantGameAppOpenAccessAuthMiddleware</c>）、
/// 客户端 SDK 与 Postman 预请求脚本均按此对齐。
/// <para>规范正文（唯一权威 spec）：gfx-doc/dev/signature/spec.md。</para>
/// <para>标准来源：RFC 2104（HMAC-SHA256）+ RFC 3986 §2.1/§3.4（percent-encoding + canonical query）
/// + RFC 4648 §4（Base64）+ FIPS 180-4（SHA-256）；消息串风格对齐微信支付 v3（每字段独立 \n 分隔）。</para>
/// </summary>
public static class SignatureSpec
{
    /// <summary>
    /// 协议版本（嵌入 message 首行，防降级；同时是签名头第 1 段）。
    /// </summary>
    public const string ProtocolPrefix = "v2";

    /// <summary>
    /// message 字段分隔符（微信支付 v3 风格：每字段独立一行）。
    /// </summary>
    public const char FieldSeparator = '\n';

    /// <summary>
    /// 算法标识（RFC 2104 HMAC-SHA256）。
    /// </summary>
    public const string HashAlgorithm = "HMAC-SHA256";

    /// <summary>
    /// 签名输出编码（RFC 4648 §4 Base64）。
    /// </summary>
    public const string OutputEncoding = "Base64";

    /// <summary>
    /// 服务密钥最小熵（字节，256-bit）。
    /// </summary>
    public const int SecretMinBytes = 32;

    /// <summary>
    /// 传输签名单头名（与微信支付 v3 一致；注意与 cola 客户端历史 <c>GameFrameX-Signature</c> 头是两个不同的头）。
    /// </summary>
    public const string SignatureHeader = "Signature";

    /// <summary>
    /// 签名头格式：<c>v2;{keyId};{ts};{mid};{base64sig}</c>（Hub OpenAccess 场景 accessKey 充当 keyId、nonce 充当 mid）。
    /// </summary>
    public const string SignatureHeaderFormat = "{ver};{keyId};{ts};{mid};{sig}";
}