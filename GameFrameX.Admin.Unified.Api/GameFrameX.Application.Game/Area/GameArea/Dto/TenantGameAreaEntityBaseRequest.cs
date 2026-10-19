using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Area.GameArea.Dto;

/// <summary>
/// 游戏区服实体基类 - 包含 AddInput 和 UpdateInput 的公共字段
/// </summary>
public abstract class TenantGameAreaEntityBaseRequest : BaseUpdateRequest
{
    /// <summary>
    /// 名称
    /// </summary>
    [Required(ErrorMessage = "名称不能为空")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    [Required(ErrorMessage = "详细描述不能为空")]
    public string Description { get; set; } = null!;

    /// <summary>
    /// 本地化Key
    /// </summary>
    public string LocalizationKey { get; set; } = string.Empty;

    /// <summary>
    /// 是否是默认
    /// </summary>
    [Required(ErrorMessage = "是否是默认不能为空")]
    public bool IsDefault { get; set; }

    /// <summary>
    /// 是否推荐
    /// </summary>
    [Required(ErrorMessage = "是否推荐不能为空")]
    public bool IsRecommend { get; set; }

    /// <summary>
    /// 大区分类Id
    /// </summary>
    [Required(ErrorMessage = "大区分类Id不能为空")]
    public long ClassifyId { get; set; }

    /// <summary>
    /// 游戏应用Id。区服主归属，新增必填且必须属于当前租户；
    /// 更新时由服务层从既有实体回填（不可变），防止区服跨应用迁移。
    /// </summary>
    [Required(ErrorMessage = "游戏应用Id不能为空")]
    [AdaptMember("GameAppId")]
    public long AppId { get; set; }

    /// <summary>
    /// Http 管理连接地址
    /// </summary>
    [Required(ErrorMessage = "Http 管理连接地址不能为空")]
    public string HttpManageUrl { get; set; } = null!;

    /// <summary>
    /// TcpSocket 连接地址
    /// </summary>
    [Required(ErrorMessage = "TcpSocket 连接地址不能为空")]
    public string TcpSocketUrl { get; set; } = string.Empty;

    /// <summary>
    /// WebSocket连接地址
    /// </summary>
    [Required(ErrorMessage = "WebSocket连接地址不能为空")]
    public string WebSocketUrl { get; set; } = string.Empty;

    /// <summary>
    /// 最大玩家数量
    /// </summary>
    [Required(ErrorMessage = "最大玩家数量不能为空")]
    public int Capacity { get; set; }

    /// <summary>
    /// 启用的日期
    /// </summary>
    [Required(ErrorMessage = "启用的日期不能为空")]
    public long CreationTime { get; set; }

    /// <summary>
    /// 服务器最后一次维护或更新的日期
    /// </summary>
    [Required(ErrorMessage = "服务器最后一次维护或更新的日期不能为空")]
    public long? LastMaintenanceTime { get; set; }

    /// <summary>
    /// 游戏版本号
    /// </summary>
    [Required(ErrorMessage = "游戏版本号不能为空")]
    public string ServerVersion { get; set; } = null!;

    /// <summary>
    /// 服务器的 Docker 容器 ID
    /// </summary>
    public string ServerContainerId { get; set; } = string.Empty;

    /// <summary>
    /// 服务器的游戏模式类型
    /// </summary>
    [Required(ErrorMessage = "服务器的游戏模式类型不能为空")]
    public string ServerType { get; set; } = null!;

    /// <summary>
    /// 语言
    /// </summary>
    [Required(ErrorMessage = "语言不能为空")]
    public GameLanguage Language { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [Required(ErrorMessage = "状态不能为空")]
    public GameServerStatus ServerStatus { get; set; }

    /// <summary>
    /// 客户端版本
    /// </summary>
    public string AppVersion { get; set; } = string.Empty;

    /// <summary>
    /// 指定可以加入此服务器的玩家类型
    /// </summary>
    public string AllowedPlayerProfile { get; set; } = string.Empty;

    /// <summary>
    /// 特定服务器举行的事件或活动信息
    /// </summary>
    public string ServerEvents { get; set; } = string.Empty;

    /// <summary>
    /// 访问权限设置，控制哪些玩家可以访问服务器
    /// </summary>
    public string AccessControls { get; set; } = string.Empty;

    /// <summary>
    /// 运行参数。此前 BaseRequest 缺此声明，Adapt 无来源导致实体 NOT NULL 列恒 23502。
    /// </summary>
    public string ServerParams { get; set; } = string.Empty;

    /// <summary>
    /// 客户端转发 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。
    /// <para>Client API 代理玩家普通请求（建角、改名、查角色、创建订单等）的 Server HTTP 路由前缀，默认 <c>game/api</c>。</para>
    /// </summary>
    public string ClientDispatchApiPrefix { get; set; } = "game/api";

    /// <summary>
    /// 履约投递 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。
    /// <para>后台履约、发货、退款、邮件投递、广告奖励、Campaign 发布、后台运营操作等资产或运营投递的前缀，默认 <c>game/api</c>。</para>
    /// </summary>
    public string FulfillmentApiPrefix { get; set; } = "game/api";

    /// <summary>
    /// GM 命令 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。默认空，空表示 Admin 不构造 GM URL。
    /// </summary>
    public string GmApiPrefix { get; set; } = string.Empty;

    /// <summary>
    /// 管理控制 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。默认空，空表示 Admin 不构造控制 URL。
    /// </summary>
    public string ControlApiPrefix { get; set; } = string.Empty;

    /// <summary>
    /// 是否允许客户端调试 GM 通道（来源 change C150 §3.2 / C175）。仅 Admin 配置标记，最终放行由 Game Server 启动参数与环境门禁决定，默认关闭。
    /// </summary>
    public bool EnableClientGmApi { get; set; }
}