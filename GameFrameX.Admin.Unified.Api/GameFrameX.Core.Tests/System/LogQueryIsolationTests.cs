using GameFrameX.Core.Logging;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Log;
using System.Text.Json;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// C156 日志边界隔离测试。日志不再集中到独立日志库，平台日志（SystemLog*）归属主库 system_log_*，
/// 租户日志（Log*）归属当前租户库 log_*。隔离由实体归属 + 物理库边界共同保证：
/// 平台与租户日志为不同 CLR 类型，分别路由到不同数据库（路由/catalog 验收见 SqlSugarDatabaseInitializationTests）。
/// </summary>
public class LogQueryIsolationTests
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
        Assert.False(SqlSugarEntityBoundary.IsBusinessTable(typeof(SystemLogOperationEntity)));
        Assert.False(SqlSugarEntityBoundary.RequiresContext(typeof(SystemLogOperationEntity)));
        Assert.Equal("system_log_operation_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(SystemLogOperationEntity), nameof(SystemLogOperationEntity)));
    }

    [Fact]
    public void TenantLogEntities_UseMainDatabase()
    {
        // 历史日志表保留名称，读写统一归主库。
        Assert.True(SqlSugarEntityBoundary.IsBusinessTable(typeof(LogVisitEntity)));
        Assert.True(SqlSugarEntityBoundary.IsBusinessTable(typeof(LogOperationEntity)));
        Assert.True(SqlSugarEntityBoundary.IsBusinessTable(typeof(LogExceptionEntity)));
        Assert.True(SqlSugarEntityBoundary.IsBusinessTable(typeof(LogAuditEntity)));
        Assert.True(SqlSugarEntityBoundary.IsBusinessTable(typeof(LogDiffEntity)));
        Assert.False(SqlSugarEntityBoundary.RequiresContext(typeof(LogOperationEntity)));
        Assert.True(SqlSugarEntityBoundary.IsMainDatabaseTable(typeof(LogOperationEntity)));
        Assert.Equal("log_operation_entity", SqlSugarEntityBoundary.GetPhysicalTableName(typeof(LogOperationEntity), nameof(LogOperationEntity)));
    }

    [Fact]
    public void PlatformAndTenantLogs_UseDistinctClrTypes()
    {
        // C156 禁止同一 CLR 实体按运行上下文切物理表：平台与租户日志必须是不同实体类型。
        Assert.NotEqual(typeof(SystemLogVisitEntity), typeof(LogVisitEntity));
        Assert.NotEqual(typeof(SystemLogOperationEntity), typeof(LogOperationEntity));
        Assert.NotEqual(typeof(SystemLogExceptionEntity), typeof(LogExceptionEntity));
        Assert.NotEqual(typeof(SystemLogAuditEntity), typeof(LogAuditEntity));
        Assert.NotEqual(typeof(SystemLogDiffEntity), typeof(LogDiffEntity));
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
    public void TryReadLoginScope_LoginParameters_ReturnsAccount()
    {
        var parametersJson = JsonSerializer.Serialize(new[]
        {
            new
            {
                name = "input",
                value = new
                {
                    account = "admin",
                    id = 1300000000001,
                    host = (string?)null,
                },
            },
        });

        var scope = DatabaseLoggingWriter.TryReadLoginScope(parametersJson);

        Assert.NotNull(scope);
        Assert.Equal("admin", scope.Account);
    }
}