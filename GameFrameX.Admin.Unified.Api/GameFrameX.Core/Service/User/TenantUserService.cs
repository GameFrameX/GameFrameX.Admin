using GameFrameX.Core.Service.Base;
using GameFrameX.Core.Service.Organization;
using GameFrameX.Core.Service.TenantPackage;
using GameFrameX.Core.Service.User.Dto;
using GameFrameX.Entity.Enum;
using GameFrameX.Foundation.Http.Normalization;

namespace GameFrameX.Core.Service.User;

/// <summary>
/// 租户用户服务
/// </summary>
[ApiDescriptionSettings(Order = 490)]
public class TenantUserService : BaseSystemService
{
    private readonly SqlSugarRepository<TenantUserEntity> _tenantUserRepository;
    private readonly TenantOrganizationService _tenantOrganizationService;
    private readonly TenantUserExtensionOrganizationService _tenantUserExtOrganizationService;
    private readonly TenantUserRoleService _tenantUserRoleService;
    private readonly SystemConfigService _systemConfigService;

    public TenantUserService(
        CurrentUserContext currentUserContext,
        TenantQuotaHelper tenantQuotaHelper,
        SqlSugarRepository<TenantUserEntity> tenantUserRepository,
        TenantOrganizationService tenantOrganizationService,
        TenantUserExtensionOrganizationService tenantUserExtOrganizationService,
        TenantUserRoleService tenantUserRoleService,
        SystemConfigService systemConfigService)
        : base(currentUserContext, tenantQuotaHelper)
    {
        _tenantUserRepository = tenantUserRepository;
        _tenantOrganizationService = tenantOrganizationService;
        _tenantUserExtOrganizationService = tenantUserExtOrganizationService;
        _tenantUserRoleService = tenantUserRoleService;
        _systemConfigService = systemConfigService;
    }

    internal static void NormalizeNewUser(TenantUserEntity user)
    {
        foreach (var property in typeof(TenantUserEntity).GetProperties().Where(u => u.PropertyType == typeof(string) && u.CanWrite))
        {
            if (property.GetValue(user) == null)
            {
                property.SetValue(user, string.Empty);
            }
        }
    }

