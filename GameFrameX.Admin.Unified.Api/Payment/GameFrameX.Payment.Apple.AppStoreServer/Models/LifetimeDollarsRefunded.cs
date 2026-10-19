// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

namespace GameFrameX.Payment.Apple.AppStoreServer.Models;

/// <summary>
/// A value that indicates the dollar amount of refunds the customer has received in your app, since purchasing the app, across all platforms.
/// <para>指示客户自购买应用以来在所有平台上在您的应用中收到的退款金额（以美元为单位）的值。</para>
/// <para>https://developer.apple.com/documentation/appstoreserverapi/lifetimedollarsrefunded</para>
/// </summary>
public enum LifetimeDollarsRefunded
{
    /// <summary>
    /// Undeclared
    /// <para>未声明</para>
    /// </summary>
    UNDECLARED = 0,

    /// <summary>
    /// 0 USD
    /// <para>0 美元</para>
    /// </summary>
    ZERO_DOLLARS = 1,

    /// <summary>
    /// 0.01 USD - 49.99 USD
    /// <para>0.01 到 49.99 美元</para>
    /// </summary>
    ONE_CENT_TO_FORTY_NINE_DOLLARS_AND_NINETY_NINE_CENTS = 2,

    /// <summary>
    /// 50.00 USD - 99.99 USD
    /// <para>50.00 到 99.99 美元</para>
    /// </summary>
    FIFTY_DOLLARS_TO_NINETY_NINE_DOLLARS_AND_NINETY_NINE_CENTS = 3,

    /// <summary>
    /// 100.00 USD - 499.99 USD
    /// <para>100.00 到 499.99 美元</para>
    /// </summary>
    ONE_HUNDRED_DOLLARS_TO_FOUR_HUNDRED_NINETY_NINE_DOLLARS_AND_NINETY_NINE_CENTS = 4,

    /// <summary>
    /// 500.00 USD - 999.99 USD
    /// <para>500.00 到 999.99 美元</para>
    /// </summary>
    FIVE_HUNDRED_DOLLARS_TO_NINE_HUNDRED_NINETY_NINE_DOLLARS_AND_NINETY_NINE_CENTS = 5,

    /// <summary>
    /// 1000.00 USD - 1999.99 USD
    /// <para>1000.00 到 1999.99 美元</para>
    /// </summary>
    ONE_THOUSAND_DOLLARS_TO_ONE_THOUSAND_NINE_HUNDRED_NINETY_NINE_DOLLARS_AND_NINETY_NINE_CENTS = 6,

    /// <summary>
    /// &gt;= 2000.00 USD
    /// <para>2000.00 美元及以上</para>
    /// </summary>
    TWO_THOUSAND_DOLLARS_OR_GREATER = 7,
}