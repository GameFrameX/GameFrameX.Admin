using GameFrameX.Client.Api.Services.PackageVersion;
using GameFrameX.Core.Const;
using GameFrameX.Entity.Tenant.Game.RemoteConfig;

namespace GameFrameX.Client.Api.Services.RemoteConfig;

/// <summary>
/// 客户端远程配置投放解析服务。
/// </summary>
public sealed class RemoteConfigDeliveryService
{
    private readonly Repository<TenantGameAppRemoteConfigEntity> _repository;
    private readonly GameAppPackageResolver _resolver;

    /// <summary>
    /// 初始化客户端远程配置投放解析服务。
    /// </summary>
    /// <param name="repository">远程配置仓储。</param>
    /// <param name="resolver">母包解析器。</param>
    public RemoteConfigDeliveryService(
        Repository<TenantGameAppRemoteConfigEntity> repository,
        GameAppPackageResolver resolver)
    {
        _repository = repository;
        _resolver = resolver;
    }

    /// <summary>
    /// 查询并选择当前请求最终命中的远程配置记录。
    /// </summary>
    /// <param name="request">远程配置投放请求。</param>
    /// <returns>命中的远程配置记录；未命中时返回 null。</returns>
    public async Task<TenantGameAppRemoteConfigEntity?> MatchAsync(RemoteConfigDeliveryRequest request)
    {
        var packageNames = BuildPackageCandidates(request.PackageName);
        var candidates = await _repository.AsQueryable()
                                          .Where(x => (x.AppId == request.AppId || x.AppId == null || x.AppId == 0)
                                                      && (x.Platform == request.Platform || x.Platform == GameDimensionConst.PlatformDefault)
                                                      && (packageNames.Contains(x.Package) || x.Package == GameDimensionConst.PackageNameDefault)
                                                      && (x.AppVersion == request.AppVersion || x.AppVersion == GameDimensionConst.AppVersionDefault)
                                                      && (x.Language == request.Language || x.Language == GameDimensionConst.LanguageDefault)
                                                      && (x.Channel == request.Channel || x.Channel == GameDimensionConst.ChannelNameDefault))
                                          .ToListAsync();

        var scored = candidates.Select(item => new RemoteConfigScoredCandidate(item, CalculateScore(item, request, packageNames)));
        return RemoteConfigRolloutMatcher.SelectWinner(scored, request.TenantId, request.DeviceId);
    }

    private string[] BuildPackageCandidates(string packageName)
    {
        var values = new List<string> { packageName, };
        var masterName = _resolver.Resolve(packageName);
        if (!string.IsNullOrWhiteSpace(masterName) && !string.Equals(masterName, packageName, StringComparison.Ordinal))
        {
            values.Add(masterName);
        }

        return values.Distinct(StringComparer.Ordinal).ToArray();
    }

    private static int CalculateScore(TenantGameAppRemoteConfigEntity item, RemoteConfigDeliveryRequest request, string[] packageNames)
    {
        var score = 0;
        if (item.AppId == request.AppId && request.AppId.GetValueOrDefault() > 0)
        {
            score += 64;
        }

        if (item.Platform == request.Platform)
        {
            score += 32;
        }

        if (packageNames.Contains(item.Package))
        {
            score += 16;
        }

        if (item.AppVersion == request.AppVersion)
        {
            score += 8;
        }

        if (item.Language == request.Language)
        {
            score += 4;
        }

        if (item.Channel == request.Channel)
        {
            score += 2;
        }

        return score;
    }
}