using GameFrameX.Entity.Tenant.Game.RemoteConfig;
using GameFrameX.Core.SqlSugar;

namespace GameFrameX.Application.SeedData.Client;

/// <summary>
/// 客户端远程配置内容键定义表 种子数据
/// </summary>
public class GameAppRemoteConfigKeySeedData : ISqlSugarEntitySeedData<TenantGameAppRemoteConfigKeyEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    public IEnumerable<TenantGameAppRemoteConfigKeyEntity> HasData()
    {
        var records = new List<TenantGameAppRemoteConfigKeyEntity>
        {
            new()
            {
                KeyName = "maintenance",
                DisplayName = "维护模式",
                ValueType = "bool",
                Description = "是否处于维护状态",
                SortOrder = 0,
                IsEnabled = true,
                CreatedTime = 1745700000,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850206683001,
            },
            new()
            {
                KeyName = "noticeUrl",
                DisplayName = "公告地址",
                ValueType = "string",
                Description = "游戏内公告页面URL",
                SortOrder = 1,
                IsEnabled = true,
                CreatedTime = 1745700000,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850206683002,
            },
            new()
            {
                KeyName = "customerServiceUrl",
                DisplayName = "客服地址",
                ValueType = "string",
                Description = "客服系统页面URL",
                SortOrder = 2,
                IsEnabled = true,
                CreatedTime = 1745700000,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850206683003,
            },
            new()
            {
                KeyName = "enableRegister",
                DisplayName = "开放注册",
                ValueType = "bool",
                Description = "是否允许新用户注册",
                SortOrder = 3,
                IsEnabled = true,
                CreatedTime = 1745700000,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31850206683004,
            },
        };

        #region 处理 JsonIgnore 的Property

        foreach (var record in records)
        {
        }

        #endregion

        return records;
    }
}