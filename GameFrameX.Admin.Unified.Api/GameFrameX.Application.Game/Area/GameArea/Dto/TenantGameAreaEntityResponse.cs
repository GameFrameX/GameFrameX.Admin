using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Area.GameArea.Dto;

/// <summary>
/// 游戏区服输出参数
/// </summary>
public class TenantGameAreaEntityResponse : IBasePageResponse<Entity.Tenant.Game.TenantGameAreaEntity>
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
    /// 是否是默认
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// 是否推荐
    /// </summary>
    public bool IsRecommend { get; set; } = true;

    /// <summary>
    /// 大区分类Id
    /// </summary>
    public long ClassifyId { get; set; }


    /// <summary>
    /// 表示服务器当前的状态。例如：Online, Offline, Maintenance, Full。
    /// </summary>
    public GameServerStatus ServerStatus { get; set; }

    /// <summary>
    /// Http 管理连接地址
    /// </summary>
    public string HttpManageUrl { get; set; } = null!;

    /// <summary>
    /// TcpSocket 连接地址
    /// </summary>
    public string TcpSocketUrl { get; set; } = null!;

    /// <summary>
    /// WebSocket连接地址
    /// </summary>
    public string WebSocketUrl { get; set; } = null!;

    /// <summary>
    /// 服务器能够承载的最大玩家数量。
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// 目前服务器上的玩家数量。
    /// </summary>
    public int? CurrentLoad { get; set; }

    /// <summary>
    /// 服务器启用的日期。
    /// </summary>
    public long CreationTime { get; set; }

    /// <summary>
    /// 服务器最后一次维护或更新的日期。
    /// </summary>
    public long? LastMaintenanceTime { get; set; }

    /// <summary>
    /// 服务器运行的游戏版本号。
    /// </summary>
    public string ServerVersion { get; set; } = null!;

    /// <summary>
    /// 服务器的游戏模式类型，如：Normal, Newbie, Elite, PvP, PvE。
    /// </summary>
    public string ServerType { get; set; } = null!;

    /// <summary>
    /// 本地化Key
    /// </summary>
    public string LocalizationKey { get; set; } = null!;

    /// <summary>
    /// 服务器支持的主要语言。
    /// </summary>
    public GameLanguage Language { get; set; }

    /// <summary>
    /// 指定可以加入此服务器的玩家类型，如：NewPlayer, VIP。
    /// </summary>
    public string AllowedPlayerProfile { get; set; } = null!;

    /// <summary>
    /// 特定服务器举行的事件或活动信息。
    /// </summary>
    public string ServerEvents { get; set; } = null!;

    /// <summary>
    /// 访问权限设置，控制哪些玩家可以访问服务器。
    /// </summary>
    public string AccessControls { get; set; } = null!;

    /// <summary>
    /// 服务器的 Docker 容器 ID
    /// </summary>
    public string ServerContainerId { get; set; } = null!;

    /// <summary>
    /// 游戏应用Id。区服主归属 = TenantId + GameAppId，每条区服必属某个具体游戏应用。
    /// </summary>
    public long GameAppId { get; set; }

    /// <summary>
    /// 渠道id
    /// </summary>
    public long? ChannelId { get; set; } = 0;

    /// <summary>
    /// 平台id
    /// </summary>
    public long? PlatformId { get; set; } = 0;

    /// <summary>
    /// 包id。可选访问策略。
    /// </summary>
    public long? PackageId { get; set; } = 0;

    /// <summary>
    /// 运行参数
    /// </summary>
    public string ServerParams { get; set; } = null!;

    /// <summary>
    /// 客户端版本
    /// </summary>
    public string AppVersion { get; set; } = null!;

    /// <summary>
    /// 客户端转发 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。默认 game/api。
    /// </summary>
    public string ClientDispatchApiPrefix { get; set; } = "game/api";

    /// <summary>
    /// 履约投递 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。默认 game/api。
    /// </summary>
    public string FulfillmentApiPrefix { get; set; } = "game/api";

    /// <summary>
    /// GM 命令 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。默认空，空表示不构造 GM URL。
    /// </summary>
    public string GmApiPrefix { get; set; } = null!;

    /// <summary>
    /// 管理控制 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。默认空，空表示不构造控制 URL。
    /// </summary>
    public string ControlApiPrefix { get; set; } = null!;

    /// <summary>
    /// 是否允许客户端调试 GM 通道（Admin 配置标记，来源 change C150 §3.2 / C175）。
    /// </summary>
    public bool EnableClientGmApi { get; set; }

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
    /// 区服分类名称
    /// </summary>
    public string ClassifyIdName { get; set; } = null!;
}