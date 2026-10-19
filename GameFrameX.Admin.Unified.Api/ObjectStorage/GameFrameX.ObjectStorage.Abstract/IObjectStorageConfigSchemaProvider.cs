namespace GameFrameX.ObjectStorage.Abstract;

/// <summary>
/// 对象存储配置 Schema 提供者：枚举可用 Provider，并按 Provider 输出字段表单结构。
/// </summary>
public interface IObjectStorageConfigSchemaProvider
{
    /// <summary>所有已注册 Provider（标准名 + 标签）。</summary>
    List<ObjectStorageProviderItem> GetProviders();

    /// <summary>按 Provider 名输出 Schema（未注册返回 null）。</summary>
    ObjectStorageConfigSchema GetSchema(string provider);
}