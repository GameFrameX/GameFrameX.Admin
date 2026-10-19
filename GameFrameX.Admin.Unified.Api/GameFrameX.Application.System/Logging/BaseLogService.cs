using System.Linq.Expressions;
using System.Reflection;
using GameFrameX.Application.System.Logging.Dto;
using GameFrameX.Core.Service.User;
using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Application.System.Logging;

/// <summary>
/// 系统日志服务基类，提供统一的日志查询、清空和导出功能。
/// C156 起：每种日志拆为平台 <typeparamref name="TSystemLog"/>（主库 system_log_*）与租户 <typeparamref name="TTenantLog"/>（租户库 tenant_log_*）两套实体，
/// 查询/清空/导出按当前登录身份分流——SuperAdmin 访问平台日志主库，普通租户访问当前租户库的租户日志。
/// <typeparamref name="TDto"/> 为统一投影目标（共享字段基类），保证前端字段形状不随实体拆分而变化。
/// </summary>
/// <typeparam name="TSystemLog">平台日志实体（[PlatformTable]，主库）</typeparam>
/// <typeparam name="TTenantLog">租户日志实体（[TenantTable]，当前租户库）</typeparam>
/// <typeparam name="TDto">日志投影 DTO，字段与实体共享基类一致</typeparam>
public abstract class BaseLogService<TSystemLog, TTenantLog, TDto> : IAdminApiController
    where TSystemLog : EntityTenantBase, new()
    where TTenantLog : EntityTenantBase, new()
    where TDto : class, new()
{
    /// <summary>
    /// 平台日志仓储（主库 system_log_*）
    /// </summary>
    protected readonly SqlSugarRepository<TSystemLog> SystemLogRepository;

    /// <summary>
    /// 租户日志仓储（当前租户库 tenant_log_*）
    /// </summary>
    protected readonly SqlSugarRepository<TTenantLog> TenantLogRepository;

    /// <summary>
    /// 当前登录用户，用于判定 SuperAdmin 平台上下文。
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
    /// 当前是否为平台上下文（SuperAdmin 访问主库平台日志）。普通租户上下文访问当前租户库的租户日志。
    /// </summary>
    protected bool IsPlatformContext
    {
        get { return CurrentUserContext.SuperAdmin; }
    }

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
        return IsPlatformContext
                   ? await QueryPagedAsync(SystemLogRepository, input, timestampRange)
                   : await QueryPagedAsync(TenantLogRepository, input, timestampRange);
    }

    /// <summary>
    /// 清空日志。平台上下文清空主库平台日志，租户上下文清空当前租户库的租户日志。
    /// </summary>
    /// <returns>清空结果</returns>
    [ApiDescriptionSettings(Name = "Clear")]
    [HttpPost]
    [DisplayName("清空日志")]
    public virtual async Task<bool> ClearAsync()
    {
        var count = IsPlatformContext
                        ? await SystemLogRepository.AsDeleteable().Where(u => u.Id > 0).ExecuteCommandAsync()
                        : await TenantLogRepository.AsDeleteable().Where(u => u.Id > 0).ExecuteCommandAsync();
        return count > 0;
    }

    /// <summary>
    /// 构建导出用日志查询（按时间范围降序），返回 ExportLogResponse 投影列表。
    /// </summary>
    /// <param name="input">导出参数</param>
    /// <returns>导出 DTO 列表</returns>
    protected async Task<List<ExportLogResponse>> QueryExportListAsync<TLog>(SqlSugarRepository<TLog> repo, LogRequest input)
        where TLog : EntityTenantBase, new()
    {
        var timestampRange = ConvertToTimestampRange(input);
        return await repo.AsQueryable()
                         .WhereIF(input.StartTime.HasValue && input.EndTime.HasValue,
                                  u => u.CreatedTime >= timestampRange.StartTimestamp && u.CreatedTime <= timestampRange.EndTimestamp)
                         .OrderBy(u => u.CreatedTime, OrderByType.Desc)
                         .Select<ExportLogResponse>().ToListAsync();
    }

    /// <summary>
    /// 按时间范围与分页参数查询日志，返回 <typeparamref name="TDto"/> 投影分页。
    /// </summary>
    private async Task<SqlSugarPagedList<TDto>> QueryPagedAsync<TLog>(SqlSugarRepository<TLog> repo, PageLogRequest input, LogTimestampRange timestampRange)
        where TLog : EntityTenantBase, new()
    {
        var searchKey = input.SearchKey?.Trim();
        return await repo.AsQueryable()
                         .WhereIF(input.StartTime.HasValue, u => u.CreatedTime >= timestampRange.StartTimestamp)
                         .WhereIF(input.EndTime.HasValue, u => u.CreatedTime <= timestampRange.EndTimestamp)
                         .WhereIF(!string.IsNullOrWhiteSpace(searchKey) && SearchableTextColumns.Count > 0,
                                  BuildSearchKeyFilter<TLog>(searchKey!))
                         .OrderBuilder(input)
                         .Select<TDto>()
                         .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 按 <see cref="SearchableTextColumns"/> 拼接 OR Contains 表达式，用于 SearchKey 模糊匹配。
    /// 表达式树按 <c>u =&gt; u.Col1.Contains(key) OR u.Col2.Contains(key) OR ...</c> 形式构建，
    /// 所有节点均为简单属性访问 + <see cref="string.Contains(string)"/> + 单一 <paramref name="searchKey"/> 参数，
    /// SqlSugar 可正确翻译为 SQL（参考 4952fbe6 修复租户用户/角色列表 SqlSugar NotSupportedException 的教训：
    /// lambda 内禁止 <c>?? new List</c> 等不可翻译的兜底写法）。
    /// </summary>
    /// <param name="searchKey">已 trim 的非空关键字</param>
    private Expression<Func<TLog, bool>> BuildSearchKeyFilter<TLog>(string searchKey) where TLog : EntityTenantBase, new()
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