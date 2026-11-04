using System.Security.Claims;
using GameFrameX.Application.System.Logging;
using GameFrameX.Application.System.Logging.Dto;
using GameFrameX.Core.Cache;
using GameFrameX.Core.Tests.System;
using GameFrameX.Core.Const;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Log;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace GameFrameX.Core.Tests.Logging;

public sealed class LogPagePostgresTests
{
    [SkippableTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SortedPageAndExport_IncludeCurrentAndHistoricalLogs(bool superAdmin)
    {
        SqlSugarCache.Configure(new FakeCache());
        Skip.IfNot(PostgresTestDatabase.TryCreate(out var postgres, out var reason), reason);
        using (postgres)
        using (var scope = new SqlSugarScope(postgres!.BuildDbConfig("main", false)))
        {
            var database = scope.GetConnectionScope("main");
            database.CodeFirst.InitTables<SystemLogOperationEntity, TenantLogOperationEntity>();
            database.Insertable(CreateLog<SystemLogOperationEntity>(1, 100, "current-admin")).ExecuteCommand();
            database.Insertable(CreateLog<TenantLogOperationEntity>(2, 50, "historical-admin")).ExecuteCommand();
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimConst.AccountType, ((int)(superAdmin ? AccountTypeEnum.SuperAdmin : AccountTypeEnum.NormalUser)).ToString()),
            }, "test"));
            var context = new CurrentUserContext(new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = principal } });
            var service = new LogOperationService(new() { Context = database }, new() { Context = database }, context);
            var page = await service.PageAsync(new PageLogRequest { Page = 1, PageSize = 1, OrderBy = "createdTime", IsDescending = true });
            Assert.Equal(2, page.Total);
            Assert.Equal("current-admin", Assert.Single(page.Items).Account);
            var filtered = await service.PageAsync(new PageLogRequest { Page = 1, PageSize = 10, OrderBy = "createdTime", SearchKey = "historical" });
            Assert.Equal("historical-admin", Assert.Single(filtered.Items).Account);
            var export = (FileStreamResult)await service.ExportLogOpAsync(new LogRequest());
            using (export.FileStream)
            {
                Assert.True(export.FileStream.Length > 0);
            }
        }
    }

    private static TLog CreateLog<TLog>(long identity, long timestamp, string account) where TLog : LogOperationFields, new()
    {
        var row = new TLog();
        foreach (var property in typeof(TLog).GetProperties().Where(property => property.PropertyType == typeof(string) && property.CanWrite))
        {
            property.SetValue(row, string.Empty);
        }

        row.Id = identity;
        row.CreatedTime = timestamp;
        row.LogDateTime = timestamp;
        row.Account = account;
        return row;
    }
}
