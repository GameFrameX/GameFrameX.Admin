using GameFrameX.Core.Bootstrap;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Foundation.Options;
using GameFrameX.Web.Api;

// GFX-873 / C381 信号③：进程级未处理异常兜底（后台线程致命异常 / 主线程崩溃打印阶段），须早于任何可能崩溃的构建代码
CrashExitWatchdog.EnableDomainUnhandledExceptionFallback();

var builder = WebApplication.CreateBuilder(args);

// Foundation 日志管道进程级开关（Loki 等）：环境变量 / 命令行同 Client/Hub 口径（如 IsGrafanaLoki、GrafanaLokiUrl）
var launcherOptions = OptionsBuilder.Create<LauncherOptions>(args);

WebComponent.Load(builder, launcherOptions);

var startup = new Startup();
startup.ConfigureServices(builder.Services);

var app = builder.Build();
startup.Configure(app, app.Environment);

// GFX-873 / C381 信号②：host 已停（StopHost / 停机）后进程 15s 未退出即强退——
// BackgroundService 启动失败时 Run 正常返回无异常，仅此信号覆盖该类僵死
var applicationLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
applicationLifetime.ApplicationStopped.Register(() => CrashExitWatchdog.Arm("GameFrameX.Web.Api host 已停止"));

// GFX-873 / C381 信号①：逃逸 Run 的未处理异常若在运行时传播路径僵死（linux/arm64 容器实测），
// watchdog 超时强退保证进程必死，容器 restart 策略才可触发；原样重抛保留默认崩溃语义
try
{
    app.Run();
}
catch (Exception exception)
{
    CrashExitWatchdog.Arm("GameFrameX.Web.Api host 未处理异常", exception);
    throw;
}
