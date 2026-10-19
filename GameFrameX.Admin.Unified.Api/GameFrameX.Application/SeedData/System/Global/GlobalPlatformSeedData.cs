using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Global;

namespace GameFrameX.Application.SeedData.System.Global;

/// <summary>
/// 全局平台配置表 表种子数据
/// </summary>
public class GlobalPlatformSeedData : ISqlSugarEntitySeedData<GlobalPlatformEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GlobalPlatformEntity> HasData()
    {
        var records = new List<GlobalPlatformEntity>
        {
            new()
            {
                Name = "default",
                Description = "默认缺省平台",
                CreatedTime = 1706584855,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                IsDefault = true,
                Id = 31850202486597,
            },
            new()
            {
                Name = "Android",
                Description = "安卓平台",
                CreatedTime = 1706586319,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850577223493,
            },
            new()
            {
                Name = "iOS",
                Description = "IPhone或Ipad",
                CreatedTime = 1706586341,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850582821957,
            },
            new()
            {
                Name = "Windows",
                Description = "PC平台",
                CreatedTime = 1706586352,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850585613125,
            },
            new()
            {
                Name = "MacOs",
                Description = "MacOs平台",
                CreatedTime = 1706586373,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850590968133,
            },
            new()
            {
                Name = "WebGL",
                Description = "Web网页平台",
                CreatedTime = 1706586405,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172933,
            },
            new()
            {
                Name = "Linux",
                Description = "Linux平台",
                CreatedTime = 1706586406,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172934,
            },
            new()
            {
                Name = "UWP",
                Description = "Windows通用平台",
                CreatedTime = 1706586407,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172935,
            },
            new()
            {
                Name = "tvOS",
                Description = "Apple tvOS平台",
                CreatedTime = 1706586408,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172936,
            },
            new()
            {
                Name = "VisionOS",
                Description = "Apple VisionOS平台",
                CreatedTime = 1706586409,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172937,
            },
            new()
            {
                Name = "HarmonyOS",
                Description = "鸿蒙平台",
                CreatedTime = 1706586410,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172938,
            },
            new()
            {
                Name = "WeChatMiniGame",
                Description = "微信小游戏平台",
                CreatedTime = 1706586411,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172939,
            },
            new()
            {
                Name = "AlipayMiniGame",
                Description = "支付宝小游戏平台",
                CreatedTime = 1706586412,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172940,
            },
            new()
            {
                Name = "DouYinMiniGame",
                Description = "抖音小游戏平台",
                CreatedTime = 1706586413,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172941,
            },
            new()
            {
                Name = "KuaiShouMiniGame",
                Description = "快手小游戏平台",
                CreatedTime = 1706586414,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172942,
            },
            new()
            {
                Name = "BaiduMiniGame",
                Description = "百度小游戏平台",
                CreatedTime = 1706586415,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172943,
            },
            new()
            {
                Name = "JingDongMiniGame",
                Description = "京东小游戏平台",
                CreatedTime = 1706586416,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172944,
            },
            new()
            {
                Name = "TaobaoMiniGame",
                Description = "淘宝小程序平台",
                CreatedTime = 1706586417,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172945,
            },
            new()
            {
                Name = "MeituanMiniGame",
                Description = "美团小游戏平台",
                CreatedTime = 1706586418,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172946,
            },
            new()
            {
                Name = "BilibiliMiniGame",
                Description = "Bilibili小游戏平台",
                CreatedTime = 1706586419,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172947,
            },
            new()
            {
                Name = "DiscordMiniGame",
                Description = "Discord小游戏平台",
                CreatedTime = 1706586420,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172948,
            },
            new()
            {
                Name = "YouTubeMiniGame",
                Description = "YouTube小游戏平台",
                CreatedTime = 1706586421,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172949,
            },
            new()
            {
                Name = "FacebookMiniGame",
                Description = "Facebook小游戏平台",
                CreatedTime = 1706586422,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172950,
            },
            new()
            {
                Name = "GooglePlayMiniGame",
                Description = "Google Play小游戏平台",
                CreatedTime = 1706586423,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172951,
            },
            new()
            {
                Name = "TikTokMiniGame",
                Description = "TikTok小游戏平台",
                CreatedTime = 1706586424,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172952,
            },
            new()
            {
                Name = "CrazyGamesMiniGame",
                Description = "CrazyGames小游戏平台",
                CreatedTime = 1706586425,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172953,
            },
            new()
            {
                Name = "PokiMiniGame",
                Description = "Poki小游戏平台",
                CreatedTime = 1706586426,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172954,
            },
            new()
            {
                Name = "HuaweiMiniGame",
                Description = "华为小游戏平台",
                CreatedTime = 1706586427,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172955,
            },
            new()
            {
                Name = "OPPOMiniGame",
                Description = "OPPO小游戏平台",
                CreatedTime = 1706586428,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172956,
            },
            new()
            {
                Name = "VivoMiniGame",
                Description = "vivo小游戏平台",
                CreatedTime = 1706586429,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172957,
            },
            new()
            {
                Name = "XiaomiMiniGame",
                Description = "小米小游戏平台",
                CreatedTime = 1706586430,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172958,
            },
            new()
            {
                Name = "TapTapMiniGame",
                Description = "TapTap小游戏平台",
                CreatedTime = 1706586431,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172959,
            },
            new()
            {
                Name = "Xbox",
                Description = "Xbox平台",
                CreatedTime = 1706586432,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172960,
            },
            new()
            {
                Name = "PlayStation",
                Description = "PlayStation平台",
                CreatedTime = 1706586433,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172961,
            },
            new()
            {
                Name = "NintendoSwitch",
                Description = "Nintendo Switch平台",
                CreatedTime = 1706586434,
                UpdateTime = 1706617241,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850599172962,
            },
        };

        #region 处理 JsonIgnore 的Property

        #endregion

        // 后处理数据的特殊字段
        //for (int i = 0; i < records.Count; i++) { }

        return records;
    }
}