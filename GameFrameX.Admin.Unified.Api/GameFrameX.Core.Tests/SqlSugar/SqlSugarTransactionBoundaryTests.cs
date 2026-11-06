using System.Reflection;
using GameFrameX.Core.Service.Menu;
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
            typeof(UserService).Assembly,
            typeof(GameFrameX.Application.Platform.OpenAccess.SystemOpenAccessService).Assembly,
        };

        var localTransactionMethods = assemblies
                                      .SelectMany(u => u.GetTypes())
                                      .SelectMany(u => u.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                                      .Where(u => u.GetCustomAttributes<SqlSugarTransactionAttribute>(true).Any())
                                      .ToList();
        Assert.Contains(localTransactionMethods, u => u.DeclaringType == typeof(UserService) && u.Name == nameof(UserService.AddUserAsync));
        Assert.Contains(localTransactionMethods, u => u.DeclaringType == typeof(RoleService) && u.Name == nameof(RoleService.DeleteRoleAsync));
        Assert.Contains(localTransactionMethods, u => u.DeclaringType == typeof(GlobalMenuService) && u.Name == nameof(GlobalMenuService.DeleteMenuAsync));
        Assert.Contains(localTransactionMethods, u => u.DeclaringType == typeof(PlatformCaptchaConfigService) && u.Name == nameof(PlatformCaptchaConfigService.UpdateAsync));
        Assert.Contains(localTransactionMethods, u => u.DeclaringType == typeof(PlatformJwtConfigService) && u.Name == nameof(PlatformJwtConfigService.UpdateAsync));
    }

    [Fact]
    public void InternalTransactionalServices_UseExplicitTransactionRunner()
    {
        AssertUsesTransactionRunner(typeof(RoleMenuService), nameof(RoleMenuService.GrantRoleMenuAsync));
        AssertUsesTransactionRunner(typeof(UserRoleService), nameof(UserRoleService.GrantUserRoleAsync));
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