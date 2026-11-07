using GameFrameX.Foundation.Options.Attributes;

namespace GameFrameX.Core.SqlSugar;

/// <summary>
/// Client / Game / Hub 三 host 共享的进程启动选项。
/// <para>本类是进程启动参数的统一入口：数据库连接（命令行 --DbType/--ConnectionString 与 GFX_MAIN_DB_* 环境变量同一入口，命令行优先）与日志管道开关；数据库配置最终经 LauncherOptionsMainDatabase 以最高优先级进入配置管线。</para>
/// <para>所有请求期业务配置（邮件、广告回调、沙箱、区域输出、Client API token、Game 登录票据策略等）已迁移到主库，通过 <c>IClientApiOptionsProvider</c> 按应用读取，禁止回流到本类。</para>
/// </summary>
public sealed class LauncherOptions
{
    /// <summary>
    /// 数据库类型。命令行 --DbType 与环境变量 GFX_MAIN_DB_TYPE 统一入口（命令行优先）；
    /// 与 <see cref="ConnectionString"/> 必须成对提供，半套在进入配置管线时拒绝。
    /// </summary>
    [Option(nameof(DbType), Required = false, EnvironmentVariable = "GFX_MAIN_DB_TYPE")]
    public string DbType { get; set; } = null!;

    /// <summary>
    /// 数据库连接字符串。命令行 --ConnectionString 与环境变量 GFX_MAIN_DB_CONNECTION_STRING 统一入口（命令行优先）；
    /// 与 <see cref="DbType"/> 必须成对提供。
    /// </summary>
    [Option(nameof(ConnectionString), Required = false, EnvironmentVariable = "GFX_MAIN_DB_CONNECTION_STRING", Sensitive = true)]
    public string ConnectionString { get; set; } = null!;
    /// <summary>
    /// Grafana Loki 服务的 URL 地址。进程级日志管道在业务请求前初始化，Loki endpoint 属于 host 观测设施，不随请求切换。
    /// </summary>
    [Option(nameof(GrafanaLokiUrl), Required = false)]
    public string GrafanaLokiUrl { get; set; } = null!;

    /// <summary>
    /// 是否启用 Grafana Loki 日志记录。控制进程是否启用 Loki sink，属于部署 / 运维开关。
    /// </summary>
    [Option(nameof(IsGrafanaLoki), DefaultValue = false)]
    public bool IsGrafanaLoki { get; set; }

    /// <summary>
    /// 是否启用控制台日志记录。控制进程控制台日志输出，属于部署 / 运维开关。
    /// </summary>
    [Option(nameof(IsConsole), DefaultValue = false)]
    public bool IsConsole { get; set; }
}