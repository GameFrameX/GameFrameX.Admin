using GameFrameX.ObjectStorage.Abstract;

namespace GameFrameX.ObjectStorage.TencentCos;

/// <summary>
/// 腾讯云 COS 选项。SecretKey 为敏感字段。
/// </summary>
[ObjectStorageProvider("tencent-cos", "腾讯云 COS")]
public class TencentCosOptionSetting : IObjectStorageOptionSetting
{
    /// <summary>地域，如 ap-guangzhou。</summary>
    [ObjectStorageField(Label = "地域 Region", Required = true, Help = "例如 ap-guangzhou")]
    public string Region { get; set; } = null!;

    /// <summary>Bucket，格式 名称-APPID。</summary>
    [ObjectStorageField(Label = "Bucket", Required = true, Help = "格式：名称-APPID")]
    public string Bucket { get; set; } = null!;

    /// <summary>SecretId。</summary>
    [ObjectStorageField(Label = "SecretId", Required = true)]
    public string SecretId { get; set; } = null!;

    /// <summary>SecretKey（敏感）。</summary>
    [ObjectStorageField(Label = "SecretKey", Required = true, Sensitive = true)]
    public string SecretKey { get; set; } = null!;

    /// <summary>公共访问域名（可选）。</summary>
    [ObjectStorageField(Label = "公共访问域名", Help = "可选，例如 https://cdn.example.com")]
    public string PublicBaseUrl { get; set; } = null!;
}