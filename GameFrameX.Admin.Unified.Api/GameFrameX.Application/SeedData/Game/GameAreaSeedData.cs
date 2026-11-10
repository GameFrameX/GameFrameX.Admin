using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.SeedData.Game;

/// <summary>
/// 游戏区服 表种子数据（默认示例区服，归入 default 区服分类；连接地址为占位值，实际部署后由运维修正）
/// </summary>
public class GameAreaSeedData : ISqlSugarEntitySeedData<GameAreaEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GameAreaEntity> HasData()
    {
        var records = new List<GameAreaEntity>
        {
            new()
            {
                Name = "default",
                Description = "默认区服",
                IsDefault = true,
                IsRecommend = true,
                ClassifyId = GameBusinessSeedIds.AreaClassifyId,
                ServerStatus = GameServerStatus.Online,
                HttpManageUrl = "https://game.example.test",
                TcpSocketUrl = "tcp://game.example.test:7000",
                WebSocketUrl = "wss://game.example.test/ws",
                Capacity = 1000,
                CurrentLoad = 0,
                CreationTime = 1745836800,
                ServerVersion = "1.0.0",
                ServerType = "Normal",
                LocalizationKey = "area_default",
                Language = GameLanguage.ChineseSimplified,
                AllowedPlayerProfile = "all",
                ServerEvents = "none",
                AccessControls = "public",
                ServerContainerId = string.Empty,
                ServerParams = "{}",
                AppVersion = "1.0.0",
                IsEnabled = true,
                CreatedTime = 1745836800,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                IsDeleted = false,
                Id = GameBusinessSeedIds.AreaId,
            },
        };

        return records;
    }
}
