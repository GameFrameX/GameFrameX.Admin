// 麻省理工学院许可证
// 
// 版权所有 (c) 2021-2023 zuohuaijun，大名科技（天津）有限公司  联系电话/微信：18020030720  QQ：515096995
// 
// 特此免费授予获得本软件的任何人以处理本软件的权利，但须遵守以下条件：在所有副本或重要部分的软件中必须包括上述版权声明和本许可声明。
// 
// 软件按“原样”提供，不提供任何形式的明示或暗示的保证，包括但不限于对适销性、适用性和非侵权的保证。
// 在任何情况下，作者或版权持有人均不对任何索赔、损害或其他责任负责，无论是因合同、侵权或其他方式引起的，与软件或其使用或其他交易有关。

using GameFrameX.Core.Enum;

namespace GameFrameX.Entity.Game;

/// <summary>
/// 游戏内公告
/// </summary>
[SugarTable(null, "游戏内公告")]
[SysTable]
[IncreTable]
public class GameAnnouncementEntity : EntityBase
{
    /// <summary>
    /// 区服ID
    /// </summary>
    [SugarColumn(ColumnDescription = "区服ID")]
    public long AreaId { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    [SugarColumn(ColumnDescription = "标题", Length = 256)]
    public string Title { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [SugarColumn(ColumnDescription = "内容", Length = 4096)]
    public string Content { get; set; }

    /// <summary>
    /// 公告开始显示的时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "开始时间")]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 公告结束显示的时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "结束时间")]
    public DateTime EndTime { get; set; }

    /// <summary>
    /// 用于确定公告的显示优先级。
    /// </summary>
    [SugarColumn(ColumnDescription = "优先级")]
    public int Priority { get; set; }

    /// <summary>
    /// 指示公告当前是否应该被激活或显示。
    /// </summary>
    [SugarColumn(ColumnDescription = "是否激活")]
    public bool IsActive { get; set; }
}