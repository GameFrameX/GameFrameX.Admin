using GameFrameX.Core.Service.PlatformCaptcha;

namespace GameFrameX.Core.Tests.System;

internal sealed class FakePlatformCaptchaConfigProvider : IPlatformCaptchaConfigProvider
{
    public Task<PlatformCaptchaSettings> GetRequiredSettingsAsync()
    {
        return Task.FromResult(new PlatformCaptchaSettings());
    }
}