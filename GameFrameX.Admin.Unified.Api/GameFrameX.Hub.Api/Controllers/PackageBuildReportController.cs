using GameFrameX.Core.Const;
using GameFrameX.Core.Service;
using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Entity.Tenant.Game.App;
using GameFrameX.Hub.Api.Dto.PackageBuildReport;
using GameFrameX.Hub.Api.Middleware;
using Microsoft.AspNetCore.Authorization;
using SqlSugar;

namespace GameFrameX.Hub.Api.Controllers;

/// <summary>
/// 打包上报控制器。
/// <para>机器对机器调用（打包机 / CI / 流水线），由 <see cref="TenantGameAppOpenAccessAuthMiddleware"/> 鉴权（非 JWT），故标记 <c>[AllowAnonymous]</c>（C236 起 JWT 默认鉴权由各控制器显式 <c>[Authorize]</c> 承担，本控制器不参与）。</para>
/// <para>分包 Id 驱动：查分包解析 <c>GameAppId / PackageName / PlatformCode / ChannelCode</c>，校验分包属凭证绑定 App（防跨 App），
/// 资源包 → <see cref="TenantGameAppAssetPackageVersionEntity"/>，整包 → <see cref="TenantGameAppReleaseVersionEntity"/>，幂等 upsert（存在则更新，否则新增）。</para>
/// <para>遵循 Hub.Api Controller 惯例（Repository 直接操作 Entity，不调 Application Service），故未提层 ScopeValidator / 未复用 Application.RemoteConfig 的 CheckXxxUnique —— Controller 自写归属与查重。</para>
/// </summary>
[AllowAnonymous]
public class PackageBuildReportController : BaseController
{
    private readonly Repository<TenantGameAppPackageEntity> _packageRepo;
    private readonly Repository<TenantGameAppAssetPackageVersionEntity> _assetVersionRepo;
    private readonly Repository<TenantGameAppReleaseVersionEntity> _releaseVersionRepo;

    /// <summary>
    /// 打包上报控制器构造函数。
    /// </summary>
    /// <param name="packageRepo">分包仓储（解析归属与包名 / 平台 / 渠道）。</param>
    /// <param name="assetVersionRepo">资源包版本仓储。</param>
    /// <param name="releaseVersionRepo">整包版本仓储。</param>
    public PackageBuildReportController(
        Repository<TenantGameAppPackageEntity> packageRepo,
        Repository<TenantGameAppAssetPackageVersionEntity> assetVersionRepo,
        Repository<TenantGameAppReleaseVersionEntity> releaseVersionRepo)
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
        // 凭证绑定 App（中间件已鉴权并注入）
        var credentialAppId = HttpContext.Items[TenantGameAppOpenAccessAuthMiddleware.AppIdItemsKey] as long?;
        if (!credentialAppId.HasValue || credentialAppId.Value <= 0)
        {
            return HttpJsonResultData<BaseOperationAckResponse>.Fail("缺少凭证应用上下文");
        }

        // 分包解析（仓储按当前租户路由）
        var package = await _packageRepo.GetFirstAsync(p => p.Id == input.PackageId);
        if (package == null)
        {
            return HttpJsonResultData<BaseOperationAckResponse>.Fail("分包不存在");
        }

        // 归属校验：分包必须属于凭证绑定 App（跨 App 即拒，凭证滥用防线）
        if (package.GameAppId != credentialAppId.Value)
        {
            return HttpJsonResultData<BaseOperationAckResponse>.Fail("分包不属于凭证绑定应用");
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
            // 乐观锁冲突透传（对齐 GameAssetPackageVersionController 既有模式）
            throw;
        }
        catch (Exception ex)
        {
            return HttpJsonResultData<BaseOperationAckResponse>.Fail(ex.Message);
        }

        return HttpJsonResultData<BaseOperationAckResponse>.Success();
    }

    /// <summary>
    /// 资源包版本幂等 upsert（AppId + Package + AppVersion + AssetPackageName + Language 命中则更新，否则新增）。
    /// </summary>
    private async Task UpsertAssetVersionAsync(TenantGameAppPackageEntity package, PackageBuildReportRequest input, string language)
    {
        var existing = await _assetVersionRepo.GetFirstAsync(v =>
                                                                 v.AppId == package.GameAppId
                                                                 && v.Package == package.PackageName
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

        var entity = new TenantGameAppAssetPackageVersionEntity
        {
            AppId = package.GameAppId,
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
    /// 整包版本幂等 upsert（AppId + Package + AppVersion + Language 命中则更新，否则新增）。
    /// </summary>
    private async Task UpsertReleaseVersionAsync(TenantGameAppPackageEntity package, PackageBuildReportRequest input, string language)
    {
        var existing = await _releaseVersionRepo.GetFirstAsync(v =>
                                                                   v.AppId == package.GameAppId
                                                                   && v.Package == package.PackageName
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

        var entity = new TenantGameAppReleaseVersionEntity
        {
            AppId = package.GameAppId,
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