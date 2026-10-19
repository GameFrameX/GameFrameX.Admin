using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game.ServerConfig;

/// <summary>
/// 游戏运营配置表（登录 Config md5 差异下发的内容源，原厂 10 键：bakeOut/badge/bulletinLanguage/bundleController/chapterUpdate/fileReplace/frame/general_conf/notify/reward_replace）
/// </summary>
[SugarTable(null, "游戏运营配置表")]
[TenantTable]
[IncrementTable]
public sealed class TenantGameAppServerConfigEntity : EntityBasic
{
    /// <summary>
    /// 应用作用域，0 / null 表示租户默认配置（出厂基线行；应用行按 Key 覆盖基线行下发）
    /// </summary>
    [SugarColumn(ColumnDescription = "应用作用域", IsNullable = true)]
    public long? AppId { get; set; }

    /// <summary>
    /// 配置键
    /// </summary>
    [SugarColumn(ColumnDescription = "配置键", Length = EntityStringLength.Length256)]
    public string Key { get; set; } = null!;

    /// <summary>
    /// 配置内容 JSON 原文（保存时服务端归一化重写）
    /// </summary>
    [SugarColumn(ColumnDescription = "配置内容", Length = -1)]
    public string Config { get; set; } = null!;

    /// <summary>
    /// 内容版本号（归一化 Config 的 MD5 hex，小写 32 位）。服务端保存时自动重算，
    /// 构造保证「内容变 md5 必变」——客户端登录按 md5 差异决定是否拉全量
    /// </summary>
    [SugarColumn(ColumnDescription = "内容版本号", Length = EntityStringLength.Length256)]
    public string Md5 { get; set; } = null!;

    /// <summary>
    /// 键级启停：false 时登录 Config 不下发该键（客户端删除本地该键保持空态）
    /// </summary>
    [SugarColumn(ColumnDescription = "键级启停", DefaultValue = "false")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 详细描述
    /// </summary>
    [SugarColumn(ColumnDescription = "详细描述", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string? Description { get; set; }
}
