using SqlSugar;

namespace GameFrameX.DatabaseMigration;

internal static class Program
{
    private static int Main(string[] arguments)
    {
        try
        {
            if (arguments.SequenceEqual(new[] { "--self-test" }))
            {
                MigrationSmokeTests.Run();
                return 0;
            }

            if (arguments.SequenceEqual(new[] { "--self-test-postgres" }))
            {
                PostgresMigrationSmokeTests.Run(RequiredEnvironment("GFX_TEST_PG_CONNECTION_STRING"));
                return 0;
            }

            if (arguments.Length == 1 && arguments[0] is "--redact-logs-dry-run" or "--redact-logs")
            {
                var cleanupType = Enum.Parse<DbType>(RequiredEnvironment("GFX_MIGRATION_DATABASE_TYPE"), true);
                using (var database = SingleMainMigration.Connect(cleanupType, RequiredEnvironment("GFX_MIGRATION_TARGET")))
                {
                    database.Ado.BeginTran();
                    try
                    {
                        LogCredentialCleanup.Apply(database, arguments[0] == "--redact-logs-dry-run");
                        database.Ado.CommitTran();
                    }
                    catch
                    {
                        database.Ado.RollbackTran();
                        throw;
                    }
                }

                return 0;
            }

            if (arguments.Length != 1 || (arguments[0] != "--dry-run" && arguments[0] != "--execute"))
            {
                Console.Error.WriteLine("用法：--dry-run | --execute | --self-test | --self-test-postgres | --redact-logs-dry-run | --redact-logs；连接通过 GFX_MIGRATION_SOURCE / GFX_MIGRATION_TARGET，类型通过 GFX_MIGRATION_DATABASE_TYPE 指定。");
                return 64;
            }

            var databaseType = Enum.Parse<DbType>(RequiredEnvironment("GFX_MIGRATION_DATABASE_TYPE"), true);
            using (var source = SingleMainMigration.Connect(databaseType, RequiredEnvironment("GFX_MIGRATION_SOURCE")))
            using (var target = SingleMainMigration.Connect(databaseType, RequiredEnvironment("GFX_MIGRATION_TARGET")))
            {
                SingleMainMigration.Run(source, target, arguments[0] == "--dry-run");
            }

            return 0;
        }
        catch (Exception exception)
        {
            if (arguments.SequenceEqual(new[] { "--self-test" }))
            {
                Console.Error.WriteLine(exception);
            }

            // 数据库异常可能包含连接凭据或行内容，不直接打印异常消息。
            Console.Error.WriteLine($"迁移终止，目标事务已回滚；错误类型：{exception.GetType().Name}。");
            for (Exception? current = exception; current != null; current = current.InnerException)
            {
                if (current.Data["MigrationTable"] is string tableName
                    && tableName.All(character => char.IsAsciiLetterOrDigit(character) || character == '_'))
                {
                    Console.Error.WriteLine($"当前表：{tableName}");
                }

                if (current is System.Data.Common.DbException databaseException
                    && databaseException.SqlState is string sqlState
                    && sqlState.Length == 5 && sqlState.All(char.IsAsciiLetterOrDigit))
                {
                    Console.Error.WriteLine($"SQLSTATE：{sqlState}");
                }
            }
            if (exception is InvalidOperationException && exception.Source == typeof(SingleMainMigration).Assembly.GetName().Name)
            {
                Console.Error.WriteLine(exception.Message);
            }

            return 1;
        }
    }

    private static string RequiredEnvironment(string name)
    {
        return Environment.GetEnvironmentVariable(name) ?? throw new ArgumentException($"缺少环境变量 {name}");
    }
}
