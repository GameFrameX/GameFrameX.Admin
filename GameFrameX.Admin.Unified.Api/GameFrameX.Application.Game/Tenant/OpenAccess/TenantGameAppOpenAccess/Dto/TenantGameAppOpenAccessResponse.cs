namespace GameFrameX.Application.Game.Tenant.OpenAccess.TenantGameAppOpenAccess.Dto;

/// <summary>
/// 应用开放访问凭证输出。
/// <para>注意：<see cref="Entity.Tenant.Game.App.TenantGameAppOpenAccessEntity.AccessSecret"/> 永不输出（明文仅签发 / 轮换时一次性返回）。
/// Output 不含 AccessSecret 字段，Service 投影时也不得引入。</para>
/// </summary>
public class TenantGameAppOpenAccessResponse
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用作用域。强制绑定具体应用，不支持 0 或租户默认凭证。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 绑定的租户系统用户 Id。Hub.Api 写入版本数据和审计列时使用该用户身份。
    /// </summary>
    public long BindUserId { get; set; }

    /// <summary>
    /// 责任场景编码，例如 ci、internal-builder、asset-pipeline、release-pipeline。自由文本标识，非权限映射。
    /// </summary>
    public string Scenario { get; set; } = null!;

    /// <summary>
    /// 使用目的说明，用于区分同一应用下多个凭证的职责边界。
    /// </summary>
    public string Purpose { get; set; } = null!;

    /// <summary>
    /// 责任主体名称，例如 CI 项目名、打包机名或流水线名。
    /// </summary>
    public string SubjectName { get; set; } = null!;

    /// <summary>
    /// 接口范围（允许调用的接口 / 路由前缀集合，JSON 数组字符串，OAuth scope 语义）。
    /// </summary>
    public string ScopesJson { get; set; } = null!;

    /// <summary>
    /// 访问 Key。租户内唯一，鉴权时按声明租户路由租户库查询。
    /// </summary>
    public string AccessKey { get; set; } = null!;

    /// <summary>
    /// 过期时间，Unix 秒。为空表示不过期。
    /// </summary>
    public long? ExpiresTime { get; set; }

    /// <summary>
    /// 撤销时间，Unix 秒。撤销后即使 IsEnabled 为 true 也不得继续鉴权。
    /// </summary>
    public long? RevokedTime { get; set; }

    /// <summary>
    /// 撤销原因，用于审计排查。
    /// </summary>
    public string RevokeReason { get; set; } = null!;

    /// <summary>
    /// 绑定用户账号。
    /// </summary>
    public string BindUserAccount { get; set; } = null!;

    /// <summary>
    /// 绑定用户姓名。
    /// </summary>
    public string BindUserName { get; set; } = null!;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 行版本号（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }
}