// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json.Serialization;
using System.Text;
using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.Xsolla.Option;

namespace GameFrameX.Payment.Xsolla.Reports.Transactions;

public static class Search
{
    /// <summary>
    /// 交易搜索参数
    /// </summary>
    public sealed class FindRequest
    {
        /// <summary>
        /// 日期范围的开始时间。
        /// </summary>
        [JsonPropertyName("datetime_from")]
        public DateTime DatetimeFrom { get; set; }

        /// <summary>
        /// 日期范围的结束时间。
        /// </summary>
        [JsonPropertyName("datetime_to")]
        public DateTime DatetimeTo { get; set; }

        /// <summary>
        /// 交易ID。
        /// </summary>
        [JsonPropertyName("transaction_id")]
        public string TransactionId { get; set; } = null!;

        /// <summary>
        /// 在搜索结果中显示的交易类型：
        ///  all	所有交易。
        ///  canceled	取消的交易（测试交易除外）。
        ///  test	测试交易。
        ///  transferred	成功的交易（测试交易除外）。
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        /// <summary>
        /// 用户电话号码（采用国际格式）。
        /// </summary>
        [JsonPropertyName("phone")]
        public string Phone { get; set; } = null!;

        /// <summary>
        /// 存储在己侧的游戏内唯一用户ID。请确保传入现有用户ID
        /// </summary>
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = null!;

        /// <summary>
        /// 用户名。
        /// </summary>
        [JsonPropertyName("user_name")]
        public string UserName { get; set; } = null!;

        /// <summary>
        /// 用户标识的自定义参数。
        /// </summary>
        [JsonPropertyName("user_custom")]
        public string UserCustom { get; set; } = null!;

        /// <summary>
        /// 用户电子邮件。
        /// </summary>
        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;

        /// <summary>
        /// 游戏中的交易ID。值对于每个用户付款必须唯一。
        /// </summary>
        [JsonPropertyName("external_id")]
        public string ExternalId { get; set; } = null!;

        /// <summary>
        /// 元素编号，从该元素开始生成列表（从0开始数）。
        /// </summary>
        [JsonPropertyName("offset")]
        public int Offset { get; set; }

        /// <summary>
        /// 页面上元素数量的限制。
        /// </summary>
        [JsonPropertyName("limit")]
        public int Limit { get; set; }

        /// <summary>
        /// 一种交易状态。
        /// awaitingRefund	交易正在等待退款决定。用户申请退款后，艾克索拉客户支持将手动处理该申请并作出决定。
        /// canceled	有两种可能场景：
        /// 交易被支付系统侧取消。例如用户帐户中资金不足。
        /// 已对用户退款。
        /// created	用户发起了交易但交易尚未被处理。
        /// done	交易已成功处理。
        /// error	处理交易时发生了错误。此类支付可通过联系艾克索拉客户支持进行退款。
        /// partiallyRefunded	用户收到了部分退款。
        /// processing	交易正在处理中。
        /// refunded	退款已退回用户的艾克索拉余额。
        /// review	交易处理被反欺诈系统暂停，当前正对交易进行额外验核。
        ///
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;
    }

    /// <summary>
    /// 异步查询交易记录
    /// </summary>
    /// <param name="httpClient">HTTP客户端实例</param>
    /// <param name="findParams">查询参数对象</param>
    /// <param name="optionSetting">支付选项设置</param>
    /// <returns>返回JSON格式的查询结果字符串</returns>
    /// <exception cref="ArgumentNullException">当httpClient或findParams为null时抛出</exception>
    /// <remarks>
    /// 该方法通过Xsolla API查询交易记录，支持以下查询条件：
    /// - 时间范围（开始时间和结束时间）
    /// - 交易ID
    /// - 交易类型
    /// - 用户信息（电话、ID、用户名、自定义参数、邮箱）
    /// - 外部交易ID
    /// - 分页参数（偏移量和限制数）
    /// - 交易状态
    /// </remarks>
    public static async Task<string> FindAsync(HttpClient httpClient, FindRequest findParams, IPaymentOptionSetting optionSetting)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(findParams);
        ArgumentNullException.ThrowIfNull(optionSetting);
        var xsollaPayOptionSetting = (XsollaPayOptionSetting)optionSetting;
        var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{xsollaPayOptionSetting.XsollaPaymentMerchantId}:{xsollaPayOptionSetting.XsollaPaymentApiKey}"));
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {base64String}");
        const string format = "json";
        // 构建查询参数字符串
        var queryParams = new StringBuilder();

        if (findParams.DatetimeFrom != default)
        {
            queryParams.Append($"datetime_from={findParams.DatetimeFrom:yyyy-MM-dd}&");
        }

        if (findParams.DatetimeTo != default)
        {
            queryParams.Append($"datetime_to={findParams.DatetimeTo:yyyy-MM-dd}&");
        }

        if (!string.IsNullOrEmpty(findParams.TransactionId))
        {
            queryParams.Append($"transaction_id={findParams.TransactionId}&");
        }

        if (!string.IsNullOrEmpty(findParams.Type))
        {
            queryParams.Append($"type={findParams.Type}&");
        }

        if (!string.IsNullOrEmpty(findParams.Phone))
        {
            queryParams.Append($"phone={Uri.EscapeDataString(findParams.Phone)}&");
        }

        if (!string.IsNullOrEmpty(findParams.UserId))
        {
            queryParams.Append($"user_id={Uri.EscapeDataString(findParams.UserId)}&");
        }

        if (!string.IsNullOrEmpty(findParams.UserName))
        {
            queryParams.Append($"user_name={Uri.EscapeDataString(findParams.UserName)}&");
        }

        if (!string.IsNullOrEmpty(findParams.UserCustom))
        {
            queryParams.Append($"user_custom={Uri.EscapeDataString(findParams.UserCustom)}&");
        }

        if (!string.IsNullOrEmpty(findParams.Email))
        {
            queryParams.Append($"email={Uri.EscapeDataString(findParams.Email)}&");
        }

        if (!string.IsNullOrEmpty(findParams.ExternalId))
        {
            queryParams.Append($"external_id={Uri.EscapeDataString(findParams.ExternalId)}&");
        }

        if (findParams.Offset != default)
        {
            queryParams.Append($"offset={findParams.Offset}&");
        }

        if (findParams.Limit != default)
        {
            queryParams.Append($"limit={findParams.Limit}&");
        }

        if (!string.IsNullOrEmpty(findParams.Status))
        {
            queryParams.Append($"status={findParams.Status}&");
        }

        // 移除最后一个&符号
        var finalQueryString = queryParams.ToString().TrimEnd('&');

        var request = await httpClient.GetAsync($"https://api.xsolla.com/merchant/v2/merchants/{xsollaPayOptionSetting.XsollaPaymentMerchantId}/reports/transactions/search.{format}?project_id={xsollaPayOptionSetting.XsollaPaymentProjectId}&{finalQueryString}");
        var response = await request.Content.ReadAsStringAsync();

        return response;
    }
}