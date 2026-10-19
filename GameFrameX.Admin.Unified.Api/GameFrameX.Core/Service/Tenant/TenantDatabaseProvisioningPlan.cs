namespace GameFrameX.Core.Service.Tenant;

public sealed record TenantDatabaseProvisioningPlan(
    long TenantId,
    string AdminConnectionString,
    string DatabaseName,
    string Username,
    string Password,
    string TenantConnectionString);