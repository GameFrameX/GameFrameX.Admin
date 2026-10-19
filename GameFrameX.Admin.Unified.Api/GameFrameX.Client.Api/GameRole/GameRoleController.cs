using GameFrameX.Client.Api.Services.GameLoginTicket;
using GameFrameX.Client.Api.Services.Auth;
using GameFrameX.Application.Game.ServerManager;
using GameFrameX.Client.Api.GameLoginTicketKey.Dto;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Penalty.GamePenalty;
using GameFrameX.Client.Api.GameRole.Dto;
using GameFrameX.Client.Api.Middleware;
using GameFrameX.Client.Api.Services;
using GameFrameX.Core.Service;
using GameFrameX.Entity.Enum;
using GameFrameX.Foundation.Extensions;

namespace GameFrameX.Client.Api.GameRole;

/// <summary>
/// 程序版本控制器
/// </summary>
public class GameRoleController : BaseController<TenantGameRoleEntity>
{
    private readonly HttpClient _httpClientFactory;
    private readonly ClientApiTokenIssuer _clientApiTokenIssuer;
    private readonly GameLoginTicketIssuer _gameLoginTicketIssuer;
    private readonly Repository<TenantGameAreaEntity> _areaService;
    private readonly ITenantGamePenaltyEnforcementService _tenantGamePenaltyEnforcementService;

    /// <summary>
    /// </summary>
    /// <param name="service"></param>
    /// <param name="httpClientFactory"></param>
    /// <param name="clientApiTokenIssuer"></param>
    /// <param name="gameLoginTicketIssuer"></param>
    /// <param name="areaService"></param>
    /// <param name="tenantGamePenaltyEnforcementService"></param>
    public GameRoleController(HttpClient httpClientFactory,
        SqlSugarRepository<TenantGameRoleEntity> service,
        Repository<TenantGameAreaEntity> areaService,
        ClientApiTokenIssuer clientApiTokenIssuer,
        GameLoginTicketIssuer gameLoginTicketIssuer,
        ITenantGamePenaltyEnforcementService tenantGamePenaltyEnforcementService) : base(service)
    {
        _httpClientFactory = httpClientFactory;
        _clientApiTokenIssuer = clientApiTokenIssuer;
        _gameLoginTicketIssuer = gameLoginTicketIssuer;
        _areaService = areaService;
        _tenantGamePenaltyEnforcementService = tenantGamePenaltyEnforcementService;
    }

