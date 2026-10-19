using GameFrameX.Entity.Base;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏区服
/// </summary>
[SugarTable(null, "游戏区服")]
[TenantTable]
[IncrementTable]
public class TenantGameAreaEntity : EntityBasicSelect
{
    /// <summary>
    /// 区服默认启用。启用标记复用基类 IsEnabled（C301 归一，原独立列 is_enable 移除）：
    /// 全局 ISafeEnabledFilter 会隐藏 IsEnabled==false 的行，管理列表如需展示禁用区服须按 FixPattern 用 Filter(null, true) 绕过。
    /// </summary>
    public TenantGameAreaEntity()
    {
        IsEnabled = true;
    }

    /// <summary>
    /// 是否是默认
    /// </summary>
    [SugarColumn(ColumnDescription = "是否是默认", DefaultValue = "false")]
    public bool IsDefault { get; set; }

    /// <summary>
    /// 是否推荐
    /// </summary>
    [SugarColumn(ColumnDescription = "是否推荐", DefaultValue = "false")]
    public bool IsRecommend { get; set; } = true;

    /// <summary>
    /// 大区分类Id
    /// </summary>
    [SugarColumn(ColumnDescription = "大区分类Id", DefaultValue = "0")]
    public long ClassifyId { get; set; }

    /// <summary>
    /// 表示服务器当前的状态。例如：Online, Offline, Maintenance, Full。
    /// </summary>
    [SugarColumn(ColumnDescription = "状态", DefaultValue = "0")]
    public GameServerStatus ServerStatus { get; set; }

    /// <summary>
    /// Http 管理连接地址
    /// </summary>
    [SugarColumn(ColumnDescription = "Http 管理连接地址")]
    public string HttpManageUrl { get; set; } = null!;

    /// <summary>
    /// TcpSocket 连接地址
    /// </summary>
    [SugarColumn(ColumnDescription = "TcpSocket 连接地址")]
    public string TcpSocketUrl { get; set; } = null!;

    /// <summary>
    /// WebSocket连接地址
    /// </summary>
    [SugarColumn(ColumnDescription = "WebSocket连接地址")]
    public string WebSocketUrl { get; set; } = null!;

    /// <summary>
    /// 服务器能够承载的最大玩家数量。
    /// </summary>
    [SugarColumn(ColumnDescription = "最大玩家数量", DefaultValue = "0")]
    public int Capacity { get; set; }

    /// <summary>
    /// 目前服务器上的玩家数量。
    /// </summary>
    [SugarColumn(ColumnDescription = "目前的玩家数量")]
    public int? CurrentLoad { get; set; }

    /// <summary>
    /// 服务器启用的日期。
    /// </summary>
    [SugarColumn(ColumnDescription = "启用的日期", DefaultValue = "0")]
    public long CreationTime { get; set; }

    /// <summary>
    /// 服务器最后一次维护或更新的日期。
    /// </summary>
    [SugarColumn(ColumnDescription = "服务器最后一次维护或更新的日期")]
    public long? LastMaintenanceTime { get; set; }

    /// <summary>
    /// 服务器运行的游戏版本号。
    /// </summary>
    [SugarColumn(ColumnDescription = "游戏版本号")]
    public string ServerVersion { get; set; } = null!;

    /// <summary>
    /// 服务器的游戏模式类型，如：Normal, Newbie, Elite, PvP, PvE。
    /// </summary>
    [SugarColumn(ColumnDescription = "服务器的游戏模式类型")]
    public string ServerType { get; set; } = null!;

    /// <summary>
    /// 本地化Key
    /// </summary>
    [SugarColumn(ColumnDescription = "本地化Key", Length = EntityStringLength.Length1024)]
    public string LocalizationKey { get; set; } = null!;

    /// <summary>
    /// 服务器支持的主要语言。
    /// </summary>
    [SugarColumn(ColumnDescription = "语言", DefaultValue = "0")]
    public GameLanguage Language { get; set; }

