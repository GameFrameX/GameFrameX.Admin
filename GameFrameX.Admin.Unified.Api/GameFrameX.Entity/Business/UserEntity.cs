using GameFrameX.Entity.System;

namespace GameFrameX.Entity.Business;

/// <summary>
/// 租户库用户表，仅承载普通租户身份与管理员角色账号，必须在明确租户上下文中访问。
/// </summary>
[SugarTable(null, "租户库用户表")]
[BusinessTable]
[IncrementTable]
public class UserEntity : SystemUserBase
{
}