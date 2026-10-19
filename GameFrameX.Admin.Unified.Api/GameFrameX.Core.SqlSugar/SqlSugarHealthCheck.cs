using Microsoft.Extensions.Diagnostics.HealthChecks;
using SqlSugar;

namespace GameFrameX.Core.SqlSugar;

public sealed class SqlSugarHealthCheck : IHealthCheck
{
    private readonly ISqlSugarClient _db;

    public SqlSugarHealthCheck(ISqlSugarClient db)
    {
        _db = db;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _db.Ado.CheckConnection();
            return Task.FromResult(HealthCheckResult.Healthy("Database connection is available."));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Database connection check failed.", ex));
        }
    }
}