    /// <summary>
    /// 指定可以加入此服务器的玩家类型，如：NewPlayer, VIP。
    /// </summary>
    [SugarColumn(ColumnDescription = "指定可以加入此服务器的玩家类型", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string AllowedPlayerProfile { get; set; } = null!;

    /// <summary>
    /// 特定服务器举行的事件或活动信息。
    /// </summary>
    [SugarColumn(ColumnDescription = "特定服务器举行的事件或活动信息", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string ServerEvents { get; set; } = null!;

    /// <summary>
    /// 访问权限设置，控制哪些玩家可以访问服务器。
    /// </summary>
    [SugarColumn(ColumnDescription = "访问权限设置，控制哪些玩家可以访问服务器", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string AccessControls { get; set; } = null!;

    /// <summary>
    /// 服务器的 Docker 容器 ID
    /// </summary>
    [SugarColumn(ColumnDescription = "服务器的 Docker 容器 ID")]
    public string ServerContainerId { get; set; } = null!;

    /// <summary>
    /// 游戏应用Id。区服主归属 = TenantId + GameAppId，每条区服必属某个具体游戏应用，GameAppId 必须 &gt; 0。
    /// </summary>
    [SugarColumn(ColumnDescription = "游戏应用Id", DefaultValue = "0")]
    public long GameAppId { get; set; }

    /// <summary>
    /// 渠道id
    /// </summary>
    [SugarColumn(ColumnDescription = "渠道id")]
    public long? ChannelId { get; set; } = 0;

    /// <summary>
    /// 平台id
    /// </summary>
    [SugarColumn(ColumnDescription = "平台id")]
    public long? PlatformId { get; set; } = 0;

    /// <summary>
    /// 包id。可选访问策略。
    /// </summary>
    [SugarColumn(ColumnDescription = "包id")]
    public long? PackageId { get; set; } = 0;

    /// <summary>
    /// 运行参数
    /// </summary>
    [SugarColumn(ColumnDescription = "运行参数", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string ServerParams { get; set; } = null!;

    /// <summary>
    /// 客户端版本
    /// </summary>
    [SugarColumn(ColumnDescription = "客户端版本", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string AppVersion { get; set; } = null!;

    /// <summary>
    /// 客户端转发 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。
    /// <para>Client API 代理玩家普通请求（建角、改名、查角色、创建订单等）的 Server HTTP 路由前缀。</para>
    /// <para>默认 <c>game/api</c>；由 <see cref="GameFrameX.Application.Game.ServerManager.GameServerEndpointUrlBuilder"/> 按 <c>ClientDispatch</c> 能力消费。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "客户端转发 Server HTTP 路由前缀", Length = EntityStringLength.Length256, IsNullable = true)]
    public string ClientDispatchApiPrefix { get; set; } = "game/api";

    /// <summary>
    /// 履约投递 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。
    /// <para>后台履约、发货、退款、邮件投递、广告奖励、Campaign 发布、后台运营操作等资产或运营投递的前缀。</para>
    /// <para>默认 <c>game/api</c>；由 <see cref="GameFrameX.Application.Game.ServerManager.GameServerEndpointUrlBuilder"/> 按 <c>Fulfillment</c> 能力消费。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "履约投递 Server HTTP 路由前缀", Length = EntityStringLength.Length256, IsNullable = true)]
    public string FulfillmentApiPrefix { get; set; } = "game/api";

    /// <summary>
    /// GM 命令 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。
    /// <para>GM / 调试命令前缀。默认空，空表示 Admin 不构造 GM URL（构造器按 <c>Gm</c> 能力消费时空值抛异常，拒绝拼出根地址调用）。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "GM 命令 Server HTTP 路由前缀", Length = EntityStringLength.Length256, IsNullable = true)]
    public string GmApiPrefix { get; set; } = null!;

    /// <summary>
    /// 管理控制 Server HTTP 路由前缀（来源 change C150 §3.2 / C175）。
    /// <para>管理控制 / 运维控制接口（reload、健康检查、全服在线人数等控制类 Server API）前缀。默认空，空表示 Admin 不构造控制 URL（构造器按 <c>Control</c> 能力消费时空值抛异常）。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "管理控制 Server HTTP 路由前缀", Length = EntityStringLength.Length256, IsNullable = true)]
    public string ControlApiPrefix { get; set; } = null!;

    /// <summary>
    /// Online 平台管理 Server HTTP 路由前缀（来源 change C339 / vault:C2 S1.7）。
    /// <para>GFX Online 平台管理接口（Admin→Online Runtime 的作用域调用）前缀。默认 <c>online/admin</c>；空值拒绝构造 URL（同 Gm/Control 守卫，空表示该区服未开放 Online 管理）。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "Online 平台管理 Server HTTP 路由前缀", Length = EntityStringLength.Length256, IsNullable = true, DefaultValue = "online/admin")]
    public string OnlineAdminApiPrefix { get; set; } = "online/admin";

    /// <summary>
    /// 是否允许客户端调试 GM 通道（Admin 配置标记，来源 change C150 §3.2 / C175）。
    /// <para>默认关闭。这只是 Admin 侧配置入口，最终是否放行仍由 Game Server 启动参数与环境门禁决定。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "是否允许客户端调试 GM 通道", DefaultValue = "false")]
    public bool EnableClientGmApi { get; set; }
}