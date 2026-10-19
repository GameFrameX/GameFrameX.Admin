namespace GameFrameX.Client.Api.GameUserMisc.Dto;

/// <summary>
/// 登出标记请求（原 BLMarkLogout 入参；仅删号成功后上报，响应 rcode 客户端不消费）
/// </summary>
public class MarkLogoutRequest
{
    /// <summary>
    /// 玩家 id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 客户端版本号
    /// </summary>
    public string Version { get; set; } = string.Empty;
}