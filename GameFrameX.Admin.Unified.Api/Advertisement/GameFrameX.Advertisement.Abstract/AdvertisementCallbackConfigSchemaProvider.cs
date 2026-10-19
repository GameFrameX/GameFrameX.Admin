using System.Reflection;

namespace GameFrameX.Advertisement.Abstract;

/// <summary>
/// 广告回调配置 schema 提供器默认实现。
/// </summary>
public sealed class AdvertisementCallbackConfigSchemaProvider : IAdvertisementCallbackConfigSchemaProvider
{
    /// <inheritdoc />
    public AdvertisementCallbackConfigSchema? GetSchema(string provider)
    {
        return AdvertisementCallbackProviderRegistry.TryGet(provider, out var binding) && binding != null ? BuildSchema(binding) : null;
    }

    /// <inheritdoc />
    public List<AdvertisementCallbackProviderItem> GetProviders()
    {
        return AdvertisementCallbackProviderRegistry.GetProviders();
    }

    /// <summary>
    /// 标准化广告 Provider。
    /// </summary>
    public static string NormalizeProvider(string provider)
    {
        return AdvertisementCallbackProviderRegistry.TryGet(provider, out var binding) && binding != null ? binding.Provider : string.Empty;
    }

    private static AdvertisementCallbackConfigSchema BuildSchema(AdvertisementCallbackProviderRegistry.ProviderBinding binding)
    {
        var instance = CreateInstance(binding.OptionType);
        var fields = new List<AdvertisementCallbackConfigField>();
        foreach (var property in binding.OptionType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanWrite || property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            var attr = property.GetCustomAttribute<AdvertisementCallbackFieldAttribute>();
            if (attr != null && attr.Ignore)
            {
                continue;
            }

            fields.Add(BuildField(property, attr, instance));
        }

        return new AdvertisementCallbackConfigSchema
        {
            Provider = binding.Provider,
            Label = binding.Label,
            Fields = fields,
        };
    }

    private static AdvertisementCallbackConfigField BuildField(PropertyInfo property, AdvertisementCallbackFieldAttribute? attr, object? instance)
    {
        attr = attr ?? new AdvertisementCallbackFieldAttribute();
        var type = ResolveType(property, attr);
        return new AdvertisementCallbackConfigField
        {
            Key = property.Name,
            Label = string.IsNullOrWhiteSpace(attr.Label) ? property.Name : attr.Label,
            Type = type,
            Required = attr.Required,
            Help = attr.Help,
            Default = ReadDefault(property, instance),
            Sensitive = attr.Sensitive || type == "secret",
            Options = BuildOptions(attr.Options),
        };
    }

    private static string ResolveType(PropertyInfo property, AdvertisementCallbackFieldAttribute attr)
    {
        if (attr.Options != null && attr.Options.Length > 0)
        {
            return "select";
        }

        var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        var typeCode = Type.GetTypeCode(propertyType);
        if (typeCode == TypeCode.Boolean)
        {
            return "bool";
        }

        if (typeCode == TypeCode.Int32 || typeCode == TypeCode.Int64 ||
            typeCode == TypeCode.Double || typeCode == TypeCode.Decimal)
        {
            return "number";
        }

        if (propertyType == typeof(string))
        {
            if (attr.Multiline)
            {
                return "multiline";
            }

            if (attr.Sensitive)
            {
                return "secret";
            }

            return "text";
        }

        return "text";
    }

    private static object? ReadDefault(PropertyInfo property, object? instance)
    {
        if (instance == null)
        {
            return null;
        }

        try
        {
            return property.GetValue(instance);
        }
        catch
        {
            return null;
        }
    }

    private static List<AdvertisementCallbackConfigOption>? BuildOptions(string[] options)
    {
        if (options == null || options.Length == 0)
        {
            return null;
        }

        return options
               .Select(value => new AdvertisementCallbackConfigOption { Value = value, Label = value, })
               .ToList();
    }

    private static object? CreateInstance(Type optionType)
    {
        try
        {
            return Activator.CreateInstance(optionType);
        }
        catch
        {
            return null;
        }
    }
}