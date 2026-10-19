using GameFrameX.Core.Service.PlatformRateLimit;

namespace GameFrameX.Core.Tests.System;

public class PlatformRateLimitConfigTests
{
    [Fact]
    public void Serialize_DefaultSettings_StaysWithinSystemConfigValueLimit()
    {
        var settings = PlatformRateLimitConfigProvider.CreateDefaultSettings();

        var value = PlatformRateLimitConfigProvider.Serialize(settings);

        Assert.Contains("\"permitLimit\":20", value);
        Assert.True(value.Length <= PlatformRateLimitConfigConstants.MaxSerializedLength);
    }

    [Fact]
    public void BuildPartitionKey_IncludesVersionAndIp()
    {
        var settings = new PlatformRateLimitSettings { Version = 7, PermitLimit = 30, WindowSeconds = 2, };

        var key = PlatformRateLimitConfigProvider.BuildPartitionKey(settings, "127.0.0.1");

        Assert.Equal("v7:127.0.0.1", key);
    }

    [Theory]
    [InlineData(0, 1, 0)]
    [InlineData(20, 0, 0)]
    [InlineData(20, 1, -1)]
    public void Validate_RejectsInvalidLimits(int permitLimit, int windowSeconds, int queueLimit)
    {
        var settings = new PlatformRateLimitSettings
        {
            PermitLimit = permitLimit,
            WindowSeconds = windowSeconds,
            QueueLimit = queueLimit,
        };

        Assert.ThrowsAny<Exception>(() => PlatformRateLimitConfigProvider.Validate(settings));
    }

    [Fact]
    public void StaticLimitJson_IsRemovedFromSourceAndProjectCopyItems()
    {
        var repoRoot = FindRepoRoot();

        Assert.False(File.Exists(Path.Combine(repoRoot, "GameFrameX.Application/Configuration/Limit.json")));
        var projectFile = File.ReadAllText(Path.Combine(repoRoot, "GameFrameX.Application/GameFrameX.Application.csproj"));
        Assert.DoesNotContain("Configuration\\Limit.json", projectFile);
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "GameFrameX.Admin.Unified.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Cannot locate GameFrameX.Admin.Unified.slnx");
    }
}