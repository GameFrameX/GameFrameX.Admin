using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game.App;

/// <summary>
/// 应用开放访问凭证。
/// <para>物理表名由类名下划线化推导为 <c>tenant_game_app_open_access_entity</c>（C34 契约：受管实体 <c>SugarTable</c> 首参为 <c>null</c>，不硬编码表名）。</para>
/// <para>应用级机器对机器调用凭证（打包上报、CI、流水线等），落租户库，绑定具体应用（AppId > 0）与租户内真实用户。</para>
/// <para>对称平台级 <see cref="Entity.System.SystemOpenAccessEntity"/>：System = 平台开放接口身份，GameApp = 应用机器凭证。</para>
/// <para>鉴权走 Hub.Api <c>TenantGameAppOpenAccessAuthMiddleware</c>（HMAC-SHA256 签名 + Scopes 接口范围校验），与 SystemOpenAccessEntity 的 C181 Signature scheme 独立共存。</para>
/// </summary>
[SugarTable(null, "应用开放访问凭证")]
[TenantTable]
[IncrementTable]
public class TenantGameAppOpenAccessEntity : EntityBasic, IAppScoped
{
    public TenantGameAppOpenAccessEntity()
    {
        IsEnabled = true;
    }

    /// <summary>
    /// 应用作用域。强制绑定具体应用，不支持 0 或租户默认凭证。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用作用域", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 绑定的租户系统用户 Id。Hub.Api 写入版本数据和审计列时使用该用户身份。
    /// </summary>
    [SugarColumn(ColumnDescription = "绑定用户Id", DefaultValue = "0")]
    public long BindUserId { get; set; }

    /// <summary>
    /// 责任场景编码，例如 ci、internal-builder、asset-pipeline、release-pipeline。自由文本标识，非权限映射。
    /// </summary>
    [SugarColumn(ColumnDescription = "责任场景编码", Length = EntityStringLength.Length256)]
    public string Scenario { get; set; } = null!;

    /// <summary>
    /// 使用目的说明，用于区分同一应用下多个凭证的职责边界。
    /// </summary>
    [SugarColumn(ColumnDescription = "使用目的", Length = EntityStringLength.Length512)]
    public string Purpose { get; set; } = null!;

    /// <summary>
    /// 责任主体名称，例如 CI 项目名、打包机名或流水线名。
    /// </summary>
    [SugarColumn(ColumnDescription = "责任主体", Length = EntityStringLength.Length256)]
    public string SubjectName { get; set; } = null!;

    /// <summary>
    /// 接口范围（允许调用的接口 / 路由前缀集合，JSON 数组字符串，OAuth scope 语义）。
    /// <para>鉴权中间件校验当前请求路由前缀 ∈ 本集合，越范围返回 403。打包上报对应 scope "package-report"。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "接口范围", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string ScopesJson { get; set; } = null!;

    /// <summary>
    /// 访问 Key。租户内唯一，鉴权时按声明租户路由租户库查询。
    /// </summary>
    [SugarColumn(ColumnDescription = "访问Key", Length = EntityStringLength.Length256)]
    public string AccessKey { get; set; } = null!;

    /// <summary>
    /// 访问密钥。C371（GFX-838）起经 <c>TenantSecretCrypto</c> 密文落租户库（HMAC-SHA256 签名验证需服务端持有可逆共享密钥）。
    /// <para>签发 / 轮换时仅返回一次明文，列表 / 详情不得输出。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "访问Secret", Length = EntityStringLength.Length512)]
    public string AccessSecret { get; set; } = null!;

    /// <summary>
    /// 过期时间，Unix 秒。为空表示不过期。
    /// </summary>
    [SugarColumn(ColumnDescription = "过期时间", IsNullable = true)]
    public long? ExpiresTime { get; set; }

    /// <summary>
    /// 撤销时间，Unix 秒。撤销后即使 IsEnabled 为 true 也不得继续鉴权。
    /// </summary>
    [SugarColumn(ColumnDescription = "撤销时间", IsNullable = true)]
    public long? RevokedTime { get; set; }

    /// <summary>
    /// 撤销原因，用于审计排查。
    /// </summary>
    [SugarColumn(ColumnDescription = "撤销原因", Length = EntityStringLength.Length512, IsNullable = true)]
    public string RevokeReason { get; set; } = null!;
}