namespace GameFrameX.Core.Authorization.Diagnostics;

// 本文件为 C128 诊断契约的纯数据快照模型。
// 设计原则：引擎只消费这些 POCO，绝不直接依赖 SqlSugar 实体或 HttpContext，
// 从而保证 MenuEndpointBindingDiagnosticsEngine 可被单元测试零成本调用。
// 薄壳调用方（MenuEndpointBindingDiagnosticsService）负责把 DB 行 + 历史框架 ActionDescriptor 投影成下列快照。

/// <summary>
/// 活端点快照（来自 历史框架 <c>IActionDescriptorCollectionProvider</c> 枚举）。
/// </summary>
public sealed class LiveEndpointSnapshot
{
    /// <summary>归一化 HTTP 方法（大写）。</summary>
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>规范化路由（api/... 小写）。</summary>
    public string RoutePath { get; set; } = string.Empty;

    /// <summary>是否公开（[AllowAnonymous]）。</summary>
    public bool IsPublic { get; set; }

    /// <summary>控制器类名（诊断展示用）。</summary>
    public string? Controller { get; set; }

    /// <summary>Action 方法名（诊断展示用）。</summary>
    public string? Action { get; set; }
}