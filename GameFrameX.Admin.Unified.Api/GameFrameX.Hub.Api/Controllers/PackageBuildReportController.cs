using GameFrameX.Core.Const;
using GameFrameX.Core.Service;
using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Entity.Business.Game.App;
using GameFrameX.Hub.Api.Dto.PackageBuildReport;
using GameFrameX.Hub.Api.Middleware;
using Microsoft.AspNetCore.Authorization;
using SqlSugar;

namespace GameFrameX.Hub.Api.Controllers;

/// <summary>
/// 打包上报控制器。
/// <para>机器对机器调用（打包机 / CI / 流水线），由 <see cref="GameAppOpenAccessAuthMiddleware"/> 鉴权（非 JWT），故标记 <c>[AllowAnonymous]</c>（C236 起 JWT 默认鉴权由各控制器显式 <c>[Authorize]</c> 承担，本控制器不参与）。</para>
/// <para>分包 Id 驱动：查分包解析 <c>PackageName / PlatformCode / ChannelCode</c>，校验分包存在且启用，
/// 资源包 → <see cref="GameAppAssetPackageVersionEntity"/>，整包 → <see cref="GameAppReleaseVersionEntity"/>，幂等 upsert（存在则更新，否则新增）。</para>
/// <para>遵循 Hub.Api Controller 惯例（Repository 直接操作 Entity，不调 Application Service），故未提层 ScopeValidator / 未复用 Application.RemoteConfig 的 CheckXxxUnique —— Controller 自写归属与查重。</para>
/// </summary>
[AllowAnonymous]
public class PackageBuildReportController : BaseController
{
    private readonly Repository<GameAppPackageEntity> _packageRepo;
    private readonly Repository<GameAppAssetPackageVersionEntity> _assetVersionRepo;
    private readonly Repository<GameAppReleaseVersionEntity> _releaseVersionRepo;

    /// <summary>
    /// 打包上报控制器构造函数。
    /// </summary>
    /// <param name="packageRepo">分包仓储（解析归属与包名 / 平台 / 渠道）。</param>
    /// <param name="assetVersionRepo">资源包版本仓储。</param>
    /// <param name="releaseVersionRepo">整包版本仓储。</param>
    public PackageBuildReportController(
        Repository<GameAppPackageEntity> packageRepo,
        Repository<GameAppAssetPackageVersionEntity> assetVersionRepo,
        Repository<GameAppReleaseVersionEntity> releaseVersionRepo)
    {
        _packageRepo = packageRepo;
        _assetVersionRepo = assetVersionRepo;
        _releaseVersionRepo = releaseVersionRepo;
    }

    /// <summary>
    /// 打包上报（资源包 / 整包）。
    /// </summary>
    /// <param name="input">上报输入。</param>
    /// <returns>上报结果。</returns>
    [HttpPost]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> Report([FromBody] PackageBuildReportRequest input)
    {
        var package = await _packageRepo.GetFirstAsync(p => p.Id == input.PackageId && p.IsEnabled == true);
        if (package == null)
        {
            return HttpJsonResultData<BaseOperationAckResponse>.Fail("分包不存在或已禁用");
        }

        var language = string.IsNullOrWhiteSpace(input.Language) ? GameDimensionConst.LanguageDefault : input.Language;
        var reportType = (input.ReportType ?? string.Empty).Trim().ToLowerInvariant();

        try
        {
            if (reportType == "asset")
            {
                await UpsertAssetVersionAsync(package, input, language);
            }
            else if (reportType == "release")
            {
                await UpsertReleaseVersionAsync(package, input, language);
            }
            else
            {
                return HttpJsonResultData<BaseOperationAckResponse>.Fail("未知上报类型（asset / release）");
            }
        }
        catch (VersionExceptions)
        {
            // 乐观锁冲突透传，保持版本写入端点的既有语义。
            throw;
        }
        catch (Exception ex)
        {
            return HttpJsonResultData<BaseOperationAckResponse>.Fail(ex.Message);
        }

        return HttpJsonResultData<BaseOperationAckResponse>.Success();
    }

    /// <summary>
    /// 资源包版本幂等 upsert（Package + AppVersion + AssetPackageName + Language 命中则更新，否则新增）。
    /// </summary>
    private async Task UpsertAssetVersionAsync(GameAppPackageEntity package, PackageBuildReportRequest input, string language)
    {
        var existing = await _assetVersionRepo.GetFirstAsync(v =>
                                                                 v.Package == package.PackageName
                                                                 && v.AppVersion == input.AppVersion
                                                                 && v.AssetPackageName == input.AssetPackageName
                                                                 && v.Language == language);

        if (existing != null)
        {
            existing.ResourceVersion = input.ResourceVersion;
            existing.RootPath = input.RootPath;
            existing.Platform = package.PlatformCode;
            existing.Channel = package.ChannelCode;
            await _assetVersionRepo.UpdateWithOptimisticLockAsync(existing);
            return;
        }

        var entity = new GameAppAssetPackageVersionEntity
        {
            Package = package.PackageName,
            Platform = package.PlatformCode,
            Channel = package.ChannelCode,
            AppVersion = input.AppVersion,
            AssetPackageName = input.AssetPackageName,
            ResourceVersion = input.ResourceVersion,
            RootPath = input.RootPath,
            Language = language,
        };
        await _assetVersionRepo.InsertAsync(entity);
    }

    /// <summary>
    /// 整包版本幂等 upsert（Package + AppVersion + Language 命中则更新，否则新增）。
    /// </summary>
    private async Task UpsertReleaseVersionAsync(GameAppPackageEntity package, PackageBuildReportRequest input, string language)
    {
        var existing = await _releaseVersionRepo.GetFirstAsync(v =>
                                                                   v.Package == package.PackageName
                                                                   && v.AppVersion == input.AppVersion
                                                                   && v.Language == language);

        if (existing != null)
        {
            existing.DownloadUrl = input.DownloadUrl;
            existing.Platform = package.PlatformCode;
            existing.Channel = package.ChannelCode;
            await _releaseVersionRepo.UpdateWithOptimisticLockAsync(existing);
            return;
        }

        var entity = new GameAppReleaseVersionEntity
        {
            Package = package.PackageName,
            Platform = package.PlatformCode,
            Channel = package.ChannelCode,
            AppVersion = input.AppVersion,
            DownloadUrl = input.DownloadUrl,
            Language = language,
        };
        await _releaseVersionRepo.InsertAsync(entity);
    }
}
