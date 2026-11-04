using GameFrameX.Entity.Tenant.Game.App;

namespace GameFrameX.Application.ClientDelivery.AppScope;

/// <summary>校验具体分包存在且启用，空值和默认包允许兜底。</summary>
public static class PackageScopeValidator
{
    /// <summary>默认兜底包名。</summary>
    public const string DefaultPackage = "default";

    /// <summary>空值及默认包放行，其它包名必须存在且启用。</summary>
    public static async Task ValidateAsync(string? package, ISugarQueryable<TenantGameAppPackageEntity> packageQuery)
    {
        var normalized = string.IsNullOrWhiteSpace(package) ? string.Empty : package.Trim();
        if (normalized.Length == 0 || normalized == DefaultPackage)
        {
            return;
        }

        var exists = await packageQuery
                           .Where(u => u.PackageName == normalized && u.IsEnabled != false)
                           .AnyAsync();
        if (!exists)
        {
            throw BusinessError.Oh($"包名 {normalized} 不存在或未启用");
        }
    }
}