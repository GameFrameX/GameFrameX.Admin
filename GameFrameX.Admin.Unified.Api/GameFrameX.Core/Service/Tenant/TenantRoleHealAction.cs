namespace GameFrameX.Core.Service.Tenant;

/// <summary>
/// 单个注册租户的 PG 角色自愈动作（角色缺失时由 <see cref="PostgreSqlTenantDatabaseProvisioner"/> 规划产出）。
/// </summary>
/// <param name="TenantId">租户 ID。</param>
/// <param name="Username">注册表连接串里的登录角色名。</param>
/// <param name="DatabaseName">注册表连接串里的租户库名。</param>
/// <param name="Password">注册表连接串里的密码（重建角色的唯一密码来源）。</param>
/// <param name="ConnectionString">注册表解密后的完整连接串（建库路径构造计划用）。</param>
/// <param name="RequiresDatabaseCreation">租户库是否同样缺失（缺失则走完整开通，含建库；否则只建号 + 权限矩阵归位）。</param>
/// <param name="IsDefaultTenant">是否默认租户（自愈失败时阻断启动，其余租户仅告警）。</param>
public sealed record TenantRoleHealAction(
    long TenantId,
    string Username,
    string DatabaseName,
    string Password,
    string ConnectionString,
    bool RequiresDatabaseCreation,
    bool IsDefaultTenant);
