using GameFrameX.Entity.Tenant.Game.Resource;

namespace GameFrameX.Application.Game.App.AppScope;

/// <summary>
/// 资源包定义应用作用域校验器。
/// </summary>
public static class AssetPackageScopeValidator
{
    /// <summary>
    /// 校验资源包定义属于当前应用，避免资源包版本跨应用引用资源包名称。
    /// </summary>
    public static async Task ValidateAsync(long? appId, string? assetPackageName, ISugarQueryable<TenantGameAppAssetPackageEntity> assetPackageQuery)
    {
        var resolvedAppId = appId ?? 0;
        if (resolvedAppId <= 0)
        {
            throw BusinessError.Oh("请选择应用后再新增资源包版本");
        }

        var normalizedName = assetPackageName?.Trim() ?? string.Empty;
        if (normalizedName.Length == 0)
        {
            throw BusinessError.Oh("资源包名称不能为空");
        }

        var exists = await assetPackageQuery
                           .Where(u => u.GameAppId == resolvedAppId && u.Name == normalizedName)
                           .AnyAsync();
        if (!exists)
        {
            throw BusinessError.Oh("资源包不属于当前应用");
        }
    }
}