using System.Linq.Expressions;
using GameFrameX.Entity.Base;

namespace GameFrameX.Application.Game.App.AppScope;

/// <summary>
/// 应用作用域查询级默认处理扩展。
/// <para>
/// 把分页查询里重复出现的 <c>.WhereIF(input.AppId.HasValue, u =&gt; u.AppId == input.AppId.Value)</c>
/// 收敛为单一命名入口，统一 <c>AppId</c> 可空过滤的三态语义，避免各服务各写一份口径漂移。
/// </para>
/// <para>
/// 决策依据：<c>gfx-doc/dev/game-business/app-package-scope-naming-and-default-semantics.md</c>（GFX-90 / C94）。
/// 值级合法性（事实数据 <c>&gt; 0</c>、配置覆盖 <c>&gt;= 0</c>）仍由 <see cref="AppScopeRules"/> 在写入路径强制，
/// 本方法只负责读取路径的默认表达集中处理。
/// </para>
/// </summary>
public static class AppScopeQueryFilter
{
    /// <summary>
    /// 按可空 <paramref name="appId"/> 过滤应用作用域查询。
    /// <para>
    /// 三态语义：
    /// <list type="bullet">
    /// <item><c>null</c>：不应用过滤，返回原查询。适用于后台未指定应用的全局视图，结果仍受租户隔离约束。</item>
    /// <item><c>0</c>：显式过滤到租户默认行（<c>AppId == 0</c>）。仅对配置覆盖类实体有意义；事实数据不应存在 <c>AppId=0</c> 行。</item>
    /// <item>大于 0：过滤到指定应用行。</item>
    /// </list>
    /// </para>
    /// <para>
    /// 调用方传入的 <paramref name="appIdFilter" /> 应为 <c>u =&gt; u.AppId == &lt;appId 值&gt;</c> 形式，
    /// 仅在 <paramref name="appId"/> 有值时被采用，避免对可空值重复求值。
    /// </para>
    /// </summary>
    /// <typeparam name="T">实体类型，必须含 <c>AppId</c> 列。</typeparam>
    /// <param name="query">待过滤的 SqlSugar 查询。</param>
    /// <param name="appId">可空应用 ID：<c>null</c> 不过滤；<c>0</c> 租户默认；正值应用级。</param>
    /// <param name="appIdFilter">实体 <c>AppId</c> 等值过滤谓词。</param>
    /// <returns>应用了过滤条件的查询；<paramref name="appId"/> 为 <c>null</c> 时返回原查询。</returns>
    public static ISugarQueryable<T> ApplyAppIdFilter<T>(this ISugarQueryable<T> query, long? appId, Expression<Func<T, bool>> appIdFilter)
    {
        if (query == null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        if (!appId.HasValue)
        {
            return query;
        }

        return query.Where(appIdFilter);
    }

    /// <summary>
    /// 按 <see cref="IAppScoped"/> 实体的应用列过滤应用作用域查询（省略显式谓词重载，只传成员选择器）。
    /// <para>选择器必须由调用方以具体实体 lambda 写出（<c>u =&gt; u.AppId</c>）：泛型方法内直写 <c>u.AppId</c>
    /// 会编译为 <see cref="IAppScoped.AppId"/> 接口成员绑定，SqlSugar 查不到实体列映射后按裸名折叠小写生成
    /// <c>"appid"</c>，PostgreSQL 即 42703 列不存在（issue #3，密钥/邮件配置等分页 500 根因）。</para>
    /// <para>谓词经 <see cref="Expression.Equal"/> 组合选择器体与常量，生成的 SQL 与手写
    /// <c>Where(u =&gt; u.AppId == 局部变量)</c> 逐字同构（<c>"app_id" = @AppId0</c>）。三态语义同
    /// <see cref="ApplyAppIdFilter{T}(ISugarQueryable{T}, long?, Expression{Func{T, bool}})"/>。</para>
    /// </summary>
    /// <typeparam name="T">实现 <see cref="IAppScoped"/> 的实体。</typeparam>
    /// <param name="query">待过滤的 SqlSugar 查询。</param>
    /// <param name="appId">可空应用 ID：<c>null</c> 不过滤；<c>0</c> 租户默认；正值应用级。</param>
    /// <param name="appIdSelector">实体应用列成员选择器（调用方具体类型 lambda）。</param>
    /// <returns>应用了过滤条件的查询；<paramref name="appId"/> 为 <c>null</c> 时返回原查询。</returns>
    public static ISugarQueryable<T> ApplyAppIdFilter<T>(this ISugarQueryable<T> query, long? appId, Expression<Func<T, long>> appIdSelector) where T : IAppScoped
    {
        if (query == null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        if (!appId.HasValue)
        {
            return query;
        }

        var body = Expression.Equal(appIdSelector.Body, Expression.Constant(appId.Value));
        var predicate = Expression.Lambda<Func<T, bool>>(body, appIdSelector.Parameters[0]);
        return query.Where(predicate);
    }
}