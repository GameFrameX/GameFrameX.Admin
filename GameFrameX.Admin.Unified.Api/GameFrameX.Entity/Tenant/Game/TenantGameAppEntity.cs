using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏应用
/// </summary>
[SugarTable(null, "游戏应用")]
[TenantTable]
[IncrementTable]
public class TenantGameAppEntity : EntityBasicSelect
{
    /// <summary>
    /// 应用密钥（已退役兼容字段，恒空）。C371（GFX-838）批量清零收尾后验证仅走 <see cref="AppSecretHash"/> 常量时间比对；
    /// 新增 / 轮换时此列写 null，一次性明文仅 Add/Rotate 响应回显。列保留仅为兼容存量 schema，不再承载任何数据。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用密钥（已退役，恒空）", Length = EntityStringLength.Length512, IsNullable = true)]
    public string? AppSecret { get; set; }

    /// <summary>
    /// 应用图标
    /// </summary>
    [SugarColumn(ColumnDescription = "应用图标", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string Icon { get; set; } = null!;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string Remark { get; set; } = null!;

    /// <summary>
    /// 应用密钥哈希（PBKDF2-SHA256 + 随机 salt，Base64）；权威验证路径，为空的行一律拒绝验证（C371 清零收尾后不应存在）
    /// </summary>
    [SugarColumn(ColumnDescription = "应用密钥哈希", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string AppSecretHash { get; set; } = null!;

    /// <summary>
    /// 应用密钥哈希算法标识（默认 PBKDF2-SHA256），便于后续算法升级
    /// </summary>
    [SugarColumn(ColumnDescription = "应用密钥哈希算法", Length = EntityStringLength.Length256, IsNullable = true)]
    public string AppSecretHashAlgorithm { get; set; } = null!;

    /// <summary>
    /// 是否要求请求签名（首版默认 false，落地开关供后续灰度启用）
    /// </summary>
    [SugarColumn(ColumnDescription = "是否要求请求签名", DefaultValue = "false")]
    public bool SignatureRequired { get; set; }

    /// <summary>
    /// 是否要求设备证明（首版默认 false，落地开关供后续灰度启用）
    /// </summary>
    [SugarColumn(ColumnDescription = "是否要求设备证明", DefaultValue = "false")]
    public bool DeviceProofRequired { get; set; }

    /// <summary>
    /// 风控策略 opaque 字符串，由 C10 / C11 风控与内容运营消费
    /// </summary>
    [SugarColumn(ColumnDescription = "风控策略", Length = EntityStringLength.Length512, IsNullable = true)]
    public string RiskPolicy { get; set; } = null!;

    /// <summary>
    /// 密钥最近轮换时间；null 表示从未轮换
    /// </summary>
    [SugarColumn(ColumnDescription = "密钥最近轮换时间", IsNullable = true)]
    public long? LastRotatedTime { get; set; }
}