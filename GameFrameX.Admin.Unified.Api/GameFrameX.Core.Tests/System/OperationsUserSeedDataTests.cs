using GameFrameX.Application.SeedData.System;

namespace GameFrameX.Core.Tests.System;

public class OperationsUserSeedDataTests
{
    [Fact]
    public void TenantUserSeedData_UsesGameOperationsDemoAccounts()
    {
        var users = TenantUserSeedData.BuildData("test-password")
                                      .ToDictionary(user => user.Account, StringComparer.Ordinal);

        Assert.DoesNotContain("superadmin", users.Keys);
        Assert.Contains("admin", users.Keys);
        Assert.Contains("ops_manager", users.Keys);
        Assert.Contains("gm_support", users.Keys);
        Assert.Contains("content_ops", users.Keys);
        Assert.Contains("server_ops", users.Keys);
        Assert.Contains("payment_ops", users.Keys);
        Assert.Contains("marketing_ops", users.Keys);
        Assert.Contains("data_analyst", users.Keys);
        Assert.Contains("audit_security", users.Keys);
        Assert.Contains("app_config_admin", users.Keys);

        Assert.Equal(1300000000104, users["ops_manager"].OrganizationId);
        Assert.Equal(1300000000102, users["ops_manager"].PosId);
        Assert.Equal(1300000000106, users["gm_support"].OrganizationId);
        Assert.Equal(1300000000114, users["gm_support"].PosId);
        Assert.Equal(1300000000303, users["server_ops"].OrganizationId);
        Assert.Equal(1300000000108, users["server_ops"].PosId);

        Assert.DoesNotContain(users.Keys, account => account.StartsWith("user", StringComparison.Ordinal));
    }

    [Fact]
    public void TenantUserRoleSeedData_AssignsEachDemoAccountToMatchingGameRole()
    {
        var usersByAccount = TenantUserSeedData.BuildData("test-password")
                                               .ToDictionary(user => user.Account, user => user.Id, StringComparer.Ordinal);
#pragma warning disable CS8714 // 种子数据 Code 实际非空，但实体声明为 string?，TKey 推断冲突；测试仅按已知非空 Code 查询
#pragma warning disable CS8621 // 同上：lambda 返回 string? 与 ToDictionary notnull TKey 推断的 Func<string> 不匹配
        var rolesByCode = new TenantRoleSeedData().HasData().ToDictionary(role => role.Code, role => role.Id, StringComparer.Ordinal);
#pragma warning restore CS8621
#pragma warning restore CS8714
        var userRoles = new TenantUserRoleSeedData().HasData().ToHashSet();

        Assert.Contains(userRoles, item => item.UserId == usersByAccount["admin"] && item.RoleId == rolesByCode["sys_admin"]);
        Assert.Contains(userRoles, item => item.UserId == usersByAccount["ops_manager"] && item.RoleId == rolesByCode["ops_manager"]);
        Assert.Contains(userRoles, item => item.UserId == usersByAccount["gm_support"] && item.RoleId == rolesByCode["gm_support"]);
        Assert.Contains(userRoles, item => item.UserId == usersByAccount["content_ops"] && item.RoleId == rolesByCode["content_ops"]);
        Assert.Contains(userRoles, item => item.UserId == usersByAccount["server_ops"] && item.RoleId == rolesByCode["server_ops"]);
        Assert.Contains(userRoles, item => item.UserId == usersByAccount["payment_ops"] && item.RoleId == rolesByCode["payment_ops"]);
        Assert.Contains(userRoles, item => item.UserId == usersByAccount["marketing_ops"] && item.RoleId == rolesByCode["marketing_ops"]);
        Assert.Contains(userRoles, item => item.UserId == usersByAccount["data_analyst"] && item.RoleId == rolesByCode["data_analyst"]);
        Assert.Contains(userRoles, item => item.UserId == usersByAccount["audit_security"] && item.RoleId == rolesByCode["audit_security"]);
        Assert.Contains(userRoles, item => item.UserId == usersByAccount["app_config_admin"] && item.RoleId == rolesByCode["app_config_admin"]);
    }

    [Fact]
    public void TenantUserExtensionOrganizationSeedData_AssignsDemoAccountsToGameOrganizationsAndPositions()
    {
        var usersByAccount = TenantUserSeedData.BuildData("test-password")
                                               .ToDictionary(user => user.Account, user => user.Id, StringComparer.Ordinal);
        var userExtOrgs = new TenantUserExtensionOrganizationSeedData().HasData().ToList();

        Assert.Contains(userExtOrgs, item => item.UserId == usersByAccount["ops_manager"] && item.OrganizationId == 1300000000104 && item.PosId == 1300000000102);
        Assert.Contains(userExtOrgs, item => item.UserId == usersByAccount["gm_support"] && item.OrganizationId == 1300000000106 && item.PosId == 1300000000114);
        Assert.Contains(userExtOrgs, item => item.UserId == usersByAccount["content_ops"] && item.OrganizationId == 1300000000104 && item.PosId == 1300000000107);
        Assert.Contains(userExtOrgs, item => item.UserId == usersByAccount["server_ops"] && item.OrganizationId == 1300000000303 && item.PosId == 1300000000108);
        Assert.Contains(userExtOrgs, item => item.UserId == usersByAccount["payment_ops"] && item.OrganizationId == 1300000000101 && item.PosId == 1300000000116);
        Assert.Contains(userExtOrgs, item => item.UserId == usersByAccount["app_config_admin"] && item.OrganizationId == 1300000000104 && item.PosId == 1300000000104);
    }
}