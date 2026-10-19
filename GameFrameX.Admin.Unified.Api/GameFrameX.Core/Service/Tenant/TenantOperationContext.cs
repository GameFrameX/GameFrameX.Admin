using System.Security.Claims;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Service.Tenant;

/// <summary>
/// 租户操作上下文解析器。
/// <para>普通租户始终使用 JWT 中的 <see cref="ClaimConst.TenantId"/>；请求头不能覆盖普通租户身份。</para>
/// <para>SuperAdmin 进入租户业务模式时必须显式传入 <c>GameFrameX-Target-Tenant-Id</c>，无目标租户时禁止访问租户私有表。</para>
/// </summary>
public static class TenantOperationContext
{
    private static readonly AsyncLocal<long?> AmbientTenantId = new();
    private static IHttpContextAccessor? _httpContextAccessor;

    public static HttpContext? CurrentHttpContext
    {
        get { return _httpContextAccessor?.HttpContext; }
    }

    public static ClaimsPrincipal? CurrentUser
    {
        get { return _httpContextAccessor?.HttpContext?.User; }
    }

    /// <summary>
    /// 绑定 ASP.NET Core 请求上下文访问器；启动期显式配置后，运行期不再依赖 历史框架 全局请求上下文。
    /// </summary>
    /// <param name="httpContextAccessor">当前 Host 注册的请求上下文访问器。</param>
    public static void Configure(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    /// <summary>
    /// 从当前请求解析有效租户 ID。
    /// </summary>
    /// <returns>普通租户返回 JWT 租户；SuperAdmin 仅在显式目标租户有效时返回；未登录或无目标返回 null。</returns>
    public static long? GetEffectiveTenantId()
    {
        if (AmbientTenantId.Value.HasValue)
        {
            return AmbientTenantId.Value;
        }

        var httpContext = _httpContextAccessor?.HttpContext;
        var tenantId = ResolveEffectiveTenantId(httpContext?.User, httpContext?.Request.Headers[GlobalConst.TargetTenantIdHeader].FirstOrDefault());
        if (tenantId.HasValue)
        {
            return tenantId;
        }

        return ResolveHttpContextTenantId();
    }

    /// <summary>
    /// 建立显式后台租户上下文。
    /// <para>用于后台任务、启动期校验等没有登录用户的路径；调用方必须已经验证租户来源，避免恢复隐式默认租户 fallback。</para>
    /// </summary>
    /// <param name="tenantId">已验证的目标租户 ID。</param>
    /// <returns>释放后恢复进入前的上下文。</returns>
    public static IDisposable UseTenant(long tenantId)
    {
        if (tenantId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tenantId), "租户 ID 必须大于 0。");
        }

        var previousTenantId = AmbientTenantId.Value;
        AmbientTenantId.Value = tenantId;
        return new AmbientTenantScope(previousTenantId);
    }

    /// <summary>
    /// 解析有效租户 ID；该纯函数用于测试并避免普通租户被请求头覆盖。
    /// </summary>
    /// <param name="user">当前用户 Claims。</param>
    /// <param name="targetTenantIdText">SuperAdmin 目标租户请求头值。</param>
    /// <returns>有效租户 ID；无法解析时返回 null。</returns>
    public static long? ResolveEffectiveTenantId(ClaimsPrincipal? user, string? targetTenantIdText)
    {
        if (user == null || user.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var accountType = user.FindFirst(ClaimConst.AccountType)?.Value;
        var isSuperAdmin = accountType == ((int)AccountTypeEnum.SuperAdmin).ToString();
        if (isSuperAdmin)
        {
            return TryParseTenantId(targetTenantIdText);
        }

        return TryParseTenantId(user.FindFirst(ClaimConst.TenantId)?.Value);
    }

    private static long? TryParseTenantId(string? tenantIdText)
    {
        return long.TryParse(tenantIdText, out var tenantId) && tenantId > 0 ? tenantId : null;
    }

    private static long? ResolveHttpContextTenantId()
    {
        var items = _httpContextAccessor?.HttpContext?.Items;
        if (items == null || !items.TryGetValue(ClaimConst.TenantId, out var value))
        {
            return null;
        }

        switch (value)
        {
            case long tenantId when tenantId > 0:
                return tenantId;
            case int tenantId when tenantId > 0:
                return tenantId;
            case string tenantIdText:
                return TryParseTenantId(tenantIdText);
            default:
                return null;
        }
    }

    private sealed class AmbientTenantScope : IDisposable
    {
        private readonly long? _previousTenantId;
        private bool _disposed;

        public AmbientTenantScope(long? previousTenantId)
        {
            _previousTenantId = previousTenantId;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            AmbientTenantId.Value = _previousTenantId;
            _disposed = true;
        }
    }
}