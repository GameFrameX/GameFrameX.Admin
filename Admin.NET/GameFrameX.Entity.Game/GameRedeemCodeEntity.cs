// 麻省理工学院许可证
// 
// 版权所有 (c) 2021-2023 zuohuaijun，大名科技（天津）有限公司  联系电话/微信：18020030720  QQ：515096995
// 
// 特此免费授予获得本软件的任何人以处理本软件的权利，但须遵守以下条件：在所有副本或重要部分的软件中必须包括上述版权声明和本许可声明。
// 
// 软件按“原样”提供，不提供任何形式的明示或暗示的保证，包括但不限于对适销性、适用性和非侵权的保证。
// 在任何情况下，作者或版权持有人均不对任何索赔、损害或其他责任负责，无论是因合同、侵权或其他方式引起的，与软件或其使用或其他交易有关。

namespace GameFrameX.Entity.Game;

/// <summary>
/// 游戏兑换码
/// </summary>
[SugarTable(null, "游戏兑换码")]
[SysTable]
[IncreTable]
public class GameRedeemCodeEntity : EntityBase
{
    /// <summary>
    /// 区服ID
    /// </summary>
    [SugarColumn(ColumnDescription = "区服ID")]
    public long AreaId { get; set; }

    /// <summary>
    /// 礼包码的唯一字符串。
    /// </summary>
    [SugarColumn(ColumnDescription = "兑换码")]
    public string Code { get; set; }

    /// <summary>
    /// 对礼包内容的描述。
    /// </summary>
    [SugarColumn(ColumnDescription = "描述")]
    public string Description { get; set; }

    /// <summary>
    /// 表示礼包码是否有效。
    /// </summary>
    [SugarColumn(ColumnDescription = "是否激活")]
    public bool IsActive { get; set; }

    /// <summary>
    /// 礼包码的可使用次数限制。
    /// </summary>
    [SugarColumn(ColumnDescription = "可使用次数")]
    public int UsageLimit { get; set; }

    /// <summary>
    /// 现在已使用次数。
    /// </summary>
    [SugarColumn(ColumnDescription = "已使用次数")]
    public int UsedCount { get; set; }

    /// <summary>
    /// 礼包码有效的开始时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "有效开始时间")]
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 礼包码有效的结束时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "失效时间")]
    public DateTime EndTime { get; set; }

    /// <summary>
    /// 礼包中包含的物品或权益的详细列表。
    /// </summary>
    [SugarColumn(ColumnDescription = "礼包内容")]
    public string Contents { get; set; } // 可能是JSON序列化的字符串
}