using System.Security.Cryptography;
using System.Text;

namespace GameFrameX.ObjectStorage.Abstract;

/// <summary>
/// 对象存储历史 REST 签名工具：HMAC-SHA1、Base64Url、SHA1 十六进制。
/// 当前内置云厂商 Adapter 已改用官方 SDK，本类型仅保留给外部兼容代码。
/// </summary>
public static class ObjectStorageSignHelper
{
    /// <summary>计算 HMAC-SHA1 并返回 Base64 字符串。</summary>
    public static string HmacSha1Base64(string key, string data)
    {
        return Convert.ToBase64String(HmacSha1(key, data));
    }

    /// <summary>计算 HMAC-SHA1 字节。</summary>
    public static byte[] HmacSha1(string key, string data)
    {
        using (var alg = new HMACSHA1(Encoding.UTF8.GetBytes(key ?? string.Empty)))
        {
            return alg.ComputeHash(Encoding.UTF8.GetBytes(data ?? string.Empty));
        }
    }

    /// <summary>计算 HMAC-SHA1 并返回小写十六进制字符串（COS v5 用）。</summary>
    public static string HmacSha1Hex(string key, string data)
    {
        return ToHexLower(HmacSha1(key, data));
    }

    /// <summary>计算 SHA1 十六进制字符串（COS v5 用）。</summary>
    public static string Sha1Hex(string data)
    {
        using (var sha1 = SHA1.Create())
        {
            return ToHexLower(sha1.ComputeHash(Encoding.UTF8.GetBytes(data ?? string.Empty)));
        }
    }

    /// <summary>URL 安全 Base64（Qiniu 用）。</summary>
    public static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    /// <summary>URL 安全 Base64（Qiniu 用）。</summary>
    public static string Base64UrlEncode(string data)
    {
        return Base64UrlEncode(Encoding.UTF8.GetBytes(data ?? string.Empty));
    }

    private static string ToHexLower(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }
}