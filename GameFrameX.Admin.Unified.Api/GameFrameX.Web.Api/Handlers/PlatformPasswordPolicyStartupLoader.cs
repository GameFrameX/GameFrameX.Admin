using GameFrameX.Core.Service.PlatformPasswordPolicy;
using Microsoft.Extensions.Hosting;

namespace GameFrameX.Web.Api.Handlers;

/// <summary>
/// 启动期加载平台密码策略到内存快照，并镜像同步 CryptogramUtility 密码策略静态属性（C173）。
/// </summary>
public sealed class PlatformPasswordPolicyStartupLoader : IHostedService
{
    private readonly PlatformPasswordPolicyConfigProvider _configProvider;

    public PlatformPasswordPolicyStartupLoader(PlatformPasswordPolicyConfigProvider configProvider)
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