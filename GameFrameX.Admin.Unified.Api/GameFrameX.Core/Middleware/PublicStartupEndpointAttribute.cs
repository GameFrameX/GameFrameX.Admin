namespace GameFrameX.Core.Middleware;

/// <summary>
/// 标记启动期公开只读端点（Tier 0 公开只读层，C379）：命中标记的端点豁免传输签名强制。
/// <para><b>豁免原因</b>：此类端点登录前调用、纯只读、无副作用，响应为运营性公开数据（远程配置 / 程序版本 /
/// 资源版本），请求体仅为设备 / 包名 / 渠道等非秘密字段；业界同类接口（Firebase Remote Config、商店版本检查、
/// 热更 manifest）一律纯 HTTPS 无请求签名，且客户端本就不在启动期做签名——不豁免则签名灰度切 FailClosed
/// 会打断所有存量客户端启动流程。</para>
/// <para><b>豁免边界</b>：仅豁免传输签名强制，不等于匿名接口——仍要求租户 / 应用上下文解析
/// （<c>RequireTenantAppContext</c>）+ TLS，也不豁免加密信封 opt-in 与其他中间件（如租户到期拦截）。</para>
/// <para><b>扩围纪律</b>：新端点打标前必须先过分层判断口诀——「响应里有没有"这个人"的数据、请求有没有副作用」，
/// 两个都没有才属 Tier 0；有写副作用或防滥用诉求（如 RegisterDevice、验证码发送）归 Tier 1，随 App 级
/// RiskPolicy 进入签名灰度。扩围须同步更新守护测试 <c>PublicStartupEndpointGuardTests</c> 的预期端点集合。</para>
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class PublicStartupEndpointAttribute : Attribute
{
}
