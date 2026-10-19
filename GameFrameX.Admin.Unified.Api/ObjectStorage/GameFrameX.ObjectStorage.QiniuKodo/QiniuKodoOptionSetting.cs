using GameFrameX.ObjectStorage.Abstract;

namespace GameFrameX.ObjectStorage.QiniuKodo;

/// <summary>
/// 七牛云 Kodo 选项。SecretKey 为敏感字段。
/// </summary>
[ObjectStorageProvider("qiniu", "七牛云 Kodo")]
public class QiniuKodoOptionSetting : IObjectStorageOptionSetting
{
    /// <summary>对象存储空间名。</summary>
    [ObjectStorageField(Label = "Bucket", Required = true)]
    public string Bucket { get; set; } = null!;

    /// <summary>AccessKey。</summary>
    [ObjectStorageField(Label = "AccessKey", Required = true)]
    public string AccessKey { get; set; } = null!;

    /// <summary>SecretKey（敏感）。</summary>
    [ObjectStorageField(Label = "SecretKey", Required = true, Sensitive = true)]
    public string SecretKey { get; set; } = null!;

    /// <summary>绑定的访问域名（必填，七牛下载需经绑定域名）。</summary>
    [ObjectStorageField(Label = "绑定域名", Required = true, Help = "例如 https://cdn.example.com")]
    public string PublicBaseUrl { get; set; } = null!;

    /// <summary>存储区域（可选，部分场景用于选择上传入口）。</summary>
    [ObjectStorageField(Label = "区域", Help = "可选，例如 z0 / 华东")]
    public string Region { get; set; } = null!;
}