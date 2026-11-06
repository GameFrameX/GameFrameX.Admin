using GameFrameX.Core.Option;

namespace GameFrameX.Core.Bootstrap;

/// <summary>
/// 旧库拒绝启动守卫（C406 / D7）：Web.Api 初始化数据库（建表、种子）之前的 fail-closed 检查。
/// <para>2.0 为破坏性断代版本，不迁移旧数据：检测到旧 schema 标志（任一 <c>tenant_</c> 前缀表，或旧租户登记表
/// <c>system_tenant_entity</c>）即抛 <see cref="InvalidOperationException"/> 终止启动——不建表、不写种子、不写数据， // c406-keep
/// 只能用新库或整体回滚。</para>
/// </summary>
public static class LegacySchemaStartupGuard
{
    /// <summary>
/// 旧租户登记表物理名（1.x SystemTenantEntity，[PlatformTable] 派生；租户登记机制已删除，新代码永不创建）。 // c406-keep
    /// </summary>
    // c406-keep：旧 schema 标志字面量，有意引用旧表名（R1-d / R3-b），改名脚本不得改写
    private const string LegacyRegistryTable = "system_tenant_entity"; // c406-keep

    /// <summary>
    /// 旧租户业务表物理名前缀。
    /// </summary>
    // c406-keep：旧 schema 标志字面量，有意引用旧表名（R1-d / R3-b），改名脚本不得改写
    private const string LegacyTablePrefix = "tenant_"; // c406-keep

    /// <summary>
    /// 检查目标库不是旧 schema；命中旧标志即抛 <see cref="InvalidOperationException"/>。
    /// <para>新装库尚不存在时先幂等建库（与 <c>SqlSugarSetup.InitDatabase</c> 同条件跳过 SQLite/Oracle），
    /// 保证 PostgreSQL/MySQL 首装场景在库不存在时按空库放行，而不是误报连接失败。</para>
    /// </summary>
    /// <param name="database">SqlSugar 作用域客户端。</param>
    /// <param name="config">目标连接配置。</param>
    public static void EnsureNotLegacySchema(SqlSugarScope database, DbConnectionConfig config)
    {
        var dbProvider = database.GetConnectionScope(config.ConfigId);

        if (config.DbSettings.EnableInitDb && config.DbType != DbType.Sqlite && config.DbType != DbType.Oracle)
        {
            dbProvider.DbMaintenance.CreateDatabase();
        }

        var legacyTables = dbProvider.DbMaintenance.GetTableInfoList(false)
                                     .Select(table => table.Name)
                                     .Where(IsLegacyTableName)
                                     .OrderBy(name => name, StringComparer.Ordinal)
                                     .ToList();

        if (legacyTables.Count > 0)
        {
            var examples = string.Join("、", legacyTables.Take(3));
            throw new InvalidOperationException(
                $"检测到旧版多租户数据库架构（示例表：{examples}，共 {legacyTables.Count} 张）。2.0 不支持旧库升级，只能用新库或整体回滚。");
        }
    }

    /// <summary>
    /// 判断表名是否为旧 schema 标志：tenant_ 前缀表或旧租户登记表（均忽略大小写）。
    /// </summary>
    /// <param name="tableName">物理表名。</param>
    /// <returns>命中旧标志返回 true。</returns>
    private static bool IsLegacyTableName(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            return false;
        }

        return tableName.StartsWith(LegacyTablePrefix, StringComparison.OrdinalIgnoreCase)
               || string.Equals(tableName, LegacyRegistryTable, StringComparison.OrdinalIgnoreCase);
    }
}
