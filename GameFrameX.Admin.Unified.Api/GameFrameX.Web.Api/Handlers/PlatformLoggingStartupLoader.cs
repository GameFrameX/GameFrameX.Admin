using GameFrameX.Core.Service.PlatformLogging;
using Microsoft.Extensions.Hosting;

namespace GameFrameX.Web.Api.Handlers;

/// <summary>
/// 启动期加载平台日志配置到内存快照，供后续日志写路径读取启停开关。
/// </summary>
public sealed class PlatformLoggingStartupLoader : IHostedService
{
    private readonly PlatformLoggingConfigProvider _configProvider;

    public PlatformLoggingStartupLoader(PlatformLoggingConfigProvider configProvider)
    {
        _configProvider = configProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _configProvider.RefreshAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}