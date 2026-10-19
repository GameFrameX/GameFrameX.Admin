using GameFrameX.Client.Api.Services.GameLoginTicket;
using GameFrameX.Client.Api.GameRole.Dto;
using GameFrameX.Client.Api.Middleware;
using GameFrameX.Client.Api.Services;
using GameFrameX.Core.Service;

using GameFrameX.Client.Api.GameLoginTicketKey.Dto;
namespace GameFrameX.Client.Api.GameLoginTicketKey;

/// <summary>
/// Game 登录票据公钥控制器。
/// </summary>
public class GameLoginTicketKeyController : BaseController
{
    private readonly IGameLoginTicketKeyResolver _keyResolver;

    public GameLoginTicketKeyController(IGameLoginTicketKeyResolver keyResolver)
    {
        _keyResolver = keyResolver;
    }

    /// <summary>
    /// 获取当前租户应用的 Game 登录票据公钥。
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(List<GameLoginTicketPublicKeyResponse>))]
    public async Task<HttpJsonResultData<List<GameLoginTicketPublicKeyResponse>>> PublicKeys()
    {
        if (!TryGetContextLong(TenantContextAccessor.TenantIdKey, out var tenantId))
        {
            return GameApiResult.Error<List<GameLoginTicketPublicKeyResponse>>(AuthErrorCode.AuthorizeInvalid, "租户未解析");
        }

        if (!TryGetContextLong(AppResolutionMiddleware.AppIdKey, out var appId))
        {
            return GameApiResult.Error<List<GameLoginTicketPublicKeyResponse>>(AuthErrorCode.AuthorizeInvalid, "应用未认证");
        }

        var keys = await _keyResolver.GetPublicKeysAsync(tenantId, appId);
        var output = keys.Select(key => new GameLoginTicketPublicKeyResponse
        {
            TenantId = tenantId,
            AppId = appId,
            Kid = key.Kid,
            Algorithm = key.Algorithm,
            PublicKeyPem = key.PublicKeyPem,
            NotBeforeTime = key.NotBeforeTime,
            ExpireTime = key.ExpireTime,
        }).ToList();

        return HttpJsonResultData<List<GameLoginTicketPublicKeyResponse>>.Success(output);
    }

    private bool TryGetContextLong(string key, out long value)
    {
        value = 0;
        if (!HttpContext.Items.TryGetValue(key, out var item) || item == null)
        {
            return false;
        }

        return long.TryParse(item.ToString(), out value);
    }
}