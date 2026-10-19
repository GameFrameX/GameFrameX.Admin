using GameFrameX.Core.Service.PlatformCaptcha;

namespace GameFrameX.Core.Tests.System;

public class PlatformCaptchaConfigTests
{
    [Fact]
    public void Serialize_DefaultSettings_StaysWithinSystemConfigValueLimit()
    {
        var settings = new PlatformCaptchaSettings();

        var value = PlatformCaptchaConfigProvider.Serialize(settings);

        Assert.Contains("\"captchaType\":10", value);
        Assert.True(value.Length <= PlatformCaptchaConfigConstants.MaxSerializedLength);
    }

    [Fact]
    public void Validate_RejectsInvalidBubbleRadius()
    {
        var settings = new PlatformCaptchaSettings
        {
            ImageOption = new PlatformCaptchaImageSettings
            {
                BubbleMinRadius = 20,
                BubbleMaxRadius = 10,
            },
        };

        Assert.ThrowsAny<Exception>(() => PlatformCaptchaConfigProvider.Validate(settings));
    }

    [Fact]
    public void Validate_RejectsInvalidVersion()
    {
        var settings = new PlatformCaptchaSettings { Version = 0, };

        Assert.ThrowsAny<Exception>(() => PlatformCaptchaConfigProvider.Validate(settings));
    }
}