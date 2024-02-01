using GameFrameX.Application.Api.GameApplicationVersion;
using GameFrameX.Core.Extension;
using GameFrameX.Entity.Client;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GameFrameX.Web.Api.Controllers;

/// <summary>
/// 全局信息控制器
/// </summary>
public class GameGlobalInfoController : BaseController<ClientGlobalInfo>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="service"></param>
    public GameGlobalInfoController(BaseService<ClientGlobalInfo> service) : base(service)
    {
    }

    /// <summary>
    /// 获取全局信息
    /// </summary>
    /// <returns></returns>
    [ProducesResponseType(typeof(GlobalInfoResponse), 200)]
    [HttpPost]
    [HttpOptions]
    public async Task<ResultModel> GetInfo([FromBody] GlobalInfoRequest globalInfoRequest)
    {
        GlobalInfoResponse globalInfoResponse = new GlobalInfoResponse();
        ClientGlobalInfo gameGlobalInfo = await PBaseService.GetAsync(m =>
            m.Platform == globalInfoRequest.Platform
            && m.Package == globalInfoRequest.PackageName
            && m.Version == globalInfoRequest.AppVersion
            && m.Language == globalInfoRequest.Language &&
            m.Channel == globalInfoRequest.Channel);

        /*if (gameGlobalInfo == null)
        {
            gameGlobalInfo = await PBaseService.GetAsync(m =>
                m.Platform == request.Platform
                && m.Package == request.PackageName
                && m.Version == request.AppVersion
                && m.Language == request.Language &&
                m.Channel == request.Channel);
        }*/

        if (gameGlobalInfo == null)
        {
            gameGlobalInfo = await PBaseService.GetAsync(m =>
                m.Platform == globalInfoRequest.Platform
                && m.Package == globalInfoRequest.PackageName
                && m.Version == globalInfoRequest.AppVersion
                && m.Language == globalInfoRequest.Language &&
                m.Channel == ConstValue.ChannelNameDefault);
        }

        if (gameGlobalInfo == null)
        {
            gameGlobalInfo = await PBaseService.GetAsync(m =>
                m.Platform == globalInfoRequest.Platform
                && m.Package == globalInfoRequest.PackageName
                && m.Version == globalInfoRequest.AppVersion
                && m.Language == ConstValue.LanguageDefault &&
                m.Channel == ConstValue.ChannelNameDefault);
        }

        if (gameGlobalInfo == null)
        {
            gameGlobalInfo = await PBaseService.GetAsync(m =>
                m.Platform == globalInfoRequest.Platform
                && m.Package == globalInfoRequest.PackageName
                && m.Version == ConstValue.AppVersionDefault &&
                m.Language == ConstValue.LanguageDefault &&
                m.Channel == ConstValue.ChannelNameDefault);
        }

        if (gameGlobalInfo == null)
        {
            gameGlobalInfo = await PBaseService.GetAsync(m =>
                m.Platform == globalInfoRequest.Platform
                && m.Package == ConstValue.PackageNameDefault
                && m.Version == ConstValue.AppVersionDefault &&
                m.Language == ConstValue.LanguageDefault &&
                m.Channel == ConstValue.ChannelNameDefault);
        }

        if (gameGlobalInfo == null)
        {
            gameGlobalInfo = await PBaseService.GetAsync(m =>
                m.Platform == ConstValue.PlatformDefault
                && m.Package == ConstValue.PackageNameDefault
                && m.Version == ConstValue.AppVersionDefault &&
                m.Language == ConstValue.LanguageDefault &&
                m.Channel == ConstValue.ChannelNameDefault);
        }

        if (gameGlobalInfo == null)
        {
            return ResultModel.Create(ResultCode.NotFound, "not found");
        }

        globalInfoResponse.Content = gameGlobalInfo.Content;
        globalInfoResponse.CheckAppVersionUrl = gameGlobalInfo.CheckAppVersionUrl;
        globalInfoResponse.CheckResourceVersionUrl = gameGlobalInfo.CheckResourceUrl;
        return ResultModel.Create(JsonConvert.SerializeObject(globalInfoResponse));
    }
}