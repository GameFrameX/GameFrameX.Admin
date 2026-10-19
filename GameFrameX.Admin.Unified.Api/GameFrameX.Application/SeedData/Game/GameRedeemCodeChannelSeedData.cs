using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Application.SeedData.Game;

/// <summary>
/// 游戏兑换码渠道 表种子数据
/// </summary>
public class GameRedeemCodeChannelSeedData : ISqlSugarEntitySeedData<TenantGameRedeemCodeChannelEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TenantGameRedeemCodeChannelEntity> HasData()
    {
        var records = new List<TenantGameRedeemCodeChannelEntity>
        {
            new()
            {
                Name = "default",
                Description = "默认缺省渠道",
                CreatedTime = 1745836800,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = 31950206682437,
            },
        };

        #region 处理 JsonIgnore 的Property

        #endregion

        // 后处理数据的特殊字段
        //for (int i = 0; i < records.Count; i++) { }

        return records;
    }
}