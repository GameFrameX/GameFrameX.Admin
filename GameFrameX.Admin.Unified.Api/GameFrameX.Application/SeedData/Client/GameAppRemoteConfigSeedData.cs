using GameFrameX.Entity.Business.Game.RemoteConfig;
using GameFrameX.Core.SqlSugar;

namespace GameFrameX.Application.SeedData.Client;

/// <summary>
/// 客户端远程配置表 表种子数据
/// </summary>
public class GameAppRemoteConfigSeedData : ISqlSugarEntitySeedData<GameAppRemoteConfigEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<GameAppRemoteConfigEntity> HasData()
    {
        var records = new List<GameAppRemoteConfigEntity>
        {
            new()
            {
                Channel = "default",
                Platform = "default",
                AppVersion = "default",
                Package = "default",
                Language = "default",
                // 检测地址指向公网 Client.Api 域名（2026-11-10 决策）：资源检测走同域 Bundles 路径，
                // 版本检测路径与 Client.Api 路由模板 api/[controller]/[action] 对齐；
                // Content 承载 API 端点地址清单（契约见 gfx-doc C15：schemaVersion 门卫 + 空键不覆盖）
                CheckResourceUrl = "https://gameframex.client.api.alianblank.com/Bundles/com.ALianBlank.GFX/1.0.0/WebGL/DefaultPackage/20240130192515/",
                CheckAppVersionUrl = "https://gameframex.client.api.alianblank.com/api/GameAppVersion/GetInfo",
                Content = "{\"schemaVersion\":1,\"endpoints\":{\"gameApiBaseUrl\":\"http://127.0.0.1:20808\",\"clientApiBaseUrl\":\"http://127.0.0.1:20808\"}}",
                AotCodeList = "[]",
                Description = "默认",
                CreatedTime = 1706620439,
                UpdateTime = 1706621341,
                CreatedId = 1300000000101,
                CreatedName = "超级管理员",
                UpdatedId = 1300000000101,
                UpdatedName = "超级管理员",
                IsDeleted = false,
                Id = 31858697374789,
            },
        };

        #region 处理 JsonIgnore 的Property

        foreach (var record in records)
        {
            record.ModeContents = JsonSerializer.Serialize(new Dictionary<string, object>
            {
                { "formal", new { } },
            });
            record.ActiveModeCode = "formal";
        }

        #endregion

        return records;
    }
}