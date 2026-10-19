namespace GameFrameX.Entity.Enum;

/// <summary>
/// 区服游戏服 Server HTTP 接口能力类型（来源 change C150 §3.1 / C175 后端拆分）。
/// <para>调用游戏服 HTTP 时必须显式声明能力类型，由 <see cref="GameFrameX.Application.Game.ServerManager.GameServerEndpointUrlBuilder"/> 按能力类型选择区服对应前缀。</para>
/// <para>C150 期「不得新增第五类」的授权 change 即 C339：OnlineAdmin 为第五类（Online 平台管理接口，vault:C2 S1.7）；此后新增能力仍须另建 change。</para>
/// </summary>
[Description("区服游戏服 HTTP 接口能力类型")]
[AdminEnum]
public enum GameServerEndpointKindEnum
{
    /// <summary>
    /// Client API 代理玩家生命周期 / 角色 / 订单创建等普通游戏服请求。
    /// <para>示例：<c>req_player_create</c>、<c>req_player_change_name</c>、<c>req_player_get_info</c>、<c>req_player_create_order</c>。</para>
    /// </summary>
    [Description("客户端转发")] ClientDispatch = 1,

    /// <summary>
    /// 后台履约、发货、退款、邮件投递、广告奖励、Campaign 发布、后台运营操作等资产或运营投递。
    /// <para>示例：<c>req_player_send_item</c>、<c>req_player_refund_order</c>、<c>post_mail</c>、<c>req_player_send_advertisement_item</c>、<c>publish_mail_campaign</c>。</para>
    /// </summary>
    [Description("履约投递")] Fulfillment = 2,

    /// <summary>
    /// GM / 开发调试命令，只允许服务端代理或 Server 自身授权后调用。前缀默认空，空表示 Admin 不构造 GM URL。
    /// </summary>
    [Description("GM 命令")] Gm = 3,

    /// <summary>
    /// 管理控制 / 运维控制接口（reload、健康检查、在线人数等控制类 Server API）。前缀默认空，空表示 Admin 不构造控制 URL。
    /// </summary>
    [Description("管理控制")] Control = 4,

    /// <summary>
    /// GFX Online 平台管理接口（Admin→Online Runtime 的作用域调用，来源 change C339 / vault:C2 S1.7）。
    /// <para>前缀取区服 <c>OnlineAdminApiPrefix</c>，默认 <c>online/admin</c>；空值拒绝构造 URL（同 Gm/Control 守卫，空表示该区服未开放 Online 管理）。</para>
    /// </summary>
    [Description("Online 平台管理")] OnlineAdmin = 5,
}