using GameFrameX.Client.Api.Services.PackageVersion;
using GameFrameX.Core.Const;
using GameFrameX.Entity.Tenant.Game.RemoteConfig;
using GameFrameX.Client.Api.RemoteConfig.Dto;
using System.Linq.Expressions;

namespace GameFrameX.Client.Api.Services.RemoteConfig;

/// <summary>
/// 远程配置 Fallback 匹配器实现
/// </summary>
/// <remarks>
/// 针对 TenantGameAppRemoteConfigEntity 实体的 Fallback 匹配实现。
/// 注意：TenantGameAppRemoteConfigEntity 使用 AppVersion 字段存储应用版本（不是 OldVersion）。
/// </remarks>
public class RemoteConfigFallbackMatcher : GameAppPackageFallbackMatcher<RemoteConfigRequest, TenantGameAppRemoteConfigEntity>
{
    /// <summary>
    /// 初始化匹配器
    /// </summary>
    public RemoteConfigFallbackMatcher(
        Repository<TenantGameAppRemoteConfigEntity> repository,
        GameAppPackageResolver resolver)
        : base(repository, resolver)
    {
    }

    /// <summary>
    /// 构建应用作用域过滤条件；appIdFilter 为正值时要求 App 精确匹配，为 null 时匹配租户默认（AppId 为 null 或 0）
    /// </summary>
    protected override Expression<Func<TenantGameAppRemoteConfigEntity, bool>> BuildAppScopePredicate(long? appIdFilter)
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
    protected override Expression<Func<TenantGameAppRemoteConfigEntity, bool>> BuildExactPredicates(RemoteConfigRequest request)
    {
        return m => m.Platform == request.Platform &&
                    m.Package == request.PackageName &&
                    m.AppVersion == request.AppVersion &&
                    m.Channel == request.Channel &&
                    m.Language == request.Language;
    }

    /// <summary>
    /// 构建使用默认语言的查询条件
    /// </summary>
    protected override Expression<Func<TenantGameAppRemoteConfigEntity, bool>> BuildPredicatesWithDefaultLanguage(RemoteConfigRequest request)
    {
        return m => m.Platform == request.Platform &&
                    m.Package == request.PackageName &&
                    m.AppVersion == request.AppVersion &&
                    m.Channel == request.Channel &&
                    m.Language == GameDimensionConst.LanguageDefault;
    }

    /// <summary>
    /// 构建使用默认渠道（或请求渠道）的查询条件
    /// </summary>
    protected override Expression<Func<TenantGameAppRemoteConfigEntity, bool>> BuildPredicatesWithDefaultChannel(RemoteConfigRequest request)
    {
        // 注意：原始逻辑是 (m.Channel == GameDimensionConst.ChannelNameDefault || m.Channel == r.Channel)
        // 这意味着既匹配默认渠道，也匹配请求渠道
        return m => m.Platform == request.Platform &&
                    m.Package == request.PackageName &&
                    m.AppVersion == request.AppVersion &&
                    m.Language == GameDimensionConst.LanguageDefault &&
                    (m.Channel == GameDimensionConst.ChannelNameDefault || m.Channel == request.Channel);
    }

    /// <summary>
    /// 构建使用默认版本的查询条件
    /// </summary>
    protected override Expression<Func<TenantGameAppRemoteConfigEntity, bool>> BuildPredicatesWithDefaultVersion(RemoteConfigRequest request)
    {
        return m => m.Platform == request.Platform &&
                    m.Package == request.PackageName &&
                    m.AppVersion == GameDimensionConst.AppVersionDefault &&
                    m.Language == GameDimensionConst.LanguageDefault &&
                    (m.Channel == GameDimensionConst.ChannelNameDefault || m.Channel == request.Channel);
    }

    /// <summary>
    /// 构建使用默认包名的查询条件
    /// </summary>
    protected override Expression<Func<TenantGameAppRemoteConfigEntity, bool>> BuildPredicatesWithDefaultPackage(RemoteConfigRequest request)
    {
        return m => m.Platform == request.Platform &&
                    m.Package == GameDimensionConst.PackageNameDefault &&
                    m.AppVersion == GameDimensionConst.AppVersionDefault &&
                    m.Language == GameDimensionConst.LanguageDefault &&
                    (m.Channel == GameDimensionConst.ChannelNameDefault || m.Channel == request.Channel);
    }

    /// <summary>
    /// 构建使用默认平台的查询条件
    /// </summary>
    protected override Expression<Func<TenantGameAppRemoteConfigEntity, bool>> BuildPredicatesWithDefaultPlatform(RemoteConfigRequest request)
    {
        return m => m.Platform == GameDimensionConst.PlatformDefault &&
                    m.Package == GameDimensionConst.PackageNameDefault &&
                    m.AppVersion == GameDimensionConst.AppVersionDefault &&
                    m.Language == GameDimensionConst.LanguageDefault &&
                    (m.Channel == GameDimensionConst.ChannelNameDefault || m.Channel == request.Channel);
    }

    /// <summary>
    /// 克隆请求并修改包名
    /// </summary>
    protected override RemoteConfigRequest CloneRequestWithPackage(RemoteConfigRequest request, string newPackageName)
    {
        return new RemoteConfigRequest
        {
            Platform = request.Platform,
            PackageName = newPackageName,
            AppVersion = request.AppVersion,
            Language = request.Language,
            Channel = request.Channel,
            AppId = request.AppId,
            DeviceId = request.DeviceId,
        };
    }
}