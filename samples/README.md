# samples

非编译目录：存放已移出三宿主编译位、但按决策保留取回能力的示例代码。

## TapdWebHookController.cs

原 `GameFrameX.Admin.Unified.Api/GameFrameX.Hub.Api/Controllers/TapdWebHookController.cs`，TAPD 项目管理 WebHook 接收器。
该集成已退役且仓内无调用方，随 Hub 瘦身移出编译位。

恢复前必须先增加 TAPD webhook 密钥 / 签名校验：当前示例允许匿名请求，且会停止容器、软删区服，不能直接重新启用。
完成验签后，将文件移回上述 Controllers 目录，并一并恢复以下守护：

- `HubTenantMiddleware` 的 `/api/TapdWebHook` 前缀及无租户头拒绝分支，以及对应中间件测试；出处为 `d2da199` 的 `GameFrameX.Admin.Unified.Api/GameFrameX.Hub.Api/Middleware/HubTenantMiddleware.cs:27–30,70–74`。
- `ThreeHostApiRoutingContractTests` 的两行：`Assert.True(typeof(TapdWebHookController).IsDefined(typeof(AuthorizeAttribute), true));` 和 `[InlineData(typeof(TapdWebHookController), nameof(TapdWebHookController.WebHookTaskFinish), "POST", null)]`；出处为 `d2da199` 的 `GameFrameX.Admin.Unified.Api/GameFrameX.Core.Tests/Architecture/ThreeHostApiRoutingContractTests.cs:93,105`。

恢复后须验证签名失败、缺租户头与合法通知三条路径，再启用外部回调。
