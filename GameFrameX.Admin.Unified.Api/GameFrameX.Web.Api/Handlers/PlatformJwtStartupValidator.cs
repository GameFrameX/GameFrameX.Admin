using GameFrameX.Core.Service.PlatformJwt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GameFrameX.Web.Api.Handlers;

/// <summary>
/// 平台 JWT 启动校验。
/// </summary>
public sealed class PlatformJwtStartupValidator : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public PlatformJwtStartupValidator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Host 启动时先确保主库存在有效平台 JWT 配置（缺失则自动生成随机密钥缺省配置），再执行安全基线校验；禁止回退到静态 JSON。
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var provider = scope.ServiceProvider.GetRequiredService<PlatformJwtRuntimeProvider>();
        await provider.EnsureDefaultSettingsAsync();
        await provider.GetRequiredSettingsAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}