using GameFrameX.Core.Logging;
using GameFrameX.Foundation.Orm.Attribute;
using GameFrameX.Core.Const.MenuSeed;
using GameFrameX.Core.Extension.Cache;
using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Extension.Option;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Entity.Base;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.Log;
using GameFrameX.Entity.Tenant.ObjectStorage;
using GameFrameX.Foundation.Orm.Entity;
using Microsoft.Extensions.Hosting;
using MySqlConnector;

namespace GameFrameX.Core.SqlSugar;

public static class SqlSugarSetup
{
    /// <summary>
    /// SqlSugar 上下文初始化
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="hostEnvironment"></param>
    /// <param name="cache"></param>
    /// <param name="httpContextAccessor"></param>
    public static void AddSqlSugar(this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        ICache cache,
        IHttpContextAccessor httpContextAccessor)
    {
        // 注册雪花Id-支持分布式
        var effectiveTypes = ProjectTypeCatalog.EffectiveTypes;
        SqlSugarFilter.Configure(cache, httpContextAccessor, effectiveTypes);
        SqlSugarCache.Configure(cache);

        var snowFlakeOpt = configuration.GetSection("SnowFlake").Get<SnowFlakeIdOptions>() ?? new SnowFlakeIdOptions();
        services.AddSnowFlakeIdGenerator(snowFlakeOpt);
        // SqlSugar 雪花 ID 取号已进程级接管：Core.Extension 的 SqlSugarSnowflakeFallbackInitializer
        // （ModuleInitializer）安装 StaticConfig.CustomSnowFlakeFunc，覆盖未装配本类的路径（Hub / Client /
        // 测试进程）；此处不再接线 SnowFlakeSingle 内置 IdWorker（C361 / GFX-804）。

        var dbOptions = configuration.GetSection("DbConnection").Get<DbConnectionOptions>() ?? new DbConnectionOptions();
        dbOptions.PostConfigure(dbOptions, configuration);

        dbOptions.ConnectionConfigs.ForEach(SetDbConfig);


        var sqlSugar = new SqlSugarScope(dbOptions.ConnectionConfigs.Adapt<List<ConnectionConfig>>(), db =>
        {
            dbOptions.ConnectionConfigs.ForEach(config =>
            {
                var dbProvider = db.GetConnectionScope(config.ConfigId);
                SetDbAop(dbProvider, dbOptions.EnableConsoleSql, httpContextAccessor);
                SetDbDiffLog(dbProvider, config);
            });
        });
        services.AddSingleton<ISqlSugarClient>(sqlSugar); // 单例注册
        services.AddScoped(typeof(SqlSugarRepository<>)); // 仓储注册

        // 初始化数据库表结构及种子数据
        foreach (var config in dbOptions.ConnectionConfigs)
        {
            InitDatabase(sqlSugar, config, effectiveTypes);

        }
    }

