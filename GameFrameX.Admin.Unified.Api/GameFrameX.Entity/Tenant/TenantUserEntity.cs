using GameFrameX.Entity.System;

namespace GameFrameX.Entity.Tenant;

/// <summary>
/// 租户库用户表，仅承载普通租户身份与租户管理员账号，必须在明确租户上下文中访问。
/// </summary>
[SugarTable(null, "租户库用户表")]
[TenantTable]
[IncrementTable]
public class TenantUserEntity : SystemUserBase
{
}