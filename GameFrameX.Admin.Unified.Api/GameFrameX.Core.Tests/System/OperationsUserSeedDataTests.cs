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


        Assert.DoesNotContain(users.Keys, account => account.StartsWith("user", StringComparison.Ordinal));
    }

}