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
    /// 获取当前主库的 Game 登录票据公钥。
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(List<GameLoginTicketPublicKeyResponse>))]
    public async Task<HttpJsonResultData<List<GameLoginTicketPublicKeyResponse>>> PublicKeys()
    {
        var keys = await _keyResolver.GetPublicKeysAsync();
        var output = keys.Select(key => new GameLoginTicketPublicKeyResponse
        {
            Kid = key.Kid,
            Algorithm = key.Algorithm,
            PublicKeyPem = key.PublicKeyPem,
            NotBeforeTime = key.NotBeforeTime,
            ExpireTime = key.ExpireTime,
        }).ToList();

        return HttpJsonResultData<List<GameLoginTicketPublicKeyResponse>>.Success(output);
    }

}
