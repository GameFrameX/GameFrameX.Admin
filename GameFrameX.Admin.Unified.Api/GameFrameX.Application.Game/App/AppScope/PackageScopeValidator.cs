using GameFrameX.Entity.Tenant.Game.App;

namespace GameFrameX.Application.Game.App.AppScope;

/// <summary>
/// 分包级配置包名作用域校验器。
/// <para>
/// 统一「客户端远程配置 / 客户端版本 / 资源包版本」三类配置写入前的 Package 归属校验：
/// default / 空包名走兜底放行；具体包名必须命中当前租户 + 当前应用下启用分包。
/// </para>
/// <para>
/// 调用方传入 <c>packageRepository.AsQueryable()</c>：仓储自带的租户过滤与软删除过滤天然生效，
/// 本校验只叠加 GameApp / PackageName / IsEnabled 业务条件。
/// </para>
/// </summary>
public static class PackageScopeValidator
{
    /// <summary>
    /// 默认兜底包名占位值，对应运行时 tenant-default / app-default fallback 层。
    /// </summary>
    public const string DefaultPackage = "default";

    /// <summary>
    /// 校验包名是否属于当前租户 + 当前应用下的启用分包。
    /// <para>规则：</para>
    /// <list type="number">
    ///   <item>package 为空或等于 <see cref="DefaultPackage"/> → 直接放行（兜底语义，AppId 可为 0 / null）。</item>
    ///   <item>package 为具体包名且 appId &lt;= 0（租户默认层）→ 抛错，禁止「租户默认 + 具体包名」组合。</item>
    ///   <item>package 为具体包名且 appId &gt; 0 → 必须命中当前租户 + 当前应用下启用分包的 PackageName，否则抛错。</item>
    /// </list>
    /// </summary>
    /// <param name="appId">配置类应用作用域；0 / null 表示租户默认。</param>
    /// <param name="package">待校验的包名（可为 null / 空白 / "default"，均走兜底放行）。</param>
    /// <param name="packageQuery">分包查询源（调用方传 <c>packageRepository.AsQueryable()</c>，承载租户与软删除过滤）。</param>
    /// <returns>校验通过时完成；失败抛 <c>BusinessError.Oh</c> 友好异常。</returns>
    public static async Task ValidateAsync(long? appId, string? package, ISugarQueryable<TenantGameAppPackageEntity> packageQuery)
    {
        var normalized = string.IsNullOrWhiteSpace(package) ? string.Empty : package.Trim();
        if (normalized.Length == 0 || normalized == DefaultPackage)
        {
            return;
        }

        var resolvedAppId = appId ?? 0;
        if (resolvedAppId <= 0)
        {
            throw BusinessError.Oh($"指定包名必须选择应用，租户默认配置请使用 {DefaultPackage}");
        }

        var exists = await packageQuery
                           .Where(u => u.GameAppId == resolvedAppId && u.PackageName == normalized && u.IsEnabled != false)
                           .AnyAsync();
        if (!exists)
        {
            throw BusinessError.Oh($"包名 {normalized} 不属于当前应用或未启用");
        }
    }
}