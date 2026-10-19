namespace GameFrameX.Core.Service.Auth.Dto;

/// <summary>
/// 获取刷新Token结果
/// </summary>
public class RefreshTokenResponse
{
    /// <summary>
    /// 刷新Token（一次性明文返回，仅本次响应可见，禁止落日志/缓存）
    /// </summary>
    public string RefreshToken { get; set; } = null!;
}