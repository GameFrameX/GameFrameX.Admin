namespace GameFrameX.Entity.Enum;

/// <summary>
/// 文化程度枚举
/// </summary>
[Description("文化程度枚举")]
[AdminEnum]
public enum CultureLevelEnum
{
    /// <summary>
    /// 其他
    /// </summary>
    [Description("其他")] Level0 = 0,

    /// <summary>
    /// 小学
    /// </summary>
    [Description("小学")] Level1 = 1,

    /// <summary>
    /// 初中
    /// </summary>
    [Description("初中")] Level2 = 2,

    /// <summary>
    /// 普通高中
    /// </summary>
    [Description("普通高中")] Level3 = 3,

    /// <summary>
    /// 技工学校
    /// </summary>
    [Description("技工学校")] Level4 = 4,

    /// <summary>
    /// 职业教育
    /// </summary>
    [Description("职业教育")] Level5 = 5,

    /// <summary>
    /// 职业高中
    /// </summary>
    [Description("职业高中")] Level6 = 6,

    /// <summary>
    /// 中等专科
    /// </summary>
    [Description("中等专科")] Level7 = 7,

    /// <summary>
    /// 大学专科
    /// </summary>
    [Description("大学专科")] Level8 = 8,

    /// <summary>
    /// 大学本科
    /// </summary>
    [Description("大学本科")] Level9 = 9,

    /// <summary>
    /// 硕士研究生
    /// </summary>
    [Description("硕士研究生")] Level10 = 10,

    /// <summary>
    /// 博士研究生
    /// </summary>
    [Description("博士研究生")] Level11 = 11,
}