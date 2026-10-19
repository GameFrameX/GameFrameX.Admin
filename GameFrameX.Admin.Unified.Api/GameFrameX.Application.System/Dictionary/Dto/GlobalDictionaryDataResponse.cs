namespace GameFrameX.Application.System.Dictionary.Dto;

/// <summary>
/// 系统字典值输出参数
/// </summary>
public class GlobalDictionaryDataResponse
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 字典类型Id
    /// </summary>
    public long DictionaryTypeId { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 编码
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 显示样式-标签颜色
    /// </summary>
    public string? TagType { get; set; }

    /// <summary>
    /// 显示样式-Style(控制显示样式)
    /// </summary>
    public string? StyleSetting { get; set; }

    /// <summary>
    /// 显示样式-Class(控制显示样式)
    /// </summary>
    public string? ClassSetting { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 拓展数据(保存业务功能的配置项)
    /// </summary>
    public string? ExtData { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 行版本号（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }
}