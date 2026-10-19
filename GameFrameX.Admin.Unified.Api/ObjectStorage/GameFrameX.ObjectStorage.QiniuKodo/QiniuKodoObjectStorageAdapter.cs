using GameFrameX.ObjectStorage.Abstract;
using Qiniu.Storage;
using Qiniu.Util;

namespace GameFrameX.ObjectStorage.QiniuKodo;

/// <summary>
/// 七牛云 Kodo Adapter，内部使用七牛官方 .NET SDK。
/// </summary>
public class QiniuKodoObjectStorageAdapter : IObjectStorageAdapter
{
    private static QiniuKodoOptionSetting Option(ObjectStorageAdapterContext context)
    {
        return (QiniuKodoOptionSetting)context.Options;
    }

    private static string Bucket(ObjectStorageAdapterContext context)
    {
        var opt = Option(context);
        return string.IsNullOrWhiteSpace(context.Bucket) ? opt.Bucket : context.Bucket;
    }

    private static string Key(string objectKey)
    {
        return (objectKey ?? string.Empty).TrimStart('/');
    }

    private static Mac CreateMac(QiniuKodoOptionSetting opt)
    {
        return new Mac(opt.AccessKey, opt.SecretKey);
    }

    private static Config CreateConfig(QiniuKodoOptionSetting opt)
    {
        var config = new Config
        {
            UseHttps = true,
        };

        config.Zone = NormalizeRegion(opt.Region) switch
        {
            "z0" or "huadong" or "cn-east"         => Zone.ZONE_CN_East,
            "cn-east-2"                            => Zone.ZONE_CN_East_2,
            "z1" or "huabei" or "cn-north"         => Zone.ZONE_CN_North,
            "z2" or "huanan" or "cn-south"         => Zone.ZONE_CN_South,
            "na0" or "us-north"                    => Zone.ZONE_US_North,
            "as0" or "ap-southeast" or "singapore" => Zone.ZONE_AS_Singapore,
            _                                      => config.Zone,
        };

        return config;
    }

    private static string NormalizeRegion(string region)
    {
        return (region ?? string.Empty).Trim().ToLowerInvariant().Replace("_", "-");
    }

    private static string CreateUploadToken(QiniuKodoOptionSetting opt, string bucket)
    {
        var policy = new PutPolicy
        {
            Scope = bucket,
        };
        policy.SetExpires(3600);
        return new Auth(CreateMac(opt), new AuthOptions()).CreateUploadToken(policy.ToJsonString());
    }

    private static BucketManager CreateBucketManager(QiniuKodoOptionSetting opt)
    {
        return new BucketManager(CreateMac(opt), CreateConfig(opt), new AuthOptions());
    }

    private static string PublicBaseUrl(ObjectStorageAdapterContext context)
    {
        return (context.PublicBaseUrl ?? string.Empty).TrimEnd('/');
    }

    /// <inheritdoc />
    public Task PutAsync(ObjectStorageAdapterContext context, Stream content, string objectKey, string contentType, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var opt = Option(context);
            var result = new UploadManager(CreateConfig(opt)).UploadStream(
                content,
                Key(objectKey),
                CreateUploadToken(opt, Bucket(context)),
                new PutExtra
                {
                    MimeType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
                });
            if (result.Code != 200)
            {
                throw new InvalidOperationException("七牛 Kodo 上传失败：" + result.Code + " " + result.Text);
            }
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Stream> OpenReadAsync(ObjectStorageAdapterContext context, string objectKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.Run<Stream>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var baseUrl = PublicBaseUrl(context);
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new InvalidOperationException("七牛 Kodo 未配置绑定域名，无法读取");
            }

            var url = DownloadManager.CreatePublishUrl(baseUrl, Key(objectKey));
            var tempFile = Path.Combine(Path.GetTempPath(), "gfx-objectstorage-" + Guid.NewGuid().ToString("N"));
            var result = DownloadManager.Download(url, tempFile);
            if (result.Code != 200)
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }

                throw new InvalidOperationException("七牛 Kodo 读取失败：" + result.Code + " " + result.Text);
            }

            return new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task DeleteAsync(ObjectStorageAdapterContext context, string objectKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var opt = Option(context);
            var result = CreateBucketManager(opt).Delete(Bucket(context), Key(objectKey));
            if (result.Code != 200 && result.Code != 612)
            {
                throw new InvalidOperationException("七牛 Kodo 删除失败：" + result.Code + " " + result.Text);
            }
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> TestConnectionAsync(ObjectStorageAdapterContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.Run(() =>
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var opt = Option(context);
                var result = CreateBucketManager(opt).Stat(Bucket(context), ".probe");
                return result.Code == 200 || result.Code == 612 || result.Code == 404;
            }
            catch
            {
                return false;
            }
        }, cancellationToken);
    }

    /// <inheritdoc />
    public string BuildPublicUrl(ObjectStorageAdapterContext context, string objectKey)
    {
        return PublicBaseUrl(context) + "/" + Key(objectKey);
    }
}