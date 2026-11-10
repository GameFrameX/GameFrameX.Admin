using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game.Announcement;

namespace GameFrameX.Application.SeedData.Game;

/// <summary>
/// 游戏内公告 表种子数据（Language=default 兜底公告，区服不限）
/// </summary>
public class GameAnnouncementSeedData : ISqlSugarEntitySeedData<GameAnnouncementEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GameAnnouncementEntity> HasData()
    {
        var records = new List<GameAnnouncementEntity>
        {
            new()
            {
                Language = "default",
                AreaId = null,
                Title = "欢迎来到 GameFrameX",
                TitleLocalizationKey = string.Empty,
                Content = "欢迎来到 GameFrameX，祝您游戏愉快！",
                ContentLocalizationKey = string.Empty,
                StartTime = 1704067200,
                EndTime = 4102444800,
                Priority = 10,
                IsActive = true,
                IsEnabled = true,
                CreatedTime = 1745836800,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = GameBusinessSeedIds.AnnouncementId,
            },
        };

        return records;
    }
}
