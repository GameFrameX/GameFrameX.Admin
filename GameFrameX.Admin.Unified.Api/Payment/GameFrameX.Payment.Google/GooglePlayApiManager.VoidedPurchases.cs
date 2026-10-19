// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using Google.Apis.AndroidPublisher.v3.Data;

namespace GameFrameX.Payment.Google;

/// <summary>
/// Google Play 已作废（退款/取消）交易查询（C310 / GFX-718）：Android Publisher API purchases.voidedpurchases.list 封装。
/// </summary>
public static partial class GooglePlayApiManager
{
    /// <summary>
    /// 查询已作废（退款/取消）交易列表（官方唯一的退款状态查询端点；Google 无商家主动退款 API）。
    /// </summary>
    /// <param name="packageName">应用包名</param>
    /// <param name="startTimeUnixMilliseconds">过滤起始时间（Unix 毫秒，可选；不传则查全量）</param>
    /// <param name="endTimeUnixMilliseconds">过滤结束时间（Unix 毫秒，可选）</param>
    /// <param name="maxResults">单页最大条数（默认 100）</param>
    /// <returns>已作废交易列表（无则空列表）；需先 <see cref="InitializeAsync"/> 初始化服务</returns>
    public static async Task<IList<VoidedPurchase>> GetVoidedPurchasesAsync(string packageName, long? startTimeUnixMilliseconds = null, long? endTimeUnixMilliseconds = null, int maxResults = 100)
    {
        var request = _androidPublisherService.Purchases.Voidedpurchases.List(packageName);
        request.MaxResults = maxResults;
        if (startTimeUnixMilliseconds.HasValue)
        {
            request.StartTime = startTimeUnixMilliseconds.Value;
        }

        if (endTimeUnixMilliseconds.HasValue)
        {
            request.EndTime = endTimeUnixMilliseconds.Value;
        }

        var response = await request.ExecuteAsync();
        return response.VoidedPurchases ?? new List<VoidedPurchase>();
    }
}
