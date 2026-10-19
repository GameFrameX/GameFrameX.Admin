using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace GameFrameX.Payment.Abstract;

/// <summary>
/// 支付渠道绑定注册表：反射收集所有标注 <see cref="PaymentChannelAttribute"/> 的
/// <see cref="IPaymentOptionSetting"/> 实现类，作为 channelName ↔ Option Type ↔ Label 的唯一事实来源。
/// </summary>
/// <remarks>
/// <para><b>为什么扫程序集</b>：渠道绑定必须与 Option 类型同源声明（Attribute 标在类上），
/// 才能消除散落在 SchemaProvider 字典、反序列化 switch、Handler 注册处的重复映射。</para>
/// <para><b>为何只扫 <c>GameFrameX.Pay*</c> 前缀</b>：限定范围避免遍历框架与第三方程序集，
/// 既快又防止误收集无关类型；新增渠道只需新建 Pay.* 项目并在 Option 类上加标注即可被发现。</para>
/// <para><b>缓存策略</b>：进程内渠道集合固定，使用 <see cref="Lazy{T}"/> 线程安全一次性构造，后续查询零反射开销。</para>
/// </remarks>
public static class PaymentChannelRegistry
{
    private const string AssemblyPrefix = "GameFrameX.Payment";

    private static readonly Lazy<IReadOnlyDictionary<string, ChannelBinding>> _map = new(Build, true);

    /// <summary>
    /// 按渠道名（大小写不敏感）查询绑定信息。未注册返回 false。
    /// </summary>
    public static bool TryGet(string channelName, [MaybeNullWhen(false)] out ChannelBinding binding)
    {
        var key = Normalize(channelName);
        if (string.IsNullOrEmpty(key))
        {
            binding = null;
            return false;
        }

        return _map.Value.TryGetValue(key, out binding);
    }

    /// <summary>
    /// 按渠道名获取 Option 配置类型；未注册返回 null。供反序列化等需要具体类型的场景使用。
    /// </summary>
    public static Type GetOptionType(string channelName)
    {
#pragma warning disable CS8603 // 渠道未注册时返回 null，调用方应判空
        return TryGet(channelName, out var binding) ? binding.OptionType : null;
#pragma warning restore CS8603
    }

    /// <summary>
    /// 返回全部已注册渠道清单（按渠道名排序），供前端渠道下拉与 Schema 查询使用。
    /// </summary>
    public static List<PaymentConfigChannel> GetChannels()
    {
        return _map.Value
                   .Select(kv => new PaymentConfigChannel { Value = kv.Key, Label = kv.Value.Label, })
                   .OrderBy(c => c.Value, StringComparer.OrdinalIgnoreCase)
                   .ToList();
    }

    /// <summary>
    /// 全部已注册渠道名集合（大小写不敏感字典的原始键），供防漂移断言使用。
    /// </summary>
    public static IReadOnlyCollection<string> GetChannelNames()
    {
        return _map.Value.Keys.ToList();
    }

    private static IReadOnlyDictionary<string, ChannelBinding> Build()
    {
        var result = new Dictionary<string, ChannelBinding>(StringComparer.OrdinalIgnoreCase);
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
                // 跳过加载残缺的程序集，避免拖垮整个注册表构造
                continue;
            }

            CollectFromTypes(types, result);
        }

        return result;
    }

    private static void CollectFromTypes(Type[] types, Dictionary<string, ChannelBinding> result)
    {
        foreach (var type in types)
        {
            if (type.IsAbstract || type.IsInterface)
            {
                continue;
            }

            if (!typeof(IPaymentOptionSetting).IsAssignableFrom(type))
            {
                continue;
            }

            var attr = type.GetCustomAttribute<PaymentChannelAttribute>();
            if (attr == null || string.IsNullOrWhiteSpace(attr.Name))
            {
                continue;
            }

            if (result.ContainsKey(attr.Name))
            {
                throw new InvalidOperationException(
                    $"支付渠道标识冲突：'{attr.Name}' 同时标注在 {type.FullName} 与 {result[attr.Name].OptionType.FullName}");
            }

            result[attr.Name] = new ChannelBinding(type, attr.Label);
        }
    }

    private static string Normalize(string channelName)
    {
        return channelName?.Trim().ToLowerInvariant() ?? string.Empty;
    }

    /// <summary>
    /// 单个渠道的绑定信息（Option 类型 + 展示名称）。
    /// </summary>
    public sealed class ChannelBinding
    {
        /// <summary>
        /// 渠道对应的 Option 配置类型，用于反射构造与反序列化。
        /// </summary>
        public Type OptionType { get; }

        /// <summary>
        /// 渠道展示名称。
        /// </summary>
        public string Label { get; }

        public ChannelBinding(Type optionType, string label)
        {
            OptionType = optionType;
            Label = label;
        }
    }
}