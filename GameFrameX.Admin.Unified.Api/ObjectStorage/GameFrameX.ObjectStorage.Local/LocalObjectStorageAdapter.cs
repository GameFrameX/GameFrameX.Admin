using GameFrameX.ObjectStorage.Abstract;
using Microsoft.AspNetCore.Hosting;

namespace GameFrameX.ObjectStorage.Local;

/// <summary>
/// 本地磁盘对象存储 Adapter。文件写入应用 wwwroot 下受控根目录，
/// objectKey 以 '/' 分隔，物理路径按操作系统分隔符拼接。
/// </summary>
public class LocalObjectStorageAdapter : IObjectStorageAdapter
{
    private static IWebHostEnvironment? WebHostEnvironment;

    public static void Configure(IWebHostEnvironment webHostEnvironment)
    {
        WebHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
    }

    private static string ResolveWebRootPath()
    {
        var root = WebHostEnvironment?.WebRootPath;
        if (!string.IsNullOrWhiteSpace(root))
        {
            return root;
        }

        var contentRoot = WebHostEnvironment?.ContentRootPath;
        if (!string.IsNullOrWhiteSpace(contentRoot))
        {
            return Path.Combine(contentRoot, "wwwroot");
        }

        return AppContext.BaseDirectory;
    }

    private static string ResolvePhysicalPath(string objectKey)
    {
        var root = ResolveWebRootPath();
        var relative = (objectKey ?? string.Empty).Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
        // 纵深防御：解析后必须仍落在受控根内，拦截一切逃逸向量（含上层未拦到的 ".." 或绝对片段）。
        var fullRoot = Path.GetFullPath(root);
        var rootWithSep = fullRoot.EndsWith(Path.DirectorySeparatorChar) ? fullRoot : fullRoot + Path.DirectorySeparatorChar;
        var full = Path.GetFullPath(Path.Combine(fullRoot, relative));
        if (!full.StartsWith(rootWithSep, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("对象键逃逸受控根目录，拒绝访问：" + objectKey);
        }

        return full;
    }

    /// <inheritdoc />
    public async Task PutAsync(ObjectStorageAdapterContext context, Stream content, string objectKey, string contentType, CancellationToken cancellationToken = default)
    {
        var path = ResolvePhysicalPath(objectKey);
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await content.CopyToAsync(fs, cancellationToken);
        }
    }

    /// <inheritdoc />
    public Task<Stream> OpenReadAsync(ObjectStorageAdapterContext context, string objectKey, CancellationToken cancellationToken = default)
    {
        var path = ResolvePhysicalPath(objectKey);
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("本地对象不存在：" + objectKey, path);
        }

        return Task.FromResult<Stream>(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
    }

    /// <inheritdoc />
    public Task DeleteAsync(ObjectStorageAdapterContext context, string objectKey, CancellationToken cancellationToken = default)
    {
        var path = ResolvePhysicalPath(objectKey);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> TestConnectionAsync(ObjectStorageAdapterContext context, CancellationToken cancellationToken = default)
    {
        var root = ResolveWebRootPath();
        var probe = Path.Combine(root, ".objectstorage_probe_" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllText(probe, "probe");
            File.Delete(probe);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc />
    public string BuildPublicUrl(ObjectStorageAdapterContext context, string objectKey)
    {
        var key = (objectKey ?? string.Empty).TrimStart('/');
        return "/" + key;
    }
}