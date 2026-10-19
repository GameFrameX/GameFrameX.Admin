using COSXML;
using COSXML.Auth;
using COSXML.CosException;
using COSXML.Model.Bucket;
using COSXML.Model.Object;
using GameFrameX.ObjectStorage.Abstract;

namespace GameFrameX.ObjectStorage.TencentCos;

/// <summary>
/// 腾讯云 COS Adapter，内部使用腾讯云官方 COS XML .NET SDK。
/// </summary>
public class TencentCosObjectStorageAdapter : IObjectStorageAdapter
{
    private static TencentCosOptionSetting Option(ObjectStorageAdapterContext context)
    {
        return (TencentCosOptionSetting)context.Options;
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

    private static string Host(ObjectStorageAdapterContext context)
    {
        var opt = Option(context);
        return Bucket(context) + ".cos." + opt.Region + ".myqcloud.com";
    }

    private static CosXmlServer CreateServer(TencentCosOptionSetting opt)
    {
        var config = new CosXmlConfig.Builder()
                     .SetRegion(opt.Region)
                     .IsHttps(true)
                     .SetDebugLog(false)
                     .Build();
        var credentialProvider = new DefaultQCloudCredentialProvider(opt.SecretId, opt.SecretKey, 600);
        return new CosXmlServer(config, credentialProvider);
    }

    /// <inheritdoc />
    public async Task PutAsync(ObjectStorageAdapterContext context, Stream content, string objectKey, string contentType, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var opt = Option(context);
        var request = new PutObjectRequest(Bucket(context), Key(objectKey), content);
        request.SetRequestHeader("Content-Type", string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType);
        await CreateServer(opt).ExecuteAsync<PutObjectResult>(request);
        cancellationToken.ThrowIfCancellationRequested();
    }

    /// <inheritdoc />
    public async Task<Stream> OpenReadAsync(ObjectStorageAdapterContext context, string objectKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var opt = Option(context);
        var request = new GetObjectBytesRequest(Bucket(context), Key(objectKey));
        var result = await CreateServer(opt).ExecuteAsync<GetObjectBytesResult>(request);
        cancellationToken.ThrowIfCancellationRequested();
        return new MemoryStream(result.content ?? Array.Empty<byte>(), false);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(ObjectStorageAdapterContext context, string objectKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var opt = Option(context);
        var request = new DeleteObjectRequest(Bucket(context), Key(objectKey));
        try
        {
            await CreateServer(opt).ExecuteAsync<DeleteObjectResult>(request);
        }
        catch (CosServerException ex) when (ex.statusCode == 404)
        {
            // COS 删除语义按接口约定处理：对象已不存在即视为成功。
        }

        cancellationToken.ThrowIfCancellationRequested();
    }

    /// <inheritdoc />
    public async Task<bool> TestConnectionAsync(ObjectStorageAdapterContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var opt = Option(context);
            var result = await CreateServer(opt).ExecuteAsync<HeadBucketResult>(new HeadBucketRequest(Bucket(context)));
            cancellationToken.ThrowIfCancellationRequested();
            return result.IsSuccessful();
        }
        catch (CosServerException ex) when (ex.statusCode == 403)
        {
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public string BuildPublicUrl(ObjectStorageAdapterContext context, string objectKey)
    {
        var baseUrl = string.IsNullOrWhiteSpace(context.PublicBaseUrl)
                          ? "https://" + Host(context)
                          : context.PublicBaseUrl.TrimEnd('/');
        return baseUrl + "/" + Key(objectKey);
    }
}