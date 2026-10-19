# Google Play 支付验证文档

本文档介绍如何使用 GameFrameX.Pay.Google 模块进行 Google Play 应用内购买验证。

## 概述

Google Play 支付验证基于 Google Play Developer API v3，通过服务账号密钥进行身份验证，支持验证应用内购买的有效性。

## 核心组件

### 1. GoogleTransactionData

交易数据模型，包含验证所需的信息：

| 属性                         | 类型   | 必需 | 说明                           |
|------------------------------|--------|------|--------------------------------|
| PurchaseToken                | string | ✅   | 购买令牌（从客户端获取）       |
| ProductId                    | string | ✅   | 产品ID/SKU                     |
| PackageName                  | string | ✅   | 应用包名                       |
| OrderId                      | string | ❌   | 订单ID（用于验证）             |
| UserId                       | string | ❌   | 用户ID                         |
| ExpectedPurchaseState        | int    | ❌   | 期望的购买状态（默认0-已购买） |
| ExpectedConsumptionState     | int?   | ❌   | 期望的消耗状态                 |
| ExpectedAcknowledgementState | int?   | ❌   | 期望的确认状态                 |
| DeveloperPayload             | string | ❌   | 开发者载荷                     |
| IsSandbox                    | bool   | ❌   | 是否为沙盒环境                 |

### 2. GooglePayOptionSetting

Google Play 支付配置：

| 属性                  | 类型   | 说明                                           |
|-----------------------|--------|------------------------------------------------|
| PackageName           | string | Google Play 应用包名                           |
| ServiceAccountKey     | string | 服务账号密钥 JSON 内容（运行时注入，禁止入库） |
| IsSandbox             | bool   | 是否为沙盒环境                                 |
| TimeoutSeconds        | int    | API 超时时间（秒）                             |
| EnableDetailedLogging | bool   | 是否启用详细日志                               |

### 3. GooglePlayApiManager

Google Play API 管理器，负责与 Google Play Developer API 交互。

### 4. GooglePaymentHandler.VerifyReceiptAsync

主要的验证方法，验证交易的有效性，返回 `PaymentChannelVerifyReceiptResponse` 类型。

### 5. GoogleVerifyReceiptResponse

实现了 `IPaymentVerifyReceiptResponse` 接口的Google专用验证结果类，包含详细的购买信息。

## 返回类型说明

### PaymentChannelVerifyReceiptResponse

验证方法返回的主要结果类型，包含以下属性：

- `IsSuccess`: 验证是否成功
- `ErrorMessage`: 失败时的错误信息
- `OrderEntity`: 订单实体对象
- `Output`: 实现了 `IPaymentVerifyReceiptResponse` 接口的具体结果

### GoogleVerifyReceiptResponse

Google专用的验证结果类，包含详细的购买信息：

| 属性                        | 类型     | 说明                                     |
|-----------------------------|----------|------------------------------------------|
| IsSuccess                   | bool     | 验证是否成功                             |
| Sku                         | string   | 商品SKU                                  |
| RoleId                      | string   | 角色ID                                   |
| OrderId                     | string   | 订单ID                                   |
| PurchaseToken               | string   | 购买令牌                                 |
| ProductId                   | string   | 产品ID                                   |
| PurchaseState               | int      | 购买状态（0=已购买，1=已取消，2=待处理） |
| ConsumptionState            | int      | 消耗状态（0=尚未消耗，1=已消耗）         |
| AcknowledgementState        | int      | 确认状态（0=尚未确认，1=已确认）         |
| PurchaseType                | int?     | 购买类型（0=测试，1=促销，2=奖励）       |
| PurchaseTimeMillis          | long     | 购买时间（毫秒时间戳）                   |
| PurchaseTime                | DateTime | 购买时间                                 |
| RegionCode                  | string   | 地区代码                                 |
| DeveloperPayload            | string   | 开发者载荷                               |
| ObfuscatedExternalAccountId | string   | 混淆的外部账户ID                         |
| ObfuscatedExternalProfileId | string   | 混淆的外部配置文件ID                     |
| Quantity                    | int      | 购买数量                                 |
| RefundableQuantity          | int      | 可退款数量                               |
| IsSandbox                   | bool     | 是否为沙盒环境                           |
| PackageName                 | string   | 应用包名                                 |

## 使用方法

### 基本验证示例

