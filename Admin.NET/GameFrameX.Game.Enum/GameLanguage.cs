// 麻省理工学院许可证
// 
// 版权所有 (c) 2021-2023 zuohuaijun，大名科技（天津）有限公司  联系电话/微信：18020030720  QQ：515096995
// 
// 特此免费授予获得本软件的任何人以处理本软件的权利，但须遵守以下条件：在所有副本或重要部分的软件中必须包括上述版权声明和本许可声明。
// 
// 软件按“原样”提供，不提供任何形式的明示或暗示的保证，包括但不限于对适销性、适用性和非侵权的保证。
// 在任何情况下，作者或版权持有人均不对任何索赔、损害或其他责任负责，无论是因合同、侵权或其他方式引起的，与软件或其使用或其他交易有关。

using System.ComponentModel;

namespace GameFrameX.Game.Enum;

/// <summary>
/// 本地化语言。
/// </summary>
[Description("本地化语言")]
public enum GameLanguage
{
    /// <summary>
    /// 未指定。
    /// </summary>
    [Description("未指定")] Unspecified = 0,

    /// <summary>
    /// 南非荷兰语。
    /// </summary>
    [Description("南非荷兰语")] Afrikaans,

    /// <summary>
    /// 阿尔巴尼亚语。
    /// </summary>
    [Description("阿尔巴尼亚语")] Albanian,

    /// <summary>
    /// 阿拉伯语。
    /// </summary>
    [Description("阿拉伯语")] Arabic,

    /// <summary>
    /// 巴斯克语。
    /// </summary>
    [Description("巴斯克语")] Basque,

    /// <summary>
    /// 白俄罗斯语。
    /// </summary>
    [Description("白俄罗斯语")] Belarusian,

    /// <summary>
    /// 保加利亚语。
    /// </summary>
    [Description("保加利亚语")] Bulgarian,

    /// <summary>
    /// 加泰罗尼亚语。
    /// </summary>
    [Description("加泰罗尼亚语")] Catalan,

    /// <summary>
    /// 简体中文。
    /// </summary>
    [Description("简体中文")] ChineseSimplified,

    /// <summary>
    /// 繁体中文。
    /// </summary>
    [Description("繁体中文")] ChineseTraditional,

    /// <summary>
    /// 克罗地亚语。
    /// </summary>
    [Description("克罗地亚语")] Croatian,

    /// <summary>
    /// 捷克语。
    /// </summary>
    [Description("捷克语")] Czech,

    /// <summary>
    /// 丹麦语。
    /// </summary>
    [Description("丹麦语")] Danish,

    /// <summary>
    /// 荷兰语。
    /// </summary>
    [Description("荷兰语")] Dutch,

    /// <summary>
    /// 英语。
    /// </summary>
    [Description("英语")] English,

    /// <summary>
    /// 爱沙尼亚语。
    /// </summary>
    [Description("爱沙尼亚语")] Estonian,

    /// <summary>
    /// 法罗语。
    /// </summary>
    [Description("法罗语")] Faroese,

    /// <summary>
    /// 芬兰语。
    /// </summary>
    [Description("芬兰语")] Finnish,

    /// <summary>
    /// 法语。
    /// </summary>
    [Description("法语")] French,

    /// <summary>
    /// 格鲁吉亚语。
    /// </summary>
    [Description("格鲁吉亚语")] Georgian,

    /// <summary>
    /// 德语。
    /// </summary>
    [Description("德语")] German,

    /// <summary>
    /// 希腊语。
    /// </summary>
    [Description("希腊语")] Greek,

    /// <summary>
    /// 希伯来语。
    /// </summary>
    [Description("希伯来语")] Hebrew,

    /// <summary>
    /// 匈牙利语。
    /// </summary>
    [Description("匈牙利语")] Hungarian,

    /// <summary>
    /// 冰岛语。
    /// </summary>
    [Description("冰岛语")] Icelandic,

    /// <summary>
    /// 印尼语。
    /// </summary>
    [Description("印尼语")] Indonesian,

    /// <summary>
    /// 意大利语。
    /// </summary>
    [Description("意大利语")] Italian,

    /// <summary>
    /// 日语。
    /// </summary>
    [Description("日语")] Japanese,

    /// <summary>
    /// 韩语。
    /// </summary>
    [Description("韩语")] Korean,

    /// <summary>
    /// 拉脱维亚语。
    /// </summary>
    [Description("拉脱维亚语")] Latvian,

    /// <summary>
    /// 立陶宛语。
    /// </summary>
    [Description("立陶宛语")] Lithuanian,

    /// <summary>
    /// 马其顿语。
    /// </summary>
    [Description("马其顿语")] Macedonian,

    /// <summary>
    /// 马拉雅拉姆语。
    /// </summary>
    [Description("马拉雅拉姆语")] Malayalam,

    /// <summary>
    /// 挪威语。
    /// </summary>
    [Description("挪威语")] Norwegian,

    /// <summary>
    /// 波斯语。
    /// </summary>
    [Description("波斯语")] Persian,

    /// <summary>
    /// 波兰语。
    /// </summary>
    [Description("波兰语")] Polish,

    /// <summary>
    /// 巴西葡萄牙语。
    /// </summary>
    [Description("巴西葡萄牙语")] PortugueseBrazil,

    /// <summary>
    /// 葡萄牙语。
    /// </summary>
    [Description("葡萄牙语")] PortuguesePortugal,

    /// <summary>
    /// 罗马尼亚语。
    /// </summary>
    [Description("罗马尼亚语")] Romanian,

    /// <summary>
    /// 俄语。
    /// </summary>
    [Description("俄语")] Russian,

    /// <summary>
    /// 塞尔维亚克罗地亚语。
    /// </summary>
    [Description("塞尔维亚克罗地亚语")] SerboCroatian,

    /// <summary>
    /// 塞尔维亚西里尔语。
    /// </summary>
    [Description("塞尔维亚西里尔语")] SerbianCyrillic,

    /// <summary>
    /// 塞尔维亚拉丁语。
    /// </summary>
    [Description("塞尔维亚拉丁语")] SerbianLatin,

    /// <summary>
    /// 斯洛伐克语。
    /// </summary>
    [Description("斯洛伐克语")] Slovak,

    /// <summary>
    /// 斯洛文尼亚语。
    /// </summary>
    [Description("斯洛文尼亚语")] Slovenian,

    /// <summary>
    /// 西班牙语。
    /// </summary>
    [Description("西班牙语")] Spanish,

    /// <summary>
    /// 瑞典语。
    /// </summary>
    [Description("瑞典语")] Swedish,

    /// <summary>
    /// 泰语。
    /// </summary>
    [Description("泰语")] Thai,

    /// <summary>
    /// 土耳其语。
    /// </summary>
    [Description("土耳其语")] Turkish,

    /// <summary>
    /// 乌克兰语。
    /// </summary>
    [Description("乌克兰语")] Ukrainian,

    /// <summary>
    /// 越南语。
    /// </summary>
    [Description("越南语")] Vietnamese
}