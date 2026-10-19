namespace GameFrameX.Core.Service.PlatformCaptcha;

/// <summary>
/// 平台图形验证码运行时配置提供器接口（C217 可测性重构：SystemAuthService 注入此接口，便于测试 fake）。
/// </summary>
public interface IPlatformCaptchaConfigProvider
{
    /// <summary>
    /// 获取当前平台验证码配置。缺失或格式无效时拒绝继续使用验证码链路。
    /// </summary>
    Task<PlatformCaptchaSettings> GetRequiredSettingsAsync();
}