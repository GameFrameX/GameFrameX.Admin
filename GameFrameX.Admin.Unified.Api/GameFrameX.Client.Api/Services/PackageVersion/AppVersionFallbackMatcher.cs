using GameFrameX.Core.Const;
using GameFrameX.Entity.Tenant.Game.Release;
using GameFrameX.Client.Api.GameAppVersion.Dto;
using System.Linq.Expressions;

namespace GameFrameX.Client.Api.Services.PackageVersion;

/// <summary>
/// 应用版本 Fallback 匹配器实现
/// </summary>
/// <remarks>
/// 针对 TenantGameAppReleaseVersionEntity 实体的 Fallback 匹配实现。
/// 注意：TenantGameAppReleaseVersionEntity 使用 OldVersion 字段存储应用版本。
/// </remarks>
public class AppVersionFallbackMatcher : GameAppPackageFallbackMatcher<GameAppVersionRequest, TenantGameAppReleaseVersionEntity>
{
    /// <summary>
    /// 初始化匹配器
    /// </summary>
    public AppVersionFallbackMatcher(
        Repository<TenantGameAppReleaseVersionEntity> repository,
        GameAppPackageResolver resolver)
        : base(repository, resolver)
    {
    }

    /// <summary>
    /// 构建应用作用域过滤条件；appIdFilter 为正值时要求 App 精确匹配，为 null 时匹配租户默认（AppId 为 null 或 0）
    /// </summary>
    protected override Expression<Func<TenantGameAppReleaseVersionEntity, bool>> BuildAppScopePredicate(long? appIdFilter)
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
    protected override Expression<Func<TenantGameAppReleaseVersionEntity, bool>> BuildExactPredicates(GameAppVersionRequest request)
    {
        return m => m.Platform == request.Platform &&
                    m.Package == request.PackageName &&
                    m.OldVersion == request.AppVersion &&
                    m.Language == request.Language &&
                    m.Channel == request.Channel;
    }

    /// <summary>
    /// 构建使用默认语言的查询条件
    /// </summary>
    protected override Expression<Func<TenantGameAppReleaseVersionEntity, bool>> BuildPredicatesWithDefaultLanguage(GameAppVersionRequest request)
    {
        return m => m.Platform == request.Platform &&
                    m.Package == request.PackageName &&
                    m.OldVersion == request.AppVersion &&
                    m.Language == GameDimensionConst.LanguageDefault &&
                    m.Channel == request.Channel;
    }

    /// <summary>
    /// 构建使用默认渠道的查询条件
    /// </summary>
    protected override Expression<Func<TenantGameAppReleaseVersionEntity, bool>> BuildPredicatesWithDefaultChannel(GameAppVersionRequest request)
    {
        return m => m.Platform == request.Platform &&
                    m.Package == request.PackageName &&
                    m.OldVersion == request.AppVersion &&
                    m.Language == GameDimensionConst.LanguageDefault &&
                    m.Channel == GameDimensionConst.ChannelNameDefault;
    }

    /// <summary>
    /// 构建使用默认版本的查询条件
    /// </summary>
    protected override Expression<Func<TenantGameAppReleaseVersionEntity, bool>> BuildPredicatesWithDefaultVersion(GameAppVersionRequest request)
    {
        return m => m.Platform == request.Platform &&
                    m.Package == request.PackageName &&
                    m.OldVersion == GameDimensionConst.AppVersionDefault &&
                    m.Language == GameDimensionConst.LanguageDefault &&
                    m.Channel == GameDimensionConst.ChannelNameDefault;
    }

    /// <summary>
    /// 构建使用默认包名的查询条件
    /// </summary>
    protected override Expression<Func<TenantGameAppReleaseVersionEntity, bool>> BuildPredicatesWithDefaultPackage(GameAppVersionRequest request)
    {
        return m => m.Platform == request.Platform &&
                    m.Package == GameDimensionConst.PackageNameDefault &&
                    m.OldVersion == GameDimensionConst.AppVersionDefault &&
                    m.Language == GameDimensionConst.LanguageDefault &&
                    m.Channel == GameDimensionConst.ChannelNameDefault;
    }

    /// <summary>
    /// 构建使用默认平台的查询条件
    /// </summary>
    protected override Expression<Func<TenantGameAppReleaseVersionEntity, bool>> BuildPredicatesWithDefaultPlatform(GameAppVersionRequest request)
    {
        return m => m.Platform == GameDimensionConst.PlatformDefault &&
                    m.Package == GameDimensionConst.PackageNameDefault &&
                    m.OldVersion == GameDimensionConst.AppVersionDefault &&
                    m.Language == GameDimensionConst.LanguageDefault &&
                    m.Channel == GameDimensionConst.ChannelNameDefault;
    }

    /// <summary>
    /// 克隆请求并修改包名
    /// </summary>
    protected override GameAppVersionRequest CloneRequestWithPackage(GameAppVersionRequest request, string newPackageName)
    {
        return new GameAppVersionRequest
        {
            Platform = request.Platform,
            PackageName = newPackageName,
            AppVersion = request.AppVersion,
            Language = request.Language,
            Channel = request.Channel,
            AppId = request.AppId,
        };
    }
}