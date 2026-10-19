namespace GameFrameX.Application.System.OpenAccess.Dto;

/// <summary>
/// 开放接口身份输出参数
/// </summary>
// 纯 POCO 投影（C228）：属性名与 GameFrameX.Entity.System.SystemOpenAccessEntity 保持一致。
// 注意：AccessSecret 永不输出（明文仅创建/轮换时一次性返回，与 Game 侧 TenantGameAppOpenAccessResponse 对称）；
// BindTenant 为导航属性，不在投影列内。
public class OpenAccessResponse
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 身份标识
    /// </summary>
    public string AccessKey { get; set; } = string.Empty;

    /// <summary>
    /// 绑定租户Id
    /// </summary>
    public long BindTenantId { get; set; }

    /// <summary>
    /// 绑定用户Id
    /// </summary>
    public long BindUserId { get; set; }

    /// <summary>
    /// 绑定用户账号
    /// </summary>
    public string BindUserAccount { get; set; } = string.Empty;

    /// <summary>
    /// 绑定租户名称
    /// </summary>
    public string BindTenantName { get; set; } = string.Empty;

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