using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.SeedData.Game;

/// <summary>
/// 邮件模板 表种子数据
/// </summary>
public class GameMailTemplateSeedData : ISqlSugarEntitySeedData<GameMailTemplateEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GameMailTemplateEntity> HasData()
    {
        var records = new List<GameMailTemplateEntity>
        {
            new()
            {
                Name = "默认系统邮件",
                Description = "默认系统邮件模板",
                CategoryId = GameBusinessSeedIds.MailTemplateCategoryId,
                TitleTemplate = "系统公告",
                SubTitleTemplate = string.Empty,
                BodyTemplate = "亲爱的玩家：\n{{content}}",
                DefaultAttachmentsJson = "[]",
                DefaultMailType = GameMailType.SystemMail,
                DefaultExpirationDays = 30,
                Sort = 0,
                IsEnabled = true,
                CreatedTime = 1745836800,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = GameBusinessSeedIds.MailTemplateId,
            },
        };

        return records;
    }
}
