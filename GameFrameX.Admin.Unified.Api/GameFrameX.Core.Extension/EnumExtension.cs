using System.ComponentModel;
using System.Reflection;

namespace GameFrameX.Core.Extension;

/// <summary>
/// 枚举拓展
/// </summary>
public static class EnumExtension
{
    /// <summary>
    /// 获取枚举的Description
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string? GetDescription(this object value)
    {
        return value.GetType().GetMember(value.ToString() ?? string.Empty).FirstOrDefault()
                    ?.GetCustomAttribute<DescriptionAttribute>()?.Description;
    }

    /// <summary>
    /// 将枚举转成枚举信息集合
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static List<EnumItem> EnumToList(this Type type)
    {
        if (!type.IsEnum)
        {
            throw new ArgumentException("Type '" + type.Name + "' is not an enum.");
        }

        var arr = System.Enum.GetNames(type);
        return arr.Select(sl =>
        {
            var item = System.Enum.Parse(type, sl);
            var name = item.ToString() ?? string.Empty;
            return new EnumItem
            {
                Name = name,
                Describe = item.GetDescription() ?? name,
                Value = item.GetHashCode(),
            };
        }).ToList();
    }
}
