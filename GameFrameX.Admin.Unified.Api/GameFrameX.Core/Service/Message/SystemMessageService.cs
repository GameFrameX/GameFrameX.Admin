using GameFrameX.Core.Hub;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Message.Dto;
using Microsoft.AspNetCore.SignalR;

namespace GameFrameX.Core.Service.Message;

/// <summary>
/// 系统消息发送服务。
/// <para>本服务所有邮件发送均走 <see cref="ISystemEmailSender"/>，从主库 <c>SystemEmailProviderEntity</c> 读取配置；不再依赖启动期静态邮件 JSON / <c>IFluentEmail</c>。</para>
/// <para>缺平台系统邮件配置时 <see cref="SendEmailAsync"/> 返回 <c>false</c> 并写日志，禁止静默成功。</para>
/// </summary>
[ApiDescriptionSettings(Order = 370)]
public class SystemMessageService : IAdminApiController
{
    private readonly SystemCacheService _systemCacheService;
    private readonly ISystemEmailSender _systemEmailSender;
    private readonly IHubContext<OnlineUserHub, IOnlineUserHub> _chatHubContext;

    public SystemMessageService(SystemCacheService systemCacheService,
        ISystemEmailSender systemEmailSender,
        IHubContext<OnlineUserHub, IOnlineUserHub> chatHubContext)
    {
        _systemCacheService = systemCacheService;
        _systemEmailSender = systemEmailSender;
        _chatHubContext = chatHubContext;
    }

    /// <summary>
    /// 发送消息给所有人
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("发送消息给所有人")]
    [ApiDescriptionSettings(Name = "SendAllUser")]
    public async Task SendAllUserAsync(MessageRequest input)
    {
        await _chatHubContext.Clients.All.ReceiveMessageAsync(input);
    }

    /// <summary>
    /// 发送消息给除了发送人的其他人
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("发送消息给除了发送人的其他人")]
    [ApiDescriptionSettings(Name = "SendOtherUser")]
    public async Task SendOtherUserAsync(MessageRequest input)
    {
        var user = _systemCacheService.Get<TenantOnlineUserEntity>(CacheConst.KeyUserOnline + input.UserId);
        if (user != null && !string.IsNullOrEmpty(user.ConnectionId))
        {
            await _chatHubContext.Clients.AllExcept(user.ConnectionId).ReceiveMessageAsync(input);
        }
    }

    /// <summary>
    /// 发送消息给某个人
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("发送消息给某个人")]
    [ApiDescriptionSettings(Name = "SendUser")]
    public async Task SendUserAsync(MessageRequest input)
    {
        var user = _systemCacheService.Get<TenantOnlineUserEntity>(CacheConst.KeyUserOnline + input.UserId);
        if (user == null || string.IsNullOrEmpty(user.ConnectionId))
        {
            return;
        }

        await _chatHubContext.Clients.Client(user.ConnectionId).ReceiveMessageAsync(input);
    }

    /// <summary>
    /// 发送消息给某些人
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("发送消息给某些人")]
    [ApiDescriptionSettings(Name = "SendUsers")]
    public async Task SendUsersAsync(MessageRequest input)
    {
        if (input.UserIds == null || input.UserIds.Count == 0)
        {
            return;
        }

        var userlist = new List<string>();
        foreach (var userid in input.UserIds)
        {
            var user = _systemCacheService.Get<TenantOnlineUserEntity>(CacheConst.KeyUserOnline + userid);
            if (user != null && !string.IsNullOrEmpty(user.ConnectionId))
            {
                userlist.Add(user.ConnectionId);
            }
        }

        await _chatHubContext.Clients.Clients(userlist).ReceiveMessageAsync(input);
    }

    /// <summary>
    /// 发送平台系统邮件。
    /// <para>从主库 <c>SystemEmailProviderEntity</c> 读取配置（按 <c>SystemEmailPurpose.PlatformOps</c> 默认）；缺配置返回 <c>false</c>，禁止静默成功或 fallback 到租户玩家邮件。</para>
    /// <para>缺省收件人：使用当前命中的 <c>SystemEmailProviderEntity</c> 配置的 <c>DefaultToEmail</c>；若配置也未提供则发送失败。</para>
    /// </summary>
    /// <param name="message">邮件正文。</param>
    /// <param name="title">邮件主题。</param>
    /// <param name="isHtml">是否 HTML 邮件；当前 <c>ISystemEmailSender</c> 实现按纯文本发送，HTML 标志位为预留扩展点。</param>
    /// <returns>是否发送成功。</returns>
    [DisplayName("发送平台系统邮件")]
    [ApiDescriptionSettings(Name = "SendEmail")]
    public async Task<bool> SendEmailAsync([Required] string message, string title = "系统邮件", bool isHtml = false)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        return await _systemEmailSender.SendAsync(SystemEmailPurpose.PlatformOps, ResolveDefaultReceiver(), title, message);
    }

    /// <summary>
    /// 解析平台系统邮件缺省收件人；从 <see cref="ISystemEmailSender.LastSelectedDefaultToEmail"/> 读取最近一次选择的 Provider 的默认收件人。
    /// <para>首次调用时该字段可能为 null；调用方应保证至少一次成功选择，或显式传入收件人。</para>
    /// </summary>
    private string? ResolveDefaultReceiver()
    {
        return _systemEmailSender.LastSelectedDefaultToEmail;
    }
}