using GameFrameX.Entity.Enum;

namespace GameFrameX.Client.Api.GameArea.Dto;

/// <summary>
/// 游戏区服
/// </summary>
public sealed class TenantGameAreaEntityResponse
{
    /// <summary>
    /// 区服Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 服务器名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 是否推荐
    /// </summary>
    public bool IsRecommend { get; set; }

    /// <summary>
    /// TcpSocket 连接地址
    /// </summary>
    public string TcpSocketUrl { get; set; } = null!;

    /// <summary>
    /// Http 管理连接地址
    /// </summary>
    public string HttpManageUrl { get; set; } = null!;

    /// <summary>
    /// 表示服务器当前的状态。例如：Online, Offline, Maintenance, Full。
    /// </summary>
    public GameServerStatus ServerStatus { get; set; }

    /// <summary>
    /// 本地化Key
    /// </summary>
    public string LocalizationKey { get; set; } = null!;

    /// <summary>
    /// WebSocket连接地址
    /// </summary>
    public string WebSocketUrl { get; set; } = null!;
}