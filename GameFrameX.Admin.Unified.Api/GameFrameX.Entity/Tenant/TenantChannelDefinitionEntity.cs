using GameFrameX.Entity.System;

namespace GameFrameX.Entity.Tenant;

/// <summary>
/// 租户渠道定义表，只保存在当前租户库，不能写入平台主库。
/// 作为租户级可用渠道字典保持租户级共享；应用专属渠道投放规则由引用方（应用 / 分包 / 支付 / 兑换码等业务表）
/// 在各自引用表上落 AppId / 分包字段表达，本字典不补 AppId。
/// </summary>
[SugarTable(null, "租户渠道定义表")]
[TenantTable]
[IncrementTable]
public class TenantChannelDefinitionEntity : ChannelDefinition
{
}