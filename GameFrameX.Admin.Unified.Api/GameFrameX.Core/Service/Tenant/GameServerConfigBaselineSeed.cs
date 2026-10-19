using System.Reflection;
using System.Text.Json;
using GameFrameX.Entity.Tenant.Game.ServerConfig;

namespace GameFrameX.Core.Service.Tenant;

/// <summary>
/// 游戏运营配置出厂基线行工厂（C314）。
/// <para>单一事实源为嵌入资源 GameServerConfigs.json（原厂实测 10 键 + 原厂 md5），
/// 供两条种子渠道共享：租户库建库种子（DefaultTenantDatabaseSeeder，自动推导租户连接 EnableInitSeed=false
/// 时 ISqlSugarEntitySeedData 渠道不生效，故走本工厂）与框架种子（GameAppServerConfigSeedData 薄壳）。</para>
/// <para>md5 沿用原厂值不重算——拉过原厂配置的老客户端 md5 自洽不重传；行 Id 用固定基数保证跨环境稳定。</para>
/// </summary>
public static class GameServerConfigBaselineSeed
{
    private const string ResourceName = "GameFrameX.Core.SeedData.Client.GameServerConfigs.json";
    private const long IdBase = 31920000000000;
    private const long SeedTime = 1745700000;

    /// <summary>
    /// 构建全部出厂基线行（AppId=null 租户默认；登录下发时被应用行按 Key 覆盖）
    /// </summary>
    public static List<TenantGameAppServerConfigEntity> BuildRows()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using (var stream = assembly.GetManifestResourceStream(ResourceName))
        {
            if (stream == null)
            {
                throw new InvalidOperationException("嵌入资源缺失：" + ResourceName);
            }

            using (var document = JsonDocument.Parse(stream))
            {
                var records = new List<TenantGameAppServerConfigEntity>();
                var index = 0;
                foreach (var property in document.RootElement.EnumerateObject())
                {
                    var entry = property.Value;
                    records.Add(new TenantGameAppServerConfigEntity
                    {
                        AppId = null,
                        Key = property.Name,
                        Config = entry.GetProperty("config").GetRawText(),
                        Md5 = entry.GetProperty("md5").GetString() ?? string.Empty,
                        Enabled = true,
                        Description = "原厂出厂基线（种子）",
                        CreatedTime = SeedTime,
                        UpdateTime = SeedTime,
                        Id = IdBase + index,
                    });
                    index++;
                }

                return records;
            }
        }
    }
}
