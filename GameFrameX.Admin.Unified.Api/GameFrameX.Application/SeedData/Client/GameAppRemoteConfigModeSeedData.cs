using GameFrameX.Entity.Tenant.Game.RemoteConfig;
using GameFrameX.Core.SqlSugar;

namespace GameFrameX.Application.SeedData.Client;

/// <summary>
/// 客户端远程配置模式定义表 种子数据
/// </summary>
public class GameAppRemoteConfigModeSeedData : ISqlSugarEntitySeedData<TenantGameAppRemoteConfigModeEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    public IEnumerable<TenantGameAppRemoteConfigModeEntity> HasData()
    {
        var records = new List<TenantGameAppRemoteConfigModeEntity>
        {
            new()
            {
                ModeCode = "review",
                DisplayName = "审核模式",
                Description = "应用商店审核用",
                SortOrder = 0,
                IsEnabled = true,
                CreatedTime = 1745700000,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850206684001,
            },
            new()
            {
                ModeCode = "formal",
                DisplayName = "正式模式",
                Description = "线上运营用",
                SortOrder = 1,
                IsEnabled = true,
                CreatedTime = 1745700000,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850206684002,
            },
        };

        #region 处理 JsonIgnore 的Property

        #endregion

        return records;
    }
}