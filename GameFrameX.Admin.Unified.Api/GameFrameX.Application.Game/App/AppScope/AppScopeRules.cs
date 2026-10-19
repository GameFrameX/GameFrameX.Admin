using GameFrameX.Entity.Base;

namespace GameFrameX.Application.Game.App.AppScope;

/// <summary>
/// 后端应用作用域共享校验规则。
/// <para>
/// 统一事实数据 / 配置 fallback 的 <c>AppId</c> 值级口径，避免每个服务重复手写不一致的判断。
/// 规则来源：<c>gfx-doc/dev/game-business/app-scope-backend-foundation.md</c>。
/// </para>
/// <para>
/// 本类型是纯值级规则，不依赖 DI / 仓储 / HttpContext，可在单元测试、后台任务、Client.Api 三方复用。
/// 应用归属（<c>AppId</c>/<c>GameAppId</c> 对应当前租户下的真实应用）仍由各服务通过多租户过滤仓储校验，
/// 例如 <c>GameAreaEntityService.ValidateGameAppBelongsToTenantAsync</c>。
/// </para>
/// </summary>
public static class AppScopeRules
{
    /// <summary>
    /// 租户默认配置占位 AppId。<c>0</c> 仅用于配置类对象的 fallback 层，不允许写入事实数据。
    /// </summary>
    public const long TenantDefaultAppId = 0;

    /// <summary>
    /// 校验事实数据的应用作用域：<c>AppId</c> 必须大于 0。
    /// <para>
    /// 事实数据（角色、订单、账单、邮件、兑换码、兑换记录、商城订单等）禁止使用 <c>AppId=0</c> 表达应用归属。
    /// </para>
    /// </summary>
    /// <param name="appId">待校验的应用 ID。</param>
    /// <param name="entityName">对象名称，用于异常消息（默认「事实数据」）。</param>
    /// <returns>校验通过的 <paramref name="appId"/>，便于链式赋值。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="appId"/> 小于等于 0。</exception>
    public static long EnsureFactAppId(long appId, string entityName = "事实数据")
    {
        if (!IsValidFactAppId(appId))
        {
            throw new ArgumentOutOfRangeException(nameof(appId), appId, $"{entityName} 的应用Id必须大于0，不允许使用 AppId=0 表达应用归属。");
        }

        return appId;
    }

    /// <summary>
    /// 校验可空事实数据应用作用域：读取应用级事实数据时也必须显式传入 <c>AppId &gt; 0</c>。
    /// </summary>
    /// <param name="appId">待校验的可空应用 ID。</param>
    /// <param name="entityName">对象名称，用于异常消息（默认「事实数据」）。</param>
    /// <returns>校验通过的非空应用 ID。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="appId"/> 为空或小于等于 0。</exception>
    public static long EnsureFactAppId(long? appId, string entityName = "事实数据")
    {
        if (!appId.HasValue)
        {
            throw new ArgumentOutOfRangeException(nameof(appId), appId, $"{entityName} 的应用Id不能为空，请选择应用后继续。");
        }

        return EnsureFactAppId(appId.Value, entityName);
    }

    /// <summary>
    /// 校验配置类对象的应用作用域：<c>AppId</c> 必须大于等于 0。
    /// <para>
    /// <c>AppId=0</c> 表示租户默认配置（fallback）；大于 0 表示应用级覆盖。
    /// </para>
    /// </summary>
    /// <param name="appId">待校验的应用 ID。</param>
    /// <param name="entityName">对象名称，用于异常消息（默认「配置」）。</param>
    /// <returns>校验通过的 <paramref name="appId"/>，便于链式赋值。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="appId"/> 小于 0。</exception>
    public static long EnsureConfigAppId(long appId, string entityName = "配置")
    {
        if (!IsValidConfigAppId(appId))
        {
            throw new ArgumentOutOfRangeException(nameof(appId), appId, $"{entityName} 的应用Id必须大于等于0，AppId=0 表示租户默认配置。");
        }

        return appId;
    }

    /// <summary>
    /// 校验事实数据实体的应用作用域：<see cref="IAppScoped.AppId"/> 必须大于 0。
    /// <para>实体级便捷入口，等价于 <see cref="EnsureFactAppId(long, string)"/>，便于直接传实体而非显式取 <c>AppId</c>。</para>
    /// </summary>
    /// <typeparam name="T">实现 <see cref="IAppScoped"/> 的事实数据实体。</typeparam>
    /// <param name="entity">待校验实体。</param>
    /// <param name="entityName">对象名称，用于异常消息（默认「事实数据」）。</param>
    /// <returns>校验通过的 <paramref name="entity"/>，便于链式。</returns>
    public static T EnsureFactAppScope<T>(T entity, string entityName = "事实数据") where T : IAppScoped
    {
        EnsureFactAppId(entity.AppId, entityName);
        return entity;
    }

    /// <summary>
    /// 校验配置类实体的应用作用域：<see cref="IAppScoped.AppId"/> 必须大于等于 0。
    /// <para>实体级便捷入口，等价于 <see cref="EnsureConfigAppId(long, string)"/>。</para>
    /// </summary>
    /// <typeparam name="T">实现 <see cref="IAppScoped"/> 的配置类实体。</typeparam>
    /// <param name="entity">待校验实体。</param>
    /// <param name="entityName">对象名称，用于异常消息（默认「配置」）。</param>
    /// <returns>校验通过的 <paramref name="entity"/>，便于链式。</returns>
    public static T EnsureConfigAppScope<T>(T entity, string entityName = "配置") where T : IAppScoped
    {
        EnsureConfigAppId(entity.AppId, entityName);
        return entity;
    }

    /// <summary>
    /// 判断 <c>AppId</c> 是否为租户默认配置占位值。
    /// <para>仅用于配置类对象 fallback 判定，不得用于事实数据。</para>
    /// </summary>
    /// <param name="appId">待判定的应用 ID。</param>
    /// <returns><c>appId == 0</c> 时返回 true。</returns>
    public static bool IsTenantDefaultAppId(long appId)
    {
        return appId == TenantDefaultAppId;
    }

    /// <summary>
    /// 判断 <c>AppId</c> 是否为合法的事实数据应用作用域（大于 0）。
    /// </summary>
    /// <param name="appId">待判定的应用 ID。</param>
    /// <returns><c>appId &gt; 0</c> 时返回 true。</returns>
    public static bool IsValidFactAppId(long appId)
    {
        return appId > TenantDefaultAppId;
    }

    /// <summary>
    /// 判断 <c>AppId</c> 是否为合法的配置类应用作用域（大于等于 0）。
    /// </summary>
    /// <param name="appId">待判定的应用 ID。</param>
    /// <returns><c>appId &gt;= 0</c> 时返回 true。</returns>
    public static bool IsValidConfigAppId(long appId)
    {
        return appId >= TenantDefaultAppId;
    }
}