    /// <summary>
    /// 配置连接属性
    /// </summary>
    /// <param name="config"></param>
    public static void SetDbConfig(DbConnectionConfig config)
    {
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config, new SqlSugarCache());
    }

    /// <summary>
    /// 配置Aop
    /// </summary>
    /// <param name="db"></param>
    /// <param name="enableConsoleSql"></param>
    public static void SetDbAop(SqlSugarScopeProvider db,
        bool enableConsoleSql,
        IHttpContextAccessor httpContextAccessor)
    {
        var config = db.CurrentConnectionConfig;

        // 设置超时时间
        db.Ado.CommandTimeOut = 30;

        // 打印SQL语句
        if (enableConsoleSql)
        {
            db.Aop.OnLogExecuting = (sql, pars) =>
            {
                var originColor = Console.ForegroundColor;
                if (sql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                if (sql.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase) || sql.StartsWith("INSERT", StringComparison.OrdinalIgnoreCase))
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }

                if (sql.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }

                Console.WriteLine("【" + DateTime.Now + "——执行SQL】\r\n" + UtilMethods.GetSqlString(config.DbType, sql, pars) + "\r\n");
                Console.ForegroundColor = originColor;
            };
            db.Aop.OnError = ex =>
            {
                if (ex.Parametres == null)
                {
                    return;
                }

                var originColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                var pars = db.Utilities.SerializeObject(((SugarParameter[])ex.Parametres).ToDictionary(it => it.ParameterName, it => it.Value));
                Console.WriteLine("【" + DateTime.Now + "——错误SQL】\r\n" + UtilMethods.GetSqlString(config.DbType, ex.Sql, (SugarParameter[])ex.Parametres) + "\r\n");
                Console.ForegroundColor = originColor;
            };
            db.Aop.OnLogExecuted = (sql, pars) =>
            {
                // 执行时间超过5秒
                if (db.Ado.SqlExecutionTime.TotalSeconds > 5)
                {
                    var fileName = db.Ado.SqlStackTrace.FirstFileName; // 文件名
                    var fileLine = db.Ado.SqlStackTrace.FirstLine; // 行号
                    var firstMethodName = db.Ado.SqlStackTrace.FirstMethodName; // 方法名
                    var log = $"【所在文件名】：{fileName}\r\n【代码行数】：{fileLine}\r\n【方法名】：{firstMethodName}\r\n" + $"【sql语句】：{UtilMethods.GetSqlString(config.DbType, sql, pars)}";
                    var originColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine(log);
                    Console.ForegroundColor = originColor;
                }
            };
        }

        // 数据审计
        db.Aop.DataExecuting = (oldValue, entityInfo) =>
        {
            SqlSugarAopHelper.HandleBaseAudit(oldValue, entityInfo);

            if (entityInfo.OperationType == DataFilterType.InsertByObject)
            {
                var user = httpContextAccessor.HttpContext?.User;
                if (user != null)
                {
                    if (entityInfo.PropertyName == nameof(EntityBasic.CreatedId))
                    {
                        var createUserId = ((dynamic)entityInfo.EntityValue).CreatedId;
                        if (createUserId == 0 || createUserId == null)
                        {
                            entityInfo.SetValue(user.FindFirst(ClaimConst.UserId)?.Value);
                        }
                    }
                    else if (entityInfo.PropertyName == nameof(EntityBasic.CreatedName))
                    {
                        var createUserName = ((dynamic)entityInfo.EntityValue).CreatedName;
                        if (string.IsNullOrEmpty(createUserName))
                        {
                            entityInfo.SetValue(GetCurrentUserDisplayName(httpContextAccessor));
                        }
                    }

                }
            }

            if (entityInfo.OperationType == DataFilterType.UpdateByObject)
            {
                if (entityInfo.PropertyName == nameof(EntityBasic.UpdatedId))
                {
                    entityInfo.SetValue(httpContextAccessor.HttpContext?.User.FindFirst(ClaimConst.UserId)?.Value);
                }
                else if (entityInfo.PropertyName == nameof(EntityBasic.UpdatedName))
                {
                    entityInfo.SetValue(GetCurrentUserDisplayName(httpContextAccessor));
                }
            }
        };

        // 超管排除其他过滤器
        if (httpContextAccessor.HttpContext?.User.FindFirst(ClaimConst.AccountType)?.Value == ((int)AccountTypeEnum.SuperAdmin).ToString())
        {
            return;
        }

        // 配置假删除过滤器
        SqlSugarUnifiedConfig.ConfigureBaseFilters(db);

        // 配置 启用/禁用过滤器（ISafeEnabledFilter）。
        SqlSugarUnifiedConfig.ConfigureClientFilters(db);

        // 主库同时承载角色和用户数据范围。
        SqlSugarFilter.SetDataScopeFilter(db);

    }

    /// <summary>
    /// 开启主库差异日志，使用 SystemLogDiffEntity 保存。
    /// </summary>
    /// <param name="db">当前操作库连接（差异日志写入目标）</param>
    /// <param name="config">当前操作库配置（提供 DbType 用于 SQL 还原）</param>
    private static void SetDbDiffLog(SqlSugarScopeProvider db, DbConnectionConfig config)
    {
        if (!config.DbSettings.EnableDiffLog)
        {
            return;
        }

        // SqlSugar 事件为同步 Action；必须写完审计后再返回，避免 async void 与下一条业务命令争用连接。
        db.Aop.OnDiffLogEvent = u =>
        {
            // 默认保留 SqlSugar 原始数据；UpdateAsync 时裁剪为只含变化字段
            var beforeList = u.BeforeData;
            var afterList = u.AfterData;
            string? snapshot = null;

            if (u.DiffType == DiffType.update && beforeList != null && afterList != null)
            {
                // UpdateAsync：完整 AfterData 入快照，BeforeData/AfterData 仅保留变化字段
                snapshot = Json.Serialize(afterList);
                var changedMap = BuildChangedKeysByTable(beforeList, afterList);
                beforeList = FilterDiffTables(beforeList, changedMap);
                afterList = FilterDiffTables(afterList, changedMap);
            }
            else if (u.DiffType == DiffType.insert && afterList != null)
            {
                // Insert：保留全字段，并写入快照
                snapshot = Json.Serialize(afterList);
            }
            // DeleteAsync：保留 BeforeData 全字段，不加快照（无 AfterData）

            var logDiff = new SystemLogDiffEntity();
            FillDiffLogFields(logDiff, u, config, beforeList, afterList, snapshot);
            db.Insertable(logDiff).ExecuteCommand();
            var serialized = Json.Serialize(logDiff);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(DateTime.Now + $"\r\n*****差异日志开始*****\r\n{Environment.NewLine}{serialized}{Environment.NewLine}*****差异日志结束*****\r\n");
        };
    }

    /// <summary>
    /// 填充差异日志实体公共字段。SystemLogDiffEntity 与 TenantLogDiffEntity 共享 <see cref="LogDiffFields"/> 字段，
    /// 调用方按当前操作库归属决定具体实体类型与写入连接。
    /// </summary>
    private static void FillDiffLogFields(LogDiffFields target, DiffLogModel u, DbConnectionConfig config, List<DiffLogTableInfo>? beforeList, List<DiffLogTableInfo>? afterList, string? snapshot)
    {
        // 操作后记录（UpdateAsync 时仅变化字段；Insert/DeleteAsync 时全字段）
        target.AfterData = SensitiveLogRedactor.Redact(Json.Serialize(afterList));
        // 操作前记录（UpdateAsync 时仅变化字段；Insert/DeleteAsync 时全字段）
        target.BeforeData = SensitiveLogRedactor.Redact(Json.Serialize(beforeList));
        // 全字段快照（仅 Insert/UpdateAsync 写入）
        target.Snapshot = SensitiveLogRedactor.Redact(snapshot);
        // 传进来的对象
        target.BusinessData = SensitiveLogRedactor.Redact(Json.Serialize(u.BusinessData));
        // 枚举（insert、update、delete）
        target.DiffType = u.DiffType.ToString();
        target.Sql = SensitiveLogRedactor.Mask;
        target.Parameters = SensitiveLogRedactor.Mask;
        target.Elapsed = u.Time == null ? 0 : (long)u.Time.Value.TotalMilliseconds;
    }

    /// <summary>
    /// 规范化字段值用于对比：null 与空字符串区分，避免误判为相等
    /// </summary>
    /// <param name="value">字段原始值</param>
    /// <returns>规范化后的字符串表示</returns>
    private static string NormalizeDiffValue(object? value)
    {
        if (value == null)
        {
            return "\0NULL";
        }

        return value.ToString() ?? string.Empty;
    }

    /// <summary>
    /// 按"表名 → 变化列名集合"提取所有发生变化的列
    /// </summary>
    /// <param name="before">操作前所有表的数据</param>
    /// <param name="after">操作后所有表的数据</param>
    /// <returns>表名到变化列名集合的映射</returns>
    private static Dictionary<string, HashSet<string>> BuildChangedKeysByTable(List<DiffLogTableInfo> before, List<DiffLogTableInfo> after)
    {
        var result = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
        foreach (var a in after)
        {
            var b = before.FirstOrDefault(x => x.TableName == a.TableName);
            var beforeDict = (b?.Columns ?? new List<DiffLogColumnInfo>())
                .ToDictionary(c => c.ColumnName, c => NormalizeDiffValue(c.Value));
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var col in a.Columns ?? new List<DiffLogColumnInfo>())
            {
                if (!beforeDict.TryGetValue(col.ColumnName, out var beforeNorm))
                {
                    // 操作前缺失的列 → 视为新增变化
                    keys.Add(col.ColumnName);
                    continue;
                }

                if (beforeNorm != NormalizeDiffValue(col.Value))
                {
                    keys.Add(col.ColumnName);
                }
            }

            result[a.TableName] = keys;
        }

        return result;
    }

    /// <summary>
    /// 按变化列集合筛选多张表的 Columns，返回仅包含变化列的新列表
    /// </summary>
    /// <param name="source">原始表数据列表</param>
    /// <param name="changedMap">表名到变化列名集合的映射</param>
    /// <returns>仅含变化列的新列表；源为空则返回 null</returns>
    private static List<DiffLogTableInfo>? FilterDiffTables(List<DiffLogTableInfo> source, Dictionary<string, HashSet<string>> changedMap)
    {
        if (source == null)
        {
            return null;
        }

        var result = new List<DiffLogTableInfo>();
        foreach (var table in source)
        {
            if (!changedMap.TryGetValue(table.TableName, out var keys))
            {
                continue;
            }

            result.Add(new DiffLogTableInfo
            {
                TableName = table.TableName,
                TableDescription = table.TableDescription,
                Columns = (table.Columns ?? new List<DiffLogColumnInfo>())
                          .Where(c => keys.Contains(c.ColumnName))
                          .ToList(),
            });
        }

        return result;
    }

    /// <summary>
    /// 初始化数据库
    /// </summary>
    /// <param name="db"></param>
    /// <param name="config"></param>
    /// <param name="effectiveTypes"></param>
    internal static void InitDatabase(SqlSugarScope db, DbConnectionConfig config, IEnumerable<Type> effectiveTypes)
    {
        var dbProvider = db.GetConnectionScope(config.ConfigId);
        dbProvider.CurrentConnectionConfig = config;

        // Journal Mode 连接串关键字为 Microsoft.Data.Sqlite 6.0+ 专属，低版本包环境启动即报不支持；
        // 统一改用 PRAGMA 开启 WAL（WAL 持久存于库文件头，此处执行一次即对四个 host 长期生效）
        if (config.DbType == DbType.Sqlite)
        {
            // Microsoft.Data.Sqlite 打开连接建库文件时不会创建缺失的父目录，先按工作目录建好（如 ./db/）
            EnsureSqliteDataDirectory(config.ConnectionString);
            dbProvider.Ado.ExecuteCommand("PRAGMA journal_mode=WAL;");
        }

        // 初始化/创建数据库
        if (config.DbSettings.EnableInitDb)
        {
            // SqlSugar 5.1.4.x 的 SQLite CreateDatabase 用正则 \/.+\/ 从连接串抠目录，
            // DataSource=./db/xx.db 会被抠成绝对路径 /db/ 并尝试在文件系统根目录建目录（macOS 只读根卷启动即崩）；
            // SQLite 库文件随上方首次打开连接自动创建，跳过 SqlSugar 的建库调用
            if (config.DbType != DbType.Sqlite && config.DbType != DbType.Oracle)
            {
                dbProvider.DbMaintenance.CreateDatabase();
            }
        }

        // 初始化表结构
        if (config.TableSettings.EnableInitTable)
        {
            var entityTypes = effectiveTypes.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass && u.IsDefined(typeof(SugarTable), false))
                                            .WhereIF(config.TableSettings.EnableIncreTable, u => u.IsDefined(typeof(IncrementTableAttribute), false)).ToList();

            entityTypes = entityTypes.Where(SqlSugarEntityBoundary.IsMainDatabaseTable).ToList();

            foreach (var entityType in entityTypes)
            {
                try
                {
                    if (entityType.GetCustomAttribute<SplitTableAttribute>() == null)
                    {
                        dbProvider.CodeFirst.InitTables(entityType);
                    }
                    else
                    {
                        dbProvider.CodeFirst.SplitTables().InitTables(entityType);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[InitTables] 初始化表 {entityType.Name} 失败：{e.Message}");
                    throw;
                }
            }

        }

        // 初始化种子数据（C278：默认 insert-only 只补缺；带 [IncreSeed] 特性的权威收敛种子保留覆盖更新，
        // 如 GlobalMenuSeedData——global_menu.path/component 以其 upsert 为唯一权威纠正源；
        // EnableIncreSeed=true 时收窄为只执行权威收敛种子）
        if (config.SeedSettings.EnableInitSeed)
        {
            var seedDataTypes = effectiveTypes.Where(u => !u.IsInterface && !u.IsAbstract && u.IsClass && u.GetInterfaces().Any(i => i.HasImplementedRawGeneric(typeof(ISqlSugarEntitySeedData<>))))
                                              .WhereIF(config.SeedSettings.EnableIncreSeed, u => u.IsDefined(typeof(IncrementSeedAttribute), false)).ToList();

            // C331（GFX-741）：当前 global_menu 种子 Key→Id 映射，供主库启动自愈清理种子演化残留行
            // （旧 Key 残留行与同 Key 异 Id 重复行两类，见 MenuSeedStaleRowCleaner）
            Dictionary<string, long>? globalMenuSeedIdsByKey = null;
            foreach (var seedType in seedDataTypes)
            {
                var entityType = seedType.GetInterfaces().First().GetGenericArguments().First();
                _ = SqlSugarEntityBoundary.GetOwnership(entityType);

                var instance = Activator.CreateInstance(seedType);
                var hasDataMethod = seedType.GetMethod("HasData");
                var seedData = ((IEnumerable?)hasDataMethod?.Invoke(instance, null))?.Cast<object>();
                if (seedData == null)
                {
                    continue;
                }

                var seedItems = seedData.ToList();
                if (seedItems.Count == 0)
                {
                    continue;
                }

                if (entityType == typeof(GlobalMenuEntity))
                {
                    globalMenuSeedIdsByKey = seedItems.Cast<GlobalMenuEntity>()
                                                      .Where(u => !string.IsNullOrWhiteSpace(u.Key))
                                                      .GroupBy(u => u.Key!, StringComparer.Ordinal)
                                                      .ToDictionary(u => u.Key, u => u.First().Id, StringComparer.Ordinal);
                }

                var entityInfo = dbProvider.EntityMaintenance.GetEntityInfo(entityType);
                var primaryKeyColumns = entityInfo.Columns.Where(u => u.IsPrimarykey).ToList();
                if (primaryKeyColumns.Count > 0)
                {
                    // 权威收敛种子（[IncreSeed]）按主键 insert + update 覆盖收敛；普通种子 insert-only 只补缺，
                    // 不覆盖用户已修改的已有行（临时禁用查询过滤器，避免软删除等过滤器导致 upsert 找不到已有记录）
                    var isAuthoritativeSeed = seedType.IsDefined(typeof(IncrementSeedAttribute), false);
                    try
                    {
                        dbProvider.QueryFilter.Clear();
                        var storageable = dbProvider.StorageableByObject(seedItems);
                        storageable.WhereColumns(primaryKeyColumns.Select(u => u.PropertyName).ToArray());
                        var storage = storageable.ToStorage();
                        var insertedRowCount = storage.AsInsertable.ExecuteCommand();
                        if (isAuthoritativeSeed)
                        {
                            storage.AsUpdateable.ExecuteCommand();
                        }

                        // 仅本次真正插入数据库（行此前不存在）才回调（首装语义）：实现 ISeedDataInsertCallback 的种子类
                        // （如 SystemUserSeedData 打印随机初始密码）不会在重启等重复初始化时再次输出未生效内容
                        if (insertedRowCount > 0 && instance is ISeedDataInsertCallback seedInsertCallback)
                        {
                            seedInsertCallback.OnSeedRowsInserted(insertedRowCount);
                        }
                    }
                    finally
                    {
                        // 恢复查询过滤器
                        SqlSugarUnifiedConfig.ConfigureBaseFilters(dbProvider);
                    }
                }
                else
                {
                    // 无主键则只进行插入
                    if (!dbProvider.Queryable(entityInfo.DbTableName, entityInfo.DbTableName).Any())
                    {
                        dbProvider.InsertableByObject(seedItems).ExecuteCommand();
                    }
                }
            }

            if (config.ConfigId.ToString() == SqlSugarConst.MainConfigId)
            {
                // 新装主库空配置时提供本地存储；已有配置（包括停用或软删）不覆盖。
                if (effectiveTypes.Contains(typeof(TenantObjectStorageConfigEntity))
                    && dbProvider.DbMaintenance.IsAnyTable(dbProvider.EntityMaintenance.GetEntityInfo<TenantObjectStorageConfigEntity>().DbTableName, false))
                {
                    DefaultTenantDatabaseSeeder.SeedDefaultObjectStorageConfigIfEmpty(dbProvider);
                }

                // C117 (GFX-117)：8 棵游戏业务菜单路径已收敛到新前缀（appCenter/playerCenter/goodsCenter/
                // redeemCenter/webShopCenter/contentCenter/opsCenter/automationCenter），旧 /game/<子域> 兼容迁移兜底已移除。
                // global_menu.path / component 由 GlobalMenuSeedData upsert 作为唯一权威纠正源；
                // dev/test 若被旧 seed 残留 / role-menu 派生数据阻碍一次性收敛，直接清库由 CodeFirst + seed 完整重建。
                // C331（GFX-741）：实体改名后旧 Key 行不再被按主键 upsert 命中而残留，与新 Key 行同 Permission 并存
                // 会让 ValidateDbMenus 启动崩溃；加载前先软删「非当前种子且非 permission:auto 运行期行」的自愈清理。
                // 同因扩展：旧布局 Id 行（StableSeedId 紧凑布局切换前）同样不被按主键 upsert 命中，与新权威 Id 行
                // 同 Key 并存触发「global_menu.Key 重复」启动崩溃；一并按 Key→权威 Id 映射清理。
                if (globalMenuSeedIdsByKey != null && globalMenuSeedIdsByKey.Count > 0)
                {
                    var removedMenuKeys = MenuSeedStaleRowCleaner.RemoveStaleRows(dbProvider, globalMenuSeedIdsByKey);
                    if (removedMenuKeys.Count > 0)
                    {
                        // static Log 未接线（NullLogger 会吞警告），启动期运维可见输出走 Console（同 HostMainDbConnectionResolver 先例）
                        Console.WriteLine($"[Warning] global_menu 种子演化残留自愈清理：软删除 {removedMenuKeys.Count} 行（Key 不在当前种子或 Key 命中种子但 Id 漂移的重复行，均非 permission:auto 运行期行）：{string.Join(", ", removedMenuKeys)}。");
                    }
                }

                // seed upsert 完成后，从 DB 加载运行期菜单缓存。
                MenuSeedCatalog.LoadFromDb(dbProvider);
            }
        }
    }

    /// <summary>
    /// 确保 SQLite 连接串中 DataSource 指向文件的父目录存在（相对路径按进程工作目录解析，与 Microsoft.Data.Sqlite 一致）。
    /// </summary>
    private static void EnsureSqliteDataDirectory(string connectionString)
    {
        foreach (var segment in connectionString.Split(';'))
        {
            var separatorIndex = segment.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = segment[..separatorIndex].Trim();
            if (!string.Equals(key, "DataSource", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(key, "Data Source", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var dataSource = segment[(separatorIndex + 1)..].Trim();
            if (dataSource.Contains(":memory:"))
            {
                return;
            }

            var directory = Path.GetDirectoryName(Path.GetFullPath(dataSource));
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return;
        }
    }

    private static string? GetCurrentUserDisplayName(IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;
        var realName = user?.FindFirst(ClaimConst.RealName)?.Value;
        if (!string.IsNullOrWhiteSpace(realName))
        {
            return realName;
        }

        return user?.FindFirst(ClaimConst.Account)?.Value;
    }
}