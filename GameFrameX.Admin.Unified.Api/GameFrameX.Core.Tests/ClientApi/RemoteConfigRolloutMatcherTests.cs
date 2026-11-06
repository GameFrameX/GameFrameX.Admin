using GameFrameX.Client.Api.Services.RemoteConfig;
using GameFrameX.Client.Api.Services;
using GameFrameX.Entity.Business.Game.RemoteConfig;

namespace GameFrameX.Core.Tests.ClientApi;

public class RemoteConfigRolloutMatcherTests
{
    [Fact]
    public void IsMatch_DisabledRecord_ReturnsFalse()
    {
        var record = CreateRecord(rolloutPercent: 100);
        record.IsEnabled = false;

        Assert.False(RemoteConfigRolloutMatcher.IsMatch(record, "device-a"));
    }

    [Fact]
    public void IsMatch_WhitelistedDevice_BypassesRolloutPercent()
    {
        var record = CreateRecord(rolloutPercent: 0);
        record.DeviceWhitelistJson = "[\"device-a\"]";

        Assert.True(RemoteConfigRolloutMatcher.IsMatch(record, "DEVICE-A"));
    }

    [Fact]
    public void IsMatch_MissingDeviceId_OnlyMatchesFullRollout()
    {
        Assert.True(RemoteConfigRolloutMatcher.IsMatch(CreateRecord(rolloutPercent: 100), ""));
        Assert.False(RemoteConfigRolloutMatcher.IsMatch(CreateRecord(rolloutPercent: 99), ""));
    }

    [Fact]
    public void SelectWinner_UsesPriorityBeforeScopeScore()
    {
        var lowPriorityExact = CreateRecord(1, 1, 100);
        var highPriorityDefault = CreateRecord(2, 10, 100);

        var winner = RemoteConfigRolloutMatcher.SelectWinner(
            new[]
            {
                new RemoteConfigScoredCandidate(lowPriorityExact, 100),
                new RemoteConfigScoredCandidate(highPriorityDefault, 10),
            },
            "device-a");

        Assert.Same(highPriorityDefault, winner);
    }

    [Fact]
    public void SelectWinner_SamePriorityAndScore_UsesLatestEnabledRecord()
    {
        var previous = CreateRecord(1, 10, 100);
        var latest = CreateRecord(2, 10, 100);
        var disabled = CreateRecord(3, 10, 100);
        disabled.IsEnabled = false;

        var winner = RemoteConfigRolloutMatcher.SelectWinner(
            new[]
            {
                new RemoteConfigScoredCandidate(previous, 10),
                new RemoteConfigScoredCandidate(latest, 10),
                new RemoteConfigScoredCandidate(disabled, 10),
            },
            "device-a");

        Assert.Same(latest, winner);
    }

    private static GameAppRemoteConfigEntity CreateRecord(
        long id = 1,
        int priority = 0,
        int rolloutPercent = 100)
    {
        return new GameAppRemoteConfigEntity
        {
            Id = id,
            Priority = priority,
            RolloutPercent = rolloutPercent,
            Content = "{}",
            Channel = "google",
            Platform = "android",
            AppVersion = "1.0.0",
            Package = "main",
            Language = "zh-cn",
            CheckAppVersionUrl = "",
            CheckResourceUrl = "",
            AotCodeList = "",
            Description = "",
        };
    }
}