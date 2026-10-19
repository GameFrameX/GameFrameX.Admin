using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game.RemoteConfig;

[SugarTable(null, "客户端远程配置表")]
[TenantTable]
[IncrementTable]
public sealed class TenantGameAppRemoteConfigEntity : EntityBasic
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
    /// App版本
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
    /// 检测资源更新地址
    /// </summary>
    [SugarColumn(ColumnDescription = "检测资源更新地址", Length = EntityStringLength.Length512)]
    public string CheckResourceUrl { get; set; } = null!;

    /// <summary>
    /// 检测APP更新地址
    /// </summary>
    [SugarColumn(ColumnDescription = "检测APP更新地址", Length = EntityStringLength.Length512)]
    public string CheckAppVersionUrl { get; set; } = null!;

    /// <summary>
    /// 内容
    /// </summary>
    [SugarColumn(ColumnDescription = "内容", Length = -1)]
    public string Content { get; set; } = null!;

    /// <summary>
    /// Aot 代码列表
    /// </summary>
    [SugarColumn(ColumnDescription = "Aot 代码列表", Length = -1)]
    public string AotCodeList { get; set; } = null!;

    /// <summary>
    /// 模式内容映射 JSON (key=ModeCode, value=content object)
    /// </summary>
    [SugarColumn(ColumnDescription = "模式内容映射", Length = -1, IsNullable = true)]
    public string? ModeContents { get; set; }

    /// <summary>
    /// 当前激活模式编码
    /// </summary>
    [SugarColumn(ColumnDescription = "当前激活模式编码", Length = EntityStringLength.Length256, IsNullable = true)]
    public string? ActiveModeCode { get; set; }

    /// <summary>
    /// 投放优先级。同一请求命中多条远程配置记录时，数值越大越优先。
    /// </summary>
    [SugarColumn(ColumnDescription = "投放优先级", DefaultValue = "0")]
    public int Priority { get; set; }

    /// <summary>
    /// 灰度比例 0-100。未命中设备白名单时，按稳定设备标识取模判断是否命中。
    /// </summary>
    [SugarColumn(ColumnDescription = "灰度比例", DefaultValue = "0")]
    public int RolloutPercent { get; set; } = 100;

    /// <summary>
    /// 设备白名单 JSON 数组。命中白名单的设备绕过灰度比例，直接命中该记录。
    /// </summary>
    [SugarColumn(ColumnDescription = "设备白名单 JSON", Length = -1, IsNullable = true)]
    public string? DeviceWhitelistJson { get; set; }

    /// <summary>
    /// 详细描述
    /// </summary>
    [SugarColumn(ColumnDescription = "详细描述", Length = EntityStringLength.Length1024)]
    public string Description { get; set; } = null!;
}