    /// <summary>
    /// 修改角色名称
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(PlayerChangeNameResponse))]
    public async Task<HttpJsonResultData<PlayerChangeNameResponse>> ChangePlayerName([FromBody] GamePlayerChangeNameRequest input)
    {
        if (input.Name.IsNullOrEmptyOrWhiteSpace())
        {
            return GameApiResult.Error<PlayerChangeNameResponse>(GameRoleErrorCode.RoleNameNotEmpty, "角色名称不能为空");
        }

        var gameRoleEntity = await PRepository.GetFirstAsync(u => u.Id == input.PlayerId);
        if (gameRoleEntity == null)
        {
            return GameApiResult.Error<PlayerChangeNameResponse>(GameRoleErrorCode.RoleNotFound, "角色不存在");
        }

        var gameAreaEntity = await _areaService.AsQueryable().Filter(null, true).Where(u => u.IsDeleted == false).FirstAsync(u => u.Id == input.AreaId);
        if (gameAreaEntity == null)
        {
            return GameApiResult.Error<PlayerChangeNameResponse>(AreaErrorCode.AreaNotFound, "区服不存在");
        }

        var content = await _httpClientFactory.PostJsonToStringAsync(GameServerEndpointUrlBuilder.Build(gameAreaEntity, GameServerEndpointKindEnum.ClientDispatch, "req_player_change_name"), new PlayerChangeNameRequest { PlayerId = input.PlayerId, Name = input.Name, });
        if (content == null)
        {
            return GameApiResult.Error<PlayerChangeNameResponse>(SystemErrorCode.InnerError, "游戏服务器响应错误");
        }

        var httpJsonResultData = content.ToHttpJsonResult<PlayerChangeNameResponse>();
        if (!httpJsonResultData.IsSuccess)
        {
            return HttpJsonResultData<PlayerChangeNameResponse>.Error(httpJsonResultData.Code, "修改角色名称失败");
        }

        gameRoleEntity.Name = input.Name;
        var updateRows = await PRepository.Context.UpdateWithOptimisticLockAsync(gameRoleEntity);
        if (updateRows > 0)
        {
            return HttpJsonResultData<PlayerChangeNameResponse>.Success(httpJsonResultData.Data);
        }

        return GameApiResult.Error<PlayerChangeNameResponse>(SystemErrorCode.InnerError, "修改角色名称失败");
    }

    /// <summary>
    /// 增加游戏角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(TenantGameRoleEntityResponse))]
    public async Task<HttpJsonResultData<TenantGameRoleEntityResponse>> Add([FromBody] AddTenantGameRoleEntityRequest input)
    {
        // 应用作用域：AppId 只来自可信中间件上下文，不接受请求体覆盖（app-scope-backend-foundation.md §5.2）
        if (!TryGetContextLong(AppResolutionMiddleware.AppIdKey, out var appId))
        {
            return GameApiResult.Error<TenantGameRoleEntityResponse>(AuthErrorCode.AuthorizeInvalid, "应用未认证");
        }

        AppScopeRules.EnsureFactAppId(appId, "游戏角色");

        var user = await PRepository.AsSugarClient().Queryable<TenantGameUserEntity>().Where(u => u.Id == input.UserId).FirstAsync();
        if (user == null)
        {
            return GameApiResult.Error<TenantGameRoleEntityResponse>(AccountErrorCode.AccountExists, "账号不存在");
        }

        var gameAreaEntity = await PRepository.AsSugarClient().Queryable<TenantGameAreaEntity>().Filter(null, true).Where(u => u.IsDeleted == false).Where(u => u.Id == input.AreaId).FirstAsync();
        if (gameAreaEntity == null)
        {
            return GameApiResult.Error<TenantGameRoleEntityResponse>(AreaErrorCode.AreaNotFound, "区服不存在");
        }

        if (gameAreaEntity.ServerStatus != GameServerStatus.Online)
        {
            return GameApiResult.Error<TenantGameRoleEntityResponse>(AreaErrorCode.AreaCannotRegister, "当前状态不能注册");
        }

        if (gameAreaEntity.ServerStatus == GameServerStatus.Full)
        {
            return GameApiResult.Error<TenantGameRoleEntityResponse>(AreaErrorCode.AreaHasFull, "当前区服已满员");
        }

        var content = await _httpClientFactory.PostJsonToStringAsync(GameServerEndpointUrlBuilder.Build(gameAreaEntity, GameServerEndpointKindEnum.ClientDispatch, "req_player_create"),
                                                                     new GamePlayerRegisterRequest
                                                                     {
                                                                         UserId = input.UserId,
                                                                         Name = input.Name,
                                                                         HairstyleConfigId = input.HairstyleConfigId,
                                                                         HeroConfigId = input.HeroConfigId,
                                                                     });
        if (content == null)
        {
            return GameApiResult.Error<TenantGameRoleEntityResponse>(SystemErrorCode.InnerError, "游戏服务器响应错误");
        }

        var respPlayerCreate = content.ToHttpJsonResult<PlayerCreateResponse>();
        if (respPlayerCreate.IsSuccess)
        {
            var entity = input.Adapt<TenantGameRoleEntity>();
            entity.Id = respPlayerCreate.Data.PlayerInfo.Id;
            entity.Level = (int)respPlayerCreate.Data.PlayerInfo.Level;
            entity.CreatedTime = TimerHelper.UnixTimeSeconds();
            entity.AppId = appId; // 事实数据应用作用域，来自可信中间件上下文
            // entity.LastLoginTime = DateTime.Now;
            var res = await PRepository.InsertAsync(entity);
            if (res)
            {
                var result = entity.Adapt<TenantGameRoleEntityResponse>();
                result.HairstyleConfigId = input.HairstyleConfigId;
                result.HeroConfigId = input.HeroConfigId;
                return HttpJsonResultData<TenantGameRoleEntityResponse>.Success(result);
            }
        }

        return HttpJsonResultData<TenantGameRoleEntityResponse>.Error(respPlayerCreate.Code, "创建角色失败");
    }

    /// <summary>
    ///     删除游戏角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> Delete([FromBody] DeleteTenantGameRoleEntityRequest input)
    {
        var entity = await PRepository.GetFirstAsync(u => u.Id == input.Id);
        if (entity == null)
        {
            return HttpJsonResultData<BaseOperationAckResponse>.NotFound();
        }

        await PRepository.FakeDeleteAsync(entity); //假删除
        // ponytail: 删除成败原本序列化 bool 进 data（"true"/"false"），C227 起 data 恒 null 由 code==0 表达成功；若未来需要区分软删失败，升级为携带状态的 Output
        return HttpJsonResultData<BaseOperationAckResponse>.Success();
    }

    /// <summary>
    ///     更新游戏角色
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(TenantGameRoleEntityResponse))]
    public async Task<HttpJsonResultData<TenantGameRoleEntityResponse>> Update([FromBody] UpdateTenantGameRoleEntityRequest input)
    {
        var existing = await PRepository.GetFirstAsync(u => u.Id == input.Id);
        var entity = input.Adapt<TenantGameRoleEntity>();
        // 应用归属不可变：从既有实体回填 AppId，避免 Adapt 默认值覆盖为 0 导致角色跨应用丢失边界。
        if (existing != null)
        {
            entity.AppId = existing.AppId;
        }

        var res = await PRepository.AsUpdateable(entity).IgnoreColumns(true).ExecuteCommandWithOptLockAsync(true);
        var result = entity.Adapt<TenantGameRoleEntityResponse>();
        return HttpJsonResultData<TenantGameRoleEntityResponse>.Success(result);
    }


    /// <summary>
    /// 获取账号角色列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(List<TenantGameRoleEntityResponse>))]
    public async Task<HttpJsonResultData<List<TenantGameRoleEntityResponse>>> GetList([FromBody] GetListTenantGameRoleEntityRequest input)
    {
        var gameRoleEntities = await PRepository.GetListAsync(a => a.UserId == input.UserId);
        var result = new List<TenantGameRoleEntityResponse>(gameRoleEntities.Count);
        foreach (var entity in gameRoleEntities)
        {
            result.Add(entity.Adapt<TenantGameRoleEntityResponse>());
        }

        return HttpJsonResultData<List<TenantGameRoleEntityResponse>>.Success(result);
    }

    /// <summary>
    /// 登录账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GameLoginTicketResponse))]
    public async Task<HttpJsonResultData<GameLoginTicketResponse>> Login([FromBody] GamePlayerLoginRequest input)
    {
        if (HasForbiddenContextField(input))
        {
            return GameApiResult.Error<GameLoginTicketResponse>(AuthErrorCode.AuthorizeInvalid, "请求体不能包含租户、应用或账号上下文字段");
        }

        if (!TryGetContextLong(TenantContextAccessor.TenantIdKey, out var tenantId))
        {
            return GameApiResult.Error<GameLoginTicketResponse>(AuthErrorCode.AuthorizeInvalid, "租户未解析");
        }

        if (!TryGetContextLong(AppResolutionMiddleware.AppIdKey, out var appId))
        {
            return GameApiResult.Error<GameLoginTicketResponse>(AuthErrorCode.AuthorizeInvalid, "应用未认证");
        }

        var tokenValidation = await _clientApiTokenIssuer.ValidateAsync(input.AccessToken);
        if (!tokenValidation.IsValid)
        {
            return GameApiResult.Error<GameLoginTicketResponse>(AuthErrorCode.AuthorizeInvalid, "授权无效");
        }

        var user = await PRepository.AsSugarClient().Queryable<TenantGameUserEntity>().Where(u => u.Id == tokenValidation.UserId).FirstAsync();
        if (user == null)
        {
            _gameLoginTicketIssuer.AuditRejected(tenantId, appId, tokenValidation.UserId, input.PlayerId, input.AreaId, "account_not_found");
            return GameApiResult.Error<GameLoginTicketResponse>(AccountErrorCode.AccountNotFound, "账号不存在");
        }

        if (user.Status == GameUserStatus.Deleted)
        {
            _gameLoginTicketIssuer.AuditRejected(tenantId, appId, user.Id, input.PlayerId, input.AreaId, "account_deleted");
            return GameApiResult.Error<GameLoginTicketResponse>(AccountErrorCode.AccountDeleting, "账号处于注销状态");
        }

        if (user.Status != GameUserStatus.Active)
        {
            _gameLoginTicketIssuer.AuditRejected(tenantId, appId, user.Id, input.PlayerId, input.AreaId, "account_disabled");
            return GameApiResult.Error<GameLoginTicketResponse>(AccountErrorCode.AccountDisabled, "账号已被禁用");
        }

        // 处罚事实源登录阻断漏斗：账号级封禁/限制登录命中即拦截
        var accountBlocking = await _tenantGamePenaltyEnforcementService.GetLoginBlockingPenaltyAsync(PenaltyTargetTypeEnum.Account, user.Id.ToString(), appId);
        if (accountBlocking != null)
        {
            _gameLoginTicketIssuer.AuditRejected(tenantId, appId, user.Id, input.PlayerId, input.AreaId, "account_banned");
            return GameApiResult.Error<GameLoginTicketResponse>(AccountErrorCode.AccountBanned, $"账号已被封禁：{accountBlocking.Reason}");
        }

        //区服状态判断
        var gameAreaEntity = await PRepository.AsSugarClient().Queryable<TenantGameAreaEntity>().Filter(null, true).Where(u => u.IsDeleted == false).Where(u => u.Id == input.AreaId).FirstAsync();
        if (gameAreaEntity == null)
        {
            _gameLoginTicketIssuer.AuditRejected(tenantId, appId, user.Id, input.PlayerId, input.AreaId, "area_not_found");
            return GameApiResult.Error<GameLoginTicketResponse>(AreaErrorCode.AreaNotFound, "区服不存在");
        }

        //不能登录
        if (gameAreaEntity.ServerStatus == GameServerStatus.Maintenance ||
            gameAreaEntity.ServerStatus == GameServerStatus.Offline ||
            gameAreaEntity.ServerStatus == GameServerStatus.Full)
        {
            _gameLoginTicketIssuer.AuditRejected(tenantId, appId, user.Id, input.PlayerId, input.AreaId, "area_cannot_login");
            return GameApiResult.Error<GameLoginTicketResponse>(AreaErrorCode.AreaCannotLogin, "当前区服不能登录");
        }

        var entity = await PRepository.GetSingleAsync(a => a.Id == input.PlayerId && a.AreaId == input.AreaId);
        if (entity == null)
        {
            _gameLoginTicketIssuer.AuditRejected(tenantId, appId, user.Id, input.PlayerId, input.AreaId, "role_not_found");
            return GameApiResult.Error<GameLoginTicketResponse>(GameRoleErrorCode.RoleNotFound, "角色不存在");
        }

        if (entity.UserId != user.Id)
        {
            _gameLoginTicketIssuer.AuditRejected(tenantId, appId, user.Id, input.PlayerId, input.AreaId, "role_not_owned");
            return GameApiResult.Error<GameLoginTicketResponse>(AuthErrorCode.AuthorizeInvalid, "授权无效");
        }

        // 处罚事实源登录阻断漏斗：角色级封禁/限制登录命中即拦截（按区服作用域过滤）
        var roleBlocking = await _tenantGamePenaltyEnforcementService.GetLoginBlockingPenaltyAsync(PenaltyTargetTypeEnum.Role, entity.Id.ToString(), appId, input.AreaId);
        if (roleBlocking != null)
        {
            _gameLoginTicketIssuer.AuditRejected(tenantId, appId, user.Id, input.PlayerId, input.AreaId, "role_banned");
            return GameApiResult.Error<GameLoginTicketResponse>(GameRoleErrorCode.RoleBanned, $"角色已被封禁：{roleBlocking.Reason}");
        }

        var result = entity.Adapt<TenantGameRoleEntityResponse>();
        await TryRefreshPlayerInfo(entity, gameAreaEntity);

        var routeTarget = BuildRouteTarget(gameAreaEntity);
        var ticket = await _gameLoginTicketIssuer.IssueAsync(new GameLoginTicketIssueContext(tenantId, appId, user.Id, entity.Id, input.AreaId, routeTarget));
        var output = new GameLoginTicketResponse
        {
            Player = result,
            GameLoginTicket = ticket.Ticket,
            TicketExpireTime = ticket.ExpireTime,
            TargetGameServerId = input.AreaId <= int.MaxValue ? (int)input.AreaId : null,
            TargetGameServerInstanceId = input.AreaId,
            TargetGameOuterHost = TryGetHost(gameAreaEntity.WebSocketUrl) ?? TryGetHost(gameAreaEntity.TcpSocketUrl),
            TargetGameOuterPort = TryGetPort(gameAreaEntity.WebSocketUrl) ?? TryGetPort(gameAreaEntity.TcpSocketUrl),
            RouteTarget = routeTarget,
        };

        return HttpJsonResultData<GameLoginTicketResponse>.Success(output);
    }

    private async Task TryRefreshPlayerInfo(TenantGameRoleEntity entity, TenantGameAreaEntity gameAreaEntity)
    {
        var areaAddress = GameServerEndpointUrlBuilder.Build(gameAreaEntity, GameServerEndpointKindEnum.ClientDispatch, "req_player_get_info");
        var request = new PlayerGetInfoRequest
        {
            PlayerId = entity.Id,
        };

        try
        {
            var content = await _httpClientFactory.PostJsonToStringAsync(areaAddress, request);
            var response = content.ToHttpJsonResult<PlayerGetInfoResponse>();
            if (response.IsSuccess)
            {
                entity.Avatar = response.Data.PlayerInfo.Avatar;
                entity.State = response.Data.PlayerInfo.State;
                entity.AvatarFrame = response.Data.PlayerInfo.AvatarFrame;
                entity.CurrentLevelConfigId = response.Data.PlayerInfo.CurrentLevelConfigId;
                entity.LastLoginTime = TimerHelper.UnixTimeSeconds();
                entity.OnlineTime = response.Data.OnlineTime / 60;

                await PRepository.Context.UpdateWithOptimisticLockAsync(entity);
            }
        }
        catch (Exception ex)
        {
            LogHelper.Warning($"刷新玩家信息失败，不阻断 Game 登录票据签发。playerId={entity.Id} areaId={entity.AreaId} message={ex.Message}");
        }
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

    private static bool HasForbiddenContextField(GamePlayerLoginRequest input)
    {
        if (input.ExtraFields == null)
        {
            return false;
        }

        return input.ExtraFields.Keys.Any(k =>
                                              string.Equals(k, "tenantId", StringComparison.OrdinalIgnoreCase) ||
                                              string.Equals(k, "appId", StringComparison.OrdinalIgnoreCase) ||
                                              string.Equals(k, "userId", StringComparison.OrdinalIgnoreCase));
    }

    private static string BuildRouteTarget(TenantGameAreaEntity area)
    {
        if (area.WebSocketUrl.IsNotNullOrEmpty())
        {
            return area.WebSocketUrl;
        }

        if (area.TcpSocketUrl.IsNotNullOrEmpty())
        {
            return area.TcpSocketUrl;
        }

        return area.Id.ToString();
    }

    private static string TryGetHost(string value)
    {
#pragma warning disable CS8603 // 私有 helper：Uri 解析失败时返回 null，调用方用 ?? 链式回退，跨 DTO 非空属性是 ORM 既有约定
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) ? uri.Host : null;
#pragma warning restore CS8603
    }

    private static uint? TryGetPort(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Port > 0 ? (uint)uri.Port : null;
    }

    /// <summary>
    /// 获取账号角色详情
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(TenantGameRoleEntityResponse))]
    public async Task<HttpJsonResultData<TenantGameRoleEntityResponse>> Get([FromBody] GetTenantGameRoleEntityRequest input)
    {
        var entity = await PRepository.GetSingleAsync(a => a.UserId == input.UserId && a.AreaId == input.AreaId);
        var result = entity.Adapt<TenantGameRoleEntityResponse>();
        return HttpJsonResultData<TenantGameRoleEntityResponse>.Success(result);
    }

    /// <summary>
    /// 根据角色id获取账号角色详情
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(TenantGameRoleEntityResponse))]
    public async Task<HttpJsonResultData<TenantGameRoleEntityResponse>> GetById([FromBody] GetByIdTenantGameRoleEntityRequest input)
    {
        if (long.TryParse(input.Id, out var id))
        {
            var entity = await PRepository.GetFirstAsync(a => a.Id == id);
            var result = entity.Adapt<TenantGameRoleEntityResponse>();
            var gameAreaEntity = await _areaService.AsQueryable().Filter(null, true).Where(u => u.IsDeleted == false).FirstAsync(g => g.Id == entity.AreaId);
            if (gameAreaEntity.IsNotNull())
            {
                result.ServerName = gameAreaEntity.Name;
            }

            return HttpJsonResultData<TenantGameRoleEntityResponse>.Success(result);
        }

        return GameApiResult.Error<TenantGameRoleEntityResponse>(GameRoleErrorCode.RoleNotFound, "角色不存在");
    }
}