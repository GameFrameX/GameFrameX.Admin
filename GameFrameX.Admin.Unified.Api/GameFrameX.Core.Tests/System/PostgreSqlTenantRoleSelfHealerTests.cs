using GameFrameX.Core.Service.Tenant;
using Npgsql;
using Xunit;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// PostgreSqlTenantDatabaseProvisioner.BuildHealActions 规划口径单测（纯函数，注入目录状态验证）。
/// <para>覆盖注册租户 PG 角色启动自愈的三条铁律：角色在场跳过、连接串残缺记 invalid 不产动作、
/// 库同样缺失时走完整开通标记。</para>
/// </summary>
public class PostgreSqlTenantRoleSelfHealerTests
{
    [Fact]
    public void BuildHealActions_ExistingRole_IsSkipped()
    {
        var registrations = new[]
        {
            new TenantRoleRegistration(1300000000001, "Host=postgres;Port=5432;Database=gameframex_admin_tenant_1300000000001;Username=gameframex_admin_tenant_user_1300000000001;Password=demo", true),
        };

        var actions = PostgreSqlTenantDatabaseProvisioner.BuildHealActions(
            registrations,
            new HashSet<string>(StringComparer.Ordinal) { "gameframex_admin_tenant_user_1300000000001", },
            new HashSet<string>(StringComparer.Ordinal) { "gameframex_admin_tenant_1300000000001", },
            out var invalidRegistrations);

        Assert.Empty(actions);
        Assert.Empty(invalidRegistrations);
    }

    [Fact]
    public void BuildHealActions_MissingRoleWithExistingDatabase_PlansRoleOnlyHeal()
    {
        var registrations = new[]
        {
            new TenantRoleRegistration(1300000000001, "Host=postgres;Port=5432;Database=gameframex_admin_tenant_1300000000001;Username=gameframex_admin_tenant_user_1300000000001;Password=demo-password", true),
        };

        var actions = PostgreSqlTenantDatabaseProvisioner.BuildHealActions(
            registrations,
            new HashSet<string>(StringComparer.Ordinal),
            new HashSet<string>(StringComparer.Ordinal) { "gameframex_admin_tenant_1300000000001", },
            out var invalidRegistrations);

        var action = Assert.Single(actions);
        Assert.Equal(1300000000001, action.TenantId);
        Assert.Equal("gameframex_admin_tenant_user_1300000000001", action.Username);
        Assert.Equal("gameframex_admin_tenant_1300000000001", action.DatabaseName);
        Assert.Equal("demo-password", action.Password);
        Assert.False(action.RequiresDatabaseCreation);
        Assert.True(action.IsDefaultTenant);
        Assert.Empty(invalidRegistrations);
    }

    [Fact]
    public void BuildHealActions_MissingRoleAndDatabase_PlansFullProvisioning()
    {
        var registrations = new[]
        {
            new TenantRoleRegistration(51255252080389, "Host=postgres;Port=5432;Database=gameframex_admin_tenant_51255252080389;Username=gameframex_admin_tenant_user_51255252080389;Password=demo-password", false),
        };

        var actions = PostgreSqlTenantDatabaseProvisioner.BuildHealActions(
            registrations,
            new HashSet<string>(StringComparer.Ordinal),
            new HashSet<string>(StringComparer.Ordinal),
            out var invalidRegistrations);

        var action = Assert.Single(actions);
        Assert.True(action.RequiresDatabaseCreation);
        Assert.False(action.IsDefaultTenant);
        Assert.Empty(invalidRegistrations);
    }

    [Fact]
    public void BuildHealActions_MalformedConnection_IsInvalidWithoutAction()
    {
        var registrations = new[]
        {
            new TenantRoleRegistration(1300000000001, "not a valid connection string ===", true),
            new TenantRoleRegistration(51255252080389, "Host=postgres;Username=only-user", false),
        };

        var actions = PostgreSqlTenantDatabaseProvisioner.BuildHealActions(
            registrations,
            new HashSet<string>(StringComparer.Ordinal),
            new HashSet<string>(StringComparer.Ordinal),
            out var invalidRegistrations);

        Assert.Empty(actions);
        // 解析失败 + 字段残缺各记一条，且不含可执行动作
        Assert.Equal(2, invalidRegistrations.Count);
        Assert.Contains(invalidRegistrations, u => u.Contains("1300000000001"));
        Assert.Contains(invalidRegistrations, u => u.Contains("51255252080389"));
    }

    [Fact]
    public void BuildHealActions_RegistrationConnectionParsesBackToOriginalValues()
    {
        // 规划产物携带的连接串必须能直接构造 Npgsql 连接（完整开通路径的 TenantConnectionString 来源）
        var registrations = new[]
        {
            new TenantRoleRegistration(51255252080389, "Host=postgres;Port=5432;Database=gameframex_admin_tenant_51255252080389;Username=gameframex_admin_tenant_user_51255252080389;Password=demo-password", false),
        };

        var actions = PostgreSqlTenantDatabaseProvisioner.BuildHealActions(
            registrations,
            new HashSet<string>(StringComparer.Ordinal),
            new HashSet<string>(StringComparer.Ordinal),
            out _);

        var builder = new NpgsqlConnectionStringBuilder(Assert.Single(actions).ConnectionString);
        Assert.Equal("gameframex_admin_tenant_user_51255252080389", builder.Username);
        Assert.Equal("demo-password", builder.Password);
    }
}
