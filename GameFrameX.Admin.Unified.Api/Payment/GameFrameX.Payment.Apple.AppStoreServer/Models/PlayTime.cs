// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// A value that indicates the amount of time that the customer used the app.
/// <para>指示客户使用应用的时间量的值。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/playtime</para>
/// </summary>
public enum PlayTime
{
    /// <summary>
    /// The play time is undeclared.
    /// <para>使用时间未声明。</para>
    /// </summary>
    UNDECLARED = 0,

    /// <summary>
    /// The play time is between 0 and 5 minutes.
    /// <para>使用时间在 0 到 5 分钟之间。</para>
    /// </summary>
    ZERO_TO_FIVE_MINUTES = 1,

    /// <summary>
    /// The play time is between 5 and 60 minutes.
    /// <para>使用时间在 5 到 60 分钟之间。</para>
    /// </summary>
    FIVE_TO_SIXTY_MINUTES = 2,

    /// <summary>
    /// The play time is between 1 and 6 hours.
    /// <para>使用时间在 1 到 6 小时之间。</para>
    /// </summary>
    ONE_TO_SIX_HOURS = 3,

    /// <summary>
    /// The play time is between 6 and 24 hours.
    /// <para>使用时间在 6 到 24 小时之间。</para>
    /// </summary>
    SIX_HOURS_TO_TWENTY_FOUR_HOURS = 4,

    /// <summary>
    /// The play time is between 1 and 4 days.
    /// <para>使用时间在 1 到 4 天之间。</para>
    /// </summary>
    ONE_DAY_TO_FOUR_DAYS = 5,

    /// <summary>
    /// The play time is between 4 and 16 days.
    /// <para>使用时间在 4 到 16 天之间。</para>
    /// </summary>
    FOUR_DAYS_TO_SIXTEEN_DAYS = 6,

    /// <summary>
    /// The play time is over 16 days.
    /// <para>使用时间超过 16 天。</para>
    /// </summary>
    OVER_SIXTEEN_DAYS = 7,
}