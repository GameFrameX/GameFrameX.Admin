using GameFrameX.ObjectStorage.Abstract;

namespace GameFrameX.ObjectStorage.AliyunOss;

/// <summary>
/// 阿里云 OSS 选项。AccessKeySecret 为敏感字段，脱敏展示，提交 ****** 时保留原值。
/// </summary>
[ObjectStorageProvider("aliyun", "阿里云 OSS")]
public class AliyunOssOptionSetting : IObjectStorageOptionSetting
{
    /// <summary>Endpoint，如 oss-cn-hangzhou.aliyuncs.com。</summary>
    [ObjectStorageField(Label = "Endpoint", Required = true, Help = "不带 Bucket 的访问域名，例如 oss-cn-hangzhou.aliyuncs.com")]
    public string Endpoint { get; set; } = null!;

    /// <summary>Bucket 名称。</summary>
    [ObjectStorageField(Label = "Bucket", Required = true)]
    public string BucketName { get; set; } = null!;

    /// <summary>AccessKeyId。</summary>
    [ObjectStorageField(Label = "AccessKeyId", Required = true)]
    public string AccessKeyId { get; set; } = null!;

    /// <summary>AccessKeySecret（敏感）。</summary>
    [ObjectStorageField(Label = "AccessKeySecret", Required = true, Sensitive = true)]
    public string AccessKeySecret { get; set; } = null!;

    /// <summary>公共访问域名（可选）。填写后下载/预览使用该域名拼接，否则回退到内网 Endpoint。</summary>
    [ObjectStorageField(Label = "公共访问域名", Help = "可选，例如 https://cdn.example.com")]
    public string PublicBaseUrl { get; set; } = null!;
}