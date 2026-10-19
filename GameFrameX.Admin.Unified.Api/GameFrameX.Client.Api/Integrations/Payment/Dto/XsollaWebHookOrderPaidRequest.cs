// GameFrameX 组织下的以及组织衍生的项目的版权、商标、专利和其他相关权利均受相应法律法规的保护。使用本项目应遵守相关法律法规和许可证的要求。
// 
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
// 
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯他人合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.Text.Json.Serialization;

namespace GameFrameX.Client.Api.Integrations.Payment.Dto;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// {
///    "items": [
///        {
///            "sku": "18004",
///            "type": "virtual_good",
///            "is_pre_order": false,
///            "quantity": 1,
///            "amount": "6.5000",
///            "promotions": []
///        }
///    ],
///    "notification_type": "order_paid",
///    "order": {
///        "id": 362310922,
///        "mode": "sandbox",
///        "currency_type": "real",
///        "currency": "SGD",
///        "amount": "6.5000",
///        "status": "paid",
///        "platform": "xsolla",
///        "comment": null,
///        "invoice_id": "1734537180",
///        "promotions": []
///    },
///    "user": {
///        "external_id": "1125905017757380608",
///        "email": "zl_dev@zilancreative.com",
///        "country": "SG"
///    },
///    "billing": {
///        "notification_type": "payment",
///        "settings": {
///            "project_id": 280066,
///            "merchant_id": 789907
///        },
///        "purchase": {
///            "total": {
///                "currency": "SGD",
///                "amount": 6.5
///            }
///        },
///        "transaction": {
///            "id": 1734537180,
///            "external_id": "700505389293573",
///            "payment_method_order_id": "175318703359466114",
///            "dry_run": 1,
///            "agreement": 1338329,
///            "card_bin": "411111",
///            "card_suffix": "1111",
///            "card_brand": "Visa",
///            "saved_payment_method": 2,
///            "payment_type": 1,
///            "payment_date": "2025-07-22T15:24:02+03:00",
///            "payment_method": 1380,
///            "payment_method_name": "Bank card"
///        },
///        "payment_details": {
///            "payment": {
///                "currency": "USD",
///                "amount": 5.47
///            },
///            "payment_method_sum": {
///                "currency": "USD",
///                "amount": 5.47
///            },
///            "xsolla_balance_sum": {
///                "currency": "USD",
///                "amount": 0
///            },
///            "payout": {
///                "currency": "USD",
///                "amount": 4.51
///            },
///            "xsolla_fee": {
///                "currency": "USD",
///                "amount": 0.27
///            },
///            "payment_method_fee": {
///                "currency": "USD",
///                "amount": 0.28
///            },
///            "vat": {
///                "currency": "USD",
///                "amount": 0
///            },
///            "sales_tax": {
///                "currency": "USD",
///                "amount": 0.41
///            },
///            "direct_wht": {
///                "currency": "USD",
///                "amount": 0
///            },
///            "payout_currency_rate": "1",
///            "repatriation_commission": {
///                "currency": "USD",
///                "amount": 0
///            },
///            "country_wht": {
///                "currency": "USD",
///                "amount": 0
///            },
///            "user_acquisition_fee": {
///                "currency": "USD",
///                "amount": 0
///            }
///        },
///        "payment_account": {
///            "id": "41124325",
///            "name": "411111******1111",
///            "type": "card",
///            "payment_method": "1380"
///        }
///    }
///}
/// </remarks>
public class XsollaWebHookOrderPaidRequest
{
    /*
    /// <summary>
    /// 道具列表
    /// </summary>
    public ListAsync<ItemsItem> items { get; set; }
    */

    /// <summary>
    /// 通知类型
    /// </summary>
    [JsonPropertyName("notification_type")]
    public string NotificationType { get; set; } = null!;

    /// <summary>
    /// 订单信息
    /// </summary>
    [JsonPropertyName("order")]
    public OrderInfo Order { get; set; } = null!;

    /// <summary>
    /// 用户信息
    /// </summary>
    [JsonPropertyName("user")]
    public UserInfo User { get; set; } = null!;

    /*/// <summary>
    /// 交易和付款详细信息。
    /// </summary>
    public Billing billing { get; set; }*/

    /*
    /// <summary>
    ///
    /// </summary>
    public sealed class ItemsItem
    {
        /// <summary>
        /// SkuId
        /// </summary>
        public string sku { get; set; }

        /// <summary>
        /// 为以下的其中之一 virtual_good、virtual_currency、game_key、bundle
        /// </summary>
        public string type { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool is_pre_order { get; set; }

        /// <summary>
        /// 项目数量
        /// </summary>
        public int quantity { get; set; }

        /// <summary>
        /// 基于商品数量的商品总成本 ，总金额
        /// </summary>
        public string amount { get; set; }

        /// <summary>
        /// 应用的促销。如果未应用促销，则会在参数中传递一个空数组
        /// </summary>
        public ListAsync<string> promotions { get; set; }
    }*/

    /// <summary>
    /// 
    /// </summary>
    public sealed class OrderInfo
    {
        /// <summary>
        /// 艾克索拉端用户订单的唯一标识符
        /// </summary>
        [JsonPropertyName("id")]
        public int Id { get; set; }

        /// <summary>
        /// 为以下之一  “default”“sandbox”
        /// </summary>
        [JsonPropertyName("mode")]
        public string Mode { get; set; } = null!;

        /// <summary>
        /// 付款货币类型，为以下之一 real(真实)、virtual(虚拟)、unknown(未知)
        /// </summary>
        [JsonPropertyName("currency_type")]
        public string CurrencyType { get; set; } = null!;

