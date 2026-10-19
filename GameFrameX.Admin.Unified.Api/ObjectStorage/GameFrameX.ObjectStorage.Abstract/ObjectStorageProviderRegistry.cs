using System.Reflection;

namespace GameFrameX.ObjectStorage.Abstract;

/// <summary>
/// 对象存储 Provider 注册表。启动期扫描所有 GameFrameX.* 程序集中实现了
/// <see cref="IObjectStorageOptionSetting"/> 且标注 <see cref="ObjectStorageProviderAttribute"/> 的类型。
/// 查找大小写无关（OrdinalIgnoreCase），但对外输出声明处的标准 Provider 名。
/// </summary>
public static class ObjectStorageProviderRegistry
{
    private const string AssemblyPrefix = "GameFrameX.";

    private static readonly Lazy<Dictionary<string, ObjectStorageProviderBinding>> _store = new(Build, true);

    /// <summary>已注册的 Provider 绑定（键为标准 Provider 名）。</summary>
    public static IReadOnlyDictionary<string, ObjectStorageProviderBinding> Providers
    {
        get { return _store.Value; }
    }

    /// <summary>按 Provider 名查找（大小写无关）。</summary>
    public static bool TryGet(string provider, out ObjectStorageProviderBinding binding)
    {
#pragma warning disable CS8625, CS8601 // Try* 模式：false 返回配套 out 参数置 null，调用方依 false 短路不读取 binding；Dictionary.TryGetValue 的 [MaybeNullWhen(false)] 流分析无法传递到外层非空 out 参数。
        if (string.IsNullOrWhiteSpace(provider))
        {
            binding = null;
            return false;
        }

        return _store.Value.TryGetValue(provider.Trim(), out binding);
#pragma warning restore CS8625, CS8601
    }

    /// <summary>获取所有 Provider 的稳定名与展示标签（标准名排序）。</summary>
    public static IEnumerable<ObjectStorageProviderBinding> List()
    {
        return _store.Value.Values;
    }

    private static Dictionary<string, ObjectStorageProviderBinding> Build()
    {
        var dict = new Dictionary<string, ObjectStorageProviderBinding>(StringComparer.OrdinalIgnoreCase);

        LoadObjectStorageAssemblies();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var name = assembly.GetName().Name;
            if (name == null || !name.StartsWith(AssemblyPrefix, StringComparison.Ordinal))
            {
                continue;
            }

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException)
            {
                continue;
            }

            foreach (var type in types)
            {
                if (type.IsAbstract || !type.IsClass)
                {
                    continue;
                }

                if (!typeof(IObjectStorageOptionSetting).IsAssignableFrom(type))
                {
                    continue;
                }

                var attr = type.GetCustomAttribute<ObjectStorageProviderAttribute>();
                if (attr == null || string.IsNullOrWhiteSpace(attr.Name))
                {
                    continue;
                }

                var key = attr.Name.Trim();
                if (dict.ContainsKey(key))
                {
                    continue;
                }

                dict[key] = new ObjectStorageProviderBinding(key, attr.Label, type);
            }
        }

        return dict;
    }

    private static void LoadObjectStorageAssemblies()
    {
        var baseDirectory = AppContext.BaseDirectory;
        if (string.IsNullOrWhiteSpace(baseDirectory) || !Directory.Exists(baseDirectory))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(baseDirectory, "GameFrameX.ObjectStorage.*.dll", SearchOption.TopDirectoryOnly))
        {
            var assemblyName = Path.GetFileNameWithoutExtension(file);
            if (AppDomain.CurrentDomain.GetAssemblies().Any(assembly => assembly.GetName().Name == assemblyName))
            {
                continue;
            }

            try
            {
                Assembly.LoadFrom(file);
            }
            catch
            {
                // ponytail: provider discovery is best-effort; broken optional assemblies are ignored and covered by registry tests.
            }
        }
    }
}