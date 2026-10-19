using System.Reflection;

namespace GameFrameX.Advertisement.Abstract;

/// <summary>
/// 广告回调 Provider 绑定注册表。
/// </summary>
public static class AdvertisementCallbackProviderRegistry
{
    private const string AssemblyPrefix = "GameFrameX.Advertisement";

    private static readonly Lazy<IReadOnlyDictionary<string, ProviderBinding>> Map = new(Build, true);

    /// <summary>
    /// 按 Provider 查询绑定信息。未注册返回 false。
    /// </summary>
    public static bool TryGet(string provider, out ProviderBinding? binding)
    {
        var key = Normalize(provider);
        if (key.Length == 0)
        {
            binding = null;
            return false;
        }

        return Map.Value.TryGetValue(key, out binding);
    }

    /// <summary>
    /// 返回全部已注册 Provider 清单。
    /// </summary>
    public static List<AdvertisementCallbackProviderItem> GetProviders()
    {
        return Map.Value
                  .Values
                  .Select(binding => new AdvertisementCallbackProviderItem { Value = binding.Provider, Label = binding.Label, })
                  .OrderBy(item => item.Value, StringComparer.Ordinal)
                  .ToList();
    }

    /// <summary>
    /// 返回全部已注册 Provider 标识集合。
    /// </summary>
    public static IReadOnlyCollection<string> GetProviderNames()
    {
        return Map.Value.Values.Select(binding => binding.Provider).ToList();
    }

    /// <summary>
    /// 标准化广告 Provider。
    /// </summary>
    public static string Normalize(string provider)
    {
        return provider?.Trim() ?? string.Empty;
    }

    private static IReadOnlyDictionary<string, ProviderBinding> Build()
    {
        var result = new Dictionary<string, ProviderBinding>(StringComparer.Ordinal);
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly == null)
            {
                continue;
            }

            var name = assembly.GetName().Name;
            if (string.IsNullOrEmpty(name) || !name.StartsWith(AssemblyPrefix, StringComparison.Ordinal))
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

            CollectFromTypes(types, result);
        }

        return result;
    }

    private static void CollectFromTypes(Type[] types, Dictionary<string, ProviderBinding> result)
    {
        foreach (var type in types)
        {
            if (type.IsAbstract || type.IsInterface || !typeof(IAdvertisementCallbackOptionSetting).IsAssignableFrom(type))
            {
                continue;
            }

            var attr = type.GetCustomAttribute<AdvertisementCallbackProviderAttribute>();
            if (attr == null || string.IsNullOrWhiteSpace(attr.Name))
            {
                continue;
            }

            if (result.ContainsKey(attr.Name))
            {
                throw new InvalidOperationException(
                    $"广告回调 Provider 标识冲突：'{attr.Name}' 同时标注在 {type.FullName} 与 {result[attr.Name].OptionType.FullName}");
            }

            result[attr.Name] = new ProviderBinding(attr.Name, attr.Label, type);
        }
    }

    /// <summary>
    /// 单个 Provider 的绑定信息。
    /// </summary>
    public sealed class ProviderBinding
    {
        /// <summary>
        /// Provider 标识。
        /// </summary>
        public string Provider { get; }

        /// <summary>
        /// Provider 展示名称。
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Provider 对应的 Option 类型。
        /// </summary>
        public Type OptionType { get; }

        public ProviderBinding(string provider, string label, Type optionType)
        {
            Provider = provider;
            Label = string.IsNullOrWhiteSpace(label) ? provider : label;
            OptionType = optionType;
        }
    }
}