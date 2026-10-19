using GameFrameX.Client.Api.Services;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Tests.C116;

/// <summary>
/// C116（GFX-114 / C11-D 未阻塞子集）单元测试：
/// 风险策略 fail closed/open 解析默认安全 + NullAppAttestationVerifier fail-hard 保证 + 新增枚举锁定。
/// </summary>
public class C116AntiCheatAndRiskEnforcementTests
{
    [Theory]
    [InlineData("FailOpen", false, false, RiskEnforcementModeEnum.FailOpen)]
    [InlineData("failclosed", true, false, RiskEnforcementModeEnum.FailClosed)]
    [InlineData("AuditOnly", false, false, RiskEnforcementModeEnum.AuditOnly)]
    [InlineData("", true, false, RiskEnforcementModeEnum.FailClosed)]
    [InlineData("", false, false, RiskEnforcementModeEnum.AuditOnly)]
    [InlineData("   ", false, true, RiskEnforcementModeEnum.FailClosed)]
    [InlineData("not-a-mode", true, false, RiskEnforcementModeEnum.FailClosed)]
    [InlineData("not-a-mode", false, false, RiskEnforcementModeEnum.AuditOnly)]
    [InlineData(null, false, false, RiskEnforcementModeEnum.AuditOnly)]
    public void Parser_Resolves_Default_Safe(
        string? riskPolicy, bool signatureRequired, bool deviceProofRequired,
        RiskEnforcementModeEnum expected)
    {
        var actual = RiskEnforcementModeParser.Parse(riskPolicy ?? string.Empty, signatureRequired, deviceProofRequired);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task NullAppAttestationVerifier_Throws_And_Never_Passes()
    {
        var verifier = new NullAppAttestationVerifier();
        await Assert.ThrowsAsync<NotImplementedException>(() => verifier.VerifyAsync(1, null!));
    }

    [Fact]
    public void AntiCheatEnums_Lock_Expected_Values()
    {
        Assert.Equal(1, (int)AntiCheatProviderEnum.TopOn);
        Assert.Equal(2, (int)AntiCheatProviderEnum.GoogleAdMob);
        Assert.Equal(3, (int)AntiCheatProviderEnum.UnityAds);
        Assert.Equal(99, (int)AntiCheatProviderEnum.Custom);

        Assert.Equal(1, (int)RiskLevelEnum.Low);
        Assert.Equal(4, (int)RiskLevelEnum.Critical);

        Assert.Equal(1, (int)AntiCheatSuggestedActionEnum.None);
        Assert.Equal(4, (int)AntiCheatSuggestedActionEnum.Ban);

        Assert.Equal(1, (int)ManualReviewStateEnum.Pending);
        Assert.Equal(4, (int)ManualReviewStateEnum.Escalated);

        Assert.Equal(1, (int)RiskEnforcementModeEnum.AuditOnly);
        Assert.Equal(2, (int)RiskEnforcementModeEnum.FailOpen);
        Assert.Equal(3, (int)RiskEnforcementModeEnum.FailClosed);

        Assert.Equal(1, (int)AntiCheatTargetTypeEnum.Account);
        Assert.Equal(2, (int)AntiCheatTargetTypeEnum.Role);
        Assert.Equal(3, (int)AntiCheatTargetTypeEnum.Device);
        Assert.Equal(4, (int)AntiCheatTargetTypeEnum.Ip);
    }
}