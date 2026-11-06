using GameFrameX.Entity.System;

namespace GameFrameX.Entity.Business;

/// <summary>
/// 用户表，承载普通管理员身份与管理员角色账号。
/// </summary>
[SugarTable(null, "用户表")]
[BusinessTable]
[IncrementTable]
public class UserEntity : SystemUserBase
{
}