        /// <summary>
        /// 订单货币。虚拟货币使用 SKU，真实货币使用三个字母的 ISO 4217 代码
        /// </summary>
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = null!;

        /// <summary>
        /// 基于所选货币的购物车总成本
        /// </summary>
        [JsonPropertyName("amount")]
        public string Amount { get; set; } = null!;

        /// <summary>
        /// 订单状态
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = null!;

        /// <summary>
        /// 平台，为以下值之一： “xsolla”“playstation_network”“xbox_live”“pc_standalone”“nintendo_shop”“google_play”“app_store_ios”“android_standalone”“ios_standalone”“android_other”“ios_other”“pc_other”
        /// </summary>
        [JsonPropertyName("platform")]
        public string Platform { get; set; } = null!;

        /// <summary>
        /// 用户对订单的评论。
        /// </summary>
        [JsonPropertyName("comment")]
        public string Comment { get; set; } = null!;

        /// <summary>
        /// 虚拟货币付款或免费商品的 null 值
        /// </summary>
        [JsonPropertyName("invoice_id")]
        public string InvoiceId { get; set; } = null!;

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("promotions")]
        public List<string> Promotions { get; set; } = null!;
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class UserInfo
    {
        /// <summary>
        /// 用户 ID
        /// </summary>
        [JsonPropertyName("external_id")]
        public string ExternalId { get; set; } = null!;

        /// <summary>
        /// 用户电子邮件地址
        /// </summary>
        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;
    }

    /*/// <summary>
    ///
    /// </summary>
    public sealed class Settings
    {
        /// <summary>
        /// 项目ID
        /// </summary>
        public int project_id { get; set; }

        /// <summary>
        /// 商户ID
        /// </summary>
        public int merchant_id { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class Total
    {
        /// <summary>
        ///
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        ///
        /// </summary>
        public float amount { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class Purchase
    {
        /// <summary>
        ///
        /// </summary>
        public Total total { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class Transaction
    {
        /// <summary>
        ///
        /// </summary>
        public int id { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string external_id { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string payment_method_order_id { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int dry_run { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int agreement { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string card_bin { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string card_suffix { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string card_brand { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int saved_payment_method { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int payment_type { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string payment_date { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int payment_method { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string payment_method_name { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class Payment
    {
        /// <summary>
        ///
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        ///
        /// </summary>
        public double amount { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class Payment_method_sum
    {
        /// <summary>
        ///
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        ///
        /// </summary>
        public double amount { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class Xsolla_balance_sum
    {
        /// <summary>
        ///
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int amount { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class Payout
    {
        /// <summary>
        ///
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        ///
        /// </summary>
        public double amount { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class Xsolla_fee
    {
        /// <summary>
        ///
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        ///
        /// </summary>
        public double amount { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class Payment_method_fee
    {
        /// <summary>
        ///
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        ///
        /// </summary>
        public double amount { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class Vat
    {
        /// <summary>
        ///
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        ///
        /// </summary>
        public double amount { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class Sales_tax
    {
        /// <summary>
        ///
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        ///
        /// </summary>
        public double amount { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class Direct_wht
    {
        /// <summary>
        ///
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        ///
        /// </summary>
        public double amount { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class Repatriation_commission
    {
        /// <summary>
        ///
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        ///
        /// </summary>
        public double amount { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class Country_wht
    {
        /// <summary>
        ///
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        ///
        /// </summary>
        public double amount { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    public sealed class User_acquisition_fee
    {
        /// <summary>
        ///
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        ///
        /// </summary>
        public double amount { get; set; }
    }*/

    /// <summary>
    /// 
    /// </summary>
    public sealed class Payment_details
    {
        /*/// <summary>
        ///
        /// </summary>
        public Payment payment { get; set; }

        /// <summary>
        ///
        /// </summary>
        public Payment_method_sum payment_method_sum { get; set; }

        /// <summary>
        ///
        /// </summary>
        public Xsolla_balance_sum xsolla_balance_sum { get; set; }

        /// <summary>
        ///
        /// </summary>
        public Payout payout { get; set; }

        /// <summary>
        ///
        /// </summary>
        public Xsolla_fee xsolla_fee { get; set; }

        /// <summary>
        ///
        /// </summary>
        public Payment_method_fee payment_method_fee { get; set; }

        /// <summary>
        ///
        /// </summary>
        public Vat vat { get; set; }

        /// <summary>
        ///
        /// </summary>
        public Sales_tax sales_tax { get; set; }

        /// <summary>
        ///
        /// </summary>
        public Direct_wht direct_wht { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string payout_currency_rate { get; set; }

        /// <summary>
        ///
        /// </summary>
        public Repatriation_commission repatriation_commission { get; set; }

        /// <summary>
        ///
        /// </summary>
        public Country_wht country_wht { get; set; }

        /// <summary>
        ///
        /// </summary>
        public User_acquisition_fee user_acquisition_fee { get; set; }*/
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class Billing
    {
        /*/// <summary>
        ///
        /// </summary>
        public string notification_type { get; set; }

        /// <summary>
        ///
        /// </summary>
        public Settings settings { get; set; }

        /// <summary>
        ///
        /// </summary>
        public Purchase purchase { get; set; }

        /// <summary>
        ///
        /// </summary>
        public Transaction transaction { get; set; }

        /// <summary>
        ///
        /// </summary>
        public Payment_details payment_details { get; set; }*/
    }
}