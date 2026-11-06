using GameFrameX.Entity.Base;
﻿using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Entity.Business.Game;

/// <summary>
/// 游戏角色广告
/// </summary>
[SugarTable(null, "游戏角色广告")]
[BusinessTable]
[IncrementTable]
public class GameRoleAdvertisementEntity : EntityBase
{

    /// <summary>
    /// 广告回调命中的游戏应用分包 Id（与 <see cref="Game.App.GameAppPackageEntity.Id"/> 一致），来自公开回调 URL 的 packageId 路由段；
    /// 与该笔回调验签所选配置的分包一致。新装库才有值，历史行为 0。
    /// </summary>
    [SugarColumn(ColumnDescription = "回调命中游戏应用分包Id", DefaultValue = "0")]
    public long PackageId { get; set; }

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
