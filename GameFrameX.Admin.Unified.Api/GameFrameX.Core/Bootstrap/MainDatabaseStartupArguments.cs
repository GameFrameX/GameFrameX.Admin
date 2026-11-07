using GameFrameX.Core.Extension.Const;

namespace GameFrameX.Core.Bootstrap;

/// <summary>
/// 主库连接的成对裸启动参数（--DbType / --ConnectionString）翻译为标准配置层。
/// </summary>
/// <remarks>
/// 三个 host 共用同一优先级链：裸启动参数（成对，最高）&gt; GFX_MAIN_DB_* 成对环境变量 &gt; 显式 DbConnection 配置节（appsettings / 环境变量 / 命令行）&gt; 共享 Database.json 兜底。
/// 实现方式是把成对参数翻译成 DbConnection 配置节键，以最高优先级追加进配置管线并写入
/// <c>DbConnection:MainDatabaseFromStartupArguments</c> 标记；DbConnectionOptions 见标记即跳过 GFX_MAIN_DB_* 覆盖与成对校验，保证命令行参数语义压过环境变量。
/// 支持两种形态：--DbType PostgreSQL --ConnectionString "..." 与 --DbType=PostgreSQL --ConnectionString=...（键大小写不敏感）。
/// </remarks>
public static class MainDatabaseStartupArguments
{
    private const string DatabaseTypeArgumentName = "DbType";
    private const string ConnectionStringArgumentName = "ConnectionString";
    private const string PrecedenceMarkerKey = "DbConnection:MainDatabaseFromStartupArguments";

    /// <summary>
    /// 识别成对裸启动参数并追加为最高优先级配置层；参数缺失时不追加任何键。
    /// </summary>
    /// <param name="builder">host 配置构建器。</param>
    /// <param name="args">进程启动参数。</param>
    /// <returns>原配置构建器。</returns>
    public static IConfigurationBuilder AddMainDatabaseStartupArguments(this IConfigurationBuilder builder, string[] args)
    {
        var databaseType = FindArgumentValue(args, DatabaseTypeArgumentName);
        var connectionString = FindArgumentValue(args, ConnectionStringArgumentName);
        if (databaseType == null && connectionString == null)
        {
            return builder;
        }

        if (databaseType == null || connectionString == null)
        {
            throw new InvalidOperationException(
                "裸启动参数必须成对提供：--DbType 与 --ConnectionString（或改用 GFX_MAIN_DB_TYPE + GFX_MAIN_DB_CONNECTION_STRING 成对环境变量 / DbConnection 配置节）。");
        }

        return builder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DbConnection:ConnectionConfigs:0:ConfigId"] = SqlSugarConst.MainConfigId,
            ["DbConnection:ConnectionConfigs:0:DbType"] = databaseType,
            ["DbConnection:ConnectionConfigs:0:ConnectionString"] = connectionString,
            [PrecedenceMarkerKey] = bool.TrueString,
        });
    }

    private static string? FindArgumentValue(string[] args, string name)
    {
        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index].Trim();
            if (argument.Length == 0 || argument[0] != '-')
            {
                continue;
            }

            var keyPart = argument.TrimStart('-');
            string value;
            var equalsIndex = keyPart.IndexOf('=');
            if (equalsIndex >= 0)
            {
                value = keyPart[(equalsIndex + 1)..];
                keyPart = keyPart[..equalsIndex];
            }
            else
            {
                if (index + 1 >= args.Length)
                {
                    continue;
                }

                var nextArgument = args[index + 1].Trim();
                if (nextArgument.Length > 1 && nextArgument[0] == '-')
                {
                    continue;
                }

                value = nextArgument;
            }

            if (string.Equals(keyPart, name, StringComparison.OrdinalIgnoreCase))
            {
                return string.IsNullOrWhiteSpace(value) ? null : value;
            }
        }

        return null;
    }
}
