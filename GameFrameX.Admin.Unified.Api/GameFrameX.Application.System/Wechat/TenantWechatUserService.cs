using GameFrameX.Wechat.Open.Dto;

namespace GameFrameX.Application.System.Wechat;

/// <summary>
/// 微信账号服务
/// </summary>
[ApiDescriptionSettings(Order = 220)]
public class TenantWechatUserService : IAdminApiController
{
    private readonly SqlSugarRepository<TenantWechatUserEntity> _systemWechatUserRepository;

    public TenantWechatUserService(SqlSugarRepository<TenantWechatUserEntity> systemWechatUserRepository)
    {
        _systemWechatUserRepository = systemWechatUserRepository;
    }

    /// <summary>
    /// 获取微信用户列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取微信用户列表")]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<TenantWechatUserResponse>> PageAsync(PageWechatUserRequest input)
    {
        var nickName = input.NickName?.Trim() ?? string.Empty;
        var phoneNumber = input.PhoneNumber?.Trim() ?? string.Empty;
        return await _systemWechatUserRepository.AsQueryable()
                                         .WhereIF(!string.IsNullOrWhiteSpace(nickName), u => (u.NickName ?? string.Empty).Contains(nickName))
                                         .WhereIF(!string.IsNullOrWhiteSpace(phoneNumber), u => (u.Mobile ?? string.Empty).Contains(phoneNumber))
                                         .OrderBy(u => u.Id, OrderByType.Desc)
                                         .Select<TenantWechatUserResponse>()
                                         .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加微信用户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add")]
    [HttpPost]
    [DisplayName("增加微信用户")]
    public async Task AddWechatUserAsync(AddWechatUserRequest input)
    {
        await _systemWechatUserRepository.InsertAsync(input.Adapt<TenantWechatUserEntity>());
    }

    /// <summary>
    /// 更新微信用户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新微信用户")]
    public async Task UpdateWechatUserAsync(UpdateWechatUserRequest input)
    {
        var weChatUser = input.Adapt<TenantWechatUserEntity>();
        await _systemWechatUserRepository.AsUpdateable(weChatUser).IgnoreColumns(true).ExecuteCommandWithOptLockAsync(true);
    }

    /// <summary>
    /// 删除微信用户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Delete")]
    [HttpPost]
    [DisplayName("删除微信用户")]
    public async Task DeleteWechatUserAsync(DeleteWechatUserRequest input)
    {
        await _systemWechatUserRepository.DeleteAsync(u => u.Id == input.Id);
    }
}