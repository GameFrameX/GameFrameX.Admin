using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game.Release;

namespace GameFrameX.Application.SeedData.Game;

/// <summary>
/// 客户端资源包版本 表种子数据（DefaultPackage × 1.0.0，资源下载根路径与远程配置检测地址同域）
/// </summary>
public class GameAppAssetPackageVersionSeedData : ISqlSugarEntitySeedData<GameAppAssetPackageVersionEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GameAppAssetPackageVersionEntity> HasData()
    {
        var records = new List<GameAppAssetPackageVersionEntity>
        {
            new()
            {
                Channel = "default",
                Platform = "default",
                AppVersion = "1.0.0",
                Package = "default",
                Language = "default",
                AssetPackageName = "DefaultPackage",
                RootPath = "https://gameframex.client.api.alianblank.com/Bundles/com.ALianBlank.GFX/1.0.0/WebGL/DefaultPackage/",
                ResourceVersion = "20240130192515",
                Description = "默认资源包版本",
                DeviceGroupId = null,
                IsEnabled = true,
                CreatedTime = 1745836800,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = GameBusinessSeedIds.AssetPackageVersionId,
            },
        };

        return records;
    }
}
