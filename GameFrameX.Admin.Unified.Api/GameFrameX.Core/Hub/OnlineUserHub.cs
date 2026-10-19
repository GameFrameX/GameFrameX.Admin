using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Message;
using GameFrameX.Core.Service.Message.Dto;
using GameFrameX.Core.Service.Tenant;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace GameFrameX.Core.Hub;

/// <summary>
/// 在线用户集线器
/// </summary>
public class OnlineUserHub : Hub<IOnlineUserHub>
{
    /// <summary>
    /// 在线用户 SignalR Hub 的稳定外部路径；前端连接和代理配置依赖该路径，不允许随内部实现迁移改变。
    /// </summary>
    public const string HubPath = "/hubs/onlineUser";

    private static readonly TimeSpan DisconnectGracePeriod = TimeSpan.FromSeconds(8);

    private readonly SystemMessageService _systemMessageService;
    private readonly SystemCacheService _systemCacheService;

    public OnlineUserHub(SystemMessageService systemMessageService,
        SystemCacheService systemCacheService)
    {
        _systemMessageService = systemMessageService;
        _systemCacheService = systemCacheService;
    }

    /// <summary>
    /// 连接
    /// </summary>
    /// <returns></returns>
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext == null)
        {
            return;
        }

        var token = httpContext.Request.Query["access_token"];
        var claims = ProjectJwtReader.ReadJwtToken(token)?.Claims;
        if (claims == null)
        {
            return;
        }

        var client = Parser.GetDefault().Parse(httpContext.Request.Headers["User-Agent"]);

        var userId = claims.FirstOrDefault(u => u.Type == ClaimConst.UserId)?.Value;
        var tenantId = ResolveTenantId(claims);
        if (!tenantId.HasValue)
        {
            return;
        }

        var user = new TenantOnlineUserEntity
        {
            ConnectionId = Context.ConnectionId,
            UserId = string.IsNullOrWhiteSpace(userId) ? 0 : long.Parse(userId),
            UserName = claims.FirstOrDefault(u => u.Type == ClaimConst.Account)?.Value ?? string.Empty,
            RealName = claims.FirstOrDefault(u => u.Type == ClaimConst.RealName)?.Value,
            Time = TimerHelper.UnixTimeSeconds(),
            Ip = httpContext.GetRemoteIpAddressToIPv4(),
            Browser = client.UA.Family + client.UA.Major,
            Os = client.OS.Family + client.OS.Major,
            TenantId = tenantId.Value,
        };

        using var tenantScope = TenantOperationContext.UseTenant(tenantId.Value);
        var systemOnlineUserRepository = httpContext.RequestServices.GetRequiredService<SqlSugarRepository<TenantOnlineUserEntity>>();
        await systemOnlineUserRepository.InsertAsync(user);
        _systemCacheService.Set(CacheConst.KeyUserOnline + user.UserId, user);
    }

    /// <summary>
    /// 断开
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (string.IsNullOrEmpty(Context.ConnectionId))
        {
            return;
        }

        var httpContext = Context.GetHttpContext();
        var token = httpContext?.Request.Query["access_token"];
        var claims = ProjectJwtReader.ReadJwtToken(token)?.Claims;
        var tenantId = ResolveTenantId(claims);
        if (!tenantId.HasValue)
        {
            return;
        }

        using var tenantScope = TenantOperationContext.UseTenant(tenantId.Value);
        var systemOnlineUserRepository = httpContext!.RequestServices.GetRequiredService<SqlSugarRepository<TenantOnlineUserEntity>>();
        var user = await systemOnlineUserRepository.AsQueryable().Filter("", true).FirstAsync(u => u.ConnectionId == Context.ConnectionId);
        if (user == null)
        {
            return;
        }

        await Task.Delay(DisconnectGracePeriod);

        await systemOnlineUserRepository.DeleteAsync(u => u.Id == user.Id);

        var hasOtherConnection = await systemOnlineUserRepository.AsQueryable().Filter("", true)
                                                          .AnyAsync(u => u.TenantId == user.TenantId && u.UserId == user.UserId);
        if (hasOtherConnection)
        {
            return;
        }

        _systemCacheService.Remove(CacheConst.KeyUserOnline + user.UserId);
    }

    /// <summary>
    /// 发送信息给某个人
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task ClientsSendMessageAsync(MessageRequest message)
    {
        await _systemMessageService.SendUserAsync(message);
    }

    /// <summary>
    /// 发送信息给所有人
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task ClientsSendMessagetoAllAsync(MessageRequest message)
    {
        await _systemMessageService.SendAllUserAsync(message);
    }

    /// <summary>
    /// 发送消息给某些人（除了本人）
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task ClientsSendMessagetoOtherAsync(MessageRequest message)
    {
        await _systemMessageService.SendOtherUserAsync(message);
    }

    /// <summary>
    /// 发送消息给某些人
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public async Task ClientsSendMessagetoUsersAsync(MessageRequest message)
    {
        await _systemMessageService.SendUsersAsync(message);
    }

    private static long? ResolveTenantId(IEnumerable<Claim>? claims)
    {
        var tenantIdText = claims?.FirstOrDefault(u => u.Type == ClaimConst.TenantId)?.Value;
        return long.TryParse(tenantIdText, out var tenantId) && tenantId > 0 ? tenantId : null;
    }
}