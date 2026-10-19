using GameFrameX.Core.Logging;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Log;
using System.Text.Json;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// C156 日志边界隔离测试。日志不再集中到独立日志库，平台日志（SystemLog*）归属主库 system_log_*，
/// 租户日志（TenantLog*）归属当前租户库 tenant_log_*。隔离由实体归属 + 物理库边界共同保证：
/// 平台与租户日志为不同 CLR 类型，分别路由到不同数据库（路由/catalog 验收见 SqlSugarDatabaseInitializationTests）。
/// </summary>
public class LogTenantQueryIsolationTests
{
    [Fact]
    public void SystemLogEntities_ArePlatformMainDatabase()
    {
        // 平台日志归属主库 system_log_*，SuperAdmin 平台查询只读主库，不触碰任何租户库。
        Assert.True(SqlSugarEntityBoundary.IsPlatformTable(typeof(SystemLogVisitEntity)));
        Assert.True(SqlSugarEntityBoundary.IsPlatformTable(typeof(SystemLogOperationEntity)));
        Assert.True(SqlSugarEntityBoundary.IsPlatformTable(typeof(SystemLogExceptionEntity)));
        Assert.True(SqlSugarEntityBoundary.IsPlatformTable(typeof(SystemLogAuditEntity)));
        Assert.True(SqlSugarEntityBoundary.IsPlatformTable(typeof(SystemLogDiffEntity)));
        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(SystemLogOperationEntity)));
        Assert.False(SqlSugarEntityBoundary.IsTenantTable(typeof(SystemLogOperationEntity)));
        Assert.False(SqlSugarEntityBoundary.RequiresTenantContext(typeof(SystemLogOperationEntity)));
        Assert.Equal("system_log_operation_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(SystemLogOperationEntity), nameof(SystemLogOperationEntity)));
    }

    [Fact]
    public void TenantLogEntities_RequireTenantDatabase()
    {
        // 租户日志归属当前租户库 tenant_log_*，必须随租户上下文路由；主库不承载租户日志。
        Assert.True(SqlSugarEntityBoundary.IsTenantTable(typeof(TenantLogVisitEntity)));
        Assert.True(SqlSugarEntityBoundary.IsTenantTable(typeof(TenantLogOperationEntity)));
        Assert.True(SqlSugarEntityBoundary.IsTenantTable(typeof(TenantLogExceptionEntity)));
        Assert.True(SqlSugarEntityBoundary.IsTenantTable(typeof(TenantLogAuditEntity)));
        Assert.True(SqlSugarEntityBoundary.IsTenantTable(typeof(TenantLogDiffEntity)));
        Assert.True(SqlSugarEntityBoundary.RequiresTenantContext(typeof(TenantLogOperationEntity)));
        Assert.False(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(TenantLogOperationEntity)));
        Assert.Equal("tenant_log_operation_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(TenantLogOperationEntity), nameof(TenantLogOperationEntity)));
    }

    [Fact]
    public void PlatformAndTenantLogs_UseDistinctClrTypes()
    {
        // C156 禁止同一 CLR 实体按运行上下文切物理表：平台与租户日志必须是不同实体类型。
        Assert.NotEqual(typeof(SystemLogVisitEntity), typeof(TenantLogVisitEntity));
        Assert.NotEqual(typeof(SystemLogOperationEntity), typeof(TenantLogOperationEntity));
        Assert.NotEqual(typeof(SystemLogExceptionEntity), typeof(TenantLogExceptionEntity));
        Assert.NotEqual(typeof(SystemLogAuditEntity), typeof(TenantLogAuditEntity));
        Assert.NotEqual(typeof(SystemLogDiffEntity), typeof(TenantLogDiffEntity));
    }

    [Fact]
    public void IsSuccessfulUnifiedReturn_SuccessResponse_ReturnsTrue()
    {
        var returnInformationJson = JsonSerializer.Serialize(new
        {
            httpStatusCode = 200,
            value = JsonSerializer.Serialize(new { code = 200, type = "success", }),
        });

        Assert.True(DatabaseLoggingWriter.IsSuccessfulUnifiedReturn(returnInformationJson));
    }

    [Fact]
    public void IsSuccessfulUnifiedReturn_FailedResponse_ReturnsFalse()
    {
        var returnInformationJson = JsonSerializer.Serialize(new
        {
            httpStatusCode = 500,
            value = JsonSerializer.Serialize(new { code = 500, type = "error", }),
        });

        Assert.False(DatabaseLoggingWriter.IsSuccessfulUnifiedReturn(returnInformationJson));
    }

    [Fact]
    public void TryReadLoginScope_LoginParameters_ReturnsTenantAndAccount()
    {
        var parametersJson = JsonSerializer.Serialize(new[]
        {
            new
            {
                name = "input",
                value = new
                {
                    account = "admin",
                    tenantId = 1300000000001,
                    tenantHost = (string?)null,
                },
            },
        });

        var scope = DatabaseLoggingWriter.TryReadLoginScope(parametersJson);

        Assert.NotNull(scope);
        Assert.Equal("admin", scope.Account);
        Assert.Equal(1300000000001, scope.TenantId);
    }
}