namespace GameFrameX.Core.Hub;

public interface IOnlineUserHub
{
    /// <summary>
    /// 强制下线
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task ForceOfflineAsync(object context);

    /// <summary>
    /// 发布站内消息
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task PublicNoticeAsync(TenantNoticeEntity context);

    /// <summary>
    /// 接收消息
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task ReceiveMessageAsync(object context);
}