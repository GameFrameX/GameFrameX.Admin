# samples

非编译目录：存放已移出三宿主编译位、但按决策保留取回能力的示例代码。

## TapdWebHookController.cs

原 `GameFrameX.Hub.Api/Controllers/TapdWebHookController.cs`，TAPD 项目管理 WebHook 接收器。
与 Web.Api 动态 API 面重复且仓内无调用方，随 Hub 瘦身移出编译位；如需恢复 TAPD 集成，
将该文件移回 `GameFrameX.Hub.Api/Controllers/`（其依赖均在 Hub.Api 项目引用面内）即可重新编译生效。
