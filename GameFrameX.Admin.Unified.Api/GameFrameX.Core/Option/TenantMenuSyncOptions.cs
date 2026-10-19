using GameFrameX.Core.Extension.Option;

namespace GameFrameX.Core.Option;

/// <summary>
/// 租户菜单同步管线配置选项（C353）。进程内 Channel 队列 + 后台 HostedService，单实例假设与 Admin host 内存缓存一致。
/// <para>配置节 <c>TenantMenuSync</c>（Web.Api appsettings.json）；取值下限约束在消费点执行（<see cref="Service.TenantMenuSync.TenantMenuSyncWorker"/>）。</para>
/// </summary>
[OptionsSettings("TenantMenuSync")]
public sealed class TenantMenuSyncOptions
{
    /// <summary>
    /// 后台执行并发上限（默认 4；每租户一次 Applier，互不阻塞；&lt;1 时消费点按 1 兜底）
    /// </summary>
    public int WorkerConcurrency { get; set; } = 4;

    /// <summary>
    /// 周期兜底扫描间隔秒数（默认 1800 = 30 分钟；失败自愈依赖该扫描，无需人工或重启；&lt;60 时消费点按 60 兜底）
    /// </summary>
    public int ScanIntervalSeconds { get; set; } = 1800;
}