```csharp
// 1. 创建Google支付配置
var googleSettings = new GooglePayOptionSetting
{
    PackageName = "com.example.mygame",
    ServiceAccountKey = "{ ...服务账号 JSON 全文... }", // 运行时注入，禁止入库凭证文件
    IsSandbox = true, // 沙盒环境
    TimeoutSeconds = 30,
    EnableDetailedLogging = true
};

// 2. 创建交易数据
var transactionData = new GoogleTransactionData
{
    PurchaseToken = "abcdefghijklmnopqrstuvwxyz123456", // 从客户端获取的购买令牌
    ProductId = "premium_pack_001", // 商品SKU
    PackageName = "com.example.mygame", // 应用包名
    OrderId = "GPA.1234-5678-9012-34567", // 可选：订单ID
    UserId = "user_123456", // 可选：用户ID
    ExpectedPurchaseState = 0, // 期望状态：0=已购买
    IsSandbox = true
};

// 3. 创建支付处理器
var paymentHandler = new GooglePaymentHandler();
var httpClient = new HttpClient();

// 4. 验证交易
var result = await paymentHandler.VerifyReceiptAsync(
    transactionData, httpClient, googleSettings);

// 5. 处理结果
if (result.IsSuccess)
{
    Console.WriteLine("✅ Google Play交易验证成功！");
    var googleResult = (GoogleVerifyReceiptResponse)result.Output;
    Console.WriteLine($"订单ID: {googleResult.OrderId}");
    Console.WriteLine($"购买令牌: {googleResult.PurchaseToken}");
    Console.WriteLine($"产品ID: {googleResult.ProductId}");
    Console.WriteLine($"购买状态: {googleResult.PurchaseState}");
    Console.WriteLine($"购买时间: {googleResult.PurchaseTime}");
    
    // 进行发货等后续操作
    await ProcessSuccessfulPayment(result);
}
else
{
    Console.WriteLine($"❌ Google Play交易验证失败: {result.ErrorMessage}");
}
```

## 验证流程

1. **参数验证** - 检查必需参数（购买令牌、产品ID、包名）
2. **API调用** - 通过Google Play Developer API查询购买信息
3. **状态验证** - 验证购买状态是否为"已购买"
4. **数据验证** - 验证订单ID、开发者载荷等信息（如果提供）
5. **状态匹配** - 验证购买状态、消耗状态、确认状态等
6. **结果返回** - 返回详细的验证结果

## 配置要求

### 1. Google Play Console 配置

- 在 Google Play Console 中启用 Google Play Developer API
- 创建服务账号并下载密钥文件
- 为服务账号分配适当的权限

### 2. 服务账号密钥

服务账号密钥通过支付渠道配置（`GooglePaymentServiceAccountKey`，标记为 `Sensitive`） **运行时注入**，填入 Google Cloud 服务账号 JSON 全文。

- **仅接受 JSON 内容**：直接传入服务账号 JSON 文本；缺失时 `GooglePlayApiManager.Initialize` fail-closed（抛异常），不会回退读取产物目录下的明文凭证文件。
- **凭证禁止入库**：凭证文件（`service-account.json` / `client_secret.json` 等）不得提交到仓库；`.gitignore` 已覆盖 `GameFrameX.Client.Api/Google/*.json`，CI 门禁 `credential-leak-gate` 会扫描 `private_key` / `client_secret` / `GOCSPX-` / `BEGIN PRIVATE KEY` 等关键词阻止回归。
- **轮换**：在 GCP Console 重新签发密钥后，更新支付渠道配置中的 JSON 内容即可，无需改动代码或重新部署凭证文件。

### 3. 权限要求

服务账号需要以下权限：

- `https://www.googleapis.com/auth/androidpublisher`

## 状态说明

### 购买状态 (PurchaseState)

- `0`: 已购买 - 购买成功且有效
- `1`: 已取消 - 购买已被取消
- `2`: 待处理 - 购买正在处理中

### 消耗状态 (ConsumptionState)

- `0`: 尚未消耗 - 商品尚未被消耗
- `1`: 已消耗 - 商品已被消耗（适用于消耗性商品）

### 确认状态 (AcknowledgementState)

- `0`: 尚未确认 - 购买尚未被确认
- `1`: 已确认 - 购买已被确认

### 购买类型 (PurchaseType)

- `0`: 测试 - 测试购买（如许可测试账户）
- `1`: 促销 - 促销购买（如使用促销代码）
- `2`: 奖励 - 奖励购买（如观看广告获得）

## 最佳实践

### 1. 安全性

- 妥善保管服务账号密钥文件
- 在服务器端进行验证，不要在客户端验证
- 定期轮换服务账号密钥

### 2. 错误处理

- 实现重试机制处理网络错误
- 记录验证失败的详细日志
- 对异常情况进行适当的用户提示

### 3. 性能优化

- 使用连接池复用HTTP连接
- 实现缓存机制避免重复验证
- 设置合适的超时时间

### 4. 业务逻辑

- 检查购买是否需要确认
- 处理消耗性商品的消耗状态
- 实现幂等性避免重复发货

## 常见问题

### Q: 如何处理未确认的购买？

A: 检查 `AcknowledgementState`，如果为0，建议通过客户端SDK确认购买。

### Q: 消耗性商品如何处理？

A: 检查 `ConsumptionState`，消耗性商品需要通过客户端SDK消耗后才能再次购买。

### Q: 如何区分测试购买和正式购买？

A: 检查 `PurchaseType` 属性，0表示测试购买。

### Q: 验证失败如何处理？

A: 记录详细的错误日志，检查网络连接、API权限、参数格式等。

## 商品数据格式

```json
[
  {
    "sku": "product_001",
    "packageName": "com.example.app",
    "productType": "inapp",
    "status": "active",
    "purchaseType": "managedUser",
    "defaultPrice": {
      "priceMicros": "990000",
      "currency": "USD"
    },
    "prices": {
      "US": {
        "priceMicros": "990000",
        "currency": "USD"
      }
    },
    "listings": {
      "en-US": {
        "title": "Product Title",
        "description": "Product Description"
      }
    },
    "defaultLanguage": "en-US"
  }
]
```

## 示例代码

完整的示例代码请参考 `Examples/GoogleVerificationExample.cs` 文件。