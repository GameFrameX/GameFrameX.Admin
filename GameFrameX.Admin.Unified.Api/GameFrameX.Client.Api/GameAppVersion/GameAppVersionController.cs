using GameFrameX.Client.Api.Services.PackageVersion;
using GameFrameX.Entity.Tenant.Game.Release;
using GameFrameX.Client.Api.GameAppVersion.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Core.Middleware;
using GameFrameX.Core.Service;


namespace GameFrameX.Client.Api.GameAppVersion;

/// <summary>
/// 程序版本控制器
/// </summary>
public class GameAppVersionController : BaseController<TenantGameAppReleaseVersionEntity>
{
    private readonly IFallbackMatcher<GameAppVersionRequest, TenantGameAppReleaseVersionEntity> _matcher;

    /// <summary>
    ///
    /// </summary>
    /// <param name="service"></param>
    /// <param name="matcher"></param>
    public GameAppVersionController(SqlSugarRepository<TenantGameAppReleaseVersionEntity> service, IFallbackMatcher<GameAppVersionRequest, TenantGameAppReleaseVersionEntity> matcher) : base(service)
    {
        _matcher = matcher;
    }

    /// <summary>
    /// 获取游戏版本信息
    /// </summary>
    /// <returns></returns>
    [PublicStartupEndpoint]
    [ProducesResponseType(typeof(GameAppVersionResponse), 200)]
    [HttpPost]
    public async Task<HttpJsonResultData<GameAppVersionResponse>> GetInfo([FromBody] GameAppVersionRequest gameAppVersionRequest)
    {
        var gameAppVersion = await _matcher.MatchAsync(gameAppVersionRequest);

        if (gameAppVersion == null)
        {
            return HttpJsonResultData<GameAppVersionResponse>.NotFound();
        }

        var response = new GameAppVersionResponse();
        response.IsForce = gameAppVersion.IsForce;
        response.IsUpgrade = gameAppVersionRequest.AppVersion != gameAppVersion.AppVersion;
        response.UpdateAnnouncement = gameAppVersion.Announcement;
        response.UpdateTitle = gameAppVersion.Title ?? string.Empty;
        response.AppDownloadUrl = gameAppVersion.DownloadUrl ?? string.Empty;

        return HttpJsonResultData<GameAppVersionResponse>.Success(response);
    }
}