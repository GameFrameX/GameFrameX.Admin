using GameFrameX.Core.Const;
using GameFrameX.Entity.Tenant.Game.Release;
using GameFrameX.Client.Api.GameAssetPackageVersion.Dto;
using System.Linq.Expressions;

namespace GameFrameX.Client.Api.Services.PackageVersion;

/// <summary>
/// 资源包版本 Fallback 匹配器实现
/// </summary>
/// <remarks>
/// 针对 TenantGameAppAssetPackageVersionEntity 实体的 Fallback 匹配实现，复用基类的 2 轮（原始包名 + 母包包名）与 6 级 Fallback 序列，
/// 并叠加应用作用域（AppId）优先级。
/// </remarks>
public class AssetPackageVersionFallbackMatcher : GameAppPackageFallbackMatcher<AssetPackageVersionRequest, TenantGameAppAssetPackageVersionEntity>
{
    /// <summary>
    /// 初始化匹配器
    /// </summary>
    public AssetPackageVersionFallbackMatcher(
        Repository<TenantGameAppAssetPackageVersionEntity> repository,
        GameAppPackageResolver resolver)
        : base(repository, resolver)
    {
    }

    /// <summary>
    /// 构建应用作用域过滤条件；appIdFilter 为正值时要求 App 精确匹配，为 null 时匹配租户默认（AppId 为 null 或 0）
    /// </summary>
    protected override Expression<Func<TenantGameAppAssetPackageVersionEntity, bool>> BuildAppScopePredicate(long? appIdFilter)
    {
        if (appIdFilter.HasValue && appIdFilter.Value > 0)
        {
            var appIdValue = appIdFilter.Value;
            return m => m.AppId == appIdValue;
        }

        return m => m.AppId == null || m.AppId == 0;
    }

    /// <summary>
    /// 构建完全匹配所有参数的查询条件
    /// </summary>
    protected override Expression<Func<TenantGameAppAssetPackageVersionEntity, bool>> BuildExactPredicates(AssetPackageVersionRequest request)
    {
        return m => m.AssetPackageName == request.AssetPackageName &&
                    m.Platform == request.Platform &&
                    m.Package == request.PackageName &&
                    m.AppVersion == request.AppVersion &&
                    m.Language == request.Language &&
                    m.Channel == request.Channel;
    }

    /// <summary>
    /// 构建使用默认语言的查询条件
    /// </summary>
    protected override Expression<Func<TenantGameAppAssetPackageVersionEntity, bool>> BuildPredicatesWithDefaultLanguage(AssetPackageVersionRequest request)
    {
        return m => m.AssetPackageName == request.AssetPackageName &&
                    m.Platform == request.Platform &&
                    m.Package == request.PackageName &&
                    m.AppVersion == request.AppVersion &&
                    m.Channel == request.Channel &&
                    m.Language == GameDimensionConst.LanguageDefault;
    }

    /// <summary>
    /// 构建使用默认渠道的查询条件
    /// </summary>
    protected override Expression<Func<TenantGameAppAssetPackageVersionEntity, bool>> BuildPredicatesWithDefaultChannel(AssetPackageVersionRequest request)
    {
        return m => m.AssetPackageName == request.AssetPackageName &&
                    m.Platform == request.Platform &&
                    m.Package == request.PackageName &&
                    m.AppVersion == request.AppVersion &&
                    m.Language == GameDimensionConst.LanguageDefault &&
                    m.Channel == GameDimensionConst.ChannelNameDefault;
    }

    /// <summary>
    /// 构建使用默认版本的查询条件
    /// </summary>
    protected override Expression<Func<TenantGameAppAssetPackageVersionEntity, bool>> BuildPredicatesWithDefaultVersion(AssetPackageVersionRequest request)
    {
        return m => m.AssetPackageName == request.AssetPackageName &&
                    m.Platform == request.Platform &&
                    m.Package == request.PackageName &&
                    m.AppVersion == GameDimensionConst.AppVersionDefault &&
                    m.Language == GameDimensionConst.LanguageDefault &&
                    m.Channel == GameDimensionConst.ChannelNameDefault;
    }

    /// <summary>
    /// 构建使用默认包名的查询条件
    /// </summary>
    protected override Expression<Func<TenantGameAppAssetPackageVersionEntity, bool>> BuildPredicatesWithDefaultPackage(AssetPackageVersionRequest request)
    {
        return m => m.AssetPackageName == request.AssetPackageName &&
                    m.Platform == request.Platform &&
                    m.Package == GameDimensionConst.PackageNameDefault &&
                    m.AppVersion == GameDimensionConst.AppVersionDefault &&
                    m.Language == GameDimensionConst.LanguageDefault &&
                    m.Channel == GameDimensionConst.ChannelNameDefault;
    }

    /// <summary>
    /// 构建使用默认平台的查询条件
    /// </summary>
    protected override Expression<Func<TenantGameAppAssetPackageVersionEntity, bool>> BuildPredicatesWithDefaultPlatform(AssetPackageVersionRequest request)
    {
        return m => m.AssetPackageName == request.AssetPackageName &&
                    m.Platform == GameDimensionConst.PlatformDefault &&
                    m.Package == GameDimensionConst.PackageNameDefault &&
                    m.AppVersion == GameDimensionConst.AppVersionDefault &&
                    m.Language == GameDimensionConst.LanguageDefault &&
                    m.Channel == GameDimensionConst.ChannelNameDefault;
    }

    /// <summary>
    /// 克隆请求并修改包名
    /// </summary>
    protected override AssetPackageVersionRequest CloneRequestWithPackage(AssetPackageVersionRequest request, string newPackageName)
    {
        return new AssetPackageVersionRequest
        {
            AssetPackageName = request.AssetPackageName,
            Platform = request.Platform,
            PackageName = newPackageName,
            AppVersion = request.AppVersion,
            Language = request.Language,
            Channel = request.Channel,
            DeviceUniqueIdentifier = request.DeviceUniqueIdentifier,
            AppId = request.AppId,
        };
    }
}