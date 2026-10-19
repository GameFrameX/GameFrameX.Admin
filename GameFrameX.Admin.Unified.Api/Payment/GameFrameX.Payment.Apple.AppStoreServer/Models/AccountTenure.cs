// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// The age of the customer’s account.
/// <para>客户账户的注册年限。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/accounttenure</para>
/// </summary>
public enum AccountTenure
{
    /// <summary>
    /// Undeclared
    /// <para>未声明</para>
    /// </summary>
    UNDECLARED = 0,

    /// <summary>
    /// 0-3 days
    /// <para>0-3天</para>
    /// </summary>
    ZERO_TO_THREE_DAYS = 1,

    /// <summary>
    /// 3-10 days
    /// <para>3-10天</para>
    /// </summary>
    THREE_DAYS_TO_TEN_DAYS = 2,

    /// <summary>
    /// 10-30 days
    /// <para>10-30天</para>
    /// </summary>
    TEN_DAYS_TO_THIRTY_DAYS = 3,

    /// <summary>
    /// 30-90 days
    /// <para>30-90天</para>
    /// </summary>
    THIRTY_DAYS_TO_NINETY_DAYS = 4,

    /// <summary>
    /// 90-180 days
    /// <para>90-180天</para>
    /// </summary>
    NINETY_DAYS_TO_ONE_HUNDRED_EIGHTY_DAYS = 5,

    /// <summary>
    /// 180-365 days
    /// <para>180-365天</para>
    /// </summary>
    ONE_HUNDRED_EIGHTY_DAYS_TO_THREE_HUNDRED_SIXTY_FIVE_DAYS = 6,

    /// <summary>
    /// &gt; 365 days
    /// <para>大于365天</para>
    /// </summary>
    GREATER_THAN_THREE_HUNDRED_SIXTY_FIVE_DAYS = 7,
}