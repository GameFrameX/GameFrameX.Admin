namespace GameFrameX.Hub.Api.Dto.FeatureSwitchSync;

/// <summary>
/// 功能开关服务器同步请求（C347 Hub 通道，机器凭证驱动）。
/// <para>应用作用域不可在请求侧声明：由 <c>TenantGameAppOpenAccessAuthMiddleware</c> 按凭证绑定注入
/// <c>HttpContext.Items</c>，控制器的查询只取该注入值。</para>
/// </summary>
public class FeatureSwitchSyncRequest
{
    /// <summary>
    /// 上次同步到的投放版本号（md5 hex）。与当前版本相同 → 响应 <c>Unchanged=true</c> 且 <c>Switches</c> 为空数组。
    /// </summary>
    public string? Version { get; set; }
}
