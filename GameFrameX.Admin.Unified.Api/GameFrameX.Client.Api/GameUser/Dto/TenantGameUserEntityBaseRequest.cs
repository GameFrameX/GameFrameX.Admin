namespace GameFrameX.Client.Api.GameUser.Dto;

/// <summary>
/// 游戏账号基础输入参数
/// </summary>
public class TenantGameUserEntityBaseRequest
{
    /// <summary>
    /// 昵称
    /// </summary>
    public virtual string Name { get; set; } = null!;

    /// <summary>
    /// 账号
    /// </summary>
    public virtual string UserName { get; set; } = null!;

    /// <summary>
    /// 密码
    /// </summary>
    public virtual string Password { get; set; } = null!;
}