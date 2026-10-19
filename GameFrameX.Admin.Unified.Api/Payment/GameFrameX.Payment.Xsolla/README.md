# Xsolla 订单验证功能

本文档介绍如何使用 GameFrameX.Pay.Xsolla 包中的订单验证功能。

## 功能概述

基于 [Xsolla Pay Station API](https://developers.xsolla.com/zh/api/pay-station/operation/find-transactions-fast/) 实现的订单验证功能，支持：

- 根据外部订单ID查询交易状态
- 根据交易ID查询交易详情
- 验证交易的完整性和有效性
- 支持沙盒和生产环境
- 批量交易验证
- Webhook处理

## 核心组件

### 1. TransactionManager

负责与Xsolla API交互，查询交易信息。

### 2. XsollaTransactionData

交易数据模型，实现了 `ITransactionData` 接口。

### 3. XsollaPaymentHandler.VerifyReceiptAsync

### 3. XsollaPaymentHandler.VerifyReceiptAsync

主要的验证方法，验证交易的有效性，返回 `PaymentChannelVerifyReceiptResponse` 类型。

### 4. XsollaVerifyReceiptResponse

实现了 `IPaymentVerifyReceiptResponse` 接口的Xsolla专用验证结果类，包含详细的交易信息。

## 使用方法

## 返回类型说明

### PaymentChannelVerifyReceiptResponse

验证方法返回的主要结果类型，包含以下属性：

- `IsSuccess`: 验证是否成功
- `ErrorMessage`: 失败时的错误信息
- `OrderEntity`: 订单实体对象
- `Output`: 实现了 `IPaymentVerifyReceiptResponse` 接口的具体结果

### XsollaVerifyReceiptResponse

Xsolla专用的验证结果类，实现了 `IPaymentVerifyReceiptResponse` 接口，包含详细的交易信息：

| 属性              | 类型                          | 说明           |
|-------------------|-------------------------------|----------------|
| IsSuccess         | bool                          | 验证是否成功   |
| Sku               | string                        | 商品SKU        |
| RoleId            | string                        | 角色ID         |
| OrderId           | string                        | 订单ID         |
| TransactionId     | string                        | Xsolla交易ID   |
| ExternalId        | string                        | 外部订单ID     |
| Amount            | decimal                       | 交易金额       |
| Currency          | string                        | 货币代码       |
| TransactionStatus | string                        | 交易状态       |
| TransactionTime   | DateTime                      | 交易时间       |
| IsSandbox         | bool                          | 是否为沙盒环境 |
| UserEmail         | string                        | 用户邮箱       |
| Items             | List<XsollaVerifyReceiptItem> | 购买的商品列表 |

### XsollaVerifyReceiptItem

商品项详细信息：

| 属性     | 类型    | 说明     |
|----------|---------|----------|
| Sku      | string  | 商品SKU  |
| Name     | string  | 商品名称 |
| Quantity | int     | 商品数量 |
| Price    | decimal | 商品价格 |
| Currency | string  | 价格货币 |

## 使用方法

### 基本验证示例

```csharp
using GameFrameX.Pay.Xsolla;
using GameFrameX.Pay.Xsolla.Models;
using GameFrameX.Pay.Xsolla.Option;

// 1. 配置Xsolla设置
var xsollaSettings = new XsollaPayOptionSetting
{
    MerchantId = 789907,
    ProjectId = 280066,
    ApiKey = "your_api_key_here",
    WebHookSecretKey = "your_webhook_secret_here",
    Email = "test@example.com"
};

// 2. 创建交易数据
var transactionData = new XsollaTransactionData
{
    TransactionId = "12345678",     // Xsolla交易ID
    ExternalId = "order_123456",    // 你的订单ID
    UserId = "user_789",
    Amount = 9.99m,
    Currency = "USD",
    ProductSku = "premium_pack_001",
    IsSandbox = true
};

// 3. 验证交易
using var httpClient = new HttpClient();
var paymentHandler = new XsollaPaymentHandler();
var result = await paymentHandler.VerifyReceiptAsync(
    transactionData, 
    httpClient, 
    xsollaSettings);

// 4. 处理结果
// 4. 处理结果
if (result.IsSuccess)
{
    Console.WriteLine("✅ 交易验证成功！");
    var xsollaResult = (XsollaVerifyReceiptResponse)result.Output;
    Console.WriteLine($"订单ID: {xsollaResult.OrderId}");
    Console.WriteLine($"交易ID: {xsollaResult.TransactionId}");
    Console.WriteLine($"金额: {xsollaResult.Amount} {xsollaResult.Currency}");
    Console.WriteLine($"用户ID: {xsollaResult.RoleId}");
    Console.WriteLine($"商品SKU: {xsollaResult.Sku}");
    
    // 进行发货等后续操作
    await ProcessSuccessfulPayment(result);
}
else
{
    Console.WriteLine($"❌ 交易验证失败: {result.ErrorMessage}");
}
```

### 验证流程

验证过程包含以下步骤：

1. **查询交易**: 根据外部订单ID或交易ID从Xsolla API查询交易信息
2. **状态验证**: 检查交易状态是否为成功状态（"done" 或 "paid"）
3. **金额验证**: 验证交易金额是否与预期一致
4. **货币验证**: 验证货币类型是否匹配
5. **用户验证**: 验证用户ID是否匹配
6. **商品验证**: 验证商品SKU是否匹配

### 支持的验证字段

| 字段          | 类型    | 必需 | 说明                   |
|---------------|---------|------|------------------------|
| TransactionId | string  | 否   | Xsolla交易ID           |
| ExternalId    | string  | 否   | 外部订单ID（推荐使用） |
| UserId        | string  | 否   | 用户ID                 |
| Amount        | decimal | 否   | 交易金额               |
| Currency      | string  | 否   | 货币代码               |
| ProductSku    | string  | 否   | 商品SKU                |
| IsSandbox     | bool    | 是   | 是否为沙盒环境         |

> **注意**: TransactionId 和 ExternalId 至少需要提供一个用于查询交易。

### 批量验证

```csharp
var transactions = new List<XsollaTransactionData>
{
    new() { ExternalId = "order_001", IsSandbox = true },
    new() { ExternalId = "order_002", IsSandbox = true },
    // ... 更多交易
};

var results = await XsollaVerificationExample.BatchVerifyTransactions(
    transactions, 
    xsollaSettings);

foreach (var result in results)
{
    Console.WriteLine($"订单 {result.Key}: {(result.Value ? "成功" : "失败")}");
}
```

### Webhook处理

```csharp
// 在你的Webhook控制器中
[HttpPost("xsolla-webhook")]
public async Task<IActionResult> HandleXsollaWebhook([FromBody] string payload)
{
    var success = await XsollaVerificationExample.HandleXsollaWebhook(
        payload, 
        xsollaSettings);
    
    return success ? Ok() : BadRequest();
}
```

## 错误处理

常见的验证失败原因：

- **未找到交易记录**: 提供的交易ID或外部订单ID无效
- **交易状态无效**: 交易尚未完成或已被取消
- **金额不匹配**: 实际交易金额与预期不符
- **货币类型不匹配**: 交易货币与预期不符
- **用户ID不匹配**: 交易用户与预期不符
- **商品SKU不匹配**: 交易商品与预期不符

## 最佳实践

### 1. 安全性

- 始终验证Webhook签名
- 使用HTTPS进行API调用
- 妥善保管API密钥和Webhook密钥

### 2. 性能优化

- 使用连接池复用HttpClient
- 实现适当的重试机制
- 考虑API限流，避免过于频繁的调用

### 3. 错误处理

- 记录所有验证失败的情况
- 实现人工审核机制处理异常情况
- 设置监控和告警

### 4. 数据一致性

- 使用数据库事务确保订单状态更新的原子性
- 实现幂等性处理，避免重复发货
- 定期对账，确保数据一致性

## 配置说明

### XsollaPayOptionSetting 配置项

```csharp
public class XsollaPayOptionSetting
{
    public int MerchantId { get; set; }        // 商户ID
    public int ProjectId { get; set; }         // 项目ID
    public string ApiKey { get; set; }         // API密钥
    public string WebHookSecretKey { get; set; } // Webhook密钥
    public string LoginProjectId { get; set; } // 登录项目ID
    public string ClientId { get; set; }       // OAuth2.0 客户端ID
    public string ClientSecret { get; set; }   // OAuth2.0 客户端密钥
    public string Email { get; set; }          // 沙盒环境邮箱
}
```

### 环境配置

在 `appsettings.json` 中配置：

```json
{
  "PaymentChannels": {
    "Channels": {
      "xsolla": {
        "MerchantId": 789907,
        "ProjectId": 280066,
        "ApiKey": "your_api_key_here",
        "WebHookSecretKey": "your_webhook_secret_here",
        "Email": "your_email@example.com"
      }
    }
  }
}
```

## 测试

项目包含完整的测试用例，包括：

- 单元测试
- 集成测试
- 性能测试

运行测试：

```bash
dotnet test GameFrameX.Client.Api.Tests
```

## 相关链接

- [Xsolla Pay Station API 文档](https://developers.xsolla.com/zh/api/pay-station/)
- [Xsolla Webhook 文档](https://developers.xsolla.com/zh/webhooks/)
- [GameFrameX 支付系统文档](../README-PaymentChannels.md)