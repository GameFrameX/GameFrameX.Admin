using GameFrameX.Application.SeedData.System;
using GameFrameX.Application.SeedData.System.Global;

namespace GameFrameX.Core.Tests.AppManagement;

public class GlobalPlatformSeedDataTests
{
    [Fact]
    public void GlobalPlatformSeedData_IncludesUnityApplicationHelperPlatformNames()
    {
        var platformNames = new GlobalPlatformSeedData()
                            .HasData()
                            .Select(platform => platform.Name)
                            .ToHashSet(StringComparer.Ordinal);

        string[] expectedPlatformNames =
        {
            "Android",
            "iOS",
            "Windows",
            "MacOs",
            "Linux",
            "UWP",
            "tvOS",
            "VisionOS",
            "HarmonyOS",
            "WebGL",
            "WeChatMiniGame",
            "AlipayMiniGame",
            "DouYinMiniGame",
            "KuaiShouMiniGame",
            "BaiduMiniGame",
            "JingDongMiniGame",
            "TaobaoMiniGame",
            "MeituanMiniGame",
            "BilibiliMiniGame",
            "DiscordMiniGame",
            "YouTubeMiniGame",
            "FacebookMiniGame",
            "GooglePlayMiniGame",
            "TikTokMiniGame",
            "CrazyGamesMiniGame",
            "PokiMiniGame",
            "HuaweiMiniGame",
            "OPPOMiniGame",
            "VivoMiniGame",
            "XiaomiMiniGame",
            "TapTapMiniGame",
            "Xbox",
            "PlayStation",
            "NintendoSwitch",
        };

        foreach (var expectedPlatformName in expectedPlatformNames)
        {
            Assert.Contains(expectedPlatformName, platformNames);
        }
    }
}