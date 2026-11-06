using GameFrameX.Entity.Routing;
using GameFrameX.Entity.System;

namespace GameFrameX.Entity.Global;

/// <summary>
/// 全局渠道定义表，由平台维护，业务侧只能通过白名单只读入口选择。
/// </summary>
[SugarTable(null, "全局渠道定义表")]
[GlobalTable]
[IncrementTable]
public class GlobalChannelDefinitionEntity : ChannelDefinition
{
}