using GameFrameX.Entity.Tenant.Game.Release;
using GameFrameX.Core.SqlSugar;

namespace GameFrameX.Application.SeedData.Client;

/// <summary>
/// 客户端程序版本配置表 表种子数据
/// </summary>
public class GameAppReleaseVersionSeedData : ISqlSugarEntitySeedData<TenantGameAppReleaseVersionEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TenantGameAppReleaseVersionEntity> HasData()
    {
        var records = new List<TenantGameAppReleaseVersionEntity>
        {
            new()
            {
                Channel = "default",
                Platform = "default",
                AppVersion = "1.0.0",
                Package = "default",
                Language = "default",
                OldVersion = "1.0.0",
                DownloadUrl = "http://127.0.0.1/Bundles/com.ALianBlank.GFX/1.0.0/WebGL/DefaultPackage/20240130192515/",
                IsForce = false,
                Announcement = "xxxx",
                Description = "zzz",
                CreatedTime = 1706619915,
                UpdateTime = 1706621279,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                UpdatedId = 1300000000101,
                UpdatedName = "超级管理员",
                Title = "更新标题",
                IsDeleted = false,
                Id = 31858563436357,
            },
        };

        #region 处理 JsonIgnore 的Property

        #endregion

        // 后处理数据的特殊字段
        //for (int i = 0; i < records.Count; i++) { }

        return records;
    }
}