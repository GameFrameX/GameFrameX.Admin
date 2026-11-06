using GameFrameX.Core.Service.Business;
using SqlSugar;

namespace GameFrameX.Core.Tests.System;

internal sealed class FakeDbConnectionProvider
{
    private readonly SqlSugarScope _client;
    private readonly string _configId;

    public FakeDbConnectionProvider(SqlSugarScope client, string configId)
    {
        _client = client;
        _configId = configId;
    }

    public ISqlSugarClient Database => _client.GetConnectionScope(_configId);
}