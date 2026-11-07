using System.Security.Claims;
using GameFrameX.Core.Cache;
using GameFrameX.Core.Const;
using GameFrameX.Core.Option;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Menu;
using GameFrameX.Core.Service.Role;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Core.Tests.System;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GameFrameX.Core.Tests.Authorization;

/// <summary>
/// 登录菜单树口径：超管可见全部启用菜单（含游戏业务运营域），不含按钮与软删行；
/// 普通管理员仍只见角色授权且非超管专属子树的菜单。
/// </summary>
public sealed class SuperAdminMenuTreeVisibilityTests : IDisposable
{
    private const long BusinessMenuId = 201;
    private const long ProtectedMenuId = 202;
    private const long ButtonId = 203;
    private const long DeletedMenuId = 204;

    private readonly ClientApiTestDb _database = new();
    private readonly SystemCacheService _cache = new(new MemoryCacheService(), Options.Create(new CacheOptions()));
    private readonly HttpContextAccessor _accessor = new();
    private readonly GlobalMenuService _menus;
    private readonly ServiceProvider _services;

    public SuperAdminMenuTreeVisibilityTests()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();
        _database.InitBusinessTables(typeof(GlobalMenuEntity), typeof(RoleEntity), typeof(RoleMenuEntity), typeof(UserRoleEntity));
        _database.MainDb.Insertable(new[]
        {
            new GlobalMenuEntity { Id = BusinessMenuId, Key = "test:business", Title = "业务运营", Type = MenuTypeEnum.Menu, IsEnabled = true, IsDeleted = false },
            new GlobalMenuEntity { Id = ProtectedMenuId, Key = "test:protected", Title = "平台专属", Type = MenuTypeEnum.Menu, IsEnabled = true, IsDeleted = false, SuperAdminOnly = true },
            new GlobalMenuEntity { Id = ButtonId, Key = "test:button", Title = "业务按钮", Type = MenuTypeEnum.Button, Pid = BusinessMenuId, IsEnabled = true, IsDeleted = false },
            new GlobalMenuEntity { Id = DeletedMenuId, Key = "test:deleted", Title = "已删菜单", Type = MenuTypeEnum.Menu, IsEnabled = true, IsDeleted = true },
        }).ExecuteCommand();
        _database.MainDb.Insertable(new[]
        {
            new RoleEntity { Id = 11, Name = "业务管理员", IsEnabled = true, IsDeleted = false },
        }).ExecuteCommand();
        _database.MainDb.Insertable(new[]
        {
            new RoleMenuEntity { Id = 21, RoleId = 11, MenuId = BusinessMenuId },
        }).ExecuteCommand();
        _database.MainDb.Insertable(new UserRoleEntity { Id = 31, UserId = 1, RoleId = 11 }).ExecuteCommand();

        var transaction = new SqlSugarTransactionRunner(_database.MainDb);
        var roleMenus = new RoleMenuService(new SqlSugarRepository<RoleMenuEntity> { Context = _database.MainDb },
            new SqlSugarRepository<GlobalMenuEntity> { Context = _database.MainDb }, _cache, transaction);
        var userRoles = new UserRoleService(new SqlSugarRepository<UserRoleEntity> { Context = _database.MainDb }, _cache, transaction);
        _services = new ServiceCollection()
            .AddSingleton(roleMenus)
            .AddSingleton(userRoles)
            .AddSingleton(_cache)
            .BuildServiceProvider();
        _menus = new GlobalMenuService(new CurrentUserContext(_accessor),
            new SqlSugarRepository<GlobalMenuEntity> { Context = _database.MainDb }, _cache, _services, _database.MainDb);
    }

    // HttpContextAccessor 的 AsyncLocal 是静态共享的：每次调用前重设当前 HttpContext 才能切换身份。
    private void UseIdentity(bool superAdmin)
    {
        var claims = new List<Claim> { new Claim(ClaimConst.UserId, "1") };
        if (superAdmin)
        {
            claims.Add(new Claim(ClaimConst.AccountType, ((int)AccountTypeEnum.SuperAdmin).ToString()));
        }

        _accessor.HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test")) };
    }

    [Fact]
    public async Task SuperAdminTree_ContainsBusinessMenusAndExcludesButtonsAndDeletedRows()
    {
        UseIdentity(superAdmin: true);
        var tree = await _menus.GetLoginMenuTreeAsync();

        Assert.Contains(tree, menu => menu.Id == BusinessMenuId);
        Assert.Contains(tree, menu => menu.Id == ProtectedMenuId);
        Assert.DoesNotContain(tree, menu => menu.Id == ButtonId);
        Assert.DoesNotContain(tree, menu => menu.Id == DeletedMenuId);

        var business = tree.Single(menu => menu.Id == BusinessMenuId);
        Assert.DoesNotContain(business.Children, menu => menu.Id == ButtonId);
    }

    [Fact]
    public async Task AdminTree_StillOnlyContainsGrantedMenus()
    {
        UseIdentity(superAdmin: false);
        var tree = await _menus.GetLoginMenuTreeAsync();

        Assert.Contains(tree, menu => menu.Id == BusinessMenuId);
        Assert.DoesNotContain(tree, menu => menu.Id == ProtectedMenuId);
        Assert.DoesNotContain(tree, menu => menu.Id == DeletedMenuId);
    }

    public void Dispose()
    {
        _services.Dispose();
        _database.Dispose();
    }
}
