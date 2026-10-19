using GameFrameX.Client.Api.Services.PackageVersion;
using GameFrameX.Entity.Tenant.Game.Release;
using GameFrameX.Entity.Tenant.Device;
using GameFrameX.Client.Api.GameAssetPackageVersion.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Core.Middleware;
using GameFrameX.Core.Service;
using GameFrameX.Foundation.Extensions;

namespace GameFrameX.Client.Api.GameAssetPackageVersion;

/// <summary>
/// 资源包版本控制器
/// </summary>
public class GameAssetPackageVersionController : BaseController<TenantGameAppAssetPackageVersionEntity>
{
    private readonly Repository<TenantGameAppDeviceEntity> _deviceUniqueIdentifier;
    private readonly IFallbackMatcher<AssetPackageVersionRequest, TenantGameAppAssetPackageVersionEntity> _matcher;

    /// <summary>
    ///
    /// </summary>
    /// <param name="service"></param>
    /// <param name="deviceUniqueIdentifier"></param>
    /// <param name="matcher"></param>
    public GameAssetPackageVersionController(SqlSugarRepository<TenantGameAppAssetPackageVersionEntity> service, Repository<TenantGameAppDeviceEntity> deviceUniqueIdentifier, IFallbackMatcher<AssetPackageVersionRequest, TenantGameAppAssetPackageVersionEntity> matcher) : base(service)
    {
        _deviceUniqueIdentifier = deviceUniqueIdentifier;
        _matcher = matcher;
    }

    /// <summary>
    /// 获取资源包版本信息（GFX-706 起纯只读：设备注册副作用已拆至 <see cref="RegisterDevice"/> 显式端点）
    /// </summary>
    /// <returns></returns>
    [PublicStartupEndpoint]
    [ProducesResponseType(typeof(AssetPackageVersionResponse), 200)]
    [HttpPost]
    public async Task<HttpJsonResultData<AssetPackageVersionResponse>> GetInfo([FromBody] AssetPackageVersionRequest request)
    {
        // 强制租户 + 应用上下文，缺失即拒绝；并将应用作用域注入请求，供 matcher 走 App 精确 → 租户默认双层回退
        var appId = HttpContext.RequireTenantAppContext().AppId;
        request.AppId = appId;

        // 2 轮（原始包名 + 母包包名）× 应用作用域双层（App 精确 → 租户默认）× 6 级 Fallback，全部由 matcher 承担
        var gameResourceVersion = await _matcher.MatchAsync(request);

        if (gameResourceVersion == null)
        {
            return HttpJsonResultData<AssetPackageVersionResponse>.NotFound();
        }

        var response = new AssetPackageVersionResponse();
        response.Language = gameResourceVersion.Language;
        response.Platform = gameResourceVersion.Platform;
        response.AssetPackageName = gameResourceVersion.AssetPackageName;
        response.Version = gameResourceVersion.ResourceVersion;
        response.Channel = gameResourceVersion.Channel;
        response.RootPath = gameResourceVersion.RootPath;
        response.PackageName = gameResourceVersion.Package;
        response.AppVersion = gameResourceVersion.AppVersion;
        response.AssetPackagePath = Path.Combine(gameResourceVersion.RootPath, gameResourceVersion.Package, gameResourceVersion.Platform, gameResourceVersion.AppVersion, gameResourceVersion.AssetPackageName, gameResourceVersion.ResourceVersion) + Path.DirectorySeparatorChar;

        return HttpJsonResultData<AssetPackageVersionResponse>.Success(response);
    }

    /// <summary>
    /// 显式注册设备（AppId + DeviceUniqueIdentifier 幂等：已存在直接返回既有设备 Id，不重复插入）
    /// <para>GFX-706 从 GetInfo 隐式副作用拆出；注册幂等，重复调用安全。</para>
    /// <para>C379 起归 Tier 1（应用级层）：登录前但有写副作用，不打 <see cref="PublicStartupEndpointAttribute"/>，
    /// 随 App 级 RiskPolicy 进入签名灰度——FailClosed 下缺签名头仍拒绝。</para>
    /// </summary>
    /// <param name="request">设备注册请求。</param>
    /// <returns>设备 Id（幂等命中返回既有行 Id）。</returns>
    [ProducesResponseType(typeof(DeviceRegisterResponse), 200)]
    [HttpPost]
    public async Task<HttpJsonResultData<DeviceRegisterResponse>> RegisterDevice([FromBody] DeviceRegisterRequest request)
    {
        var appId = HttpContext.RequireTenantAppContext().AppId;
        if (request.DeviceUniqueIdentifier.IsNullOrEmpty())
        {
            return HttpJsonResultData<DeviceRegisterResponse>.ParamError("设备唯一标识不能为空");
        }

        var hasExist = await _deviceUniqueIdentifier.GetAsync(m => m.AppId == appId && m.DeviceUniqueIdentifier == request.DeviceUniqueIdentifier);
        if (!hasExist.IsNull())
        {
            return HttpJsonResultData<DeviceRegisterResponse>.Success(new DeviceRegisterResponse { DeviceId = hasExist.Id, });
        }

        var tenantDeviceUniqueIdentifier = new TenantGameAppDeviceEntity
        {
            AppId = appId,
            Name = "",
            Description = "",
            DeviceUniqueIdentifier = request.DeviceUniqueIdentifier,
        };
        await _deviceUniqueIdentifier.InsertAsync(tenantDeviceUniqueIdentifier);
        return HttpJsonResultData<DeviceRegisterResponse>.Success(new DeviceRegisterResponse { DeviceId = tenantDeviceUniqueIdentifier.Id, });
    }
}