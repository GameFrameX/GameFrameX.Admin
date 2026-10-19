# Integrations/ — 第三方平台接入编排

第三方平台接入（支付 / 广告 / 登录 / 推送 / 分析 / ...）的编排层，按类别自包含组织。每个类别一个子目录，类别间零引用，新增类别零侵入其他类别。

## 目录约定

```
Integrations/
└─ <Category>/               # 如 Payment / Ad / Auth / Push
   ├─ *Controller.cs         # HTTP 入口（webhook 多端点保留）
   ├─ *Factory.cs            # provider 工厂 / 选择
   ├─ *Selector.cs           # 渠道路由（如下单选渠道）
   ├─ *ServiceExtensions.cs  # DI 注册（AddXxxChannels），Program.cs 调用
   ├─ *Service*.cs           # 编排 Service
   └─ Dto/                   # 该类别专用 DTO
```

namespace 统一 `GameFrameX.Client.Api.Integrations.<Category>`（Dto 用 `.Dto` 子命名空间）。

## 原则

- **自包含**：类别内 Controller + 编排 Service + Dto + DI 全在此目录。
- **类别间零引用**：Payment 不引用 Ad，反之亦然；跨类别共享走 Application 层或 Core。
- **Controller 放此目录**：ASP.NET Core 不限 Controller 物理目录，自包含优于集中放 `Controllers/`。
- **webhook 多端点保留**：支付 / 广告等每供应商回调 URL、报文、签名不同，客观需要多 Action（`NotifyXxx()`）， **不做统一入口**。
- **程序集层不动**：供应商实现在 solution 级独立项目（`Payment/` = `Pay.Abstract` + 各 provider；`ObjectStorage/` 同理）。本目录只做编排，不改 provider 实现。

## 抽象时机（YAGNI）

- 新类别的 **第一个**供应商：直接在 `Integrations/<Category>/` 内写实现，不预先抽 `Abstract`。
- **第二个**供应商要接入时：再抽 `<Category>.Abstract/` + 各 provider 独立项目（参考 `Payment/` / `ObjectStorage/` 的演进）。

## 新增类别步骤

1. 建 `Integrations/<Category>/`，namespace `GameFrameX.Client.Api.Integrations.<Category>`。
2. 放 Controller + 编排 Service + Dto + `ServiceExtensions`（DI 注册方法）。
3. `Program.cs` 调用 DI 注册方法（`builder.Services.AddXxxChannels();`）。
4. 若该类别有多供应商，按 YAGNI 时机抽 `<Category>.Abstract/` + provider 项目。

## 端点覆盖测试

`Core.Tests/ClientApi/ClientApiEndpointCoverageTests.cs` 的 `DiscoverClientApiActions()` 扫描 `GameFrameX.Client.Api.*` 下所有 `BaseController` 子类作为端点矩阵来源——新类别 Controller 自动纳入覆盖，无需改测试。

## 已有类别

- `Payment/`（C220 迁入，2026-08-12）：`GameOrderController`（下单 + 各渠道 webhook）+ `PaymentChannel*` 编排 + `PaymentOptionSettingProvider` + `PaymentWebhookPolicy`。
- `Ad/`（C221 迁入，C222 provider 模式）：`GameAdWebHookController` + `GameAdRewardService`（通用流程）+ `IAdCallbackHandler`（TopOn / AdMob / UnityAds 各 Handler）+ `AdCallbackFactory`。加平台 = 加 Handler + 一行 DI 注册，Service 不动。
