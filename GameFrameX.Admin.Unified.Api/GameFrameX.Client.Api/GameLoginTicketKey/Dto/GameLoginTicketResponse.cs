using GameFrameX.Client.Api.GameRole.Dto;
namespace GameFrameX.Client.Api.GameLoginTicketKey.Dto;

/// <summary>
/// Game 登录票据签发输出。
/// </summary>
public sealed class GameLoginTicketResponse
{
    /// <summary>
    /// 角色基础信息。
    /// </summary>
    public TenantGameRoleEntityResponse Player { get; set; } = null!;

    /// <summary>
    /// 短时 Game Server 准入票据。
    /// </summary>
    public string GameLoginTicket { get; set; } = null!;

    /// <summary>
    /// 票据过期时间；null 表示永不过期，非空值受平台 30 天上限保护。
    /// </summary>
    public long? TicketExpireTime { get; set; }

    /// <summary>
    /// 目标 Game 服 ID。
    /// </summary>
    public int? TargetGameServerId { get; set; }

    /// <summary>
    /// 目标 Game 实例 ID。
    /// </summary>
    public long? TargetGameServerInstanceId { get; set; }

    /// <summary>
    /// 目标外网连接地址。
    /// </summary>
    public string TargetGameOuterHost { get; set; } = null!;

    /// <summary>
    /// 目标外网连接端口。
    /// </summary>
    public uint? TargetGameOuterPort { get; set; }

    /// <summary>
    /// 统一路由目标。
    /// </summary>
    public string RouteTarget { get; set; } = null!;
}