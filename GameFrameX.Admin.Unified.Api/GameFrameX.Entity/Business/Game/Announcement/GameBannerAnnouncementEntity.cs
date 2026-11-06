using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Business.Game.Announcement;

/// <summary>
/// 游戏横幅公告
/// </summary>
[SugarTable(null, "游戏横幅公告")]
[BusinessTable]
[IncrementTable]
public class GameBannerAnnouncementEntity : EntityBasic
{

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