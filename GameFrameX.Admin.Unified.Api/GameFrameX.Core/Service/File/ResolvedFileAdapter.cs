using GameFrameX.ObjectStorage.Abstract;

namespace GameFrameX.Core.Service.File;

/// <summary>
/// 既有文件解析结果（替代原 (IObjectStorageAdapter Adapter, ObjectStorageAdapterContext Context) 元组）。
/// </summary>
internal sealed class ResolvedFileAdapter
{
    /// <summary>对象存储适配器。</summary>
    public IObjectStorageAdapter Adapter { get; }

    /// <summary>适配器上下文。</summary>
    public ObjectStorageAdapterContext Context { get; }

    public ResolvedFileAdapter(IObjectStorageAdapter adapter, ObjectStorageAdapterContext context)
    {
        Adapter = adapter;
        Context = context;
    }
}