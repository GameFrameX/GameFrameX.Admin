namespace GameFrameX.Core.Export;

/// <summary>
/// 通用数据导出请求。实体标识由各业务 Service 的 Admin API 路由隐含
/// （URL = 实体），本 DTO 不携带 tableName 或任意表直查参数，
/// 以规避「任意表 + 自由 SQL」的越权攻击面。
/// </summary>
/// <typeparam name="TQuery">实体既有的分页/筛选输入类型，复用各 Service 现有查询条件。</typeparam>
public class DataExportRequest<TQuery> where TQuery : class, new()
{
    /// <summary>
    /// 查询条件，复用各实体既有 Query 输入；租户与权限过滤由 Service 的 BuildPageQuery 强制叠加。
    /// </summary>
    public TQuery Query { get; set; } = new();

    /// <summary>
    /// 可见列白名单。仅这些字段会被导出；非白名单或敏感字段一律不输出。
    /// </summary>
    public List<ExportColumnRequest> Columns { get; set; } = new();

    /// <summary>
    /// 导出范围：全量（受单次上限保护）或仅当前页。
    /// </summary>
    public ExportRangeEnum Range { get; set; } = ExportRangeEnum.All;
}