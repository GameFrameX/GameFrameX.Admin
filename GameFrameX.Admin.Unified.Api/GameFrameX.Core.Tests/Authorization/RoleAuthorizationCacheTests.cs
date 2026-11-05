using GameFrameX.Core.Const.MenuSeed;
using System.Security.Claims;
using GameFrameX.Core.Cache;
using GameFrameX.Core.Const;
using GameFrameX.Core.Option;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Menu;
using GameFrameX.Core.Service.Role;
using GameFrameX.Core.Service.Role.Dto;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.Service.User.Dto;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Core.Tests.System;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GameFrameX.Core.Tests.Authorization;

/// <summary>预热缓存后修改授权，稳定菜单 Id 的授权结果必须立即更新。</summary>
public sealed class RoleAuthorizationCacheTests : IDisposable
{
    private readonly ClientApiTestDb _database = new();
    private readonly SystemCacheService _cache = new(new MemoryCacheService(), Options.Create(new CacheOptions()));
    private readonly TenantRoleMenuService _roleMenus;
    private readonly TenantUserRoleService _userRoles;
    private readonly TenantRoleService _roles;
    private readonly ServiceProvider _services;
    private readonly GlobalMenuService _menus;

    public RoleAuthorizationCacheTests()
    {
        MenuSeedCatalogTestData.LoadSampleMenus();
        _database.InitTenantTables(typeof(GlobalMenuEntity), typeof(TenantRoleEntity), typeof(TenantRoleMenuEntity), typeof(TenantUserRoleEntity));
        _database.MainDb.Insertable(new[]
        {
            new GlobalMenuEntity { Id = 101, Key = "cache-test:first", Title = "First", Type = MenuTypeEnum.Button, IsEnabled = true, IsDeleted = false },
            new GlobalMenuEntity { Id = 102, Key = "cache-test:second", Title = "Second", Type = MenuTypeEnum.Button, IsEnabled = true, IsDeleted = false },
        }).ExecuteCommand();
        _database.MainDb.Insertable(new[]
        {
            new TenantRoleEntity { Id = 11, Name = "First role", IsEnabled = true, IsDeleted = false },
            new TenantRoleEntity { Id = 12, Name = "Second role", IsEnabled = true, IsDeleted = false },
        }).ExecuteCommand();
        _database.MainDb.Insertable(new[]
        {
            new TenantRoleMenuEntity { Id = 21, RoleId = 11, MenuId = 101 },
            new TenantRoleMenuEntity { Id = 22, RoleId = 12, MenuId = 102 },
        }).ExecuteCommand();
        _database.MainDb.Insertable(new TenantUserRoleEntity { Id = 31, UserId = 1, RoleId = 11 }).ExecuteCommand();
        var transaction = new SqlSugarTransactionRunner(_database.MainDb);
        var menuRepository = new SqlSugarRepository<GlobalMenuEntity> { Context = _database.MainDb };
        _roleMenus = new TenantRoleMenuService(new SqlSugarRepository<TenantRoleMenuEntity> { Context = _database.MainDb }, menuRepository, _cache, transaction);
        _userRoles = new TenantUserRoleService(new SqlSugarRepository<TenantUserRoleEntity> { Context = _database.MainDb }, _cache, transaction);
        var context = new CurrentUserContext(new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimConst.UserId, "1") }, "test")) },
        });
        _services = new ServiceCollection().AddSingleton(_roleMenus).AddSingleton(_userRoles).AddSingleton(_cache).BuildServiceProvider();
        _menus = new GlobalMenuService(context, menuRepository, _cache, _services, _database.MainDb);
        _roles = new TenantRoleService(context, new SqlSugarRepository<TenantRoleEntity> { Context = _database.MainDb }, _roleMenus, _userRoles, _services);
    }

    [Fact]
    public async Task GrantAndRevokeMenus_RefreshesWarmCacheWithoutRestart()
    {
        Assert.Equal(new long[] { 101 }, await _menus.GetOwnMenuIdListAsync());
        _cache.Set(CacheConst.KeyUserMenu + 2, new List<long> { 101 });
        await _roleMenus.GrantRoleMenuAsync(new RoleMenuRequest { Id = 11, MenuIdList = new List<long> { 102 } });
        Assert.Null(_cache.Get<List<long>>(CacheConst.KeyUserMenu + 2));
        Assert.Equal(new long[] { 102 }, await _menus.GetOwnMenuIdListAsync());
        await _roleMenus.GrantRoleMenuAsync(new RoleMenuRequest { Id = 11, MenuIdList = new List<long>() });
        Assert.Empty(await _menus.GetOwnMenuIdListAsync());
    }

    [Fact]
    public async Task ReplaceAndClearUserRoles_RefreshesWarmCache()
    {
        Assert.Equal(new long[] { 101 }, await _menus.GetOwnMenuIdListAsync());
        await _userRoles.GrantUserRoleAsync(new UserRoleRequest { UserId = 1, RoleIdList = new List<long> { 12 } });
        Assert.Equal(new long[] { 102 }, await _menus.GetOwnMenuIdListAsync());
        await _userRoles.GrantUserRoleAsync(new UserRoleRequest { UserId = 1, RoleIdList = new List<long>() });
        Assert.Empty(await _menus.GetOwnMenuIdListAsync());
    }

    [Fact]
    public async Task DisableAndEnableRole_RefreshesWarmCacheAndEnforcesStatus()
    {
        Assert.Equal(new long[] { 101 }, await _menus.GetOwnMenuIdListAsync());
        _cache.Set(CacheConst.KeyUserButton + 1, new List<string> { "cache-test:first" });
        await _roles.SetStatusAsync(new RoleRequest { Id = 11, IsEnabled = false });
        Assert.Null(_cache.Get<List<string>>(CacheConst.KeyUserButton + 1));
        Assert.Empty(await _menus.GetOwnMenuIdListAsync());
        await _roles.SetStatusAsync(new RoleRequest { Id = 11, IsEnabled = true });
        Assert.Equal(new long[] { 101 }, await _menus.GetOwnMenuIdListAsync());
    }

    [Fact]
    public async Task EditingRoleStatus_RevokesWarmCacheWithoutMenuChanges()
    {
        Assert.Equal(new long[] { 101 }, await _menus.GetOwnMenuIdListAsync());
        await _roles.UpdateRoleAsync(new UpdateRoleRequest
        {
            Id = 11, Name = "First role", IsEnabled = false, RowVersion = 0,
        });
        Assert.Empty(await _menus.GetOwnMenuIdListAsync());
    }

    [Fact]
    public async Task GrantAllMenus_RejectsNestedProtectedSubtreesButKeepsReadableSelectors()
    {
        var seedMenus = MenuSeedCatalog.Menus.ToList();
        await _database.MainDb.Insertable(seedMenus).ExecuteCommandAsync();
        await _roleMenus.GrantRoleMenuAsync(new RoleMenuRequest
        {
            Id = 11, MenuIdList = seedMenus.Select(menu => menu.Id).ToList(),
        });
        var granted = await _database.MainDb.Queryable<TenantRoleMenuEntity>()
                                     .Where(relation => relation.RoleId == 11)
                                     .Select(relation => relation.MenuId).ToListAsync();
        var protectedIdentifiers = MenuSeedCatalog.SuperAdminOnlySubtreeMenuIds;
        var readableIdentifiers = MenuSeedCatalog.TenantReadableSuperAdminOnlyMenuIds;
        Assert.NotEmpty(protectedIdentifiers);
        Assert.All(protectedIdentifiers.Except(readableIdentifiers), identifier => Assert.DoesNotContain(identifier, granted));
        Assert.All(readableIdentifiers, identifier => Assert.Contains(identifier, granted));
        Assert.Contains(seedMenus.Single(menu => menu.Key == "route:home").Id, granted);
    }

    [Theory]
    [InlineData("roleMenus")]
    [InlineData("roleUsers")]
    [InlineData("userRoles")]
    public async Task DeleteAssignments_RevokesWarmAuthorization(string operation)
    {
        Assert.Equal(new long[] { 101 }, await _menus.GetOwnMenuIdListAsync());
        _cache.Set(CacheConst.KeyUserButton + 1, new List<string> { "cache-test:first" });
        if (operation == "roleMenus")
        {
            await _roleMenus.DeleteRoleMenuByRoleIdAsync(11);
        }
        else if (operation == "roleUsers")
        {
            await _userRoles.DeleteUserRoleByRoleIdAsync(11);
        }
        else
        {
            await _userRoles.DeleteUserRoleByUserIdAsync(1);
        }
        Assert.Null(_cache.Get<List<string>>(CacheConst.KeyUserButton + 1));
        Assert.Empty(await _menus.GetOwnMenuIdListAsync());
    }

    public void Dispose()
    {
        _services.Dispose();
        _database.Dispose();
    }
}
