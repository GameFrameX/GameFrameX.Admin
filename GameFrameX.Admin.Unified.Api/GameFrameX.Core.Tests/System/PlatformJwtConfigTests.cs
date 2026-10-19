using GameFrameX.Core.Service.PlatformJwt;
using System.Text;

namespace GameFrameX.Core.Tests.System;

public class PlatformJwtConfigTests
{
    [Fact]
    public void Validate_RejectsWeakSigningKey()
    {
        var settings = CreateValidSettings();
        settings.IssuerSigningKey = "too-short";

        Assert.ThrowsAny<Exception>(() => PlatformJwtRuntimeProvider.Validate(settings, true));
    }

    [Fact]
    public void Validate_RejectsDisabledSecurityBaseline()
    {
        var settings = CreateValidSettings();
        settings.ValidateLifetime = false;

        Assert.ThrowsAny<Exception>(() => PlatformJwtRuntimeProvider.Validate(settings, true));
    }

    [Fact]
    public void CreateValidationParameters_UsesStrictPlatformSettings()
    {
        var parameters = PlatformJwtRuntimeProvider.CreateValidationParameters(CreateValidSettings());

        Assert.True(parameters.ValidateIssuerSigningKey);
        Assert.True(parameters.ValidateIssuer);
        Assert.True(parameters.ValidateAudience);
        Assert.True(parameters.ValidateLifetime);
        Assert.True(parameters.RequireExpirationTime);
        Assert.Equal("GameFrameX", parameters.ValidIssuer);
        Assert.Equal("GameFrameX", parameters.ValidAudience);
        Assert.Equal(TimeSpan.FromSeconds(5), parameters.ClockSkew);
    }

    // === T5 补充：安全基线不可降级 ===

    [Fact]
    public void Validate_RejectsNonHs256Algorithm()
    {
        var settings = CreateValidSettings();
        settings.Algorithm = "HS512";

        Assert.ThrowsAny<Exception>(() => PlatformJwtRuntimeProvider.Validate(settings, true));
    }

    [Fact]
    public void Validate_RejectsClockSkewOutOfRange()
    {
        var neg = CreateValidSettings();
        neg.ClockSkew = -1;
        var over = CreateValidSettings();
        over.ClockSkew = 301;

        Assert.ThrowsAny<Exception>(() => PlatformJwtRuntimeProvider.Validate(neg, true));
        Assert.ThrowsAny<Exception>(() => PlatformJwtRuntimeProvider.Validate(over, true));
    }

    [Fact]
    public void Validate_RejectsNonPositiveTokenVersion()
    {
        var zero = CreateValidSettings();
        zero.TokenVersion = 0;
        var neg = CreateValidSettings();
        neg.TokenVersion = -3;

        Assert.ThrowsAny<Exception>(() => PlatformJwtRuntimeProvider.Validate(zero, true));
        Assert.ThrowsAny<Exception>(() => PlatformJwtRuntimeProvider.Validate(neg, true));
    }

    [Fact]
    public void Validate_RejectsEachDisabledBaselineFlagIndependently()
    {
        // 依次关闭四项校验，均必须拒绝，确保无任一项可被关掉
        var signingKey = CreateValidSettings();
        signingKey.ValidateIssuerSigningKey = false;
        var issuer = CreateValidSettings();
        issuer.ValidateIssuer = false;
        var audience = CreateValidSettings();
        audience.ValidateAudience = false;
        var lifetime = CreateValidSettings();
        lifetime.ValidateLifetime = false;

        Assert.ThrowsAny<Exception>(() => PlatformJwtRuntimeProvider.Validate(signingKey, true));
        Assert.ThrowsAny<Exception>(() => PlatformJwtRuntimeProvider.Validate(issuer, true));
        Assert.ThrowsAny<Exception>(() => PlatformJwtRuntimeProvider.Validate(audience, true));
        Assert.ThrowsAny<Exception>(() => PlatformJwtRuntimeProvider.Validate(lifetime, true));
    }

    // === T5 补充：掩码契约与版本 claim（保护 ******=保留语义与版本撤销） ===

