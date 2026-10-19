using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 用户邮件
/// </summary>
[SugarTable(null, "用户邮件")]
[SugarIndex("idx_tenant_game_user_email_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameUserEmailEntity : EntityBasic, IAppScoped
{
    /// <summary>
    /// 应用Id。事实数据应用作用域（TenantId + AppId），必须 &gt; 0。
    /// <para>来源：Client API 发邮件由 <c>AppResolutionMiddleware.AppIdKey</c> 写入。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 邮件的接收者的电子邮件地址。
    /// </summary>
    [SugarColumn(ColumnDescription = "邮件的接收者的电子邮件地址")]
    public string Email { get; set; } = null!;

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    /// 语言
    /// </summary>
    public string Language { get; set; } = null!;

    /// <summary>
    /// 模板ID
    /// </summary>
    public string TemplateId { get; set; } = null!;

    /// <summary>
    /// 验证码
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 邮件的过期日期。
    /// </summary>
    [SugarColumn(ColumnDescription = "邮件的过期日期", DefaultValue = "0")]
    public long ExpirationTime { get; set; }
}