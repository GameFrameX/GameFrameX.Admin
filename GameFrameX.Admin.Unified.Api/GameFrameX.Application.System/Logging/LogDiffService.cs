using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Log;

namespace GameFrameX.Application.System.Logging;

/// <summary>
/// 系统差异日志服务。C156 起：SuperAdmin 查询主库 system_log_diff，普通租户查询当前租户库 tenant_log_diff。
/// </summary>
[ApiDescriptionSettings(Order = 330)]
public class LogDiffService : BaseLogService<SystemLogDiffEntity, TenantLogDiffEntity, LogDiffFields>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="systemLogDiffRepository">平台差异日志仓储（主库）</param>
    /// <param name="tenantLogDiffRepository">租户差异日志仓储（当前租户库）</param>
    /// <param name="currentUserContext">当前登录用户</param>
    public LogDiffService(SqlSugarRepository<SystemLogDiffEntity> systemLogDiffRepository, SqlSugarRepository<TenantLogDiffEntity> tenantLogDiffRepository, CurrentUserContext currentUserContext)
        : base(systemLogDiffRepository, tenantLogDiffRepository, currentUserContext)
    {
    }

    /// <summary>
    /// 日志显示名称
    /// </summary>
    protected override string LogDisplayName
    {
        get { return "差异日志"; }
    }

    /// <summary>
    /// 差异日志可模糊匹配的文本列：DiffType / Sql。
    /// BeforeData / AfterData / Snapshot / BusinessData / Parameters 均为 BigString 大字段，纳入会导致全表扫描恶化，本次不纳入。
    /// </summary>
    protected override IReadOnlyCollection<string> SearchableTextColumns { get; } = new[]
    {
        nameof(LogDiffFields.DiffType),
        nameof(LogDiffFields.Sql),
    };
}