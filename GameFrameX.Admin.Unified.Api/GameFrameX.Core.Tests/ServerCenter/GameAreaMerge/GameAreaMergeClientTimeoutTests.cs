using GameFrameX.Application.ServerCenter.GameAreaMerge;
using GameFrameX.Application.ServerCenter.Online;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Business.Game;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GameFrameX.Core.Tests.ServerCenter.GameAreaMerge;

public class GameAreaMergeClientTimeoutTests
{
    [Theory]
    [InlineData(null, 1800)]
    [InlineData("2400", 2400)]
    [InlineData("0", 1800)]
    [InlineData("invalid", 1800)]
    public async Task Execute_UsesConfiguredDedicatedHttpClientTimeout(string? configuredSeconds, int expectedSeconds)
    {
        using (var context = new GameAreaMergeTestContext())
        {
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GameAreaMerge:ExecutionTimeoutSeconds"] = configuredSeconds,
            }).Build();
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<IHttpContextAccessor>(context.Accessor);
            services.AddSingleton(context.Repository<GameAreaEntity>());
            services.AddSingleton(context.Repository<GameAreaMergePlanEntity>());
            services.AddScoped<OnlineScopeContextFactory>();
            services.AddScoped<IOnlineServerEndpointResolver, OnlineServerEndpointResolver>();
            services.AddScoped<CurrentUserContext>();
            services.AddScoped<GameAreaMergeService>();
            var sharedClientResolved = false;
            services.AddHttpClient<IOnlineServerClient, OnlineServerClient>().ConfigureHttpClient(client =>
            {
                sharedClientResolved = true;
                client.Timeout = TimeSpan.FromSeconds(100);
            });

            HttpClient? executionClient = null;
            // 复用 Startup 调用的实际注册入口，不能在测试中重新复制超时配置。
            services.AddGameAreaMergeClient()
                .ConfigureHttpClient(client => executionClient = client)
                .ConfigurePrimaryHttpMessageHandler(() => context.Handler);

            using (var provider = services.BuildServiceProvider())
            using (var scope = provider.CreateScope())
            {
                var service = scope.ServiceProvider.GetRequiredService<GameAreaMergeService>();
                var plan = await service.AddAsync(new()
                {
                    TargetServerId = 101, SourceServerIds = new() { 102, 103 }, Reason = "超时配置演练",
                });
                await service.DryRunAsync(new() { Id = plan.Id });
                context.Login(12);
                await service.ApproveAsync(new() { Id = plan.Id, ReviewerOperatorId = 12, Reason = "复核超时演练" });

                var executionObserved = false;
                context.Handler.OnExecute = () =>
                {
                    executionObserved = true;
                    Assert.NotNull(executionClient);
                    Assert.True(executionClient.Timeout >= TimeSpan.FromSeconds(expectedSeconds));
                    Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), executionClient.Timeout);
                    Assert.False(sharedClientResolved);
                };
                var completed = await service.ExecuteAsync(new()
                {
                    Id = plan.Id, BackupConfirmed = true, BackupIdentifier = "timeout-rehearsal-backup",
                });

                Assert.True(executionObserved);
                Assert.Equal(GameAreaMergePlanStatus.Completed, completed.Status);
                Assert.False(sharedClientResolved);
            }
        }
    }
}
