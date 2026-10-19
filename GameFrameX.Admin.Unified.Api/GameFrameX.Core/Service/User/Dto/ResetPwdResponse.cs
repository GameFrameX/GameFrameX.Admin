namespace GameFrameX.Core.Service.User.Dto;

/// <summary>
/// 重置密码结果（平台用户与租户管理员重置共用）
/// </summary>
public class ResetPwdResponse
{
    /// <summary>
    /// 重置后的初始密码（取自系统密码配置；一次性明文返回，仅本次响应可见，禁止落日志/缓存）
    /// </summary>
    public string NewPassword { get; set; } = null!;
}