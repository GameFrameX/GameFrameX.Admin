using GameFrameX.Core.Service.Tenant;
using SqlSugar;

namespace GameFrameX.Core.Tests.System;

internal sealed class FakeTenantDbConnectionProvider : ITenantDbConnectionProvider
{
    private readonly SqlSugarScope _client;
    private readonly string _tenantConfigId;

    public FakeTenantDbConnectionProvider(SqlSugarScope client, string tenantConfigId)
    {
        _client = client;
        _tenantConfigId = tenantConfigId;
    }

    public SqlSugarScopeProvider GetTenantDbConnectionScope(long tenantId)
    {
        return _client.GetConnectionScope(_tenantConfigId);
    }
}