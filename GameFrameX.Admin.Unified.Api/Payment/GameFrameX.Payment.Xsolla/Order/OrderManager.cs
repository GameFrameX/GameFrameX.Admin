// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json.Serialization;
using System.Globalization;
using System.Text;
using GameFrameX.Foundation.Json;
using GameFrameX.Payment.Abstract;
using GameFrameX.Payment.Xsolla.Models;
using GameFrameX.Payment.Xsolla.Option;

namespace GameFrameX.Payment.Xsolla.Order;

/// <summary>
/// Xsolla订单管理类
/// </summary>
public class OrderManager : IOrderManager
{
    public class OrderParamsCreatePurchase
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("items")]
        public List<OrderParamsCreateItem> Items { get; set; } = new();
    }

    public class OrderParamsCreateSettings
    {
        /// <summary>
        /// 展示的货币
        /// https://developers.xsolla.com/zh/doc/pay-station/references/supported-currencies/
        /// </summary>
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = null!;

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("external_id")]
        public string ExternalId { get; set; } = null!;

        /// <summary>
        /// 收据语言,https://developers.xsolla.com/zh/doc/pay-station/features/localization/
        /// </summary>
        [JsonPropertyName("language")]
        public string Language { get; set; } = null!;
    }

    public sealed class OrderParamsCreateEmail
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; } = null!;
    }

    public sealed class OrderParamsCreateId
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("value")]
        public string Value { get; set; } = null!;
    }

    public sealed class OrderParamsCreateUser
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("id")]
        public OrderParamsCreateId Id { get; set; } = new();

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("email")]
        public OrderParamsCreateEmail Email { get; set; } = new();
    }

    public sealed class OrderParamsCreate
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("purchase")]
        public OrderParamsCreatePurchase Purchase { get; set; } = new();

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("settings")]
        public OrderParamsCreateSettings Settings { get; set; } = new();

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("user")]
        public OrderParamsCreateUser User { get; set; } = new();

        /// <summary>
        /// 是否是沙盒
        /// </summary>
        [JsonPropertyName("sandbox")]
        public bool Sandbox { get; set; }

        public override string ToString()
        {
            return JsonHelper.Serialize(this, JsonHelper.DefaultOptions);
        }
    }

    /// <summary>
    /// 商品项数据模型类
    /// </summary>
    public sealed class OrderParamsCreateItem
    {
        /// <summary>
        /// 初始化商品项实例
        /// </summary>
        /// <param name="quantity">商品数量</param>
        /// <param name="sku">商品SKU</param>
        public OrderParamsCreateItem(int quantity, string sku)
        {
            this.Quantity = quantity;
            this.Sku = sku;
        }

        /// <summary>
        /// 获取或设置商品数量
        /// </summary>
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        /// <summary>
        /// 获取或设置商品SKU
        /// </summary>
        [JsonPropertyName("sku")]
        public string Sku { get; set; }
    }

    public class OrderParamsCreateResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = null!;
        [JsonPropertyName("order_id")]
        public int OrderId { get; set; }
    }

    public async Task<OrderParamsCreateResponse> CreateAsync(HttpClient httpClient, OrderParamsCreate orderParamsCreate, string ip, IPaymentOptionSetting optionSetting)
    {
        ArgumentNullException.ThrowIfNull(optionSetting);
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(orderParamsCreate);

        var xsollaPayOptionSetting = (XsollaPayOptionSetting)optionSetting;

        var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{xsollaPayOptionSetting.XsollaPaymentMerchantId}:{xsollaPayOptionSetting.XsollaPaymentApiKey}"));
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {base64String}");
        httpClient.DefaultRequestHeaders.Add("X-User-Ip", ip);
        // Console.WriteLine(base64String);
        Console.WriteLine(JsonHelper.Serialize(orderParamsCreate));
        var postData = new StringContent(orderParamsCreate.ToString(), Encoding.UTF8, "application/json");
        var request = await httpClient.PostAsync("https://store.xsolla.com/api/v3/project/" + xsollaPayOptionSetting.XsollaPaymentProjectId + "/admin/payment/token", postData);
        var response = await request.Content.ReadAsStringAsync();
        if (request.IsSuccessStatusCode)
        {
            return JsonHelper.Deserialize<OrderParamsCreateResponse>(response);
        }

        throw new Exception(response);
    }

    public async Task<string> GetAsync(HttpClient httpClient, IOrderParamsGet orderParamsGet, IPaymentOptionSetting optionSetting)
    {
        ArgumentNullException.ThrowIfNull(optionSetting);
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(orderParamsGet);
        var orderParams = (OrderParamsGet)orderParamsGet;
        var xsollaPayOptionSetting = (XsollaPayOptionSetting)optionSetting;
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {orderParams.Token}");
        var request = await httpClient.GetAsync("https://store.xsolla.com/api/v2/project/" + xsollaPayOptionSetting.XsollaPaymentProjectId + "/order/" + orderParamsGet.OrderId);
        var response = await request.Content.ReadAsStringAsync();
        return response;
    }

    public async Task<string> GetListAsync(HttpClient httpClient, IOrderParamsGetList orderParamsGetList, IPaymentOptionSetting optionSetting)
    {
        ArgumentNullException.ThrowIfNull(optionSetting);
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(orderParamsGetList);
        var xsollaPayOptionSetting = (XsollaPayOptionSetting)optionSetting;
        var orderParams = (OrderParamsGetList)orderParamsGetList;
        var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{xsollaPayOptionSetting.XsollaPaymentMerchantId}:{xsollaPayOptionSetting.XsollaPaymentApiKey}"));
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {base64String}");
        var param = new { created_date_from = orderParams.StartTime.ToString(CultureInfo.InvariantCulture), created_date_until = orderParams.EndTime.ToString(CultureInfo.InvariantCulture), limit = orderParams.Limit, offset = orderParams.Offset, };
        var postData = new StringContent(JsonHelper.Serialize(param), Encoding.UTF8, "application/json");
        var request = await httpClient.PostAsync("https://store.xsolla.com/api/v3/project/" + xsollaPayOptionSetting.XsollaPaymentProjectId + "/admin/order/search", postData);
        var response = await request.Content.ReadAsStringAsync();
        return response;
    }
}