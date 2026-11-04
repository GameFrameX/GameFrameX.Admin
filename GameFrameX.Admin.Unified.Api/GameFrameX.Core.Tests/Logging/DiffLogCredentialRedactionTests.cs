using System.Reflection;
using GameFrameX.Core.Cache;
using GameFrameX.Core.Extension;
using GameFrameX.Core.Option;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.System;
using GameFrameX.Entity.Log;
using GameFrameX.Entity.Tenant.Game;
using SqlSugar;

namespace GameFrameX.Core.Tests.Logging;

public sealed class DiffLogCredentialRedactionTests
{
    [Fact]
    public async Task CredentialChanges_RedactAllAuditSnapshotsAndBoundParameters()
    {
        const string plain = "diff-credential-34567";
        SqlSugarCache.Configure(new FakeCache());
        var databasePath = Path.Combine(Path.GetTempPath(), $"gfx-log-redaction-{Guid.NewGuid():N}.db");
        var configuration = new DbConnectionConfig
        {
            ConfigId = "main", DbType = DbType.Sqlite, ConnectionString = $"DataSource={databasePath}", IsAutoCloseConnection = false,
            DbSettings = new DbSettings { EnableDiffLog = true },
        };
        SqlSugarSetup.SetDbConfig(configuration);
        try
        {
            using (var scope = new SqlSugarScope(configuration))
            {
                var database = scope.GetConnectionScope("main");
                database.CodeFirst.InitTables<TenantSessionKeyEntity, SystemLogDiffEntity>();
                typeof(SqlSugarSetup).GetMethod("SetDbDiffLog", BindingFlags.NonPublic | BindingFlags.Static)!
                    .Invoke(null, new object[] { database, configuration });
                await database.InsertWithDiffLogAsync(new TenantSessionKeyEntity { Id = 91, Kid = "audit-key", Secret = plain, Remark = "audit" });
                var row = Assert.Single(await database.Queryable<SystemLogDiffEntity>().ToListAsync());
                var snapshots = string.Join("|", row.BeforeData, row.AfterData, row.Snapshot, row.BusinessData, row.Sql, row.Parameters);
                Assert.DoesNotContain(plain, snapshots);
                Assert.Contains("******", snapshots);
            }
        }
        finally
        {
            File.Delete(databasePath);
        }
    }
}
