using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏支付配置
/// </summary>
[SugarTable(null, "游戏支付配置")]
[TenantTable]
[IncrementTable]
public class TenantGamePaymentConfigEntity : EntityBasic, IAppScoped
{
    /// <summary>
    /// 应用ID。0表示租户默认配置。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用ID", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 游戏应用ID。空表示租户默认配置。
    /// </summary>
    [SugarColumn(ColumnDescription = "游戏应用ID", IsNullable = true)]
    public long? GameAppId { get; set; }

    /// <summary>
    /// 分包ID。空表示应用默认配置。
    /// </summary>
    [SugarColumn(ColumnDescription = "分包ID", IsNullable = true)]
    public long? PackageId { get; set; }

    /// <summary>
    /// 游戏应用ID归一化值，空按0参与唯一性判断。
    /// </summary>
    [SugarColumn(ColumnDescription = "游戏应用ID归一化值", DefaultValue = "0")]
    public long GameAppIdNormalized { get; set; }

    /// <summary>
    /// 分包ID归一化值，空按0参与唯一性判断。
    /// </summary>
    [SugarColumn(ColumnDescription = "分包ID归一化值", DefaultValue = "0")]
    public long PackageIdNormalized { get; set; }

    /// <summary>
    /// 支付渠道
    /// </summary>
    [SugarColumn(ColumnDescription = "支付渠道", Length = EntityStringLength.Length256)]
    public string PayChannel { get; set; } = string.Empty;

    /// <summary>
    /// 是否沙盒配置
    /// </summary>
    [SugarColumn(ColumnDescription = "是否沙盒配置", DefaultValue = "false")]
    public bool IsSandbox { get; set; }

    /// <summary>
    /// 支付配置JSON
    /// </summary>
    [SugarColumn(ColumnDescription = "支付配置JSON", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string ConfigJson { get; set; } = string.Empty;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string Remark { get; set; } = null!;
}