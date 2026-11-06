using GameFrameX.Core.Hub;
using GameFrameX.Core.Service.OnlineUser.Dto;
using Microsoft.AspNetCore.SignalR;

namespace GameFrameX.Core.Service.OnlineUser;

/// <summary>
/// 系统在线用户服务
/// </summary>
[ApiDescriptionSettings(Order = 300)]
public class OnlineUserService : IAdminApiController
{
    private readonly SqlSugarRepository<OnlineUserEntity> _systemOnlineUerRepository;
    private readonly SystemConfigService _systemConfigService;
    private readonly IHubContext<OnlineUserHub, IOnlineUserHub> _onlineUserHubContext;

    public OnlineUserService(SqlSugarRepository<OnlineUserEntity> systemOnlineUerRepository,
        SystemConfigService systemConfigService,
        IHubContext<OnlineUserHub, IOnlineUserHub> onlineUserHubContext)
    {
        _systemOnlineUerRepository = systemOnlineUerRepository;
        _systemConfigService = systemConfigService;
        _onlineUserHubContext = onlineUserHubContext;
    }

    /// <summary>
    /// 获取在线用户分页列表
    /// </summary>
    /// <returns></returns>
    [NonAction]
    [DisplayName("获取在线用户分页列表")]
    public async Task<SqlSugarPagedList<OnlineUserResponse>> PageAsync(PageOnlineUserRequest input)
    {
        return await _systemOnlineUerRepository.AsQueryable()
                                        .WhereIF(!string.IsNullOrWhiteSpace(input.UserName), u => u.UserName.Contains(input.UserName ?? string.Empty))
                                        .WhereIF(!string.IsNullOrWhiteSpace(input.RealName), u => (u.RealName ?? string.Empty).Contains(input.RealName ?? string.Empty))
                                        .Select<OnlineUserResponse>()
                                        .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 强制下线
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    [NonAction]
    [NonValidation]
    [DisplayName("强制下线")]
    public async Task ForceOfflineAsync(OnlineUserEntity user)
    {
        if (string.IsNullOrEmpty(user.ConnectionId))
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        await _onlineUserHubContext.Clients.Client(user.ConnectionId).ForceOfflineAsync("强制下线");
        await _systemOnlineUerRepository.DeleteAsync(user);
    }

    /// <summary>
    /// 单用户登录
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task SingleLoginAsync(long userId)
    {
        if (await _systemConfigService.GetConfigValueAsync<bool>(CommonConst.SysSingleLogin))
        {
            var user = await _systemOnlineUerRepository.GetFirstAsync(u => u.UserId == userId);
            if (user == null)
            {
                return;
            }

            await ForceOfflineAsync(user);
        }
    }
}