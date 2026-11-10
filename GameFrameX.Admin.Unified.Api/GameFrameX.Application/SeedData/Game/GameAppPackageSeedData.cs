using GameFrameX.Entity.Enum;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game.App;

namespace GameFrameX.Application.SeedData.Game;

/// <summary>
/// 游戏应用分包 表种子数据（default 分包：平台引用 global 平台 default 行、渠道引用既有 business-default 业务渠道种子）
/// </summary>
public class GameAppPackageSeedData : ISqlSugarEntitySeedData<GameAppPackageEntity>
{
    /// <summary>
    /// business-default 业务渠道种子行 Id（SeedData/System/BusinessChannelSeedData）
    /// </summary>
    private const long BusinessChannelSeedId = 31950206982437;

    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GameAppPackageEntity> HasData()
    {
        var records = new List<GameAppPackageEntity>
        {
            new()
            {
                Name = "default",
                Description = "默认分包",
                PackageName = "default",
                PlatformCode = "default",
                PlatformName = "default",
                ChannelId = BusinessChannelSeedId,
                ChannelCode = "business-default",
                ChannelName = "默认业务渠道",
                PackageKind = PackageKindEnum.Normal,
                PlatformInheritanceMode = InheritanceModeEnum.Override,
                PackageVersion = "1.0.0",
                IsDefault = true,
                IsEnabled = true,
                CreatedTime = 1745836800,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = GameBusinessSeedIds.AppPackageId,
            },
        };

        return records;
    }
}
