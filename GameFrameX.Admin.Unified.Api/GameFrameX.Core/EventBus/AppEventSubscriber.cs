using GameFrameX.Core.Service.Message;

namespace GameFrameX.Core.EventBus;

/// <summary>
/// 事件订阅
/// </summary>
public class AppEventSubscriber : IEventSubscriber, IDisposable
{
    private readonly IServiceScope _serviceScope;

    public AppEventSubscriber(IServiceScopeFactory scopeFactory)
    {
        _serviceScope = scopeFactory.CreateScope();
    }

    /// <summary>
    /// 发送异常邮件
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    [EventSubscribe("Send:ErrorMail")]
    public async Task SendOrderErrorMailAsync(EventHandlerExecutingContext context)
    {
        //var mailTempPath = Path.Combine(webHostEnvironment.WebRootPath, "Temp\\ErrorMail.tp");
        //var mailTemp = File.ReadAllText(mailTempPath);
        //var mail = await _serviceScope.ServiceProvider.GetRequiredService<IViewEngine>().RunCompileFromCachedAsync(mailTemp, );

        var title = "GameFrameX 框架异常";
        await _serviceScope.ServiceProvider.GetRequiredService<SystemMessageService>().SendEmailAsync(Json.Serialize(context.Source.Payload), title, true);
    }

    /// <summary>
    /// 释放服务作用域
    /// </summary>
    public void Dispose()
    {
        _serviceScope.Dispose();
    }
}