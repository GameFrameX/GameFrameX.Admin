namespace GameFrameX.Application.System.GlobalLanguage.Dto;

/// <summary>
/// 本地化输出参数
/// </summary>
// 纯 POCO 投影（C228）：属性名与 GameFrameX.Entity.Global.GlobalLanguageEntity（含 EntityBasicSelect 的 Name/Description）保持一致，
// 供 .Select&lt;GlobalLanguageResponse&gt;() 自动投影与 Mapster 按名映射。
public class GlobalLanguageResponse : IBasePageResponse<GameFrameX.Entity.Global.GlobalLanguageEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 语言代码，ISO 3166 双字母国家/地区代码
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 是否为默认
    /// </summary>
    public bool? IsDefault { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    public string Description { get; set; } = null!;

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