    /// <summary>
    /// 获取用户分页列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取用户分页列表")]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<TenantUserResponse>> PageAsync(PageUserRequest input)
    {
        // 获取用户拥有的机构集合
        var userOrganizationIdList = await _tenantOrganizationService.GetUserOrganizationIdListAsync();
        List<long>? orgList = null;
        if (input.OrganizationId > 0) // 指定机构查询时
        {
            orgList = await _tenantOrganizationService.GetChildIdListWithSelfByIdAsync(input.OrganizationId);
            orgList = CurrentUserContext.SuperAdmin ? orgList : orgList.Where(u => userOrganizationIdList.Contains(u)).ToList();
        }
        else // 各管理员只能看到自己机构下的用户列表
        {
            orgList = CurrentUserContext.SuperAdmin ? null : userOrganizationIdList;
        }

        return await _tenantUserRepository.AsQueryable()
                                   .Where(u => u.AccountType != AccountTypeEnum.SuperAdmin)
                                   .WhereIF(orgList != null, u => orgList!.Contains(u.OrganizationId))
                                   .WhereIF(!string.IsNullOrWhiteSpace(input.Account), u => u.Account.Contains(input.Account))
                                   .WhereIF(!string.IsNullOrWhiteSpace(input.RealName), u => u.RealName.Contains(input.RealName))
                                   .WhereIF(!string.IsNullOrWhiteSpace(input.Phone), u => u.Phone.Contains(input.Phone))
                                   .OrderBy(u => u.OrderNo)
                                   .Select<TenantUserResponse>()
                                   .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 增加用户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [SqlSugarTransaction]
    [ApiDescriptionSettings(Name = "Add")]
    [HttpPost]
    [DisplayName("增加用户")]
    public async Task<long> AddUserAsync(AddUserRequest input)
    {
        var isExist = await _tenantUserRepository.AsQueryable().Filter(null, true).AnyAsync(u => u.Account == input.Account);
        if (isExist)
        {
            throw BusinessError.Oh(SystemUserErrorCode.AccountAlreadyExists);
        }

        // 用户数量限额检查
        if (!CurrentUserContext.SuperAdmin)
        {
            var package = await TenantQuotaHelper.GetPackageAsync(CurrentUserContext.TenantId);
            if (package != null)
            {
                var currentUserCount = await _tenantUserRepository.AsQueryable().CountAsync();
                if (currentUserCount >= package.MaxUserCount)
                {
                    throw BusinessError.Oh($"用户数已达上限（{package.MaxUserCount}），无法新增");
                }
            }
        }

        var password = await _systemConfigService.GetConfigValueAsync<string>(CommonConst.SysPassword);

        var user = input.Adapt<TenantUserEntity>();
        NormalizeNewUser(user);
        user.Password = CryptogramUtility.HashPassword(password);
        var newUser = await _tenantUserRepository.AsInsertable(user).ExecuteReturnEntityAsync();
        input.Id = newUser.Id;
        await UpdateRoleAndExtOrganizationAsync(input);

        return newUser.Id;
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [SqlSugarTransaction]
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新用户")]
    public async Task UpdateUserAsync(UpdateUserRequest input)
    {
        if (await _tenantUserRepository.AsQueryable().Filter(null, true).AnyAsync(u => u.Account == input.Account && u.Id != input.Id))
        {
            throw BusinessError.Oh(SystemUserErrorCode.AccountAlreadyExists);
        }

        await _tenantUserRepository.AsUpdateable(input.Adapt<TenantUserEntity>()).IgnoreColumns(true)
                            .IgnoreColumns(u => new { u.Password, u.IsEnabled, }).ExecuteCommandAsync();

        await UpdateRoleAndExtOrganizationAsync(input);

        // 删除用户机构缓存
        SqlSugarFilter.DeleteUserOrganizationCache(input.Id, _tenantUserRepository.Context.CurrentConnectionConfig.ConfigId?.ToString() ?? string.Empty);
    }

    /// <summary>
    /// 更新角色和扩展机构
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private async Task UpdateRoleAndExtOrganizationAsync(AddUserRequest input)
    {
        await GrantRoleAsync(new UserRoleRequest { UserId = input.Id, RoleIdList = input.RoleIdList, });

        await _tenantUserExtOrganizationService.UpdateUserExtOrganizationAsync(input.Id, input.ExtOrganizationIdList);
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [SqlSugarTransaction]
    [ApiDescriptionSettings(Name = "Delete")]
    [HttpPost]
    [DisplayName("删除用户")]
    public async Task DeleteUserAsync(DeleteUserRequest input)
    {
        var user = await _tenantUserRepository.GetFirstAsync(u => u.Id == input.Id) ?? throw BusinessError.Oh(SystemUserErrorCode.AccountNotFound);
        if (user.AccountType == AccountTypeEnum.SuperAdmin)
        {
            throw BusinessError.Oh(SystemUserErrorCode.DeleteSuperAdminForbidden);
        }

        if (user.Id == CurrentUserContext.UserId)
        {
            throw BusinessError.Oh(SystemUserErrorCode.DeleteSelfForbidden);
        }

        await _tenantUserRepository.DeleteAsync(user);

        // 删除用户角色
        await _tenantUserRoleService.DeleteUserRoleByUserIdAsync(input.Id);

        // 删除用户扩展机构
        await _tenantUserExtOrganizationService.DeleteUserExtOrganizationByUserIdAsync(input.Id);
    }

    /// <summary>
    /// 查看用户基本信息
    /// </summary>
    /// <returns></returns>
    [DisplayName("查看用户基本信息")]
    [ApiDescriptionSettings(Name = "GetBaseInfo")]
    public async Task<TenantUserResponse> GetBaseInfoAsync()
    {
        var user = await _tenantUserRepository.GetFirstAsync(u => u.Id == CurrentUserContext.UserId);
#pragma warning disable CS8603 // 查询未命中时返回 null 是既有 API 契约（前端按 null 判断 not-found）
        return user?.Adapt<TenantUserResponse>();
#pragma warning restore CS8603
    }

    /// <summary>
    /// 更新用户基本信息
    /// </summary>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "BaseInfo")]
    [HttpPost]
    [DisplayName("更新用户基本信息")]
    public async Task<int> UpdateBaseInfoAsync(TenantUserEntity user)
    {
        return await _tenantUserRepository.AsUpdateable(user)
                                   .IgnoreColumns(u => new { u.CreatedTime, u.Account, u.Password, u.AccountType, u.OrganizationId, u.PosId, }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 设置用户状态
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("设置用户状态")]
    [ApiDescriptionSettings(Name = "SetStatus")]
    public async Task<int> SetStatusAsync(UserRequest input)
    {
        var user = await _tenantUserRepository.GetFirstAsync(u => u.Id == input.Id) ?? throw BusinessError.Oh(SystemUserErrorCode.AccountNotFound);
        if (user.AccountType == AccountTypeEnum.SuperAdmin)
        {
            throw BusinessError.Oh(SystemUserErrorCode.ModifySuperAdminStatusForbidden);
        }

        user.IsEnabled = input.IsEnabled;
        return await _tenantUserRepository.AsUpdateable(user).UpdateColumns(u => new { u.IsEnabled, }).ExecuteCommandAsync();
    }

    /// <summary>
    /// 授权用户角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("授权用户角色")]
    [ApiDescriptionSettings(Name = "GrantRole")]
    public async Task GrantRoleAsync(UserRoleRequest input)
    {
        //var user = await _tenantUserRepository.GetFirstAsync(u => u.Id == input.UserId) ?? throw BusinessError.Oh(SystemUserErrorCode.AccountNotFound);
        //if (user.AccountType == AccountTypeEnum.SuperAdmin)
        //    throw BusinessError.Oh(SystemUserErrorCode.AssignRoleToSuperAdminForbidden);

        await _tenantUserRoleService.GrantUserRoleAsync(input);
    }

    /// <summary>
    /// 修改用户密码
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("修改用户密码")]
    [ApiDescriptionSettings(Name = "ChangePwd")]
    public async Task<int> ChangePwdAsync(ChangePwdRequest input)
    {
        var user = await _tenantUserRepository.GetFirstAsync(u => u.Id == CurrentUserContext.UserId) ?? throw BusinessError.Oh(SystemUserErrorCode.AccountNotFound);
        if (!CryptogramUtility.VerifyPassword(input.PasswordOld, user.Password))
        {
            throw BusinessError.Oh(SystemUserErrorCode.OldPasswordMismatch);
        }

        // 验证密码强度
        if (CryptogramUtility.StrongPassword)
        {
            user.Password = Regex.IsMatch(input.PasswordNew, CryptogramUtility.PasswordStrengthValidation)
                                ? CryptogramUtility.HashPassword(input.PasswordNew)
                                : throw BusinessError.Oh(CryptogramUtility.PasswordStrengthValidationMsg);
        }
        else
        {
            user.Password = CryptogramUtility.HashPassword(input.PasswordNew);
        }

        return await _tenantUserRepository.AsUpdateable(user).UpdateColumns(u => u.Password).ExecuteCommandAsync();
    }

    /// <summary>
    /// 重置用户密码
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("重置用户密码")]
    [ApiDescriptionSettings(Name = "ResetPwd")]
    public async Task<HttpJsonResultData<ResetPwdResponse>> ResetPwdAsync(ResetPwdUserRequest input)
    {
        var user = await _tenantUserRepository.GetFirstAsync(u => u.Id == input.Id) ?? throw BusinessError.Oh(SystemUserErrorCode.AccountNotFound);
        var password = await _systemConfigService.GetConfigValueAsync<string>(CommonConst.SysPassword);
        user.Password = CryptogramUtility.HashPassword(password);
        await _tenantUserRepository.AsUpdateable(user).UpdateColumns(u => u.Password).ExecuteCommandAsync();
        return HttpJsonResultData<ResetPwdResponse>.Success(new ResetPwdResponse
        {
            NewPassword = password,
        });
    }

    /// <summary>
    /// 获取用户拥有角色集合
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [DisplayName("获取用户拥有角色集合")]
    [ApiDescriptionSettings(Name = "GetOwnRoleList")]
    public async Task<List<long>> GetOwnRoleListAsync(long userId)
    {
        return await _tenantUserRoleService.GetUserRoleIdListAsync(userId);
    }

    /// <summary>
    /// 获取用户扩展机构集合
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [DisplayName("获取用户扩展机构集合")]
    // Org 为路由锁定字符串（前端 /api/tenantUser/ownExtOrgList 实际调用），GFX-710 禁简写豁免
    [ApiDescriptionSettings(Name = "GetOwnExtOrgList")]
    public async Task<List<TenantUserExtensionOrganizationResponse>> GetOwnExtOrganizationListAsync(long userId)
    {
        return await _tenantUserExtOrganizationService.GetUserExtOrganizationListAsync(userId);
    }
}