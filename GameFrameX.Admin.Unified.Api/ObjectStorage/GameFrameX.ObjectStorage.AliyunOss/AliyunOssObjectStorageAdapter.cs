using Aliyun.OSS;
using Aliyun.OSS.Common;
using GameFrameX.ObjectStorage.Abstract;

namespace GameFrameX.ObjectStorage.AliyunOss;

/// <summary>
/// 阿里云 OSS Adapter，内部使用阿里云官方 .NET SDK。
/// </summary>
public class AliyunOssObjectStorageAdapter : IObjectStorageAdapter
{
    private static AliyunOssOptionSetting Option(ObjectStorageAdapterContext context)
    {
        return (AliyunOssOptionSetting)context.Options;
    }

    private static string Bucket(ObjectStorageAdapterContext context)
    {
        var opt = Option(context);
        return string.IsNullOrWhiteSpace(context.Bucket) ? opt.BucketName : context.Bucket;
    }

    private static string Key(string objectKey)
    {
        return (objectKey ?? string.Empty).TrimStart('/');
    }

    private static OssClient CreateClient(AliyunOssOptionSetting opt)
    {
        return new OssClient(opt.Endpoint, opt.AccessKeyId, opt.AccessKeySecret);
    }

    private static bool IsAccessDenied(OssException ex)
    {
        return string.Equals(ex.ErrorCode, "AccessDenied", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsObjectMissing(OssException ex)
    {
        return string.Equals(ex.ErrorCode, "NoSuchKey", StringComparison.OrdinalIgnoreCase)
               || string.Equals(ex.ErrorCode, "NoSuchObject", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public Task PutAsync(ObjectStorageAdapterContext context, Stream content, string objectKey, string contentType, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var opt = Option(context);
            var metadata = new ObjectMetadata
            {
                ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            };
            CreateClient(opt).PutObject(Bucket(context), Key(objectKey), content, metadata);
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Stream> OpenReadAsync(ObjectStorageAdapterContext context, string objectKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.Run<Stream>(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var opt = Option(context);
            var ossObject = CreateClient(opt).GetObject(Bucket(context), Key(objectKey));
            return ossObject.Content;
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
            try
            {
                CreateClient(opt).DeleteObject(Bucket(context), Key(objectKey));
            }
            catch (OssException ex) when (IsObjectMissing(ex))
            {
                // OSS 删除语义按接口约定处理：对象已不存在即视为成功。
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
                return CreateClient(opt).DoesBucketExist(Bucket(context));
            }
            catch (OssException ex) when (IsAccessDenied(ex))
            {
                return true;
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
        var opt = Option(context);
        var baseUrl = string.IsNullOrWhiteSpace(context.PublicBaseUrl)
                          ? "https://" + Bucket(context) + "." + opt.Endpoint
                          : context.PublicBaseUrl.TrimEnd('/');
        return baseUrl + "/" + Key(objectKey);
    }
}