namespace GameFrameX.Application.Game.Online.Console.Dto;

/// <summary>
/// 玩家时间线事件分组（严格六值，来源 change C344 / vault:C9 S8.1）。
/// <para>
/// 仅作为查询侧过滤参数：按业务域收窄事件范围（如只看处罚链）。响应行内的 <c>EventType</c>/<c>Group</c>
/// 均为 Online 事件信封原值 string 透传——Admin 不做事件枚举映射、不推断分组归属（缺事件即空槽，X6 口径红线）。
/// </para>
/// </summary>
public enum OnlinePlayerTimelineEventGroup
{
    /// <summary>会话域（登录/连接/重连）。</summary>
    Session = 1,

    /// <summary>匹配域（队列/Ticket/Match）。</summary>
    Match = 2,

    /// <summary>资产域（发放/撤销/交易）。</summary>
    Asset = 3,

    /// <summary>社交域（消息/举报）。</summary>
    Social = 4,

    /// <summary>处罚域（封禁/禁言/隔离）。</summary>
    Penalty = 5,

    /// <summary>运营域（配置命中/LiveOps）。</summary>
    LiveOps = 6,
}
