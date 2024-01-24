// 麻省理工学院许可证
// 
// 版权所有 (c) 2021-2023 zuohuaijun，大名科技（天津）有限公司  联系电话/微信：18020030720  QQ：515096995
// 
// 特此免费授予获得本软件的任何人以处理本软件的权利，但须遵守以下条件：在所有副本或重要部分的软件中必须包括上述版权声明和本许可声明。
// 
// 软件按“原样”提供，不提供任何形式的明示或暗示的保证，包括但不限于对适销性、适用性和非侵权的保证。
// 在任何情况下，作者或版权持有人均不对任何索赔、损害或其他责任负责，无论是因合同、侵权或其他方式引起的，与软件或其使用或其他交易有关。

using System.ComponentModel;

namespace GameFrameX.Game.Enum;

/// <summary>
/// 游戏邮件状态
/// </summary>
[Description("游戏邮件状态")]
public enum GameMailStatus
{
    /// <summary>
    /// 邮件已创建但还未发送。
    /// </summary>
    [Description("草稿")] Draft,

    /// <summary>
    /// 邮件已发送给收件人，但还未被查看。
    /// </summary>
    [Description("已发送")] Sent,

    /// <summary>
    /// 邮件已被收件人查看。
    /// </summary>
    [Description("已阅读")] Read,

    /// <summary>
    /// 邮件中的附件已被收件人领取。
    /// </summary>
    [Description("已领取附件")] AttachmentClaimed,

    /// <summary>
    /// 邮件已从收件人的邮箱中被删除。
    /// </summary>
    [Description("已删除")] Deleted,

    /// <summary>
    /// 邮件由于过期或其他原因而被系统回收。
    /// </summary>
    [Description("已过期")] Expired,

    /// <summary>
    /// 邮件因其他特殊原因被标记为无效。
    /// </summary>
    [Description("无效")] Invalid
}