/*using System.DirectoryServices.Protocols;
using GameFrameX.Application.Base.Service;
using GameFrameX.Core.Extension;
using Http.Request.GameResourceVersion;
using Http.Response.GameResourceVersion;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GameFrameX.Web.Api.Controllers;

/// <summary>
/// 资源版本控制器
/// </summary>
public class GameResourceVersionController : BaseController<GameResourceVersionEntity>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="service"></param>
    public GameResourceVersionController(BaseService<GameResourceVersionEntity> service) : base(service)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [ProducesResponseType(typeof(ResponseResourceVersion), 200)]
    [HttpPost]
    public async Task<ResultModel> GetInfo([FromBody] ReqResourceVersion request)
    {
        ResponseResourceVersion response = new ResponseResourceVersion();

        GameResourceVersionEntity gameResourceVersion = await PBaseService.GetAsync(m => m.ModuleName == request.ModuleName &&
                                                                                         m.Platform == request.Platform && m.PackageName == request.PackageName &&
                                                                                         m.AppVersion == request.AppVersion && m.Language == request.Language &&
                                                                                         m.Channel == request.Channel && m.SubChannel == request.SubChannel);
        if (gameResourceVersion == null)
        {
            gameResourceVersion = await PBaseService.GetAsync(m => m.ModuleName == request.ModuleName &&
                                                                   m.Platform == request.Platform && m.PackageName == request.PackageName && m.AppVersion == request.AppVersion &&
                                                                   m.Language == request.Language &&
                                                                   m.Channel == request.Channel && m.SubChannel == ConstValue.SubChannelNameDefault);
        }

        if (gameResourceVersion == null)
        {
            gameResourceVersion = await PBaseService.GetAsync(m => m.ModuleName == request.ModuleName &&
                                                                   m.Platform == request.Platform && m.PackageName == request.PackageName && m.AppVersion == request.AppVersion &&
                                                                   m.Language == request.Language &&
                                                                   m.Channel == ConstValue.ChannelNameDefault);
        }

        if (gameResourceVersion == null)
        {
            gameResourceVersion = await PBaseService.GetAsync(m => m.ModuleName == request.ModuleName &&
                                                                   m.Platform == request.Platform && m.PackageName == request.PackageName && m.AppVersion == request.AppVersion &&
                                                                   m.Language == ConstValue.LanguageDefault &&
                                                                   m.Channel == ConstValue.ChannelNameDefault);
        }

        if (gameResourceVersion == null)
        {
            gameResourceVersion = await PBaseService.GetAsync(m => m.ModuleName == request.ModuleName &&
                                                                   m.Platform == request.Platform && m.PackageName == request.PackageName &&
                                                                   m.AppVersion == ConstValue.AppVersionDefault &&
                                                                   m.Language == ConstValue.LanguageDefault &&
                                                                   m.Channel == ConstValue.ChannelNameDefault);
        }

        if (gameResourceVersion == null)
        {
            gameResourceVersion = await PBaseService.GetAsync(m => m.ModuleName == request.ModuleName &&
                                                                   m.Platform == request.Platform && m.PackageName == ConstValue.PackageNameDefault &&
                                                                   m.AppVersion == ConstValue.AppVersionDefault &&
                                                                   m.Language == ConstValue.LanguageDefault &&
                                                                   m.Channel == ConstValue.ChannelNameDefault);
        }

        if (gameResourceVersion == null)
        {
            gameResourceVersion = await PBaseService.GetAsync(m => m.ModuleName == request.ModuleName &&
                                                                   m.Platform == ConstValue.PlatformDefault && m.PackageName == ConstValue.PackageNameDefault &&
                                                                   m.AppVersion == ConstValue.AppVersionDefault &&
                                                                   m.Language == ConstValue.LanguageDefault &&
                                                                   m.Channel == ConstValue.ChannelNameDefault);
        }

        if (gameResourceVersion == null)
        {
            return ResultModel.Create(ResultCode.NotFound, "not found");
        }

        response.Language = gameResourceVersion.Language;
        response.Platform = gameResourceVersion.Platform;
        response.ModuleName = gameResourceVersion.ModuleName;
        response.ResourceVersion = gameResourceVersion.ResourceVersion;
        response.Channel = gameResourceVersion.Channel;
        response.SubChannel = gameResourceVersion.SubChannel;
        response.RootPath = gameResourceVersion.RootPath;
        response.PackageName = gameResourceVersion.PackageName;
        response.AppVersion = gameResourceVersion.AppVersion;

        return ResultModel.Create(response.ToJson());
    }
}*/