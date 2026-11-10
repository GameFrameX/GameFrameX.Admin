using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game.Announcement;

namespace GameFrameX.Application.SeedData.Game;

/// <summary>
/// 游戏横幅公告 表种子数据（AreaId=0 全区服生效）
/// </summary>
public class GameBannerAnnouncementSeedData : ISqlSugarEntitySeedData<GameBannerAnnouncementEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GameBannerAnnouncementEntity> HasData()
    {
        var records = new List<GameBannerAnnouncementEntity>
        {
            new()
            {
                AreaId = 0,
                Content = "欢迎来到 GameFrameX！",
                ContentLocalizationKey = "banner_welcome_default",
                Priority = 10,
                StartTime = 1704067200,
                EndTime = 4102444800,
                Interval = 60,
                RepeatCount = 3,
                IsActive = true,
                IsEnabled = true,
                CreatedTime = 1745836800,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = GameBusinessSeedIds.BannerAnnouncementId,
            },
        };

        return records;
    }
}
