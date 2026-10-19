namespace GameFrameX.ObjectStorage.Abstract;

/// <summary>
/// 对象存储 Adapter 工厂。按 Provider 名解析 Adapter，并把租户配置（ConfigJson + 路径模板）
/// 解析为运行时上下文。DI 注入；TenantFileService 与 TestConnection 端点共用此工厂。
/// </summary>
public interface IObjectStorageAdapterFactory
{
    /// <summary>按 Provider 名解析 Adapter（未注册抛 NotSupportedException）。</summary>
    IObjectStorageAdapter Resolve(string provider);

    /// <summary>按 Provider 名解析 Adapter。</summary>
    bool TryResolve(string provider, out IObjectStorageAdapter adapter);

    /// <summary>
    /// 从 ConfigJson 解析 Option，提取 Bucket / 公共域名，组装运行时上下文。
    /// ConfigJson 非法时抛 ArgumentException。
    /// </summary>
    ObjectStorageAdapterContext BuildContext(string provider, string configJson, string pathTemplate);
}