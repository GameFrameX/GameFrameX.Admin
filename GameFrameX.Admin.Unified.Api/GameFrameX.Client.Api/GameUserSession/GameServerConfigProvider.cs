using GameFrameX.Entity.Tenant.Game.ServerConfig;

namespace GameFrameX.Client.Api.GameUserSession;

/// <summary>
/// 登录 config 差异下发内容源查询（C315：数据源自嵌入种子切租户库 tenant_game_app_server_config 表）。
/// <para>合并语义（FallbackMatcher 同款「应用特定 > 租户默认」）：AppId=null 出厂基线行打底，
/// 当前应用行按 Key 覆盖；Enabled=false 的键不下发（= 客户端删本地该键，与原厂「无内容不下发」一致）。</para>
/// <para>appId=0（无 App 上下文）时仅剩基线行，协议形态不变。</para>
/// </summary>
public static class GameServerConfigProvider
{
    /// <summary>
    /// 加载当前请求视角下生效的配置行（键 → md5 版本号 + 配置 JSON 原文）
    /// </summary>
    public static async Task<Dictionary<string, (string Md5, string Config)>> LoadEffectiveAsync(Repository<TenantGameAppServerConfigEntity> repository, long appId)
    {
        var rows = await repository.GetListAsync(u => u.Enabled && (u.AppId == null || u.AppId == appId));
        var effective = new Dictionary<string, (string, string)>(StringComparer.Ordinal);

        // 基线行（AppId=null）先入字典打底，应用行后入覆盖同键
        foreach (var row in rows.OrderBy(u => u.AppId == null ? 0 : 1))
        {
            effective[row.Key] = (row.Md5, row.Config);
        }

        return effective;
    }
}
