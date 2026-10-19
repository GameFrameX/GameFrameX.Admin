using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.Service.User;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace GameFrameX.Core.Filter;

/// <summary>
/// 租户访问门禁过滤器（Admin 端，全局注册于 <c>Web.Core/Startup</c>）。
/// <para>对已认证的非 SuperAdmin 请求，按租户启用状态与到期状态裁决：</para>
/// <list type="bullet">
/// <item>租户禁用（<c>IsEnabled==false</c>）→ 抛 TenantErrorCode.TenantDisabled（禁读禁写，「禁用即无法使用」）。</item>
/// <item>租户过期（过渡期结束）且写操作 → 抛 TenantErrorCode.TenantExpired；读操作放行。</item>
/// <item>过渡期 / 正常 → 放行（过渡期内不挡写）。</item>
/// </list>
/// <para>放行：SuperAdmin（含切目标租户代操作）、未认证请求、<see cref="BypassTenantExpirationAttribute"/> 标注的逃生通道端点。写操作按方法名前缀判定，见 <see cref="TenantReadActionClassifier"/>。</para>
/// <para>拒绝走 <c>BusinessError.Oh</c> 统一异常管道，由 <c>AdminResultProvider</c> 渲染为数字业务码 30600006/30600009 的标准响应。</para>
/// </summary>
public class TenantExpirationFilter : IAsyncActionFilter
{
    private readonly TenantAccessGate _tenantAccessGate;
    private readonly CurrentUserContext _currentUserContext;

    public TenantExpirationFilter(TenantAccessGate tenantAccessGate, CurrentUserContext currentUserContext)
    {
        _tenantAccessGate = tenantAccessGate;
        _currentUserContext = currentUserContext;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // SuperAdmin 不受限制：超管需能维护/续费任意租户（含过期/禁用租户），且超管无普通租户上下文。
        if (_currentUserContext.SuperAdmin)
        {
            await next();
            return;
        }

        // 逃生通道端点放行（[BypassTenantExpiration] 标注：登录/登出/刷新/验证码/租户状态查询），
        // 被禁用/过期租户的用户仍须能自救（登出、拿到期提示），否则会被锁死在系统内。
        if (context.Filters.Any(f => f is BypassTenantExpirationAttribute))
        {
            await next();
            return;
        }

        var isWriteAction = !TenantReadActionClassifier.IsReadAction(ResolveActionName(context));
        var verdict = await _tenantAccessGate.EvaluateAsync(_currentUserContext.TenantId, isWriteAction);

        switch (verdict)
        {
            case TenantAccessVerdict.DenyDisabled:
                throw BusinessError.Oh(TenantErrorCode.TenantDisabled);

            case TenantAccessVerdict.DenyExpiredWrite:
                throw BusinessError.Oh(TenantErrorCode.TenantExpired);
        }

        await next();
    }

    /// <summary>
    /// 解析当前 action 的 C# 方法名（判写基准）。优先取 <see cref="ControllerActionDescriptor"/>'s <see cref="System.Reflection.MethodInfo"/> 名称，回退 DisplayName。
    /// </summary>
    private static string? ResolveActionName(ActionExecutingContext context)
    {
        if (context.ActionDescriptor is ControllerActionDescriptor controllerAction)
        {
            return controllerAction.MethodInfo?.Name;
        }

        return context.ActionDescriptor.DisplayName;
    }
}