    [Fact]
    public void Constants_PinMaskingAndVersionContract()
    {
        // 这些常量是 UpdateAsync 中“******=保留密钥”分支与令牌版本撤销链路的硬依赖，回归即破坏安全语义
        Assert.Equal("******", PlatformJwtConfigConstants.MaskedSigningKey);
        Assert.Equal("platform_jwt_version", PlatformJwtConfigConstants.TokenVersionClaim);
        Assert.Equal("platform_jwt_settings", PlatformJwtConfigConstants.ConfigCode);
        Assert.Equal("HS256", PlatformJwtConfigConstants.Algorithm);
    }

    [Fact]
    public void CreateValidationParameters_PropagatesCustomIssuersAndClockSkew()
    {
        var settings = CreateValidSettings();
        settings.ValidIssuer = "Issuer-A";
        settings.ValidAudience = "Audience-B";
        settings.ClockSkew = 42;

        var parameters = PlatformJwtRuntimeProvider.CreateValidationParameters(settings);

        Assert.Equal("Issuer-A", parameters.ValidIssuer);
        Assert.Equal("Audience-B", parameters.ValidAudience);
        Assert.Equal(TimeSpan.FromSeconds(42), parameters.ClockSkew);
        Assert.NotNull(parameters.IssuerSigningKey);
    }

    [Fact]
    public void Validate_RequireSigningKeyFalse_AllowsBlankKeyButEnforcesBaseline()
    {
        // DetailAsync 路径使用 requireSigningKey:false；空密钥不应在此模式抛错（DetailAsync 自身只输出掩码+hasSigningKey=false）
        var settings = CreateValidSettings();
        settings.IssuerSigningKey = string.Empty;

        var ex = Record.Exception(() => PlatformJwtRuntimeProvider.Validate(settings, false));
        Assert.Null(ex);

        // 但安全基线（算法/版本/ClockSkew/Validate*）在任何模式下都强制
        settings.TokenVersion = 0;
        Assert.ThrowsAny<Exception>(() => PlatformJwtRuntimeProvider.Validate(settings, false));
    }

    // === 首启自动生成缺省配置（随机密钥 + 默认 Issuer/Audience） ===

    [Fact]
    public void BuildDefaultSettings_ProducesValidSettings()
    {
        var settings = PlatformJwtRuntimeProvider.BuildDefaultSettings();

        var ex = Record.Exception(() => PlatformJwtRuntimeProvider.Validate(settings, true));
        Assert.Null(ex);
    }

    [Fact]
    public void BuildDefaultSettings_SigningKeyIsStrongEnough()
    {
        var settings = PlatformJwtRuntimeProvider.BuildDefaultSettings();

        Assert.False(string.IsNullOrWhiteSpace(settings.IssuerSigningKey));
        Assert.True(Encoding.UTF8.GetByteCount(settings.IssuerSigningKey) >= 32);
    }

    [Fact]
    public void BuildDefaultSettings_SigningKeyIsRandom()
    {
        var first = PlatformJwtRuntimeProvider.BuildDefaultSettings();
        var second = PlatformJwtRuntimeProvider.BuildDefaultSettings();

        Assert.NotEqual(first.IssuerSigningKey, second.IssuerSigningKey);
    }

    [Fact]
    public void BuildDefaultSettings_UsesExpectedIssuerDefaults()
    {
        var settings = PlatformJwtRuntimeProvider.BuildDefaultSettings();

        Assert.Equal("GameFrameX.Admin", settings.ValidIssuer);
        Assert.Equal("GameFrameX.Admin", settings.ValidAudience);
        Assert.Equal(1, settings.TokenVersion);
        Assert.Equal(PlatformJwtConfigConstants.Algorithm, settings.Algorithm);
    }

    private static PlatformJwtSettings CreateValidSettings()
    {
        return new PlatformJwtSettings
        {
            IssuerSigningKey = "0123456789abcdef0123456789abcdef",
            ValidIssuer = "GameFrameX",
            ValidAudience = "GameFrameX",
            TokenVersion = 1,
        };
    }
}