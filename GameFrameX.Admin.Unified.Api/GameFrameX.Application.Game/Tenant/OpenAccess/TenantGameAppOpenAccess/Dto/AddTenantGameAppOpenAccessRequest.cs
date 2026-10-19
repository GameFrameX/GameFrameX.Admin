namespace GameFrameX.Application.Game.Tenant.OpenAccess.TenantGameAppOpenAccess.Dto;

/// <summary>
/// 新增应用开放访问凭证输入。AccessSecret 由服务端生成，仅签发响应中返回一次。
/// </summary>
public class AddTenantGameAppOpenAccessRequest : BaseAddRequest
{
    /// <summary>
    /// 当前应用 Id。必须大于 0。
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "请选择应用后再签发应用开放访问凭证")]
    public long AppId { get; set; }

    /// <summary>
    /// 绑定的租户系统用户 Id。
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "请选择绑定用户")]
    public long BindUserId { get; set; }

    /// <summary>
    /// 责任场景编码。
    /// </summary>
    [Required(ErrorMessage = "责任场景编码不能为空")]
    [StringLength(EntityStringLength.Length256)]
    public string Scenario { get; set; } = null!;

    /// <summary>
    /// 使用目的。
    /// </summary>
    [Required(ErrorMessage = "使用目的不能为空")]
    [StringLength(EntityStringLength.Length512)]
    public string Purpose { get; set; } = null!;

    /// <summary>
    /// 责任主体。
    /// </summary>
    [Required(ErrorMessage = "责任主体不能为空")]
    [StringLength(EntityStringLength.Length256)]
    public string SubjectName { get; set; } = null!;

    /// <summary>
    /// 接口范围（允许调用的路由前缀集合，JSON 数组字符串）。至少包含一个 scope，例如 ["package-report"]。
    /// </summary>
    [Required(ErrorMessage = "接口范围不能为空")]
    [StringLength(EntityStringLength.Length1024)]
    public string ScopesJson { get; set; } = null!;

    /// <summary>
    /// 过期时间，Unix 秒；为空表示不过期。
    /// </summary>
    public long? ExpiresTime { get; set; }
}