using GameFrameX.Core.Middleware;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.ClientApiConfig;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class ClientApiRiskPolicyTests
{
    [Fact]
    public void Resolve_NoEnabledPolicy_FailsClosed()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitTenantTables(typeof(TenantClientApiPolicyEntity));
            var resolver = new GameAppRiskEnforcementModeResolver(database.MainDb);
            Assert.Equal(RiskEnforcementModeEnum.FailClosed, resolver.Resolve());
        }
    }

    [Fact]
    public void Resolve_UsesLatestEnabledPolicy_WithoutRequestScope()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitTenantTables(typeof(TenantClientApiPolicyEntity));
            database.MainDb.Insertable(new[]
            {
                new TenantClientApiPolicyEntity { Id = 1, IsEnabled = true, RiskPolicy = "FailOpen", },
                new TenantClientApiPolicyEntity { Id = 2, IsEnabled = true, SignatureRequired = true, RiskPolicy = "invalid", },
                new TenantClientApiPolicyEntity { Id = 3, IsEnabled = false, RiskPolicy = "FailOpen", },
            }).ExecuteCommand();
            var resolver = new GameAppRiskEnforcementModeResolver(database.MainDb);
            Assert.Equal(RiskEnforcementModeEnum.FailClosed, resolver.Resolve());
        }
    }
}
