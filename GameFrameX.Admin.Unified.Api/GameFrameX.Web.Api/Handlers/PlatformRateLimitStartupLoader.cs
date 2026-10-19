using GameFrameX.Core.Service.PlatformRateLimit;
using Microsoft.Extensions.Hosting;

namespace GameFrameX.Web.Api.Handlers;

/// <summary>
/// 启动期加载平台限流配置到内存快照。
/// </summary>
public sealed class PlatformRateLimitStartupLoader : IHostedService
{
    private readonly PlatformRateLimitConfigProvider _configProvider;

    public PlatformRateLimitStartupLoader(PlatformRateLimitConfigProvider configProvider)
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