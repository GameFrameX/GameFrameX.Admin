using GameFrameX.Entity.Tenant.Game.Resource;

namespace GameFrameX.Application.Game.App.AppScope;

/// <summary>校验资源包定义存在。</summary>
public static class AssetPackageScopeValidator
{
    /// <summary>
    /// 校验版本引用的资源包定义存在。
    /// </summary>
    public static async Task ValidateAsync(string? assetPackageName, ISugarQueryable<TenantGameAppAssetPackageEntity> assetPackageQuery)
    {
        var normalizedName = assetPackageName?.Trim() ?? string.Empty;
        if (normalizedName.Length == 0)
        {
            throw BusinessError.Oh("资源包名称不能为空");
        }

        var exists = await assetPackageQuery
                           .Where(u => u.Name == normalizedName)
                           .AnyAsync();
        if (!exists)
        {
            throw BusinessError.Oh("资源包不存在");
        }
    }
}