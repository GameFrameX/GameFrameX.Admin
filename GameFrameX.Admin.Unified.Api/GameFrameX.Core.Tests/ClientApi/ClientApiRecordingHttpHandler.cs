using GameFrameX.Foundation.Json;
using System.Net;
using System.Text;
using GameFrameX.Client.Api.GameRole.Dto;
using GameFrameX.Client.Api.GameUserEmail.Dto;
using GameFrameX.Application.Game.Order.GameOrder.Dto;
using GameFrameX.Client.Api.GameRedeemCode.Dto;
using GameFrameX.Client.Api.GameWebShop.Dto;
using GameFrameX.Foundation.Http.Normalization;

namespace GameFrameX.Core.Tests.ClientApi;

internal sealed class ClientApiRecordingHttpHandler : HttpMessageHandler
{
    private readonly Dictionary<string, string> _responses = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<string> RequestPaths
    {
        get { return _requestPaths; }
    }

    public IReadOnlyList<string> RequestBodies
    {
        get { return _requestBodies; }
    }

    private readonly List<string> _requestPaths = new();
    private readonly List<string> _requestBodies = new();

    public static ClientApiRecordingHttpHandler WithGameServerDefaults(long createdPlayerId = 501001)
    {
        var handler = new ClientApiRecordingHttpHandler();
        handler.SetJson("/game/api/req_player_create", HttpJsonResultData<string>.Success(JsonHelper.Serialize(new PlayerCreateResponse
        {
            PlayerInfo = new PlayerInfoResponse
            {
                Id = createdPlayerId,
                Name = "Created Role",
                Level = 1u,
                State = 0,
                Avatar = 0u,
                CurrentExp = 0ul,
            },
        })).ToString());
        handler.SetJson("/game/api/req_player_change_name", HttpJsonResultData<string>.Success(JsonHelper.Serialize(new PlayerChangeNameResponse
        {
            Name = "Changed Role",
            CurrentTimes = 1,
        })).ToString());
        handler.SetJson("/game/api/req_player_get_info", HttpJsonResultData<string>.Success(JsonHelper.Serialize(new PlayerGetInfoResponse
        {
            PlayerInfo = new PlayerGetInfoResponse.PlayerDataResponse
            {
                Id = createdPlayerId,
                Name = "Refreshed Role",
                Level = 2u,
                State = 0,
                Avatar = 0u,
                CurrentExp = 0ul,
                CurrentLevelConfigId = 0,
                IsSandbox = false,
                Gender = false,
                AvatarFrame = 0u,
            },
            OnlineTime = 600,
        })).ToString());
        handler.SetJson("/game/api/req_player_create_order", HttpJsonResultData<string>.Success(JsonHelper.Serialize(new GameFrameX.Client.Api.GameWebShop.Dto.PlayerCreateOrderResponse
        {
            RoleId = createdPlayerId,
            Sku = 9001,
            PriceId = 3001,
            Amount = 650u,
            IsNeedPay = true,
            RewardsItem = new List<GameFrameX.Client.Api.GameWebShop.Dto.PropertyItemResponse> { new() { Id = 1, ItemId = 1, Count = 10, }, },
            MaxBuyCount = 1,
            AdventureSuffix = "main",
        })).ToString());
        handler.SetJson("/game/api/req_player_send_item", HttpJsonResultData<string>.Success(JsonHelper.Serialize(new GameFrameX.Client.Api.GameRedeemCode.Dto.PlayerSendItemResponse())).ToString());
        handler.SetJson("/game/api/req_player_refund_order", HttpJsonResultData<string>.Success(JsonHelper.Serialize(new PlayerRefundOrderResponse())).ToString());
        handler.SetJson("/game/api/post_mail", HttpJsonResultData<string>.Success(JsonHelper.Serialize(new GameWebShpBuySendMailResponse { ErrorCode = 0, })).ToString());
        handler.SetJson("https://zlpay.zilancreative.com/submit.php", "window.location.replace('/pay/ok')");
        return handler;
    }

    public void SetJson(string pathOrUrl, string payload)
    {
        _responses[pathOrUrl] = payload;
    }

    public int Count(string path)
    {
        return _requestPaths.Count(requestPath => string.Equals(requestPath, path, StringComparison.OrdinalIgnoreCase));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uri = request.RequestUri;
        var path = uri?.AbsolutePath ?? string.Empty;
        _requestPaths.Add(path);
        if (request.Content != null)
        {
            _requestBodies.Add(await request.Content.ReadAsStringAsync(cancellationToken));
        }

        var key = _responses.ContainsKey(uri?.ToString() ?? string.Empty)
                      ? uri!.ToString()
                      : path;
        var payload = _responses.TryGetValue(key, out var response)
                          ? response
                          : HttpJsonResultData<string>.NotFound().ToString();

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json"),
        };
    }
}