using GameFrameX.Entity.Base;
using GameFrameX.Entity.Routing;

namespace GameFrameX.Entity.System;

/// <summary>
/// 平台邮箱自助注册验证码。
/// <para>存储边界：主库 / 匿名注册流程；与租户玩家邮件验证码（<c>TenantGameUserEmailEntity</c>）完全独立，互不复用。</para>
/// </summary>
[SugarTable(null, "平台邮箱自助注册验证码")]
[SugarIndex("idx_system_register_email_code_email", nameof(Email), OrderByType.Asc)]
[PlatformTable]
[IncrementTable]
public class SystemRegisterEmailCodeEntity : EntityBasic
{
    /// <summary>
    /// 注册邮箱（同时作为租管账号）
    /// </summary>
    [SugarColumn(ColumnDescription = "注册邮箱", Length = EntityStringLength.Length256)]
    public string Email { get; set; } = null!;

    /// <summary>
    /// 验证码（明文，使用后置过期失效）
    /// </summary>
    [SugarColumn(ColumnDescription = "验证码")]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 验证码过期时间（秒级 Unix 时间戳）
    /// </summary>
    [SugarColumn(ColumnDescription = "验证码过期时间", DefaultValue = "0")]
    public long ExpirationTime { get; set; }
}
