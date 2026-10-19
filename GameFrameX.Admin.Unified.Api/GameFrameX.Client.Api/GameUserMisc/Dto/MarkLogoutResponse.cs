namespace GameFrameX.Client.Api.GameUserMisc.Dto;

/// <summary>
/// 登出标记响应（原 BLMarkLogout 出参）
/// </summary>
public class MarkLogoutResponse
{
    /// <summary>
    /// 业务结果码（0 = 成功；客户端不消费）
    /// </summary>
    public long Rcode { get; set; }
}