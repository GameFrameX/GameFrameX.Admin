using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏玩家登出记录（C8：BLMarkLogout 自建；仅删号成功后上报，在线统计用）
/// </summary>
[SugarTable(null, "游戏玩家登出记录")]
[TenantTable]
[IncrementTable]
public class TenantGameUserLogoutRecordEntity : EntityBasicSelect
{
    /// <summary>
    /// 玩家 id
    /// </summary>
    [SugarColumn(ColumnDescription = "玩家 id", DefaultValue = "0")]
    public long UserId { get; set; }

    /// <summary>
    /// 客户端版本
    /// </summary>
    [SugarColumn(ColumnDescription = "客户端版本", Length = EntityStringLength.Length256)]
    public string ClientVersion { get; set; } = string.Empty;

    /// <summary>
    /// 上报时刻（Unix 秒）
    /// </summary>
    [SugarColumn(ColumnDescription = "上报时刻", DefaultValue = "0")]
    public long LogoutTime { get; set; }
}