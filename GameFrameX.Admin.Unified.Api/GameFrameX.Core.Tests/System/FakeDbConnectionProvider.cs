using GameFrameX.Core.Service.Business;
using SqlSugar;

namespace GameFrameX.Core.Tests.System;

internal sealed class FakeDbConnectionProvider
{
    private readonly SqlSugarScope _client;
    private readonly string _tenantConfigId;

    public FakeDbConnectionProvider(SqlSugarScope client, string configId)
    {
        _client = client;
        _tenantConfigId = configId;
    }

    public ISqlSugarClient Database => _client.GetConnectionScope(_tenantConfigId);

    public SqlSugarScopeProvider GetDbConnectionScope(long id)
    {
        return _client.GetConnectionScope(_tenantConfigId);
    }
}