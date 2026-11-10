using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game;

namespace GameFrameX.Application.SeedData.Game;

/// <summary>
/// 邮件模板分类 表种子数据
/// </summary>
public class GameMailTemplateCategorySeedData : ISqlSugarEntitySeedData<GameMailTemplateCategoryEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GameMailTemplateCategoryEntity> HasData()
    {
        var records = new List<GameMailTemplateCategoryEntity>
        {
            new()
            {
                Name = "default",
                Description = "默认邮件模板分类",
                Icon = string.Empty,
                Sort = 0,
                IsEnabled = true,
                CreatedTime = 1745836800,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = GameBusinessSeedIds.MailTemplateCategoryId,
            },
        };

        return records;
    }
}
