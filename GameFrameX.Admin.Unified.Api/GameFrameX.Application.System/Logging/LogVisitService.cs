using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Log;

namespace GameFrameX.Application.System.Logging;

/// <summary>
/// 系统访问日志服务。C156 起：SuperAdmin 查询主库 system_log_visit，普通租户查询当前租户库 tenant_log_visit。
/// </summary>
[ApiDescriptionSettings(Order = 340)]
public class LogVisitService : BaseLogService<SystemLogVisitEntity, TenantLogVisitEntity, LogVisitFields>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="systemLogVisitRepository">平台访问日志仓储（主库）</param>
    /// <param name="tenantLogVisitRepository">租户访问日志仓储（当前租户库）</param>
    /// <param name="currentUserContext">当前登录用户</param>
    public LogVisitService(SqlSugarRepository<SystemLogVisitEntity> systemLogVisitRepository, SqlSugarRepository<TenantLogVisitEntity> tenantLogVisitRepository, CurrentUserContext currentUserContext)
        : base(systemLogVisitRepository, tenantLogVisitRepository, currentUserContext)
    {
    }

    /// <summary>
    /// 日志显示名称
    /// </summary>
    protected override string LogDisplayName
    {
        get { return "访问日志"; }
    }

    /// <summary>
    /// 访问日志可模糊匹配的文本列：显示标题 / 方法名 / 账号 / 真实姓名 / IP / 登录地点 / 浏览器 / 操作系统。
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
    };
}