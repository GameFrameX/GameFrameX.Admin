using System.Globalization;

namespace GameFrameX.Core.Localization.Errors;

/// <summary>
/// 错误码文案解析器：以错误码枚举成员上的 <see cref="ErrorTextAttribute"/> 为单一事实来源
/// （错误码与中英文文案同处一行声明，编译期绑定），按请求语言（<see cref="CultureInfo.CurrentUICulture"/>）
/// 解析文案，游戏生态默认中文。首次访问反射扫描构建缓存，之后字典查询；
/// error-codes.json 码表导出（ErrorCodeContractTests E1）与 Vue locale / Unity 客户端消费同源于此。
/// </summary>
public static class ErrorLocalizer
{
    public const string DefaultCulture = "zh-CN";
    public const string EnglishCulture = "en-US";

    private static readonly Lazy<IReadOnlyDictionary<int, ErrorTextAttribute>> TextTable = new(LoadTextTable);

    /// <summary>
    /// 解析枚举错误码在当前请求语言下的文案；未挂载文案时回退为数字码字符串。
    /// </summary>
    public static string Localize(global::System.Enum code)
    {
        return Localize(Convert.ToInt32(code, CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// 解析数字错误码文案；cultureName 缺省取当前 UI 文化，英文文化走 en-US 文案，其余（含中文与未匹配）走 zh-CN 默认文案。
    /// </summary>
    public static string Localize(int code, string? cultureName = null)
    {
        var name = cultureName ?? CultureInfo.CurrentUICulture.Name;
        if (TextTable.Value.TryGetValue(code, out var text))
        {
            return IsEnglishCulture(name) ? text.EnUS : text.ZhCN;
        }

        return code.ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// 供契约测试读取指定文化的完整码表（锁定本地化全覆盖与 error-codes.json 导出同源）。
    /// </summary>
    internal static IReadOnlyDictionary<string, string> GetCultureTable(string cultureName)
    {
        var english = IsEnglishCulture(cultureName);
        var table = new Dictionary<string, string>();
        foreach (var pair in TextTable.Value)
        {
            table[pair.Key.ToString(CultureInfo.InvariantCulture)] = english ? pair.Value.EnUS : pair.Value.ZhCN;
        }

        return table;
    }

    private static bool IsEnglishCulture(string cultureName)
    {
        return cultureName.StartsWith("en", StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyDictionary<int, ErrorTextAttribute> LoadTextTable()
    {
        const string errorsNamespace = "GameFrameX.Core.Enum.Errors";
        var table = new Dictionary<int, ErrorTextAttribute>();
        foreach (var enumType in typeof(ErrorLocalizer).Assembly.GetTypes())
        {
            if (!enumType.IsEnum || enumType.Namespace != errorsNamespace || !enumType.Name.EndsWith("ErrorCode", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (var field in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var text = field.GetCustomAttribute<ErrorTextAttribute>();
                if (text is null)
                {
                    continue;
                }

                var value = Convert.ToInt32(field.GetRawConstantValue(), CultureInfo.InvariantCulture);
                table[value] = text;
            }
        }

        return table;
    }
}