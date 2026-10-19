using GameFrameX.Application.System.Tenant.Notice.Dto;
using GameFrameX.Core.Authorization;
using GameFrameX.Core.Service.OnlineUser;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Enum;
using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Application.System.Tenant.Notice;

/// <summary>
/// 系统通知公告服务
/// </summary>
[ApiDescriptionSettings(Order = 380)]
public class TenantNoticeService : IAdminApiController
{
    private readonly CurrentUserContext _currentUserContext;
    private readonly IServiceProvider _serviceProvider;

    public TenantNoticeService(
        CurrentUserContext currentUserContext,
        IServiceProvider serviceProvider)
    {
        _currentUserContext = currentUserContext;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 获取通知公告分页列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取通知公告分页列表")]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<TenantNoticeResponse>> PageAsync(PageNoticeRequest input)
    {
        if (!HasTenantScope())
        {
            return EmptyPage<TenantNoticeResponse>(input);
        }

        var systemNoticeRepository = GetTenantRepository<TenantNoticeEntity>();
        return await systemNoticeRepository.AsQueryable()
                                    .WhereIF(!string.IsNullOrWhiteSpace(input.Title), u => u.Title.Contains(input.Title.Trim()))
                                    .WhereIF(input.Type > 0, u => u.Type == input.Type)
                                    .WhereIF(!_currentUserContext.SuperAdmin, u => u.CreatedId == _currentUserContext.UserId)
                                    .OrderBy(u => u.CreatedTime, OrderByType.Desc)
                                    .Select<TenantNoticeResponse>()
                                    .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加通知公告
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add")]
    [HttpPost]
    [DisplayName("增加通知公告")]
    public async Task AddNoticeAsync(AddNoticeRequest input)
    {
        EnsureTenantScope();
        var systemNoticeRepository = GetTenantRepository<TenantNoticeEntity>();
        var notice = input.Adapt<TenantNoticeEntity>();
        InitNoticeInfo(notice);
        await systemNoticeRepository.InsertAsync(notice);
    }

    /// <summary>
    /// 更新通知公告
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [SqlSugarTransaction]
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新通知公告")]
    public async Task UpdateNoticeAsync(UpdateNoticeRequest input)
    {
        EnsureTenantScope();
        var systemNoticeRepository = GetTenantRepository<TenantNoticeEntity>();
        var notice = input.Adapt<TenantNoticeEntity>();
        InitNoticeInfo(notice);
        await systemNoticeRepository.Context.UpdateWithOptimisticLockAsync(notice);
    }

    /// <summary>
    /// 删除通知公告
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [SqlSugarTransaction]
    [ApiDescriptionSettings(Name = "Delete")]
    [HttpPost]
    [DisplayName("删除通知公告")]
    public async Task DeleteNoticeAsync(DeleteNoticeRequest input)
    {
        EnsureTenantScope();
        var systemNoticeRepository = GetTenantRepository<TenantNoticeEntity>();
        var systemNoticeUserRepository = GetTenantRepository<TenantNoticeUserEntity>();

        await systemNoticeRepository.DeleteAsync(u => u.Id == input.Id);

        await systemNoticeUserRepository.DeleteAsync(u => u.NoticeId == input.Id);
    }

    /// <summary>
    /// 发布通知公告
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("发布通知公告")]
    [ApiDescriptionSettings(Name = "Publish")]
    public async Task PublishAsync(NoticeRequest input)
    {
        EnsureTenantScope();
        var systemNoticeRepository = GetTenantRepository<TenantNoticeEntity>();
        var systemNoticeUserRepository = GetTenantRepository<TenantNoticeUserEntity>();
        var systemUserRepository = GetTenantRepository<TenantUserEntity>();
        var systemOnlineUserService = _serviceProvider.GetRequiredService<TenantOnlineUserService>();

        // 更新发布状态和时间
        await systemNoticeRepository.UpdateAsync(u => new TenantNoticeEntity { Status = NoticeStatusEnum.PUBLIC, PublicTime = TimerHelper.UnixTimeSeconds(), }, u => u.Id == input.Id);

        var notice = await systemNoticeRepository.GetFirstAsync(u => u.Id == input.Id);
        if (notice == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        // 通知到的人(所有账号)
        var userIdList = await systemUserRepository.AsQueryable().Select(u => u.Id).ToListAsync();

        await systemNoticeUserRepository.DeleteAsync(u => u.NoticeId == notice.Id);
        var noticeUserList = userIdList.Select(u => new TenantNoticeUserEntity
        {
            NoticeId = notice.Id,
            UserId = u,
        }).ToList();
        await systemNoticeUserRepository.InsertRangeAsync(noticeUserList);

        // 广播所有在线账号
        await systemOnlineUserService.PublicNoticeAsync(notice, userIdList);
    }

    /// <summary>
    /// 设置通知公告已读状态
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("设置通知公告已读状态")]
    [ApiDescriptionSettings(Name = "SetRead")]
    public async Task SetReadAsync(NoticeRequest input)
    {
        EnsureTenantScope();
        var systemNoticeUserRepository = GetTenantRepository<TenantNoticeUserEntity>();

        await systemNoticeUserRepository.UpdateAsync(u => new TenantNoticeUserEntity
        {
            ReadStatus = NoticeUserStatusEnum.READ,
            ReadTime = TimerHelper.UnixTimeSeconds(),
        }, u => u.NoticeId == input.Id && u.UserId == _currentUserContext.UserId);
    }

    /// <summary>
    /// 获取接收的通知公告
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [AuthenticatedOnly]
    [DisplayName("获取接收的通知公告")]
    [ApiDescriptionSettings(Name = "GetPageReceived")]
    public async Task<SqlSugarPagedList<TenantNoticeUserResponse>> GetPageReceivedAsync([FromQuery] PageNoticeRequest input)
    {
        if (!HasTenantScope())
        {
            return EmptyPage<TenantNoticeUserResponse>(input);
        }

        var systemNoticeRepository = GetTenantRepository<TenantNoticeEntity>();
        var pageResult = await systemNoticeRepository.AsSugarClient().Queryable<TenantNoticeUserEntity>().Includes(u => u.TenantNoticeEntity)
                                              .Where(u => u.UserId == _currentUserContext.UserId)
                                              .WhereIF(!string.IsNullOrWhiteSpace(input.Title), u => u.TenantNoticeEntity.Title.Contains(input.Title.Trim()))
                                              .WhereIF(input.Type is > 0, u => u.TenantNoticeEntity.Type == input.Type)
                                              .OrderBy(u => u.TenantNoticeEntity.CreatedTime, OrderByType.Desc)
                                              .ToPagedListAsync(input.Page, input.PageSize);
        return pageResult.Adapt<SqlSugarPagedList<TenantNoticeUserResponse>>();
    }

    /// <summary>
    /// 获取未读的通知公告
    /// </summary>
    /// <returns></returns>
    [AuthenticatedOnly]
    [DisplayName("获取未读的通知公告")]
    [ApiDescriptionSettings(Name = "GetUnReadList")]
    public async Task<List<TenantNoticeResponse>> GetUnReadListAsync()
    {
        if (!HasTenantScope())
        {
            return new List<TenantNoticeResponse>();
        }

        var systemNoticeRepository = GetTenantRepository<TenantNoticeEntity>();
        var noticeUserList = await systemNoticeRepository.AsSugarClient().Queryable<TenantNoticeUserEntity>().Includes(u => u.TenantNoticeEntity)
                                                  .Where(u => u.UserId == _currentUserContext.UserId && u.ReadStatus == NoticeUserStatusEnum.UNREAD)
                                                  .OrderBy(u => u.TenantNoticeEntity.CreatedTime, OrderByType.Desc).ToListAsync();
        return noticeUserList.Select(t => t.TenantNoticeEntity.Adapt<TenantNoticeResponse>()).ToList();
    }

    /// <summary>
    /// 初始化通知公告信息
    /// </summary>
    /// <param name="notice"></param>
    [NonAction]
    private void InitNoticeInfo(TenantNoticeEntity notice)
    {
        // 新建即启用：列表装配 ISafeEnabledFilter（is_enabled IS NULL OR true），
        // 缺省 false 会让创建人立即看不到自己新建的通知（Api#284）
        notice.IsEnabled = true;
        notice.PublicUserId = _currentUserContext.UserId;
        notice.PublicUserName = _currentUserContext.RealName;
        notice.PublicOrganizationId = _currentUserContext.OrganizationId;
    }

    private static bool HasTenantScope()
    {
        return TenantOperationContext.GetEffectiveTenantId().HasValue;
    }

    private static void EnsureTenantScope()
    {
        if (!HasTenantScope())
        {
            throw BusinessError.Oh(SystemUserErrorCode.NoPermission);
        }
    }

    private SqlSugarRepository<TEntity> GetTenantRepository<TEntity>() where TEntity : EntityBase, new()
    {
        return _serviceProvider.GetRequiredService<SqlSugarRepository<TEntity>>();
    }

    private static SqlSugarPagedList<TEntity> EmptyPage<TEntity>(PageNoticeRequest input)
    {
        return Enumerable.Empty<TEntity>().ToPagedList(input.Page, input.PageSize);
    }
}