using GameFrameX.Entity.Tenant.Game.ServerConfig;

namespace GameFrameX.Application.Game.App.GameAppServerConfig.Dto;

/// <summary>
/// 游戏运营配置管理输出参数
/// </summary>
public class GameAppServerConfigResponse : IBasePageResponse<TenantGameAppServerConfigEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用作用域，0 / null 表示租户默认配置
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 配置键
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// 配置内容 JSON 原文
    /// </summary>
    public string Config { get; set; } = null!;

    /// <summary>
    /// 内容版本号（服务端保存时自动重算）
    /// </summary>
    public string Md5 { get; set; } = null!;

    /// <summary>
    /// 键级启停：false 时登录 Config 不下发该键
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 详细描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建者
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新者
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 乐观锁版本（编辑回显原样回传）
    /// </summary>
    public long? RowVersion { get; set; }
}
