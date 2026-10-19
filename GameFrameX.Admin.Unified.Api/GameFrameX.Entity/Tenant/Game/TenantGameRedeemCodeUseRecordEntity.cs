// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏兑换码使用记录
/// </summary>
[SugarTable(null, "游戏兑换码使用记录")]
[SugarIndex("idx_tenant_game_redeem_code_use_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameRedeemCodeUseRecordEntity : EntityBasic, IAppScoped
{
    /// <summary>
    /// 应用Id。事实数据应用作用域（TenantId + AppId），必须 &gt; 0。
    /// <para>来源：Client API 兑换由 <c>AppResolutionMiddleware.AppIdKey</c> 写入。兑换时兑换码自身 AppId 必须与当前应用一致。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 区服ID
    /// </summary>
    [SugarColumn(ColumnDescription = "区服ID", DefaultValue = "0")]
    public long AreaId { get; set; }

    /// <summary>
    /// 礼包码的唯一字符串。
    /// </summary>
    [SugarColumn(ColumnDescription = "兑换码")]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 角色ID
    /// </summary>
    [SugarColumn(ColumnDescription = "角色ID", DefaultValue = "0")]
    public long PlayerId { get; set; }
}