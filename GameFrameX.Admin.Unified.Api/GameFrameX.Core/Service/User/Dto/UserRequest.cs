namespace GameFrameX.Core.Service.User.Dto;

public class UserRequest : BaseIdRequest
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; } = true;
}