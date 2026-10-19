namespace GameFrameX.Core.Service.OnlineUser.Dto;

public class PageOnlineUserRequest : BasePageRequest
{
    /// <summary>
    /// 账号名称
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    /// 真实姓名
    /// </summary>
    public string RealName { get; set; } = null!;
}