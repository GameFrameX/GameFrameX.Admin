using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace GameFrameX.Email.Abstract;

/// <summary>
/// 邮件发送器注册表。
/// <para>反射收集所有标注 <see cref="EmailProviderAttribute"/> 的 <see cref="IEmailSender"/> 实现类，
/// 作为 Provider 名 ↔ <see cref="IEmailSender"/> 实例的稳定路由表。</para>
/// </summary>
/// <remarks>
/// <para><b>为什么扫程序集</b>：渠道绑定必须与实现类同源声明（Attribute 标在类上），
/// 才能消除散落在 <c>SystemEmailDispatcher</c> switch 中的重复分派。</para>
/// <para><b>为何只扫 <c>GameFrameX.Email.</c> 前缀</b>：限定范围避免遍历框架与第三方程序集，
/// 既快又防止误收集无关类型；新增渠道只需新建 <c>GameFrameX.Email.*</c> 项目并在实现类上加标注即可被发现。</para>
/// <para><b>实例化策略</b>：每个 <see cref="IEmailSender"/> 通过无参构造函数实例化并缓存。</para>
/// <para><b>缓存策略</b>：进程内渠道集合固定，使用 <see cref="Lazy{T}"/> 线程安全一次性构造。</para>
/// </remarks>
public static class EmailSenderRegistry
{
    private const string AssemblyPrefix = "GameFrameX.Email.";

    private static readonly Lazy<IReadOnlyDictionary<string, IEmailSender>> _map = new(Build, true);

    /// <summary>
    /// 按渠道名（大小写不敏感）解析发送器；未注册返回 false。
    /// </summary>
    public static bool TryGet(string providerName, [MaybeNullWhen(false)] out IEmailSender sender)
    {
        var key = Normalize(providerName);
        if (key.Length == 0)
        {
            sender = null!;
            return false;
        }

        return _map.Value.TryGetValue(key, out sender!);
    }

    /// <summary>
    /// 列出全部已注册渠道名集合。
    /// </summary>
    public static IReadOnlyCollection<string> GetProviderNames()
    {
        return _map.Value.Keys.ToList();
    }

    /// <summary>
    /// <see cref="IEmailSenderFactory"/> 实例（懒加载）。
    /// <para>保留为单例；任何 <see cref="IEmailSenderFactory"/> 调用方通过此属性取全局唯一实例。</para>
    /// </summary>
    public static IEmailSenderFactory Factory { get; } = new RegistryFacade();

    private static string Normalize(string? providerName)
    {
        return providerName?.Trim() ?? string.Empty;
    }

    private static IReadOnlyDictionary<string, IEmailSender> Build()
    {
        var result = new Dictionary<string, IEmailSender>(StringComparer.OrdinalIgnoreCase);
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
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).ToArray()!;
            }

            foreach (var type in types)
            {
                if (type == null || type.IsAbstract || type.IsInterface)
                {
                    continue;
                }

                if (!typeof(IEmailSender).IsAssignableFrom(type))
                {
                    continue;
                }

                var attribute = type.GetCustomAttribute<EmailProviderAttribute>();
                if (attribute == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(attribute.Name))
                {
                    continue;
                }

                if (Activator.CreateInstance(type) is not IEmailSender sender)
                {
                    continue;
                }

                var key = Normalize(attribute.Name);
                result[key] = sender;
            }
        }

        return result;
    }

    private sealed class RegistryFacade : IEmailSenderFactory
    {
        bool IEmailSenderFactory.TryResolve(string providerName, out IEmailSender sender)
        {
            return TryGet(providerName, out sender);
        }

        IReadOnlyCollection<string> IEmailSenderFactory.ListProviders()
        {
            return GetProviderNames();
        }
    }
}