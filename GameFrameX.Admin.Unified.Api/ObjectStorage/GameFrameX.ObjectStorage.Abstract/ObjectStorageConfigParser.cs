using System.Reflection;
using System.Text.Json;

namespace GameFrameX.ObjectStorage.Abstract;

/// <summary>
/// ConfigJson → Option 实例解析，以及从 Option 实例反射提取运行时参数（Bucket / 公共域名）。
/// AdapterFactory.BuildContext 与 TestConnection 端点共用。
/// 解析使用 PropertyNameCaseInsensitive，前端 camelCase / PascalCase 均可。
/// </summary>
public static class ObjectStorageConfigParser
{
    private static readonly JsonSerializerOptions DeserializeOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>按 Provider 把 ConfigJson 反序列化为对应 Option 实例。空配置返回该 Option 的默认实例。</summary>
    public static object ParseOptions(string provider, string configJson)
    {
        if (!ObjectStorageProviderRegistry.TryGet(provider, out var binding))
        {
            throw new ArgumentException("未注册的对象存储 Provider：" + provider, nameof(provider));
        }

        if (string.IsNullOrWhiteSpace(configJson))
        {
#pragma warning disable CS8603 // Activator.CreateInstance 对已注册非抽象 class OptionType 不返回 null（无公共无参构造抛 MissingMethodException）；公共签名 object 由 ObjectStorageAdapterContext.Options（= null!;）承载可空契约，不改 object? 以避免跨项目级联。
            return Activator.CreateInstance(binding.OptionType);
#pragma warning restore CS8603
        }

#pragma warning disable CS8603 // JsonSerializer.Deserialize 对 "null" 字面量 JSON 返回 null，与空 configJson 分支语义一致；公共签名 object 由 ObjectStorageAdapterContext.Options（= null!;）承载可空契约，不改 object?。
        return JsonSerializer.Deserialize(configJson, binding.OptionType, DeserializeOptions);
#pragma warning restore CS8603
    }

    /// <summary>从 Option 实例反射读取 Bucket / 存储空间名（属性名匹配 BucketName 或 Bucket，取首个非空）。</summary>
    public static string GetBucket(object option)
    {
        return ReadString(option, "BucketName") ?? ReadString(option, "Bucket");
    }

    /// <summary>从 Option 实例反射读取公共访问域名（属性名匹配 PublicBaseUrl 或 Domain，取首个非空）。</summary>
    public static string GetPublicBaseUrl(object option)
    {
        return ReadString(option, "PublicBaseUrl") ?? ReadString(option, "Domain");
    }

    private static string ReadString(object option, string propertyName)
    {
#pragma warning disable CS8603 // ReadString 语义即"按名查找属性值，未找到返回 null"；由 GetBucket/GetPublicBaseUrl 的 ?? 传播到 ObjectStorageAdapterContext.Bucket/PublicBaseUrl（DTO 用 = null!; 承载可空契约）；公共签名不改 string? 以避免跨项目级联。
        if (option == null)
        {
            return null;
        }

        var prop = option.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (prop == null)
        {
            return null;
        }

        return prop.GetValue(option) as string;
#pragma warning restore CS8603
    }
}