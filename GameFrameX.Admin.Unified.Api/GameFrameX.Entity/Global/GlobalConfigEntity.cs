using GameFrameX.Entity.Routing;
using GameFrameX.Entity.System;

namespace GameFrameX.Entity.Global;

/// <summary>
/// 租户可只读的全局配置表，仅允许保存经过白名单 DTO 审查的非敏感配置。
/// </summary>
[SugarTable(null, "租户可读全局配置表")]
[GlobalTable]
[IncrementTable]
public class GlobalConfigEntity : SystemConfigBase
{
}