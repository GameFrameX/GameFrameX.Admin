using GameFrameX.Core.Bootstrap;
using Microsoft.Extensions.Hosting;
using SqlSugar;

namespace GameFrameX.Web.Api.Handlers;

/// <summary>
/// 启动期执行首个业务管理员种子引导（C406 / F3）。
/// <para>AddSqlSugar（旧库守卫 + 建表 + 种子 + MenuSeedCatalog 加载）在装配期同步完成，
/// HostedService 在其后运行；注册顺序在 PlatformPasswordPolicyStartupLoader 之后，
/// 保证密码强度策略静态属性已就位再做 fail-closed 校验。仅 Web.Api 装配（Client/Hub 不引导）。</para>
/// </summary>
public sealed class InitialAdminStartupBootstrap : IHostedService
{
    private readonly ISqlSugarClient _sqlSugarClient;

    public InitialAdminStartupBootstrap(ISqlSugarClient sqlSugarClient)
    {
        _sqlSugarClient = sqlSugarClient;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return InitialAdminBootstrap.RunAsync(_sqlSugarClient);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
