using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// Game 登录票据签名 Key。
/// </summary>
[SugarTable(null, "Game 登录票据签名 Key")]
[TenantTable]
[IncrementTable]
public class TenantGameLoginTicketKeyEntity : EntityBasic, IAppScoped
{
    /// <summary>
    /// 应用 ID。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用 ID", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// Key ID，写入 JWT header 的 kid。
    /// </summary>
    [SugarColumn(ColumnDescription = "Key ID", Length = EntityStringLength.Length256)]
    public string Kid { get; set; } = string.Empty;

    /// <summary>
    /// 签名算法。
    /// </summary>
    [SugarColumn(ColumnDescription = "签名算法", Length = EntityStringLength.Length256)]
    public string Algorithm { get; set; } = "RS256";

    /// <summary>
    /// PEM 格式私钥，仅 Admin / Client API 签发侧使用。
    /// </summary>
    [SugarColumn(ColumnDescription = "PEM 格式私钥", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string PrivateKeyPem { get; set; } = string.Empty;

    /// <summary>
    /// PEM 格式公钥，供 Server 验签。
    /// </summary>
    [SugarColumn(ColumnDescription = "PEM 格式公钥", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string PublicKeyPem { get; set; } = string.Empty;

    /// <summary>
    /// 生效时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "生效时间", IsNullable = true)]
    public long? NotBeforeTime { get; set; }

    /// <summary>
    /// 过期时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "过期时间", IsNullable = true)]
    public long? ExpireTime { get; set; }

    /// <summary>
    /// 轮换时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "轮换时间", IsNullable = true)]
    public long? RotatedTime { get; set; }

    /// <summary>
    /// 备注。
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string Remark { get; set; } = null!;
}