using GameFrameX.Application.Api.GameApplicationVersion;
using GameFrameX.Application.Client.Api.GameResourceVersion;
using GameFrameX.Core.Extension;
using GameFrameX.Entity.Client;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GameFrameX.Web.Api.Controllers;

/// <summary>
/// 资源包版本控制器
/// </summary>
public class GameAssetPackageVersionController : BaseController<ClientAssetPackageVersion>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="service"></param>
    public GameAssetPackageVersionController(BaseService<ClientAssetPackageVersion> service) : base(service)
    {
    }

    /// <summary>
    /// 获取资源包版本信息
    /// </summary>
    /// <returns></returns>
    [ProducesResponseType(typeof(AssetPackageVersionResponse), 200)]
    [HttpPost]
    public async Task<ResultModel> GetInfo([FromBody] AssetPackageVersionRequest request)
    {
        AssetPackageVersionResponse response = new AssetPackageVersionResponse();

        var gameResourceVersion = await PBaseService.GetAsync(m => m.AssetPackageName == request.AssetPackageName &&
                                                                   m.Platform == request.Platform && m.Package == request.PackageName &&
                                                                   m.AppVersion == request.AppVersion && m.Language == request.Language &&
                                                                   m.Channel == request.Channel);
        if (gameResourceVersion == null)
        {
            gameResourceVersion = await PBaseService.GetAsync(m => m.AssetPackageName == request.AssetPackageName &&
                                                                   m.Platform == request.Platform && m.Package == request.PackageName && m.AppVersion == request.AppVersion &&
                                                                   m.Language == request.Language &&
                                                                   m.Channel == ConstValue.ChannelNameDefault);
        }

        if (gameResourceVersion == null)
        {
            gameResourceVersion = await PBaseService.GetAsync(m => m.AssetPackageName == request.AssetPackageName &&
                                                                   m.Platform == request.Platform && m.Package == request.PackageName && m.AppVersion == request.AppVersion &&
                                                                   m.Language == ConstValue.LanguageDefault &&
                                                                   m.Channel == ConstValue.ChannelNameDefault);
        }

        if (gameResourceVersion == null)
        {
            gameResourceVersion = await PBaseService.GetAsync(m => m.AssetPackageName == request.AssetPackageName &&
                                                                   m.Platform == request.Platform && m.Package == request.PackageName &&
                                                                   m.AppVersion == ConstValue.AppVersionDefault &&
                                                                   m.Language == ConstValue.LanguageDefault &&
                                                                   m.Channel == ConstValue.ChannelNameDefault);
        }

        if (gameResourceVersion == null)
        {
            gameResourceVersion = await PBaseService.GetAsync(m => m.AssetPackageName == request.AssetPackageName &&
                                                                   m.Platform == request.Platform && m.Package == ConstValue.PackageNameDefault &&
                                                                   m.AppVersion == ConstValue.AppVersionDefault &&
                                                                   m.Language == ConstValue.LanguageDefault &&
                                                                   m.Channel == ConstValue.ChannelNameDefault);
        }

        if (gameResourceVersion == null)
        {
            gameResourceVersion = await PBaseService.GetAsync(m => m.AssetPackageName == request.AssetPackageName &&
                                                                   m.Platform == ConstValue.PlatformDefault && m.Package == ConstValue.PackageNameDefault &&
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
        response.AssetPackageName = gameResourceVersion.AssetPackageName;
        response.Version = gameResourceVersion.Version;
        response.Channel = gameResourceVersion.Channel;
        response.RootPath = gameResourceVersion.RootPath;
        response.PackageName = gameResourceVersion.Package;
        response.AppVersion = gameResourceVersion.AppVersion;

        return ResultModel.Create(JsonConvert.SerializeObject(response));
    }
}