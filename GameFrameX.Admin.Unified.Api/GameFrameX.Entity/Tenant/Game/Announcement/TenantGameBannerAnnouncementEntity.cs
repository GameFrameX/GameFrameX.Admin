using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game.Announcement;

/// <summary>
/// 游戏横幅公告
/// </summary>
[SugarTable(null, "游戏横幅公告")]
[SugarIndex("idx_tenant_game_banner_announcement_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameBannerAnnouncementEntity : EntityBasic, IAppScoped
{
    /// <summary>
    /// 应用Id。运营内容应用作用域（TenantId + AppId），必须 &gt; 0。
    /// <para>来源：Admin 后台当前应用上下文写入。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 区服ID
    /// </summary>
    [SugarColumn(ColumnDescription = "区服ID", DefaultValue = "0")]
    public long AreaId { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [SugarColumn(ColumnDescription = "内容", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string Content { get; set; } = null!;

    /// <summary>
    /// 内容本地化Key
    /// </summary>
    [SugarColumn(ColumnDescription = "内容本地化Key", Length = EntityStringLength.Length512)]
    public string ContentLocalizationKey { get; set; } = null!;

    /// <summary>
    /// 用于确定公告的显示优先级。
    /// </summary>
    [SugarColumn(ColumnDescription = "优先级", DefaultValue = "0")]
    public int Priority { get; set; }

    /// <summary>
    /// 公告开始显示的时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "开始时间", DefaultValue = "0")]
    public long StartTime { get; set; }

    /// <summary>
    /// 公告结束显示的时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "结束时间", DefaultValue = "0")]
    public long EndTime { get; set; }

    /// <summary>
    /// 公告滚动的时间间隔或频率。单位秒
    /// </summary>
    [SugarColumn(ColumnDescription = "间隔时间", DefaultValue = "0")]
    public uint Interval { get; set; }

    /// <summary>
    /// 公告重复播放次数。
    /// </summary>
    [SugarColumn(ColumnDescription = "重复次数", DefaultValue = "1")]
    public int RepeatCount { get; set; } = 1;

    /// <summary>
    /// 指示公告当前是否应该被激活或显示。
    /// </summary>
    [SugarColumn(ColumnDescription = "是否激活", DefaultValue = "false")]
    public bool IsActive { get; set; }
}