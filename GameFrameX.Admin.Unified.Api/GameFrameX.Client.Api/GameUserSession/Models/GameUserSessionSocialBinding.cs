namespace GameFrameX.Client.Api.GameUserSession.Models;

/// <summary>
/// 登录下发社交绑定条目（原 BLLogin LoginResp.social_bindings 数组元素，SocialPackage 五字段）
/// </summary>
public class GameUserSessionSocialBinding
{
    /// <summary>
    /// 社交类型（客户端 ESsoSocialType 数值）
    /// </summary>
    public long Type { get; set; }

    /// <summary>
    /// 社交账号 id
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 社交昵称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 头像 url
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// 手机号
    /// </summary>
    public string Phone { get; set; } = string.Empty;
}