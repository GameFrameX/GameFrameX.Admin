using System.Reflection;
using GameFrameX.Payment.Abstract;

namespace GameFrameX.Application.Game.Order.GamePaymentConfig;

/// <summary>
/// 支付配置 schema 提供器实现：基于 Option 类反射生成前端表单字段描述。
/// 渠道绑定（channelName → Option Type → Label）由 <see cref="PaymentChannelRegistry"/> 统一提供，
/// 本类不再持有任何渠道到类型的硬编码映射。
/// </summary>
public class PaymentConfigSchemaProvider : IPaymentConfigSchemaProvider
{
    /// <summary>
    /// 获取指定渠道的字段 schema
    /// </summary>
    public PaymentConfigSchema GetSchema(string channelName)
    {
        if (!PaymentChannelRegistry.TryGet(channelName, out var binding))
        {
#pragma warning disable CS8603 // 接口 IPaymentConfigSchemaProvider.GetSchema 文档允许返回 null（渠道不存在），跨项目接口签名不可改
            return null;
#pragma warning restore CS8603
        }

        return BuildSchema(NormalizeChannel(channelName), binding);
    }

    /// <summary>
    /// 获取所有支持渠道的清单
    /// </summary>
    public List<PaymentConfigChannel> GetChannels()
    {
        return PaymentChannelRegistry.GetChannels();
    }

    private static PaymentConfigSchema BuildSchema(string channel, PaymentChannelRegistry.ChannelBinding binding)
    {
        var instance = CreateInstance(binding.OptionType);
        var fields = new List<PaymentConfigField>();
        foreach (var property in binding.OptionType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanWrite || property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            var attr = property.GetCustomAttribute<PaymentFieldAttribute>();
            if (attr != null && attr.Ignore)
            {
                continue;
            }

            fields.Add(BuildField(property, attr, instance));
        }

        return new PaymentConfigSchema
        {
            Channel = channel,
            Label = binding.Label,
            Fields = fields,
        };
    }

    private static PaymentConfigField BuildField(PropertyInfo property, PaymentFieldAttribute? attr, object? instance)
    {
        attr = attr ?? new PaymentFieldAttribute();
        return new PaymentConfigField
        {
            Key = property.Name,
            Label = string.IsNullOrWhiteSpace(attr.Label) ? property.Name : attr.Label,
            Type = ResolveType(property, attr),
            Required = attr.Required,
            Help = attr.Help,
#pragma warning disable CS8601 // PaymentConfigField.Default 在跨项目 DTO 中声明为非空（= null!），实际允许 null（无默认值时为 null）
            Default = ReadDefault(property, instance),
            Options = BuildOptions(attr.Options),
#pragma warning restore CS8601
        };
    }

    private static string ResolveType(PropertyInfo property, PaymentFieldAttribute attr)
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

    private static List<PaymentConfigOption>? BuildOptions(string[] options)
    {
        if (options == null || options.Length == 0)
        {
            return null;
        }

        return options
               .Select(value => new PaymentConfigOption { Value = value, Label = value, })
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

    private static string NormalizeChannel(string channelName)
    {
        return channelName?.Trim().ToLowerInvariant() ?? string.Empty;
    }
}