namespace GameFrameX.Client.Api.GameUser.Dto;

/// <summary>
/// 谷歌账号绑定状态输出参数
/// </summary>
public class GoogleBindStatusResponse
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    /// 是否已绑定谷歌账号
    /// </summary>
    public bool IsGoogleBound { get; set; }

    /// <summary>
    /// 谷歌账号ID
    /// </summary>
    public string GoogleId { get; set; } = null!;
}