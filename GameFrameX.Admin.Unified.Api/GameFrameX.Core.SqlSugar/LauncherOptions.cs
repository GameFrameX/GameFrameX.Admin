using GameFrameX.Foundation.Options.Attributes;

namespace GameFrameX.Core.SqlSugar;

/// <summary>
/// Client / Game / Hub 三 host 共享的进程启动选项。
/// <para>本类只承载必须在数据库 Provider 初始化之前可用的进程级启动参数：数据库类型、数据库连接字符串、日志管道开关。</para>
/// <para>所有请求期业务配置（邮件、广告回调、沙箱、区域输出、Client API token、Game 登录票据策略等）已迁移到租户库，通过 <c>IClientApiTenantOptionsProvider</c> 按租户 / 应用读取，禁止回流到本类。</para>
/// </summary>
public sealed class LauncherOptions
{
    /// <summary>
    /// 数据库类型（兼容层）。C269 起正式入口为 GFX_MAIN_DB_TYPE + GFX_MAIN_DB_CONNECTION_STRING 成对环境变量（或 DbConnection 配置节），
    /// 本参数仅在两者皆缺失时由 HostMainDbConnectionResolver 兜底消费并打迁移警告，故不再强制必填。
    /// </summary>
    [Option(nameof(DbType), Required = false)]
    public string DbType { get; set; } = null!;

    /// <summary>
    /// 数据库连接字符串（兼容层）。消费路径与迁移提示同 <see cref="DbType"/>，不再强制必填。
    /// </summary>
    [Option(nameof(ConnectionString), Required = false)]
    public string ConnectionString { get; set; } = null!;

    /// <summary>
    /// Grafana Loki 服务的 URL 地址。进程级日志管道在业务请求前初始化，Loki endpoint 属于 host 观测设施，不应按租户切换。
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