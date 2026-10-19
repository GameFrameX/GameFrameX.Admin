# GameFrameX Apple支付模块

这个模块提供了完整的Apple支付集成功能，支持App Store收据验证和Server API v2。

## 功能特性

- ✅ App Store收据验证
- ✅ Server API v2支持
- ✅ 沙盒和生产环境支持
- ✅ 自动重试机制
- ✅ 完整的错误处理
- ✅ 配置灵活管理
- ✅ 单元测试覆盖

## 快速开始

### 1. 安装依赖

在你的项目中添加对Apple支付模块的引用：

```xml
<ProjectReference Include="..\GameFrameX.Pay.Apple\GameFrameX.Pay.Apple.csproj" />
```

### 2. 配置服务

在 `Program.cs` 中注册Apple支付服务：

```csharp
// 方式1：从配置文件读取
builder.Services.AddApplePayment(builder.Configuration);

// 方式2：代码配置
builder.Services.AddApplePayment(config =>
{
    config.BundleId = "com.yourcompany.yourgame";
    config.Environment = "sandbox";
    config.SharedSecret = "your-shared-secret";
    config.UseServerApiV2 = true;
});
```

### 3. 配置文件设置

在 `appsettings.json` 中添加Apple支付配置：

```json
{
  "PaymentChannels": {
    "Apple": {
      "BundleId": "com.yourcompany.yourgame",
      "Environment": "sandbox",
      "ApiKeyId": "your-api-key-id",
      "IssuerId": "your-issuer-id",
      "PrivateKey": "your-private-key-pem-content",
      "SharedSecret": "your-shared-secret",
      "UseServerApiV2": true,
      "ReceiptValidationTimeoutSeconds": 30
    }
  }
}
```

### 4. 使用示例

```csharp
[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentChannelFactory _paymentFactory;

    public PaymentController(IPaymentChannelFactory paymentFactory)
    {
        _paymentFactory = paymentFactory;
    }

    [HttpPost("apple/verify")]
    public async Task<IActionResult> VerifyAppleReceipt([FromBody] AppleReceiptRequest request)
    {
        var appleHandler = _paymentFactory.GetHandler("apple");
        if (appleHandler == null)
        {
            return BadRequest("Apple支付渠道未配置");
        }

        var result = await appleHandler.CreateOrderAsync(
            orderEntity: request.Order,
            orderInput: request.Input,
            user: request.User,
            httpClient: HttpContext.RequestServices.GetRequiredService<HttpClient>()
        );

        if (result.IsSuccess)
        {
            return Ok(result.Output);
        }

        return BadRequest(result.ErrorMessage);
    }
}
```

## 配置说明

### 基础配置

| 配置项       | 类型   | 必填 | 说明                          |
|--------------|--------|------|-------------------------------|
| BundleId     | string | ✅   | 应用的Bundle ID               |
| Environment  | string | ✅   | 环境：sandbox 或 production   |
| SharedSecret | string | ✅   | App Store Connect中的共享密钥 |

### Server API v2配置

| 配置项         | 类型   | 必填 | 说明                              |
|----------------|--------|------|-----------------------------------|
| UseServerApiV2 | bool   | ❌   | 是否使用Server API v2（默认true） |
| ApiKeyId       | string | ⚠️   | API密钥ID（使用v2时必填）         |
| IssuerId       | string | ⚠️   | 发行商ID（使用v2时必填）          |
| PrivateKey     | string | ⚠️   | 私钥内容（使用v2时必填）          |

### 高级配置

| 配置项                          | 类型 | 默认值 | 说明             |
|---------------------------------|------|--------|------------------|
| ReceiptValidationTimeoutSeconds | int  | 30     | 收据验证超时时间 |

## 环境配置

### 沙盒环境

```json
{
  "Environment": "sandbox"
}
```

- 验证URL: `https://sandbox.itunes.apple.com/verifyReceipt`
- Server API: `https://api.storekit-sandbox.itunes.apple.com`

### 生产环境

```json
{
  "Environment": "production"
}
```

- 验证URL: `https://buy.itunes.apple.com/verifyReceipt`
- Server API: `https://api.storekit.itunes.apple.com`

## 错误处理

Apple支付模块提供了完整的错误处理机制：

```csharp
var result = await appleHandler.CreateOrderAsync(...);

if (!result.IsSuccess)
{
    // 处理错误
    Console.WriteLine($"Apple支付失败: {result.ErrorMessage}");
    
    // 根据错误类型进行不同处理
    if (result.ErrorMessage.Contains("收据无效"))
    {
        // 处理收据验证失败
    }
    else if (result.ErrorMessage.Contains("网络"))
    {
        // 处理网络错误，可以重试
    }
}
```

## 测试

运行Apple支付相关测试：

```bash
# 运行所有Apple支付测试
dotnet test --filter "ApplePayment"

# 运行特定测试类
dotnet test --filter "ApplePaymentTests"
```

## 常见问题

### Q: 收据验证失败怎么办？

A: 检查以下几点：

1. Bundle ID是否正确
2. 环境配置是否匹配（沙盒/生产）
3. 共享密钥是否正确
4. 收据数据是否完整

### Q: Server API v2认证失败？

A: 检查以下配置：

1. API密钥ID是否正确
2. 发行商ID是否正确
3. 私钥文件路径是否正确
4. 私钥文件格式是否为.p8

### Q: 如何切换环境？

A: 修改配置中的Environment字段：

- 开发/测试：`"sandbox"`
- 生产：`"production"`

## 更多信息

- [Apple App Store Server API文档](https://developer.apple.com/documentation/appstoreserverapi)
- [Apple收据验证指南](https://developer.apple.com/documentation/storekit/in-app_purchase/validating_receipts_with_the_app_store)
- [GameFrameX支付架构文档](../README-PaymentChannels.md)