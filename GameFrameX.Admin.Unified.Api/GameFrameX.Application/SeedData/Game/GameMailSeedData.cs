using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.SeedData.Game;

/// <summary>
/// 游戏邮件 表种子数据（全服草稿：GameMailDeliveryJob 仅按手动发送指定的 mailId 投递，草稿不会被自动扫描发出）
/// </summary>
public class GameMailSeedData : ISqlSugarEntitySeedData<GameMailEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GameMailEntity> HasData()
    {
        var records = new List<GameMailEntity>
        {
            new()
            {
                AreaId = 0,
                PlayerIdListJson = "[]",
                IsAllPost = true,
                Title = "欢迎来到 GameFrameX",
                SubTitle = "系统邮件",
                Body = "感谢您的支持，祝您游戏愉快！",
                AttachmentsJson = "[]",
                Status = GameMailStatus.Draft,
                ExpirationTime = 4102444800,
                MailType = GameMailType.SystemMail,
                IsTranslate = false,
                IsEnabled = true,
                CreatedTime = 1745836800,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = GameBusinessSeedIds.MailId,
            },
        };

        return records;
    }
}
