using System.Reflection;

namespace GameFrameX.ObjectStorage.Abstract;

/// <summary>
/// 默认对象存储 Adapter 工厂。按 Provider 名返回对应 Adapter 单例，
/// 并把租户配置（ConfigJson + 路径模板）解析为运行时上下文。
/// 在 Web.Core/Startup.cs 以 AddSingleton 手动注册到 DI，供 TenantFileService 与 TestConnection 端点共用。
/// </summary>
public class DefaultObjectStorageAdapterFactory : IObjectStorageAdapterFactory
{
    private readonly Dictionary<string, IObjectStorageAdapter> _adapters = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public IObjectStorageAdapter Resolve(string provider)
    {
        if (!TryResolve(provider, out var adapter))
        {
            throw new NotSupportedException("未支持的对象存储 Provider：" + provider);
        }

        return adapter;
    }

    /// <inheritdoc />
    public bool TryResolve(string provider, out IObjectStorageAdapter adapter)
    {
#pragma warning disable CS8625 // Try* 模式：false 返回配套 out 参数置 null；调用方依 false 短路不读取 adapter。跨项目接口 IObjectStorageAdapterFactory.TryResolve 不能加 NotNullWhen(true)，否则级联。
        adapter = null;
#pragma warning restore CS8625
        if (string.IsNullOrWhiteSpace(provider))
        {
            return false;
        }

        var key = provider.Trim();
        if (_adapters.TryGetValue(key, out var existing))
        {
            adapter = existing;
            return true;
        }

        if (!ObjectStorageProviderRegistry.TryGet(key, out var binding))
        {
            return false;
        }

        var adapterType = FindAdapterType(binding.OptionType);
        if (adapterType == null || adapterType.IsAbstract || !typeof(IObjectStorageAdapter).IsAssignableFrom(adapterType))
        {
            return false;
        }

        var created = Activator.CreateInstance(adapterType) as IObjectStorageAdapter;
        if (created == null)
        {
            return false;
        }

        adapter = created;
        _adapters[binding.Provider] = created;
        return true;
    }

    private static Type? FindAdapterType(Type optionType)
    {
        var adapterTypeName = ResolveAdapterTypeName(optionType.Name);
        try
        {
            return optionType.Assembly
                             .GetTypes()
                             .FirstOrDefault(type => type.Name == adapterTypeName
                                                     && !type.IsAbstract
                                                     && typeof(IObjectStorageAdapter).IsAssignableFrom(type));
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.FirstOrDefault(type => type != null
                                                   && type.Name == adapterTypeName
                                                   && !type.IsAbstract
                                                   && typeof(IObjectStorageAdapter).IsAssignableFrom(type));
        }
    }

    private static string ResolveAdapterTypeName(string optionTypeName)
    {
        if (optionTypeName.EndsWith("ObjectStorageOptionSetting", StringComparison.Ordinal))
        {
            return optionTypeName.Substring(0, optionTypeName.Length - "ObjectStorageOptionSetting".Length) + "ObjectStorageAdapter";
        }

        if (optionTypeName.EndsWith("OptionSetting", StringComparison.Ordinal))
        {
            return optionTypeName.Substring(0, optionTypeName.Length - "OptionSetting".Length) + "ObjectStorageAdapter";
        }

        return optionTypeName + "Adapter";
    }

    /// <inheritdoc />
    public ObjectStorageAdapterContext BuildContext(string provider, string configJson, string pathTemplate)
    {
        var options = ObjectStorageConfigParser.ParseOptions(provider, configJson);
        return new ObjectStorageAdapterContext
        {
            Provider = provider,
            Options = options,
            Bucket = ObjectStorageConfigParser.GetBucket(options),
            PublicBaseUrl = ObjectStorageConfigParser.GetPublicBaseUrl(options),
            PathTemplate = pathTemplate,
            RawConfigJson = configJson,
        };
    }
}