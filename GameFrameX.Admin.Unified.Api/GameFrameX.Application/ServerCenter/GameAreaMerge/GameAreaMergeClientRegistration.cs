using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameFrameX.Application.ServerCenter.GameAreaMerge;

/// <summary>注册独立合服客户端，使执行超时不受共享 Online 客户端默认值影响。</summary>
public static class GameAreaMergeClientRegistration
{
    /// <summary>按配置创建合服专用 HttpClient；未配置或无效时默认三十分钟。</summary>
    public static IHttpClientBuilder AddGameAreaMergeClient(this IServiceCollection services)
    {
        return services.AddHttpClient<GameAreaMergeClient>((provider, client) =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var configured = configuration["GameAreaMerge:ExecutionTimeoutSeconds"];
            client.Timeout = TimeSpan.FromSeconds(int.TryParse(configured, out var seconds) && seconds > 0 ? seconds : 1800);
        });
    }
}
