using System.Reflection;
using GameFrameX.Core.Cache;
using GameFrameX.Core.Tests.System;
using System.Runtime.CompilerServices;
using GameFrameX.Core.Extension;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Log;
using GameFrameX.Entity.Business;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

public sealed class DiffLogCompletionTests
{
    [SkippableFact]
    public async Task InsertThenUpdate_WithSlowAudit_CompletesAuditBeforeReusingPostgresConnection()
    {
        SqlSugarCache.Configure(new FakeCache());
        Skip.IfNot(PostgresTestDatabase.TryCreate(out var postgres, out var reason), reason);
        using (postgres)
        {
            var config = postgres!.BuildDbConfig("main", false);
            config.DbSettings.EnableDiffLog = true;
            using (var scope = new SqlSugarScope(config))
            {
                var database = scope.GetConnectionScope("main");
                database.Aop.DataExecuting = SqlSugarAopHelper.HandleBaseAudit;
                database.CodeFirst.InitTables<ChannelDefinitionEntity, SystemLogDiffEntity>();
                typeof(SqlSugarSetup).GetMethod("SetDbDiffLog", BindingFlags.NonPublic | BindingFlags.Static)!
                    .Invoke(null, new object[] { database, config });
                // 旧回调是 async void，慢审计尚未完成就返回，下一条 UPDATE 会争用同一个 Npgsql 连接。
                // 先检测该危险委托，避免回归时未捕获异常杀死整个测试进程。
                var callbacks = typeof(SqlSugarSetup).GetNestedTypes(BindingFlags.NonPublic)
                    .SelectMany(type => type.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                    .Where(method => method.Name.Contains("<SetDbDiffLog>", StringComparison.Ordinal)).ToArray();
                Assert.NotEmpty(callbacks);
                Assert.All(callbacks, callback => Assert.Null(callback.GetCustomAttribute<AsyncStateMachineAttribute>()));
                database.Ado.ExecuteCommand("CREATE FUNCTION slow_audit() RETURNS trigger LANGUAGE plpgsql AS $$ BEGIN PERFORM pg_sleep(0.02); RETURN NEW; END $$");
                database.Ado.ExecuteCommand("CREATE TRIGGER slow_audit BEFORE INSERT ON system_log_diff_entity FOR EACH ROW EXECUTE FUNCTION slow_audit()");
                for (var index = 1; index <= 30; index++)
                {
                    var channel = new ChannelDefinitionEntity { Id = index, Code = "regression-" + index, Name = "渠道", Description = "regression", PlatformCode = "all", IsEnabled = true };
                    await database.InsertWithDiffLogAsync(channel);
                    // 不等待后台队列：业务返回时审计必须已经落库，随后立即复用该连接。
                    Assert.Equal(index, await database.Queryable<SystemLogDiffEntity>().CountAsync());
                    await database.Updateable<ChannelDefinitionEntity>().SetColumns(row => row.Name == "updated").Where(row => row.Id == index).ExecuteCommandAsync();
                }

                Assert.Equal(30, await database.Queryable<ChannelDefinitionEntity>().CountAsync(row => row.Name == "updated"));
            }
        }
    }
}
