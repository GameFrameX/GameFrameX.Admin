using GameFrameX.Core.Const;
using Microsoft.AspNetCore.Authorization;
using GameFrameX.Core.Service;
using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Hub.Api.Dto.GameResourceVersion;
using SqlSugar;

namespace GameFrameX.Hub.Api.Controllers;

/// <summary>
/// 资源包版本控制器
/// </summary>
[Authorize]
public class GameAssetPackageVersionController : BaseController<TenantGameAppAssetPackageVersionEntity>
{
    private readonly Repository<TenantGameAppAssetPackageVersionHistoryEntity> _historyRepository;

    /// <summary>
    ///
    /// </summary>
    /// <param name="service"></param>
    /// <param name="serviceHistory"></param>
    public GameAssetPackageVersionController(Repository<TenantGameAppAssetPackageVersionEntity> service, Repository<TenantGameAppAssetPackageVersionHistoryEntity> serviceHistory) : base(service)
    {
        _historyRepository = serviceHistory;
    }

    /// <summary>
    /// 按优先级查找游戏资源版本
    /// </summary>
    /// <param name="request">请求参数</param>
    /// <returns>匹配的资源版本</returns>
    private async Task<TenantGameAppAssetPackageVersionEntity> FindGameResourceVersionAsync(AssetPackageVersionRequest request)
    {
        // 1. 完全匹配
        var result = await PRepository.GetFirstAsync(m =>
                                                         m.AssetPackageName == request.AssetPackageName &&
                                                         m.Platform == request.Platform &&
                                                         m.Package == request.PackageName &&
                                                         m.AppVersion == request.AppVersion &&
                                                         m.Language == request.Language &&
                                                         m.Channel == request.Channel);

        if (result != null)
        {
            return result;
        }

        // 2. 默认语言
        result = await PRepository.GetFirstAsync(m =>
                                                     m.AssetPackageName == request.AssetPackageName &&
                                                     m.Platform == request.Platform &&
                                                     m.Package == request.PackageName &&
                                                     m.AppVersion == request.AppVersion &&
                                                     m.Channel == request.Channel &&
                                                     m.Language == GameDimensionConst.LanguageDefault);

        if (result != null)
        {
            return result;
        }

        // 3. 默认语言和渠道
        result = await PRepository.GetFirstAsync(m =>
                                                     m.AssetPackageName == request.AssetPackageName &&
                                                     m.Platform == request.Platform &&
                                                     m.Package == request.PackageName &&
                                                     m.AppVersion == request.AppVersion &&
                                                     m.Language == GameDimensionConst.LanguageDefault &&
                                                     m.Channel == GameDimensionConst.ChannelNameDefault);

        if (result != null)
        {
            return result;
        }

        // 4. 默认版本、语言和渠道
        result = await PRepository.GetFirstAsync(m =>
                                                     m.AssetPackageName == request.AssetPackageName &&
                                                     m.Platform == request.Platform &&
                                                     m.Package == request.PackageName &&
                                                     m.AppVersion == GameDimensionConst.AppVersionDefault &&
                                                     m.Language == GameDimensionConst.LanguageDefault &&
                                                     m.Channel == GameDimensionConst.ChannelNameDefault);

        if (result != null)
        {
            return result;
        }

        // 5. 默认包名、版本、语言和渠道
        result = await PRepository.GetFirstAsync(m =>
                                                     m.AssetPackageName == request.AssetPackageName &&
                                                     m.Platform == request.Platform &&
                                                     m.Package == GameDimensionConst.PackageNameDefault &&
                                                     m.AppVersion == GameDimensionConst.AppVersionDefault &&
                                                     m.Language == GameDimensionConst.LanguageDefault &&
                                                     m.Channel == GameDimensionConst.ChannelNameDefault);

        if (result != null)
        {
            return result;
        }

        // 6. 全部默认值
        result = await PRepository.GetFirstAsync(m =>
                                                     m.AssetPackageName == request.AssetPackageName &&
                                                     m.Platform == GameDimensionConst.PlatformDefault &&
                                                     m.Package == GameDimensionConst.PackageNameDefault &&
                                                     m.AppVersion == GameDimensionConst.AppVersionDefault &&
                                                     m.Language == GameDimensionConst.LanguageDefault &&
                                                     m.Channel == GameDimensionConst.ChannelNameDefault);

        return result;
    }

    /// <summary>
    /// 更新资源包版本信息
    /// </summary>
    /// <returns></returns>
    [ProducesResponseType(typeof(HttpJsonResultData<BaseOperationAckResponse>), 200)]
    [HttpPost]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> UpdateVersion([FromBody] AssetPackageVersionRequest request)
    {
        var gameResourceVersion = await FindGameResourceVersionAsync(request);

        if (gameResourceVersion == null)
        {
            return HttpJsonResultData<BaseOperationAckResponse>.NotFound();
        }

        try
        {
            var entity = new TenantGameAppAssetPackageVersionHistoryEntity
            {
                CreatedName = "CI",
                CreatedTime = TimerHelper.UnixTimeSeconds(),
                CreatedId = 0,
                ResourceVersion = request.AssetPackageVersion,
                OldVersion = gameResourceVersion.ResourceVersion,
                VersionId = gameResourceVersion.Id,
            };
            await _historyRepository.InsertAsync(entity);

            gameResourceVersion.ResourceVersion = request.AssetPackageVersion;
            await PRepository.UpdateWithOptimisticLockAsync(gameResourceVersion);
        }
        catch (VersionExceptions)
        {
            throw;
        }
        catch (Exception ex)
        {
            return HttpJsonResultData<BaseOperationAckResponse>.Fail(ex.Message);
        }

        return HttpJsonResultData<BaseOperationAckResponse>.Success();
    }
}