using GameFrameX.Entity.Tenant.Game.Resource;
using GameFrameX.Core.SqlSugar;

namespace GameFrameX.Application.SeedData.Client;

/// <summary>
/// 客户端资源包 表种子数据
/// </summary>
public class GameAppAssetPackageSeedData : ISqlSugarEntitySeedData<TenantGameAppAssetPackageEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TenantGameAppAssetPackageEntity> HasData()
    {
        var records = new List<TenantGameAppAssetPackageEntity>
        {
            new()
            {
                Name = "DefaultPackage",
                Description = "默认资源包名称",
                CreatedTime = 1745836800,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31950206882437,
            },
        };

        #region 处理 JsonIgnore 的Property

        #endregion

        // 后处理数据的特殊字段
        //for (int i = 0; i < records.Count; i++) { }

        return records;
    }
}