using GameFrameX.Core.Extension.Option;
using GameFrameX.Core.Option;
using Microsoft.Extensions.Configuration;

namespace GameFrameX.Core.Tests.Option;

// C172（GFX-175）：退役 App.json 中主机与启动框架配置。
// 证明 AllowedHosts / AppSettings / SnowFlake 的事实源已迁入 Web.Api/appsettings.json。
// C177（GFX-176）：最终删除 Configuration/App.json，Cryptogram / GameServerSettings 的运行时来源
// 已分别由 CryptogramSecretResolver 内置默认（+环境变量）与区服级 DB 配置承接，App.json 不再存在。
// C204（GFX-633）：历史框架退役后 DynamicApiControllerSettings / FriendlyExceptionSettings /
// LocalizationSettings / CorsAccessorSettings 四节已无 IConfigurableOptions 绑定，从 appsettings.json 删除；
// 动态 API 路由（lowerCamelCase / 去 Service 后缀 / api 前缀）已由 AdminApiRouteRules 硬编码承接，不再读 JSON。
public sealed class HostStartupConfigMigrationTests
{
    [Fact]
    public void HostAppSettings_ProvidesMigratedStartupConfig_WithoutAppJson()
    {
        var config = BuildConfig(Path.Combine(FindRepoRoot(), "GameFrameX.Web.Api", "appsettings.json"));

        // AllowedHosts
        Assert.Equal("*", config["AllowedHosts"]);

        // AppSettings
        Assert.True(config.GetSection("AppSettings").GetValue<bool>("InjectSpecificationDocument"));

        // SnowFlake（Foundation Snowflake 紧凑布局配置：仅 WorkerId，0-7；C360 / GFX-803 起
        // DataCenterId 随维度移除、WorkerPrefix 随 Redis 租约退役删除）
        Assert.Equal("1", config["SnowFlake:WorkerId"]);
        Assert.Null(config["SnowFlake:DataCenterId"]);
        Assert.Null(config["SnowFlake:WorkerPrefix"]);

        // SnowFlakeIdOptions 强类型绑定（SqlSugarSetup / AddConfigurableOptions<SnowFlakeIdOptions> 依赖此路径）
        var snowFlakeOptions = config.GetSection("SnowFlake").Get<SnowFlakeIdOptions>();
        Assert.NotNull(snowFlakeOptions);
        Assert.Equal(1, snowFlakeOptions!.WorkerId);
    }

    [Fact]
    public void TenantMenuSyncOptions_BindsToAppSettingsSection()
    {
        // OptionsSettings 特性缺失时 AddConfigurableOptions 会按类名绑定 "TenantMenuSyncOptions" 节（不存在），
        // appsettings.json 的 TenantMenuSync 节被静默丢弃——默认值恰好与文件值相同掩盖过失效；特性与节名必须对齐。
        var attribute = typeof(TenantMenuSyncOptions).GetCustomAttributes(typeof(OptionsSettingsAttribute), false)
                                                     .OfType<OptionsSettingsAttribute>()
                                                     .FirstOrDefault();
        Assert.NotNull(attribute);
        Assert.Equal("TenantMenuSync", attribute!.Path);

        var config = BuildConfig(Path.Combine(FindRepoRoot(), "GameFrameX.Web.Api", "appsettings.json"));
        var options = config.GetSection("TenantMenuSync").Get<TenantMenuSyncOptions>();
        Assert.NotNull(options);
        Assert.Equal(config.GetValue<int>("TenantMenuSync:WorkerConcurrency"), options!.WorkerConcurrency);
        Assert.Equal(config.GetValue<int>("TenantMenuSync:ScanIntervalSeconds"), options.ScanIntervalSeconds);
    }

    [Fact]
    public void AppJson_HasBeenRemoved()
    {
        // C177（GFX-176）：Configuration/App.json 已最终删除。
        // 主机/启动事实源在 appsettings.json（见上例，AllowedHosts / AppSettings / SnowFlake）；Cryptogram 由 CryptogramSecretResolver
        // 内置默认 + 环境变量承接；GameServerSettings 由区服级 DB 配置承接。App.json 不应再存在。
        var appJsonPath = Path.Combine(FindRepoRoot(), "GameFrameX.Application", "Configuration", "App.json");

        Assert.False(File.Exists(appJsonPath));
    }

    private static IConfiguration BuildConfig(string path)
    {
        Assert.True(File.Exists(path), $"配置文件不存在：{path}");
        return new ConfigurationBuilder().AddJsonFile(path, false).Build();
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "GameFrameX.Admin.Unified.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Cannot find GameFrameX.Admin.Unified.slnx from test working directory.");
    }
}