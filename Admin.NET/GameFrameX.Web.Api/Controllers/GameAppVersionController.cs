using GameFrameX.Application.Api.GameApplicationVersion;
using GameFrameX.Core.Extension;
using GameFrameX.Entity.Client;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;


namespace GameFrameX.Web.Api.Controllers;

/// <summary>
/// 程序版本控制器
/// </summary>
public class GameAppVersionController : BaseController<ClientAppVersion>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="service"></param>
    public GameAppVersionController(BaseService<ClientAppVersion> service) : base(service)
    {
    }

    /// <summary>
    /// 获取游戏版本信息
    /// </summary>
    /// <returns></returns>
    [ProducesResponseType(typeof(GameAppVersionResponse), 200)]
    [HttpPost]
    public async Task<ResultModel> GetInfo([FromBody] GameAppVersionRequest gameAppVersionRequest)
    {
        GameAppVersionResponse gameAppVersionResponse = new GameAppVersionResponse();
        ClientAppVersion gameAppVersion = await PBaseService.GetAsync(m =>
            m.Platform == gameAppVersionRequest.Platform && m.Package == gameAppVersionRequest.PackageName && m.OldVersion == gameAppVersionRequest.AppVersion && m.Language == gameAppVersionRequest.Language &&
            m.Channel == gameAppVersionRequest.Channel);

        /*if (gameAppVersion == null)
        {
            gameAppVersion = await PBaseService.GetAsync(m =>
                m.Platform == request.Platform && m.Package == request.PackageName && m.OldVersion == request.AppVersion && m.Language == request.Language &&
                m.Channel == request.Channel);
        }*/

        if (gameAppVersion == null)
        {
            gameAppVersion = await PBaseService.GetAsync(m =>
                m.Platform == gameAppVersionRequest.Platform && m.Package == gameAppVersionRequest.PackageName && m.OldVersion == gameAppVersionRequest.AppVersion && m.Language == gameAppVersionRequest.Language &&
                m.Channel == ConstValue.ChannelNameDefault);
        }

        if (gameAppVersion == null)
        {
            gameAppVersion = await PBaseService.GetAsync(m =>
                m.Platform == gameAppVersionRequest.Platform && m.Package == gameAppVersionRequest.PackageName && m.OldVersion == gameAppVersionRequest.AppVersion && m.Language == ConstValue.LanguageDefault &&
                m.Channel == ConstValue.ChannelNameDefault);
        }

        if (gameAppVersion == null)
        {
            gameAppVersion = await PBaseService.GetAsync(m =>
                m.Platform == gameAppVersionRequest.Platform && m.Package == gameAppVersionRequest.PackageName && m.OldVersion == ConstValue.AppVersionDefault &&
                m.Language == ConstValue.LanguageDefault &&
                m.Channel == ConstValue.ChannelNameDefault);
        }

        if (gameAppVersion == null)
        {
            gameAppVersion = await PBaseService.GetAsync(m =>
                m.Platform == gameAppVersionRequest.Platform &&
                m.Package == ConstValue.PackageNameDefault &&
                m.Version == ConstValue.AppVersionDefault &&
                m.Language == ConstValue.LanguageDefault &&
                m.Channel == ConstValue.ChannelNameDefault);
        }

        if (gameAppVersion == null)
        {
            gameAppVersion = await PBaseService.GetAsync(m =>
                m.Platform == ConstValue.PlatformDefault && m.Package == ConstValue.PackageNameDefault && m.OldVersion == ConstValue.AppVersionDefault &&
                m.Language == ConstValue.LanguageDefault &&
                m.Channel == ConstValue.ChannelNameDefault);
        }

        if (gameAppVersion == null)
        {
            return ResultModel.Create(ResultCode.NotFound, "not found");
        }

        gameAppVersionResponse.IsForce = gameAppVersion.IsForce;
        gameAppVersionResponse.IsUpgrade = gameAppVersionRequest.AppVersion != gameAppVersion.Version;
        gameAppVersionResponse.UpdateAnnouncement = gameAppVersion.Announcement;
        gameAppVersionResponse.AppDownloadUrl = gameAppVersion.DownloadUrl;

        return ResultModel.Create(JsonConvert.SerializeObject(gameAppVersionResponse));
    }
}