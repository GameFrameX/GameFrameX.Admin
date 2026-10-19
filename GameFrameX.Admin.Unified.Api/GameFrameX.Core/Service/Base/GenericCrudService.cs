using GameFrameX.Core.Export;
using GameFrameX.Entity.Base;

namespace GameFrameX.Core.Service.Base;

/// <summary>
/// 通用 CRUD 服务基类，提供默认的 PageAsync/AddAsync/UpdateAsync/Get 实现。
/// 子类可 override 任意方法来自定义行为。
/// </summary>
[ApiDescriptionSettings(Order = 100)]
public abstract class GenericCrudService<TEntity, TAddRequest, TUpdateRequest, TResponse, TPageRequest>
    : BaseService<TEntity>
    where TEntity : EntityBasic, new()
    where TAddRequest : BaseAddRequest
    where TUpdateRequest : ICrudUpdateRequest
    where TPageRequest : BasePageRequest
{
    protected GenericCrudService(SqlSugarRepository<TEntity> repository) : base(repository)
    {
    }

    /// <summary>
    /// 复杂入参 null 兜底（C391 / GFX-923）：body 为字面 null 等场景绑定不产生 ModelState 错误，
    /// 参数可能为 null——统一抛参数校验业务异常（400 语义），替代各 action 首次解引用时的 500 NRE。
    /// </summary>
    private static void EnsureInputNotNull(object input)
    {
        if (input is null)
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed);
        }
    }

    /// <summary>
    /// 分页查询
    /// </summary>
    [HttpPost(CrudActionRoutes.Page)]
    [ApiDescriptionSettings(Name = "Page")]
    public virtual async Task<SqlSugarPagedList<TResponse>> PageAsync(TPageRequest input)
    {
        EnsureInputNotNull(input);
        var query = BuildPageQuery(input);
        query = ApplyOrdering(query, input);
        return await query.ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 默认排序字段名，子类可 override
    /// </summary>
    protected virtual string DefaultSortField
    {
        get { return "CreatedTime"; }
    }

    /// <summary>
    /// 应用排序，子类可 override 自定义排序逻辑
    /// </summary>
    protected virtual ISugarQueryable<TResponse> ApplyOrdering(ISugarQueryable<TResponse> query, TPageRequest input)
    {
        if (!string.IsNullOrEmpty(input.OrderBy))
        {
            var allowedProps = typeof(TResponse).GetProperties().Select(p => p.Name)
                                                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (!allowedProps.Contains(input.OrderBy))
            {
                return string.IsNullOrEmpty(DefaultSortField)
                           ? query
                           : query.OrderByIF(true, $"{DefaultSortField} Desc");
            }

            query = query.OrderByIF(true, $"{input.OrderBy} {(input.IsDescending ? "Desc" : "Asc")}");
        }
        else if (!string.IsNullOrEmpty(DefaultSortField))
        {
            query = query.OrderByIF(true, $"{DefaultSortField} Desc");
        }

        return query;
    }

    /// <summary>
    /// 构建分页查询条件，子类可 override 添加自定义过滤。
    /// 默认实现不添加任何过滤条件。
    /// </summary>
    protected virtual ISugarQueryable<TResponse> BuildPageQuery(TPageRequest input)
    {
        return Repository.AsQueryable()
                         .Where(u => u.IsDeleted == false)
                         .Select<TResponse>();
    }

    /// <summary>
    /// 通用导出核心实现：复用既有 <see cref="BuildPageQuery"/>（已含租户/权限过滤）
    /// 与 <see cref="ApplyOrdering"/>，按列白名单投影生成 Excel。
    /// </summary>
    /// <remarks>
    /// 各业务 Service 通过显式 <c>Export</c> 端点 opt-in 调用本方法；禁止裸 SQL，
    /// 禁止绕过 <see cref="BuildPageQuery"/> 的租户/权限过滤。租户边界由各 Service
    /// 的 <see cref="BuildPageQuery"/> override 强制叠加（参考 ChannelDefinitionManagementServiceBase）。
    /// </remarks>
    /// <param name="query">实体既有查询输入，复用各 Service 的筛选条件；分页参数仅在 <paramref name="range"/>=CurrentPage 时生效。</param>
    /// <param name="columns">前端可见列白名单；非白名单或标记 <see cref="ExportSensitiveAttribute"/> 的字段一律不输出。</param>
    /// <param name="range">导出范围：全量（受 <see cref="DataExportHelper.MaxExportRows"/> 上限保护）或仅当前页。</param>
    /// <param name="fileSuffix">导出文件名后缀（业务名）。</param>
    /// <returns>.xlsx <see cref="IActionResult"/>，可直接从控制器方法返回。</returns>
    protected virtual async Task<IActionResult> ExportCoreAsync(
        TPageRequest query,
        List<ExportColumnRequest> columns,
        ExportRangeEnum range,
        string fileSuffix)
    {
        ArgumentNullException.ThrowIfNull(query);

        var exportQuery = ApplyOrdering(BuildPageQuery(query), query);

        List<TResponse> rows;
        if (range == ExportRangeEnum.CurrentPage)
        {
            var page = query.Page <= 0 ? 1 : query.Page;
            var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;
            rows = await exportQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        }
        else
        {
            rows = await exportQuery.Take(DataExportHelper.MaxExportRows).ToListAsync();
        }

        return await DataExportHelper.BuildExcelResultAsync(rows, columns ?? new List<ExportColumnRequest>(), fileSuffix);
    }

    /// <summary>
    /// 新增
    /// </summary>
    [HttpPost(CrudActionRoutes.Add)]
    public virtual async Task<TResponse> AddAsync(TAddRequest input)
    {
        EnsureInputNotNull(input);
        await OnAddingAsync(input);
        var entity = await InnerAddAsync(input);
        await OnAddedAsync(input, entity);
        return entity.Adapt<TResponse>();
    }

    protected virtual Task OnAddingAsync(TAddRequest input)
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnAddedAsync(TAddRequest input, TEntity entity)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 更新
    /// </summary>
    [HttpPost(CrudActionRoutes.Update)]
    public virtual async Task UpdateAsync(TUpdateRequest input)
    {
        EnsureInputNotNull(input);
        await OnUpdatingAsync(input);
        var entity = input.Adapt<TEntity>();
        var before = await Repository.GetFirstAsync(e => e.Id == input.Id);
        await InnerUpdateAsync(input);
        await OnUpdatedAsync(input, before, entity);
    }

    protected virtual Task OnUpdatingAsync(TUpdateRequest input)
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnUpdatedAsync(TUpdateRequest input, TEntity before, TEntity after)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 详情
    /// </summary>
    [HttpGet(CrudActionRoutes.Detail)]
    public virtual async Task<TResponse> GetAsync([FromQuery] BaseIdRequest input)
    {
        var entity = await InnerGetAsync(input);
        return entity == null ? default : entity.Adapt<TResponse>();
    }

    /// <summary>
    /// 对查询应用日期范围过滤。
    /// 当 range 不为 null 且包含元素时，按字段名生成 "字段 &lt; 起始" 和 "字段 &gt; 结束+1天" 条件。
    /// </summary>
    /// <typeparam name="T">查询结果的类型</typeparam>
    /// <param name="query">待过滤的查询</param>
    /// <param name="range">日期范围列表（[0]=起始, [1]=结束）</param>
    /// <param name="fieldName">数据库字段名（SQL 片段，如 "u.StartTime"）</param>
    protected static ISugarQueryable<T> ApplyDateRangeFilter<T>(ISugarQueryable<T> query, List<long?> range, string fieldName)
    {
        if (range == null || range.Count == 0)
        {
            return query;
        }

        var start = range[0];
        query = query.WhereIF(start.HasValue, $"{fieldName} > @start", new { start, });
        if (range.Count > 1)
        {
            var endRange = range[1];
            if (endRange.HasValue)
            {
                var end = endRange.Value + 86400;
                query = query.Where($"{fieldName} < @end", new { end, });
            }
        }

        return query;
    }
}