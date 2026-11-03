using GameFrameX.Entity.Tenant.Game.ServerConfig;

namespace GameFrameX.Client.Api.GameUserSession;

/// <summary>
/// 登录配置差异下发内容源，读取主库启用配置，同键按主键降序确定唯一生效记录。
/// </summary>
public static class GameServerConfigProvider
{
    /// <summary>
    /// 加载生效配置行（键 → 内容版本号与配置 JSON 原文）。
    /// </summary>
    public static async Task<Dictionary<string, (string Md5, string Config)>> LoadEffectiveAsync(Repository<TenantGameAppServerConfigEntity> repository)
    {
        var rows = await repository.GetListAsync(u => u.Enabled);
        var effective = new Dictionary<string, (string, string)>(StringComparer.Ordinal);
        foreach (var row in rows.OrderByDescending(u => u.Id))
        {
            effective.TryAdd(row.Key, (row.Md5, row.Config));
        }

        return effective;
    }
}
