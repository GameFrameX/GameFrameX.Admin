using GameFrameX.Core.Logging;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Enum;
using System.Linq.Expressions;
using System.Reflection;
using GameFrameX.Application.Audit.Logging.Dto;
using GameFrameX.Core.Service.User;
using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Application.Audit.Logging;

/// <summary>
/// 系统日志服务基类，提供统一的日志查询、清空和导出功能。
/// 查询、清空和导出统一覆盖主库新日志与迁入主库的历史日志。
/// <typeparamref name="TDto"/> 为统一投影目标（共享字段基类），保证前端字段形状不随实体拆分而变化。
/// </summary>
/// <typeparam name="TSystemLog">主库新日志实体</typeparam>
/// <typeparam name="TTenantLog">主库历史日志实体</typeparam>
/// <typeparam name="TDto">日志投影 DTO，字段与实体共享基类一致</typeparam>
public abstract class BaseLogService<TSystemLog, TTenantLog, TDto> : IAdminApiController
    where TSystemLog : EntityBase, new()
    where TTenantLog : EntityBase, new()
    where TDto : class, new()
{
    /// <summary>
    /// 平台日志仓储（主库 system_log_*）
    /// </summary>
    protected readonly SqlSugarRepository<TSystemLog> SystemLogRepository;

    /// <summary>
    /// 租户日志仓储（主库 tenant_log_* 历史表）
    /// </summary>
    protected readonly SqlSugarRepository<TTenantLog> TenantLogRepository;

    /// <summary>
    /// 当前登录用户。
    /// </summary>
    protected readonly CurrentUserContext CurrentUserContext;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="systemLogRepository">平台日志仓储</param>
    /// <param name="tenantLogRepository">租户日志仓储</param>
    /// <param name="currentUserContext">当前登录用户</param>
    protected BaseLogService(SqlSugarRepository<TSystemLog> systemLogRepository, SqlSugarRepository<TTenantLog> tenantLogRepository, CurrentUserContext currentUserContext)
    {
        SystemLogRepository = systemLogRepository;
        TenantLogRepository = tenantLogRepository;
        CurrentUserContext = currentUserContext;
    }

    /// <summary>
    /// 日志显示名称，用于导出文件名等场景
    /// </summary>
    protected abstract string LogDisplayName { get; }

    /// <summary>
    /// 可被 <c>SearchKey</c> 模糊匹配的实体列名集合（不区分大小写，OR Contains 关系）。
    /// 子类按各自业务域的文本列声明；默认空集合（基类不直接实例化）。
    /// 仅参与过滤的列名（典型用户搜索列），不纳入 BigString 大字段以避免全表扫描恶化。
    /// </summary>
    protected virtual IReadOnlyCollection<string> SearchableTextColumns { get; } = Array.Empty<string>();

    /// <summary>
    /// 获取日志分页列表
    /// </summary>
    /// <param name="input">分页查询参数</param>
    /// <returns>分页日志列表</returns>
    [SuppressMonitor]
    [DisplayName("获取日志分页列表")]
    [ApiDescriptionSettings(Name = "Page")]
    public virtual async Task<SqlSugarPagedList<TDto>> PageAsync(PageLogRequest input)
    {
        var timestampRange = ConvertToTimestampRange(input);
        var current = QueryFiltered(SystemLogRepository, input, timestampRange).Select<TDto>();
        var historical = QueryFiltered(TenantLogRepository, input, timestampRange).Select<TDto>();
        var page = await SystemLogRepository.Context.UnionAll(current, historical)
                                        .OrderBy(BuildProjectionOrder(input))
                                        .ToPagedListAsync(input.Page, input.PageSize);
        foreach (var item in page.Items)
        {
            RedactSnapshot(item);
        }

        return page;
    }

    private string BuildProjectionOrder(PageLogRequest input)
    {
        // UNION 结果列使用投影属性别名，不能再映射成原表的下划线列名。
        var property = typeof(TDto).GetProperties().FirstOrDefault(candidate =>
            candidate.Name.Equals(input.OrderBy, StringComparison.OrdinalIgnoreCase));
        var propertyName = property?.Name ?? "Id";
        // PostgreSQL provider 默认将 SELECT 投影别名转为小写。
        if (SystemLogRepository.Context.CurrentConnectionConfig.DbType == DbType.PostgreSQL)
        {
            propertyName = propertyName.ToLowerInvariant();
        }

        var descending = property == null || input.IsDescending;
        var quotedName = SystemLogRepository.Context.CurrentConnectionConfig.DbType == DbType.MySql
            ? $"`{propertyName}`" : $"\"{propertyName}\"";
        return $"{quotedName} {(descending ? "DESC" : "ASC")}";
    }

    /// <summary>
    /// 清空主库新日志和历史日志。
    /// </summary>
    /// <returns>清空结果</returns>
    [ApiDescriptionSettings(Name = "Clear")]
    [HttpPost]
    [DisplayName("清空日志")]
    public virtual async Task<bool> ClearAsync()
    {
        var count = await SystemLogRepository.AsDeleteable().Where(u => u.Id > 0).ExecuteCommandAsync();
        count += await TenantLogRepository.AsDeleteable().Where(u => u.Id > 0).ExecuteCommandAsync();
        return count > 0;
    }

    /// <summary>
    /// 构建导出用日志查询（按时间范围降序），返回 ExportLogResponse 投影列表。
    /// </summary>
    /// <param name="input">导出参数</param>
    /// <returns>导出 DTO 列表</returns>
    protected async Task<List<ExportLogResponse>> QueryExportListAsync(LogRequest input)
    {
        var timestampRange = ConvertToTimestampRange(input);
        var pageInput = new PageLogRequest { StartTime = input.StartTime, EndTime = input.EndTime };
        var current = QueryFiltered(SystemLogRepository, pageInput, timestampRange).Select<ExportLogResponse>();
        var historical = QueryFiltered(TenantLogRepository, pageInput, timestampRange).Select<ExportLogResponse>();
        var rows = await SystemLogRepository.Context.UnionAll(current, historical)
                                        .OrderBy(u => u.LogDateTime, OrderByType.Desc)
                                        .ToListAsync();
        foreach (var row in rows)
        {
            RedactSnapshot(row);
        }

        return rows;
    }

    /// <summary>
    /// 按时间范围与分页参数查询日志，返回 <typeparamref name="TDto"/> 投影分页。
    /// </summary>
    private ISugarQueryable<TLog> QueryFiltered<TLog>(SqlSugarRepository<TLog> repo, PageLogRequest input, LogTimestampRange timestampRange)
        where TLog : EntityBase, new()
    {
        var searchKey = input.SearchKey?.Trim();
        return repo.AsQueryable()
                         .Where(BuildVisibilityFilter<TLog>())
                         .WhereIF(input.StartTime.HasValue, u => u.CreatedTime >= timestampRange.StartTimestamp)
                         .WhereIF(input.EndTime.HasValue, u => u.CreatedTime <= timestampRange.EndTimestamp)
                         .WhereIF(!string.IsNullOrWhiteSpace(searchKey) && SearchableTextColumns.Count > 0,
                                  BuildSearchKeyFilter<TLog>(searchKey!));
    }

    private Expression<Func<TLog, bool>> BuildVisibilityFilter<TLog>() where TLog : EntityBase, new()
    {
        if (CurrentUserContext.SuperAdmin)
        {
            return row => true;
        }

        // 包含软删账号，防止账号退场后旧日志重新对普通运营可见。
        var administrators = SystemLogRepository.Context.Queryable<SystemUserEntity>()
            .ClearFilter().Where(user => user.AccountType == AccountTypeEnum.SuperAdmin)
            .Select(user => new { user.Id, user.Account }).ToList();
        var identities = administrators.Select(user => (long?)user.Id).ToArray();
        var accounts = administrators.Select(user => user.Account).Append("superadmin").Distinct().ToArray();
        var parameter = Expression.Parameter(typeof(TLog), "row");
        var createdId = Expression.Convert(Expression.Property(parameter, nameof(EntityBase.CreatedId)), typeof(long?));
        var containsIdentity = Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), new[] { typeof(long?) }, Expression.Constant(identities), createdId);
        Expression body = Expression.OrElse(Expression.Equal(createdId, Expression.Constant(null, typeof(long?))), Expression.Not(containsIdentity));
        var accountProperty = typeof(TLog).GetProperty("Account");
        if (accountProperty != null)
        {
            var account = Expression.Property(parameter, accountProperty);
            var containsAccount = Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), new[] { typeof(string) }, Expression.Constant(accounts), account);
            body = Expression.AndAlso(body, Expression.OrElse(Expression.Equal(account, Expression.Constant(null, typeof(string))), Expression.Not(containsAccount)));
        }

        return Expression.Lambda<Func<TLog, bool>>(body, parameter);
    }

    private static void RedactSnapshot(object item)
    {
        var snapshot = global::System.Text.Json.Nodes.JsonNode.Parse(SensitiveLogRedactor.Redact(global::System.Text.Json.JsonSerializer.Serialize(item))!)!;
        foreach (var property in item.GetType().GetProperties().Where(property => property.CanWrite && property.PropertyType == typeof(string)))
        {
            property.SetValue(item, item is GameFrameX.Entity.Log.LogDiffFields && property.Name is "Sql" or "Parameters"
                ? SensitiveLogRedactor.Mask : snapshot[property.Name]?.GetValue<string>());
        }
    }

    /// <summary>
    /// 按 <see cref="SearchableTextColumns"/> 拼接 OR Contains 表达式，用于 SearchKey 模糊匹配。
    /// 表达式树按 <c>u =&gt; u.Col1.Contains(key) OR u.Col2.Contains(key) OR ...</c> 形式构建，
    /// 所有节点均为简单属性访问 + <see cref="string.Contains(string)"/> + 单一 <paramref name="searchKey"/> 参数，
    /// SqlSugar 可正确翻译为 SQL（参考 4952fbe6 修复租户用户/角色列表 SqlSugar NotSupportedException 的教训：
    /// lambda 内禁止 <c>?? new List</c> 等不可翻译的兜底写法）。
    /// </summary>
    /// <param name="searchKey">已 trim 的非空关键字</param>
    private Expression<Func<TLog, bool>> BuildSearchKeyFilter<TLog>(string searchKey) where TLog : EntityBase, new()
    {
        var parameter = Expression.Parameter(typeof(TLog), "u");
        Expression? body = null;
        var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;
        var searchKeyConstant = Expression.Constant(searchKey, typeof(string));
        foreach (var columnName in SearchableTextColumns)
        {
            var property = typeof(TLog).GetProperty(columnName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property == null || property.PropertyType != typeof(string))
            {
                continue;
            }

            var propertyAccess = Expression.Property(parameter, property);
            // u.Col.Contains(searchKey) —— SqlSugar 对 string.Contains(string) 可翻译；空值由 SQL NULL 语义处理。
            var containsCall = Expression.Call(propertyAccess, containsMethod, searchKeyConstant);
            body = body == null ? containsCall : Expression.OrElse(body, containsCall);
        }

        if (body == null)
        {
            // 防御性：调用方已用 SearchableTextColumns.Count > 0 保护，正常不会命中；命中时退化为恒真避免空表达式导致 LINQ 抛错。
            var fallbackParam = Expression.Parameter(typeof(TLog), "u");
            var trueConstant = Expression.Constant(true, typeof(bool));
            return Expression.Lambda<Func<TLog, bool>>(trueConstant, fallbackParam);
        }

        return Expression.Lambda<Func<TLog, bool>>(body, parameter);
    }

    /// <summary>
    /// 将时间范围转换为时间戳范围
    /// </summary>
    /// <param name="input">包含时间范围的输入参数</param>
    /// <returns>开始时间戳和结束时间戳范围</returns>
    protected LogTimestampRange ConvertToTimestampRange(PageLogRequest input)
    {
        var startTimestamp = input.StartTime ?? 0;
        var endTimestamp = input.EndTime ?? long.MaxValue;
        return new LogTimestampRange(startTimestamp, endTimestamp);
    }

    /// <summary>
    /// 将时间范围转换为时间戳范围（用于导出功能）
    /// </summary>
    /// <param name="input">包含时间范围的输入参数</param>
    /// <returns>开始时间戳和结束时间戳范围</returns>
    protected LogTimestampRange ConvertToTimestampRange(LogRequest input)
    {
        var startTimestamp = input.StartTime ?? 0;
        var endTimestamp = input.EndTime ?? long.MaxValue;
        return new LogTimestampRange(startTimestamp, endTimestamp);
    }
}