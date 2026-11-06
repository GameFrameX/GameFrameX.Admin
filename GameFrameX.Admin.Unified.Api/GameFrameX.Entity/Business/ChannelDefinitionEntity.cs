using GameFrameX.Entity.System;

namespace GameFrameX.Entity.Business;

/// <summary>
/// 主库共享的渠道定义表。
/// 分包、支付与兑换码等业务通过渠道标识引用此字典。
/// </summary>
[SugarTable(null, "租户渠道定义表")]
[BusinessTable]
[IncrementTable]
public class ChannelDefinitionEntity : ChannelDefinition
{
}