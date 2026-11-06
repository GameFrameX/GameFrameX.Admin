using GameFrameX.Core.Const;
using GameFrameX.Entity.Business.Game.Release;
using GameFrameX.Client.Api.GameAssetPackageVersion.Dto;
using System.Linq.Expressions;

namespace GameFrameX.Client.Api.Services.PackageVersion;

/// <summary>
/// 资源包版本 Fallback 匹配器实现
/// </summary>
/// <remarks>
/// 针对 GameAppAssetPackageVersionEntity 实体的 Fallback 匹配实现，复用基类的 2 轮（原始包名 + 母包包名）与 6 级 Fallback 序列，
/// </remarks>
public class AssetPackageVersionFallbackMatcher : GameAppPackageFallbackMatcher<AssetPackageVersionRequest, GameAppAssetPackageVersionEntity>
{
    /// <summary>
    /// 初始化匹配器
    /// </summary>
    public AssetPackageVersionFallbackMatcher(
        Repository<GameAppAssetPackageVersionEntity> repository,
        GameAppPackageResolver resolver)
        : base(repository, resolver)
    {
    }

    /// <summary>
    /// 构建完全匹配所有参数的查询条件
    /// </summary>
    protected override Expression<Func<GameAppAssetPackageVersionEntity, bool>> BuildExactPredicates(AssetPackageVersionRequest request)
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
    protected override Expression<Func<GameAppAssetPackageVersionEntity, bool>> BuildPredicatesWithDefaultLanguage(AssetPackageVersionRequest request)
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
    protected override Expression<Func<GameAppAssetPackageVersionEntity, bool>> BuildPredicatesWithDefaultChannel(AssetPackageVersionRequest request)
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
    protected override Expression<Func<GameAppAssetPackageVersionEntity, bool>> BuildPredicatesWithDefaultVersion(AssetPackageVersionRequest request)
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
    protected override Expression<Func<GameAppAssetPackageVersionEntity, bool>> BuildPredicatesWithDefaultPackage(AssetPackageVersionRequest request)
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
    protected override Expression<Func<GameAppAssetPackageVersionEntity, bool>> BuildPredicatesWithDefaultPlatform(AssetPackageVersionRequest request)
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
        };
    }
}