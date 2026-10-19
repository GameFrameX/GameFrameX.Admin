namespace GameFrameX.Application.Game.App.GameApp.Dto;

/// <summary>
/// 游戏应用输出参数
/// </summary>
public class GameAppResponse : IBasePageResponse<Entity.Tenant.Game.TenantGameAppEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// 应用密钥。DB 恒写 null + hash（hash-first，明文永不落库），读取路径恒为 null；
    /// 仅 Add / ResetSecret 响应一次性回传明文供前端展示。
    /// </summary>
    public string? AppSecret { get; set; }

    /// <summary>
    /// 应用图标
    /// </summary>
    public string Icon { get; set; } = null!;

    /// <summary>
    /// 是否要求请求签名（首版默认 false，落地开关供后续灰度启用）
    /// </summary>
    public bool SignatureRequired { get; set; }

    /// <summary>
    /// 是否要求设备证明（首版默认 false，落地开关供后续灰度启用）
    /// </summary>
    public bool DeviceProofRequired { get; set; }

    /// <summary>
    /// 风控策略 opaque 字符串，由 C10 / C11 风控与内容运营消费
    /// </summary>
    public string RiskPolicy { get; set; } = null!;

    /// <summary>
    /// 密钥最近轮换时间；null 表示从未轮换
    /// </summary>
    public long? LastRotatedTime { get; set; }

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

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; } = null!;
}