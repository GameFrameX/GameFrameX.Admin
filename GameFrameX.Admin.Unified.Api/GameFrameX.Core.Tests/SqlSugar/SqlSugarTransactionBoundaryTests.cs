using System.Reflection;
using GameFrameX.Application.System.Tenant.Notice;
using GameFrameX.Core.Service.Menu;
using GameFrameX.Core.Service.Organization;
using GameFrameX.Core.Service.PlatformCaptcha;
using GameFrameX.Core.Service.PlatformJwt;
using GameFrameX.Core.Service.Role;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.SqlSugar;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GameFrameX.Core.Tests.SqlSugar;

public sealed class SqlSugarTransactionBoundaryTests
{
    [Fact]
    public void SqlSugarTransactionAttribute_IsNativeActionFilter()
    {
        Assert.True(typeof(IAsyncActionFilter).IsAssignableFrom(typeof(SqlSugarTransactionAttribute)));
        Assert.Equal(-1000, new SqlSugarTransactionAttribute().Order);
    }

    [Fact]
    public void TransactionalMethods_UseSqlSugarTransactionAttribute()
    {
        var assemblies = new[]
        {
            typeof(TenantUserService).Assembly,
            typeof(TenantNoticeService).Assembly,
        };

        var localTransactionMethods = assemblies
                                      .SelectMany(u => u.GetTypes())
                                      .SelectMany(u => u.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                                      .Where(u => u.GetCustomAttributes<SqlSugarTransactionAttribute>(true).Any())
                                      .ToList();
        Assert.Contains(localTransactionMethods, u => u.DeclaringType == typeof(TenantUserService) && u.Name == nameof(TenantUserService.AddUserAsync));
        Assert.Contains(localTransactionMethods, u => u.DeclaringType == typeof(TenantOrganizationService) && u.Name == nameof(TenantOrganizationService.UpdateOrganizationAsync));
        Assert.Contains(localTransactionMethods, u => u.DeclaringType == typeof(TenantRoleService) && u.Name == nameof(TenantRoleService.DeleteRoleAsync));
        Assert.Contains(localTransactionMethods, u => u.DeclaringType == typeof(GlobalMenuService) && u.Name == nameof(GlobalMenuService.DeleteMenuAsync));
        Assert.Contains(localTransactionMethods, u => u.DeclaringType == typeof(PlatformCaptchaConfigService) && u.Name == nameof(PlatformCaptchaConfigService.UpdateAsync));
        Assert.Contains(localTransactionMethods, u => u.DeclaringType == typeof(PlatformJwtConfigService) && u.Name == nameof(PlatformJwtConfigService.UpdateAsync));
        Assert.Contains(localTransactionMethods, u => u.DeclaringType == typeof(TenantNoticeService) && u.Name == nameof(TenantNoticeService.UpdateNoticeAsync));
    }

    [Fact]
    public void InternalTransactionalServices_UseExplicitTransactionRunner()
    {
        AssertUsesTransactionRunner(typeof(TenantRoleMenuService), nameof(TenantRoleMenuService.GrantRoleMenuAsync));
        AssertUsesTransactionRunner(typeof(TenantRoleOrganizationService), nameof(TenantRoleOrganizationService.GrantRoleOrganizationAsync));
        AssertUsesTransactionRunner(typeof(TenantUserRoleService), nameof(TenantUserRoleService.GrantUserRoleAsync));
        AssertUsesTransactionRunner(typeof(TenantUserExtensionOrganizationService), nameof(TenantUserExtensionOrganizationService.UpdateUserExtOrganizationAsync));
    }

    private static void AssertUsesTransactionRunner(Type type, string methodName)
    {
        var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.NotNull(method);
        Assert.DoesNotContain(method!.GetCustomAttributes(true),
                              attribute => attribute.GetType() == typeof(SqlSugarTransactionAttribute));
        Assert.Contains(type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic),
                        field => field.FieldType == typeof(SqlSugarTransactionRunner));
    }
}