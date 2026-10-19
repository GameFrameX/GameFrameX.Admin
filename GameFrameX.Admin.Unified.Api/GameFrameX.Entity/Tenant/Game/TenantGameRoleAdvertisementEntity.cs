using GameFrameX.Entity.Base;
﻿using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏角色广告
/// </summary>
[SugarTable(null, "游戏角色广告")]
[SugarIndex("idx_tenant_game_role_ad_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameRoleAdvertisementEntity : EntityTenantBase, IAppScoped
{
    /// <summary>
    /// 应用Id。事实数据应用作用域（TenantId + AppId），必须 &gt; 0。
    /// <para>跟随角色 <c>AppId</c>，由 Client API 广告回调或后台写入时从角色 / 当前应用上下文取值。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    [SugarColumn(ColumnDescription = "用户ID", DefaultValue = "0")]
    public long UserId { get; set; }

    /// <summary>
    /// 唯一交易 ID
    /// </summary>
    [SugarColumn(ColumnDescription = "唯一交易 ID")]
    public string TransId { get; set; } = null!;

    /// <summary>
    /// 区服Id
    /// </summary>
    [SugarColumn(ColumnDescription = "区服Id", DefaultValue = "0")]
    public long AreaId { get; set; }

    /// <summary>
    /// 是否发送成功
    /// </summary>
    [SugarColumn(ColumnDescription = "是否发送成功", DefaultValue = "false")]
    public bool IsSendSuccess { get; set; }

    /// <summary>
    /// 角色ID
    /// </summary>
    [SugarColumn(ColumnDescription = "角色ID")]
    public string RoleId { get; set; } = null!;
}