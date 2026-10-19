using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game.Release;

[SugarTable(null, "客户端程序版本配置表")]
[TenantTable]
[IncrementTable]
public sealed class TenantGameAppReleaseVersionEntity : EntityBasic
{
    /// <summary>
    /// 应用作用域，0 / null 表示租户默认配置
    /// </summary>
    [SugarColumn(ColumnDescription = "应用作用域", IsNullable = true)]
    public long? AppId { get; set; }

    /// <summary>
    /// 渠道
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = EntityStringLength.Length256)]
    public string Channel { get; set; } = null!;

    /// <summary>
    /// 平台
    /// </summary>
    [SugarColumn(ColumnDescription = "平台", Length = EntityStringLength.Length256)]
    public string Platform { get; set; } = null!;

    /// <summary>
    /// 版本
    /// </summary>
    [SugarColumn(ColumnDescription = "版本", Length = EntityStringLength.Length256)]
    public string AppVersion { get; set; } = null!;

    /// <summary>
    /// 包名
    /// </summary>
    [SugarColumn(ColumnDescription = "包名", Length = EntityStringLength.Length256)]
    public string Package { get; set; } = null!;

    /// <summary>
    /// 语言
    /// </summary>
    [SugarColumn(ColumnDescription = "语言", Length = EntityStringLength.Length256)]
    public string Language { get; set; } = null!;

    /// <summary>
    /// 旧版本
    /// </summary>
    [SugarColumn(ColumnDescription = "旧版本", Length = EntityStringLength.Length256)]
    public string OldVersion { get; set; } = null!;

    /// <summary>
    /// 下载链接
    /// </summary>
    [SugarColumn(ColumnDescription = "下载链接", Length = EntityStringLength.Length512)]
    public string DownloadUrl { get; set; } = null!;

    /// <summary>
    /// 是否强制更新
    /// </summary>
    [SugarColumn(ColumnDescription = "是否强制更新", DefaultValue = "false")]
    public bool IsForce { get; set; }

    /// <summary>
    /// 公告标题
    /// </summary>
    [SugarColumn(ColumnDescription = "公告标题", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? Title { get; set; }

    /// <summary>
    /// 公告内容
    /// </summary>
    [SugarColumn(ColumnDescription = "公告内容", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string Announcement { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    [SugarColumn(ColumnDescription = "详细描述", Length = EntityStringLength.Length1024)]
    public string Description { get; set; } = null!;
}