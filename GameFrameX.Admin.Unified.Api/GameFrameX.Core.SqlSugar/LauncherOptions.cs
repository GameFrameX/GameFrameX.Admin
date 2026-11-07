using GameFrameX.Foundation.Options.Attributes;

namespace GameFrameX.Core.SqlSugar;

/// <summary>
/// Client / Game / Hub 三 host 共享的进程启动选项。
/// <para>本类只承载必须在数据库 Provider 初始化之前可用的进程级启动参数：日志管道开关。数据库连接一律经 HostMainDbConnectionResolver（DbConnection 节 / 共享 Database.json / GFX_MAIN_DB_* 环境变量）解析，不进本类。</para>
/// <para>所有请求期业务配置（邮件、广告回调、沙箱、区域输出、Client API token、Game 登录票据策略等）已迁移到主库，通过 <c>IClientApiOptionsProvider</c> 按应用读取，禁止回流到本类。</para>
/// </summary>
public sealed class LauncherOptions
{
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