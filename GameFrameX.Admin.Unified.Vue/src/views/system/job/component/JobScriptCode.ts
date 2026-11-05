export const jobScriptCodeTemplate = `#region using

using GameFrameX.Core.Job;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#endregion

/// <summary>
/// 动态作业任务
/// </summary>
[JobDetail("你的作业编号")]
public class DynamicJob : IJob
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DynamicJob(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        using (var serviceScope = _scopeFactory.CreateScope())
        {
            // 获取用户仓储
            // var rep = serviceScope.ServiceProvider.GetRequiredService<SqlSugarRepository<SystemUser>>();

            // 请求网址
            // using (var httpClient = new HttpClient())
            // {
            //     var result = await httpClient.GetStringAsync("http://www.baidu.com");
            //     Console.WriteLine(result);
            // }

            // 日志
            // var logger = serviceScope.ServiceProvider.GetRequiredService<ILogger<DynamicJob>>();
            // logger.LogInformation("日志消息");
        }
    }
}`;
