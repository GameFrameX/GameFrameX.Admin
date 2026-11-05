using GameFrameX.Core.Service.Base;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Role.Dto;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;

namespace GameFrameX.Core.Service.Role;

/// <summary>
/// 系统角色服务
/// </summary>
[ApiDescriptionSettings(Order = 480)]
public class TenantRoleService : BaseSystemService
{
    private readonly SqlSugarRepository<TenantRoleEntity> _systemRoleRepository;
    private readonly TenantRoleMenuService _systemRoleMenuService;
    private readonly TenantUserRoleService _systemUserRoleService;
    private readonly IServiceProvider _serviceProvider;

    public TenantRoleService(
        CurrentUserContext currentUserContext,
        SqlSugarRepository<TenantRoleEntity> systemRoleRepository,
        TenantRoleMenuService systemRoleMenuService,
        TenantUserRoleService systemUserRoleService,
        IServiceProvider serviceProvider)
        : base(currentUserContext)
    {
        _systemRoleRepository = systemRoleRepository;
        _systemRoleMenuService = systemRoleMenuService;
        _systemUserRoleService = systemUserRoleService;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 获取角色分页列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取角色分页列表")]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<TenantRoleResponse>> PageAsync(PageRoleRequest input)
    {
        return await _systemRoleRepository.AsQueryable()
                                   .WhereIF(!CurrentUserContext.SuperAdmin, u => u.CreatedId == CurrentUserContext.UserId) // 若非超管，则只能操作自己创建的角色
                                   .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name))
                                   .WhereIF(!string.IsNullOrWhiteSpace(input.Code), u => (u.Code ?? string.Empty).Contains(input.Code ?? string.Empty))
                                   .OrderBy(u => u.OrderNo)
                                   .Select<TenantRoleResponse>()
                                   .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取角色列表
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取角色列表")]
    [ApiDescriptionSettings(Name = "GetList")]
    public async Task<List<RoleResponse>> GetListAsync()
    {
        // 当前用户已拥有的角色集合
        var roleIdList = CurrentUserContext.SuperAdmin ? null : await _systemUserRoleService.GetUserRoleIdListAsync(CurrentUserContext.UserId);

        return await _systemRoleRepository.AsQueryable()
                                   .WhereIF(roleIdList != null, u => u.CreatedId == CurrentUserContext.UserId || roleIdList!.Contains(u.Id)) // 若非超管，则只显示自己创建和已拥有的角色
                                   .OrderBy(u => u.OrderNo).Select<RoleResponse>().ToListAsync();
    }

    /// <summary>
    /// 增加角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add")]
    [HttpPost]
    [DisplayName("增加角色")]
    public async Task AddRoleAsync(AddRoleRequest input)
    {
        if (await _systemRoleRepository.IsAnyAsync(u => u.Name == input.Name && u.Code == input.Code))
        {
            throw BusinessError.Oh(CommonErrorCode.RecordAlreadyExists);
        }

        var newRole = await _systemRoleRepository.AsInsertable(input.Adapt<TenantRoleEntity>()).ExecuteReturnEntityAsync();
        input.Id = newRole.Id;
        await UpdateRoleMenuAsync(input);
    }

    /// <summary>
    /// 更新角色菜单权限
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private async Task UpdateRoleMenuAsync(AddRoleRequest input)
    {
        if (input.MenuIdList == null || input.MenuIdList.Count < 1)
        {
            return;
        }

        // 将父节点为0的菜单排除，防止前端全选异常
        var pMenuIds = await _serviceProvider.GetRequiredService<SqlSugarRepository<GlobalMenuEntity>>().AsQueryable().Where(u => input.MenuIdList.Contains(u.Id) && u.Pid == 0).ToListAsync(u => u.Id);
        var menuIds = input.MenuIdList.Except(pMenuIds); // 差集
        await GrantMenuAsync(new RoleMenuRequest
        {
            Id = input.Id,
            MenuIdList = menuIds.ToList(),
        });
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新角色")]
    public async Task UpdateRoleAsync(UpdateRoleRequest input)
    {
        if (await _systemRoleRepository.IsAnyAsync(u => u.Name == input.Name && u.Code == input.Code && u.Id != input.Id))
        {
            throw BusinessError.Oh(CommonErrorCode.RecordAlreadyExists);
        }

        await _systemRoleRepository.AsUpdateable(input.Adapt<TenantRoleEntity>()).IgnoreColumns(true)
                            .IgnoreColumns(u => new { u.DataScope, }).ExecuteCommandWithOptLockAsync(true);

        await UpdateRoleMenuAsync(input);
        var cache = _serviceProvider.GetRequiredService<SystemCacheService>();
        cache.RemoveByPrefixKey(CacheConst.KeyUserMenu);
        cache.RemoveByPrefixKey(CacheConst.KeyUserButton);
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [SqlSugarTransaction]
    [ApiDescriptionSettings(Name = "Delete")]
    [HttpPost]
    [DisplayName("删除角色")]
    public async Task DeleteRoleAsync(DeleteRoleRequest input)
    {
        var systemRole = await _systemRoleRepository.GetFirstAsync(u => u.Id == input.Id) ?? throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        if (systemRole.Code == CommonConst.SysAdminRole)
        {
            throw BusinessError.Oh(RoleMenuErrorCode.DeleteSystemAdminRoleForbidden);
        }

        await _systemRoleRepository.DeleteAsync(systemRole);


        // 级联删除用户角色数据
        await _systemUserRoleService.DeleteUserRoleByRoleIdAsync(systemRole.Id);

        // 级联删除角色菜单数据
        await _systemRoleMenuService.DeleteRoleMenuByRoleIdAsync(systemRole.Id);
    }

    /// <summary>
    /// 授权角色菜单
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("授权角色菜单")]
    [ApiDescriptionSettings(Name = "GrantMenu")]
    public async Task GrantMenuAsync(RoleMenuRequest input)
    {
        await _systemRoleMenuService.GrantRoleMenuAsync(input);
    }

    /// <summary>
    /// 根据角色Id获取菜单Id集合
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("根据角色Id获取菜单Id集合")]
    [ApiDescriptionSettings(Name = "GetOwnMenuList")]
    public async Task<List<long>> GetOwnMenuListAsync([FromQuery] RoleRequest input)
    {
        return await _systemRoleMenuService.GetRoleMenuIdListAsync(new List<long> { input.Id, });
    }

    /// <summary>
    /// 设置角色状态
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("设置角色状态")]
    [ApiDescriptionSettings(Name = "SetStatus")]
    public async Task<int> SetStatusAsync(RoleRequest input)
    {
        // 状态切换必须能找到禁用角色，同时仍排除已删除记录。
        var role = await _systemRoleRepository.AsQueryable().Filter(null, true)
                                             .Where(u => u.IsDeleted == false)
                                             .FirstAsync(u => u.Id == input.Id);
        if (role == null)
        {
            return 0;
        }
        role.IsEnabled = input.IsEnabled;
        var affected = await _systemRoleRepository.AsUpdateable(role)
                                                 .UpdateColumns(u => u.IsEnabled)
                                                 .ExecuteCommandAsync();
        var cache = _serviceProvider.GetRequiredService<SystemCacheService>();
        cache.RemoveByPrefixKey(CacheConst.KeyUserMenu);
        cache.RemoveByPrefixKey(CacheConst.KeyUserButton);
        return affected;
    }
}