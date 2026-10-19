namespace GameFrameX.Core.Service.Tenant;

/// <summary>
/// 注册租户 PG 角色启动自愈结果（供调用方输出运维可见日志；默认租户失败在 heal 内直接抛错，不进本报告）。
/// </summary>
/// <param name="HealedRoles">本次重建的角色描述（租户 ID/角色名[/建库]），已按租户 ID 排序。</param>
/// <param name="Failures">非默认租户的自愈失败项（文案含恢复路径）。</param>
public sealed record TenantRoleSelfHealReport(IReadOnlyList<string> HealedRoles, IReadOnlyList<string> Failures)
{
    /// <summary>
    /// 空报告：非 PostgreSQL 主库 / 注册表无库隔离租户时的固定返回。
    /// </summary>
    public static readonly TenantRoleSelfHealReport Empty = new TenantRoleSelfHealReport(Array.Empty<string>(), Array.Empty<string>());
}
