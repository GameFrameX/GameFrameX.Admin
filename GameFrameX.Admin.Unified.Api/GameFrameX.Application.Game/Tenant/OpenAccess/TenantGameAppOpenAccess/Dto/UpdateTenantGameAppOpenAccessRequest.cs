namespace GameFrameX.Application.Game.Tenant.OpenAccess.TenantGameAppOpenAccess.Dto;

/// <summary>
/// 更新应用开放访问凭证输入。AppId 不可变（编辑态由服务端按行原值回填）。
/// </summary>
public class UpdateTenantGameAppOpenAccessRequest : BaseUpdateRequest
{
    /// <summary>
    /// 当前应用 Id（用于校验凭证归属当前 App；AppId 本身不可变，由服务端按行原值回填）。
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "请选择应用")]
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
    /// 接口范围（允许调用的路由前缀集合，JSON 数组字符串）。
    /// </summary>
    [Required(ErrorMessage = "接口范围不能为空")]
    [StringLength(EntityStringLength.Length1024)]
    public string ScopesJson { get; set; } = null!;

    /// <summary>
    /// 过期时间，Unix 秒；为空表示不过期。
    /// </summary>
    public long? ExpiresTime { get; set; }
}