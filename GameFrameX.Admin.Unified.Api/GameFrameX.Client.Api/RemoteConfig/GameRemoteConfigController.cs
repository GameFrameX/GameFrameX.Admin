using GameFrameX.Client.Api.Services.RemoteConfig;
using GameFrameX.Entity.Tenant.Game.RemoteConfig;
using GameFrameX.Client.Api.RemoteConfig.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Core.Middleware;
using GameFrameX.Core.Service;

namespace GameFrameX.Client.Api.RemoteConfig;

/// <summary>
/// 远程配置控制器
/// </summary>
public class GameRemoteConfigController : BaseController<TenantGameAppRemoteConfigEntity>
{
    private readonly RemoteConfigDeliveryService _deliveryService;

    /// <summary>
    ///
    /// </summary>
    /// <param name="service"></param>
    /// <param name="deliveryService">客户端远程配置投放解析服务</param>
    public GameRemoteConfigController(SqlSugarRepository<TenantGameAppRemoteConfigEntity> service, RemoteConfigDeliveryService deliveryService) : base(service)
    {
        _deliveryService = deliveryService;
    }

    /// <summary>
    /// 获取远程配置
    /// </summary>
    /// <returns></returns>
    [PublicStartupEndpoint]
    [ProducesResponseType(typeof(RemoteConfigResponse), 200)]
    [HttpPost]
    public async Task<HttpJsonResultData<RemoteConfigResponse>> GetInfo([FromBody] RemoteConfigRequest remoteConfigRequest)
    {
        // 强制租户 + 应用上下文，缺失即拒绝；并将应用作用域注入请求，供 matcher 走 App 精确 → 租户默认双层回退
        var tenantAppContext = HttpContext.RequireTenantAppContext();
        var tenantId = tenantAppContext.TenantId;
        var appId = tenantAppContext.AppId;
        remoteConfigRequest.AppId = appId;

        var gameRemoteConfig = await _deliveryService.MatchAsync(new RemoteConfigDeliveryRequest
        {
            TenantId = tenantId,
            AppId = appId,
            Platform = remoteConfigRequest.Platform,
            PackageName = remoteConfigRequest.PackageName,
            AppVersion = remoteConfigRequest.AppVersion,
            Language = remoteConfigRequest.Language,
            Channel = remoteConfigRequest.Channel,
            DeviceId = remoteConfigRequest.DeviceId,
        });

        if (gameRemoteConfig == null)
        {
            return HttpJsonResultData<RemoteConfigResponse>.NotFound();
        }

        var response = new RemoteConfigResponse();

        // IP 地理位置逻辑
        var ipaddress = Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim()
                        ?? Request.Headers["X-Real-IP"].FirstOrDefault()
                        ?? Request.HttpContext.Connection.RemoteIpAddress?.ToString()
                        ?? "0.0.0.0";
        if (Ip2LocationManager.IsEnabled)
        {
            var oIpResult = await Ip2LocationManager.Ip2Location.IPQueryAsync(ipaddress);
            switch (oIpResult.Status)
            {
                case "OK":
                    response.Location = new RemoteConfigLocation
                    {
                        Ip = oIpResult.IPAddress,
                        City = oIpResult.City,
                        CountryShort = oIpResult.CountryShort,
                        CountryLong = oIpResult.CountryLong,
                        Currency = oIpResult.Region,
                        Region = oIpResult.Region,
                        TimeZone = oIpResult.TimeZone,
                    };

                    if (response.Location.CountryLong == "Hong Kong" || response.Location.Region == "Hong Kong")
                    {
                        response.Location.Currency = "HKD";
                    }
                    else if (response.Location.CountryLong == "Taipei" || response.Location.Region == "Taipei")
                    {
                        response.Location.Currency = "TWD";
                    }
                    else if (response.Location.CountryLong == "China")
                    {
                        response.Location.Currency = "CNY";
                    }
                    else if (response.Location.CountryLong == "Singapore")
                    {
                        response.Location.Currency = "SGD";
                    }
                    else if (response.Location.CountryLong == "Malaysia")
                    {
                        response.Location.Currency = "MYR";
                    }
                    else
                    {
                        response.Location.Currency = "USD";
                    }

                    break;
                case "EMPTY_IP_ADDRESS":
                    LogHelper.Debug("IP Address cannot be blank.");
                    break;
                case "INVALID_IP_ADDRESS":
                    LogHelper.Debug("Invalid IP Address.");
                    break;
                case "MISSING_FILE":
                    LogHelper.Error("Invalid Database Path.");
                    break;
            }
        }

        response.Content = gameRemoteConfig.Content;
        response.CheckAppVersionUrl = gameRemoteConfig.CheckAppVersionUrl;
        response.CheckResourceVersionUrl = gameRemoteConfig.CheckResourceUrl;
        response.AotCodeList = gameRemoteConfig.AotCodeList;
        return HttpJsonResultData<RemoteConfigResponse>.Success(response);
    }
}