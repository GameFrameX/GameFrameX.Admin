namespace GameFrameX.ObjectStorage.Abstract;

/// <summary>
/// 对象存储 Adapter 契约。屏蔽各云厂商 SDK 差异，作为可替换/可测试的接缝。
/// 所有方法全链路传递 CancellationToken；失败抛异常，由调用方决定是否回滚或保留记录。
/// </summary>
public interface IObjectStorageAdapter
{
    /// <summary>
    /// 上传对象。<paramref name="content"/> 由调用方释放。
    /// </summary>
    /// <remarks>
    /// Uploads an object. The <paramref name="content"/> stream is disposed by the caller.
    /// </remarks>
    /// <param name="context">对象存储适配器运行时上下文 / Object storage adapter runtime context</param>
    /// <param name="content">要上传的对象内容流 / Object content stream to upload</param>
    /// <param name="objectKey">对象键，标识对象在存储中的唯一路径 / Object key, identifying the unique path of the object in storage</param>
    /// <param name="contentType">内容的 MIME 类型，如 <c>image/png</c> / MIME type of the content, e.g. <c>image/png</c></param>
    /// <param name="cancellationToken">取消令牌 / Cancellation token</param>
    Task PutAsync(ObjectStorageAdapterContext context, Stream content, string objectKey, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取对象，返回的 <see cref="Stream"/> 由调用方释放。
    /// </summary>
    /// <remarks>
    /// Reads an object. The returned <see cref="Stream"/> is disposed by the caller.
    /// </remarks>
    /// <param name="context">对象存储适配器运行时上下文 / Object storage adapter runtime context</param>
    /// <param name="objectKey">对象键，标识对象在存储中的唯一路径 / Object key, identifying the unique path of the object in storage</param>
    /// <param name="cancellationToken">取消令牌 / Cancellation token</param>
    /// <returns>对象内容的只读流 / Read-only stream of the object content</returns>
    Task<Stream> OpenReadAsync(ObjectStorageAdapterContext context, string objectKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除对象。对象不存在时视为成功。
    /// </summary>
    /// <remarks>
    /// Deletes an object. Treats non-existent objects as success.
    /// </remarks>
    /// <param name="context">对象存储适配器运行时上下文 / Object storage adapter runtime context</param>
    /// <param name="objectKey">对象键，标识对象在存储中的唯一路径 / Object key, identifying the unique path of the object in storage</param>
    /// <param name="cancellationToken">取消令牌 / Cancellation token</param>
    Task DeleteAsync(ObjectStorageAdapterContext context, string objectKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// 连通性测试。成功返回 <c>true</c>，失败抛异常或返回 <c>false</c>。
    /// </summary>
    /// <remarks>
    /// Connectivity test. Returns <c>true</c> on success; throws an exception or returns <c>false</c> on failure.
    /// </remarks>
    /// <param name="context">对象存储适配器运行时上下文 / Object storage adapter runtime context</param>
    /// <param name="cancellationToken">取消令牌 / Cancellation token</param>
    /// <returns>连通成功返回 <c>true</c>；失败返回 <c>false</c> / <c>true</c> if connection succeeds; <c>false</c> on failure</returns>
    Task<bool> TestConnectionAsync(ObjectStorageAdapterContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据 <paramref name="objectKey"/> 构造公共访问 URL。
    /// </summary>
    /// <remarks>
    /// Builds a public access URL based on the <paramref name="objectKey"/>.
    /// </remarks>
    /// <param name="context">对象存储适配器运行时上下文 / Object storage adapter runtime context</param>
    /// <param name="objectKey">对象键，标识对象在存储中的唯一路径 / Object key, identifying the unique path of the object in storage</param>
    /// <returns>对象的公共访问 URL / Public access URL of the object</returns>
    string BuildPublicUrl(ObjectStorageAdapterContext context, string objectKey);
}