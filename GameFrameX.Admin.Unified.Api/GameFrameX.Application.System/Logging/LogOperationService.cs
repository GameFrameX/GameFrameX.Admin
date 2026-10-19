using GameFrameX.Application.System.Logging.Dto;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Log;

namespace GameFrameX.Application.System.Logging;

/// <summary>
/// 系统操作日志服务。C156 起：SuperAdmin 查询主库 system_log_operation，普通租户查询当前租户库 tenant_log_operation。
/// </summary>
[ApiDescriptionSettings(Order = 360)]
public class LogOperationService : BaseLogService<SystemLogOperationEntity, TenantLogOperationEntity, LogOperationFields>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="systemLogOperationRepository">平台操作日志仓储（主库）</param>
    /// <param name="tenantLogOperationRepository">租户操作日志仓储（当前租户库）</param>
    /// <param name="currentUserContext">当前登录用户</param>
    public LogOperationService(SqlSugarRepository<SystemLogOperationEntity> systemLogOperationRepository, SqlSugarRepository<TenantLogOperationEntity> tenantLogOperationRepository, CurrentUserContext currentUserContext)
        : base(systemLogOperationRepository, tenantLogOperationRepository, currentUserContext)
    {
    }

    /// <summary>
    /// 日志显示名称
    /// </summary>
    protected override string LogDisplayName
    {
        get { return "操作日志"; }
    }

    /// <summary>
    /// 操作日志可模糊匹配的文本列：访问日志列 + RequestUrl + Message。
    /// Exception 字段为 BigString 长文本，本次不纳入避免全表扫描恶化（操作日志通常无 Exception）。
    /// </summary>
    protected override IReadOnlyCollection<string> SearchableTextColumns { get; } = new[]
    {
        nameof(LogVisitFields.DisplayTitle),
        nameof(LogVisitFields.ActionName),
        nameof(LogVisitFields.Account),
        nameof(LogVisitFields.RealName),
        nameof(LogVisitFields.RemoteIp),
        nameof(LogVisitFields.Location),
        nameof(LogVisitFields.Browser),
        nameof(LogVisitFields.Os),
        nameof(LogOperationFields.RequestUrl),
        nameof(LogOperationFields.Message),
    };

    /// <summary>
    /// 导出操作日志
    /// </summary>
    /// <param name="input">导出参数</param>
    /// <returns>导出结果</returns>
    [ApiDescriptionSettings(Name = "Export")]
    [NonUnify]
    [DisplayName("导出操作日志")]
    public async Task<IActionResult> ExportLogOpAsync(LogRequest input)
    {
        var list = IsPlatformContext
                       ? await QueryExportListAsync(SystemLogRepository, input)
                       : await QueryExportListAsync(TenantLogRepository, input);
        return await ExportLogResponse.ExportAsExcelAsync(list, LogDisplayName);
    }
}