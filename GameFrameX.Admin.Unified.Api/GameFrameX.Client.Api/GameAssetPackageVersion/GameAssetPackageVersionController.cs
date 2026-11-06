using GameFrameX.Client.Api.Services.PackageVersion;
using GameFrameX.Entity.Business.Game.Release;
using GameFrameX.Entity.Business.Device;
using GameFrameX.Client.Api.GameAssetPackageVersion.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Core.Middleware;
using GameFrameX.Core.Service;
using GameFrameX.Foundation.Extensions;

namespace GameFrameX.Client.Api.GameAssetPackageVersion;

/// <summary>
/// 资源包版本控制器
/// </summary>
public class GameAssetPackageVersionController : BaseController<GameAppAssetPackageVersionEntity>
{
    private readonly Repository<GameAppDeviceEntity> _deviceUniqueIdentifier;
    private readonly IFallbackMatcher<AssetPackageVersionRequest, GameAppAssetPackageVersionEntity> _matcher;

    /// <summary>
    ///
    /// </summary>
    /// <param name="service"></param>
    /// <param name="deviceUniqueIdentifier"></param>
    /// <param name="matcher"></param>
    public GameAssetPackageVersionController(SqlSugarRepository<GameAppAssetPackageVersionEntity> service, Repository<GameAppDeviceEntity> deviceUniqueIdentifier, IFallbackMatcher<AssetPackageVersionRequest, GameAppAssetPackageVersionEntity> matcher) : base(service)
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
        // 2 轮（原始包名 + 母包包名）× 6 级 Fallback，全部由 matcher 承担
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
    /// 显式注册设备（DeviceUniqueIdentifier 幂等：已存在直接返回既有设备 Id，不重复插入）
    /// <para>注册幂等，重复调用安全。</para>
    /// <para>登录前但有写副作用，不标记为公开启动端点，按主库风险策略执行签名校验。</para>
    /// </summary>
    /// <param name="request">设备注册请求。</param>
    /// <returns>设备 Id（幂等命中返回既有行 Id）。</returns>
    [ProducesResponseType(typeof(DeviceRegisterResponse), 200)]
    [HttpPost]
    public async Task<HttpJsonResultData<DeviceRegisterResponse>> RegisterDevice([FromBody] DeviceRegisterRequest request)
    {
        if (request.DeviceUniqueIdentifier.IsNullOrEmpty())
        {
            return HttpJsonResultData<DeviceRegisterResponse>.ParamError("设备唯一标识不能为空");
        }

        var hasExist = await _deviceUniqueIdentifier.GetAsync(m => m.DeviceUniqueIdentifier == request.DeviceUniqueIdentifier);
        if (!hasExist.IsNull())
        {
            return HttpJsonResultData<DeviceRegisterResponse>.Success(new DeviceRegisterResponse { DeviceId = hasExist.Id, });
        }

        var deviceUniqueIdentifier = new GameAppDeviceEntity
        {
            Name = "",
            Description = "",
            DeviceUniqueIdentifier = request.DeviceUniqueIdentifier,
        };
        await _deviceUniqueIdentifier.InsertAsync(deviceUniqueIdentifier);
        return HttpJsonResultData<DeviceRegisterResponse>.Success(new DeviceRegisterResponse { DeviceId = deviceUniqueIdentifier.Id, });
    }
}