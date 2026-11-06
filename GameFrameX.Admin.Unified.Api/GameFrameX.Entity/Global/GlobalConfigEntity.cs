using GameFrameX.Entity.Routing;
using GameFrameX.Entity.System;

namespace GameFrameX.Entity.Global;

/// <summary>
/// 业务侧可只读的全局配置表，仅允许保存经过白名单 DTO 审查的非敏感配置。
/// </summary>
[SugarTable(null, "业务侧可读全局配置表")]
[GlobalTable]
[IncrementTable]
public class GlobalConfigEntity : SystemConfigBase
{
}