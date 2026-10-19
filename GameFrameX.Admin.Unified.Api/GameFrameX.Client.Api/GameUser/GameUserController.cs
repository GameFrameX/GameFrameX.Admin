using GameFrameX.Client.Api.Services.Auth;
using GameFrameX.Application.Game.Penalty.GamePenalty;
using GameFrameX.Application.Game.User.GameUser;
using GameFrameX.Client.Api.GameUser.Dto;
using GameFrameX.Client.Api.Filter;
using GameFrameX.Client.Api.Middleware;
using GameFrameX.Client.Api.Services;
using GameFrameX.Core.Service;
using GameFrameX.Entity.Enum;
using GameFrameX.Foundation.Extensions;

namespace GameFrameX.Client.Api.GameUser;

// TenantGameUserEntity 的绑定字段（WeChatId/QqId/Email/EmailPassword/FacebookId/AppleId/GoogleId/TwitterId/DiscordId/DeviceId）声明为非空 string
// （带 = null! 初始化器的 ORM 实体），但底层 DB 列允许 NULL；解绑/清除路径需赋值 null 以写回 NULL（既有 ORM 语义），
// 这些赋值触发 CS8625。集中抑制以避免 12 处分散的 #pragma 包裹。
#pragma warning disable CS8625 // 实体绑定字段声明非空但 DB 列允许 NULL，解绑时赋 null 写回 NULL 是 ORM 语义

/// <summary>
/// 程序版本控制器
/// </summary>
public class GameUserController : BaseController<TenantGameUserEntity>
{
    private readonly ClientApiTokenIssuer _clientApiTokenIssuer;
    private readonly ClientApiSessionKeyIssuer _clientApiSessionKeyIssuer;
    private readonly Repository<TenantGameUserEmailEntity> _gameUserEmailService;
    private readonly ITenantGamePenaltyEnforcementService _tenantGamePenaltyEnforcementService;
    private readonly TenantGameUserPasswordService _tenantGameUserPasswordService;

    /*private readonly GoogleOAuth2Service _googleOAuth2Service;*/

    /// <summary>
    ///
    /// </summary>
    /// <param name="service"></param>
    /// <param name="clientApiTokenIssuer"></param>
    /// <param name="clientApiSessionKeyIssuer">登录期 per-session 会话密钥签发服务（C7）</param>
    /// <param name="gameUserEmailService"></param>
    /// <param name="tenantGamePenaltyEnforcementService"></param>
    /// <param name="tenantGameUserPasswordService">玩家账号密码哈希服务（PBKDF2，C198 / GFX-627）</param>
    /*/// <param name="googleOAuth2Service"></param>*/
    public GameUserController(SqlSugarRepository<TenantGameUserEntity> service, ClientApiTokenIssuer clientApiTokenIssuer,
        ClientApiSessionKeyIssuer clientApiSessionKeyIssuer,
        Repository<TenantGameUserEmailEntity> gameUserEmailService,
        ITenantGamePenaltyEnforcementService tenantGamePenaltyEnforcementService,
        TenantGameUserPasswordService tenantGameUserPasswordService
        /*, GoogleOAuth2Service googleOAuth2Service*/
    ) : base(service)
    {
        _clientApiTokenIssuer = clientApiTokenIssuer;
        _clientApiSessionKeyIssuer = clientApiSessionKeyIssuer;
        _gameUserEmailService = gameUserEmailService;
        _tenantGamePenaltyEnforcementService = tenantGamePenaltyEnforcementService;
        _tenantGameUserPasswordService = tenantGameUserPasswordService;
        /*_googleOAuth2Service = googleOAuth2Service;*/
    }

    #region WeChat

    /// <summary>
    /// WeChat账号登录,如果不存在则自动创建,否则直接登录
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> WeChatLogin([FromBody] LoginWeChatRequest input)
    {
        var entity = await PRepository.GetFirstAsync(m => m.WeChatId == input.WeChatId);
        if (entity == null)
        {
            entity = input.Adapt<TenantGameUserEntity>();
            entity.Name = input.WeChatName ?? "";
            entity.UserName = input.WeChatName ?? string.Empty;
            entity.Status = GameUserStatus.Active;
            entity.Password = _tenantGameUserPasswordService.Hash(entity.Id.ToString());
            entity.WeChatId = input.WeChatId;
            EnsureUserStringDefaults(entity);
            var insertAsync = await PRepository.InsertAsync(entity);
            if (insertAsync == false)
            {
                return HttpJsonResultData<GetTenantGameUserEntityResponse>.ServerError();
            }
        }

        if (entity.Status == GameUserStatus.Deleted)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountDeleting, "账号处于注销状态");
        }

        if (entity.Status != GameUserStatus.Active)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountDisabled, "账号已被禁用");
        }

        var output = await CreateLoginOutputAsync(entity, "WeChat");
        return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
    }


    /// <summary>
    /// 绑定WeChat账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> BindWeChat([FromBody] BindWeChatGameUserRequest input)
    {
        // 检查用户是否存在
        var entity = await PRepository.GetFirstAsync(u => u.Id == input.UserId);
        if (entity == null)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNotFound, "用户不存在");
        }

        // 检查账号是否已被其他用户绑定
        var gameUserEntity = await PRepository.GetFirstAsync(u => u.WeChatId == input.WeChatId && u.Id != input.UserId);
        if (gameUserEntity != null)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountWeChatIdByOtherUser, "该WeChat账号已被其他用户绑定");
        }

        // 更新用户的账号信息
        entity.WeChatId = input.WeChatId;
        var executeCommandAsync = await PRepository.AsUpdateable(entity).UpdateColumns(u => new { u.WeChatId, }).ExecuteCommandWithOptLockAsync(true);

        if (executeCommandAsync > 0)
        {
            var output = entity.Adapt<GetTenantGameUserEntityResponse>();
            output.CreatedTime = entity.CreatedTime;

            output.IsGuest = IsGuest(entity);
            return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
        }

        return HttpJsonResultData<GetTenantGameUserEntityResponse>.ServerError();
    }

    /// <summary>
    /// 解绑WeChat账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> UnbindWeChatGameUser([FromBody] UnbindWeChatGameUserRequest input)
    {
        // 检查用户是否存在
        var entity = await PRepository.GetFirstAsync(u => u.Id == input.UserId);
        if (entity == null)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNotFound, "用户不存在");
        }

        // 检查用户是否已绑定账号
        if (string.IsNullOrEmpty(entity.WeChatId))
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNoBindWeChat, "用户未绑定WeChat账号");
        }

        // 如果提供了密码，验证密码是否正确（可选的安全验证）
        if (!string.IsNullOrEmpty(input.Password))
        {
            if (!_tenantGameUserPasswordService.Verify(input.Password, entity.EmailPassword))
            {
                return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountOrPasswordError, "密码错误");
            }
        }

        // 清除账号信息
        entity.WeChatId = null;
        var executeCommandAsync = await PRepository.AsUpdateable(entity).UpdateColumns(u => new { u.WeChatId, }).ExecuteCommandWithOptLockAsync(true);

        if (executeCommandAsync > 0)
        {
            var output = entity.Adapt<GetTenantGameUserEntityResponse>();
            output.CreatedTime = entity.CreatedTime;

            output.IsGuest = IsGuest(entity);
            return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
        }

        return HttpJsonResultData<GetTenantGameUserEntityResponse>.ServerError();
    }

    #endregion


    #region QQ

    /// <summary>
    /// QQ账号登录,如果不存在则自动创建,否则直接登录
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> QQLogin([FromBody] LoginQQRequest input)
    {
        var entity = await PRepository.GetFirstAsync(m => m.QqId == input.QQId);
        if (entity == null)
        {
            entity = input.Adapt<TenantGameUserEntity>();
            entity.Name = input.QQName ?? "";
            entity.UserName = input.QQName ?? string.Empty;
            entity.Status = GameUserStatus.Active;
            entity.Password = _tenantGameUserPasswordService.Hash(entity.Id.ToString());
            entity.QqId = input.QQId;
            EnsureUserStringDefaults(entity);
            var insertAsync = await PRepository.InsertAsync(entity);
            if (insertAsync == false)
            {
                return HttpJsonResultData<GetTenantGameUserEntityResponse>.ServerError();
            }
        }

        if (entity.Status == GameUserStatus.Deleted)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountDeleting, "账号处于注销状态");
        }

        if (entity.Status != GameUserStatus.Active)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountDisabled, "账号已被禁用");
        }

        var output = await CreateLoginOutputAsync(entity, "QQ");
        return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
    }


    /// <summary>
    /// 绑定QQ账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> BindQQ([FromBody] BindQQGameUserRequest input)
    {
        // 检查用户是否存在
        var entity = await PRepository.GetFirstAsync(u => u.Id == input.UserId);
        if (entity == null)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNotFound, "用户不存在");
        }

        // 检查账号是否已被其他用户绑定
        var gameUserEntity = await PRepository.GetFirstAsync(u => u.QqId == input.QQId && u.Id != input.UserId);
        if (gameUserEntity != null)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountQQIdByOtherUser, "该WeChat账号已被其他用户绑定");
        }

        // 更新用户的账号信息
        entity.QqId = input.QQId;
        var executeCommandAsync = await PRepository.AsUpdateable(entity).UpdateColumns(u => new { QQId = u.QqId, }).ExecuteCommandWithOptLockAsync(true);

        if (executeCommandAsync > 0)
        {
            var output = entity.Adapt<GetTenantGameUserEntityResponse>();
            output.CreatedTime = entity.CreatedTime;

            output.IsGuest = IsGuest(entity);
            return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
        }

        return HttpJsonResultData<GetTenantGameUserEntityResponse>.ServerError();
    }


    /// <summary>
    /// 解绑QQ账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> UnbindQQGameUser([FromBody] UnbindQQGameUserRequest input)
    {
        // 检查用户是否存在
        var entity = await PRepository.GetFirstAsync(u => u.Id == input.UserId);
        if (entity == null)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNotFound, "用户不存在");
        }

        // 检查用户是否已绑定账号
        if (string.IsNullOrEmpty(entity.QqId))
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNoBindQQ, "用户未绑定QQ账号");
        }

        // 如果提供了密码，验证密码是否正确（可选的安全验证）
        if (!string.IsNullOrEmpty(input.Password))
        {
            if (!_tenantGameUserPasswordService.Verify(input.Password, entity.EmailPassword))
            {
                return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountOrPasswordError, "密码错误");
            }
        }

        // 清除账号信息
        entity.QqId = null;
        var executeCommandAsync = await PRepository.AsUpdateable(entity).UpdateColumns(u => new { QQId = u.QqId, }).ExecuteCommandWithOptLockAsync(true);

        if (executeCommandAsync > 0)
        {
            var output = entity.Adapt<GetTenantGameUserEntityResponse>();
            output.CreatedTime = entity.CreatedTime;

            output.IsGuest = IsGuest(entity);
            return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
        }

        return HttpJsonResultData<GetTenantGameUserEntityResponse>.ServerError();
    }

    #endregion


    #region Facebook

    /// <summary>
    /// Facebook账号登录,如果不存在则自动创建,否则直接登录
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> FacebookLogin([FromBody] LoginFacebookRequest input)
    {
        var entity = await PRepository.GetFirstAsync(m => m.FacebookId == input.FacebookId);
        if (entity == null)
        {
            entity = input.Adapt<TenantGameUserEntity>();
            entity.Name = input.FacebookName ?? "";
            entity.UserName = input.FacebookName ?? string.Empty;
            entity.Status = GameUserStatus.Active;
            entity.Password = _tenantGameUserPasswordService.Hash(entity.Id.ToString());
            entity.FacebookId = input.FacebookId;
            EnsureUserStringDefaults(entity);
            var insertAsync = await PRepository.InsertAsync(entity);
            if (insertAsync == false)
            {
                return HttpJsonResultData<GetTenantGameUserEntityResponse>.ServerError();
            }
        }

        if (entity.Status == GameUserStatus.Deleted)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountDeleting, "账号处于注销状态");
        }

        if (entity.Status != GameUserStatus.Active)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountDisabled, "账号已被禁用");
        }

        var output = await CreateLoginOutputAsync(entity, "Facebook");
        return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
    }


    /// <summary>
    /// 绑定Facebook账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> BindFacebook([FromBody] BindFacebookGameUserRequest input)
    {
        // 检查用户是否存在
        var entity = await PRepository.GetFirstAsync(u => u.Id == input.UserId);
        if (entity == null)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNotFound, "用户不存在");
        }

        // 检查账号是否已被其他用户绑定
        var gameUserEntity = await PRepository.GetFirstAsync(u => u.FacebookId == input.FacebookId && u.Id != input.UserId);
        if (gameUserEntity != null)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountFacebookIdByOtherUser, "该FaceBook账号已被其他用户绑定");
        }

        // 更新用户的账号信息
        entity.FacebookId = input.FacebookId;
        var executeCommandAsync = await PRepository.AsUpdateable(entity).UpdateColumns(u => new { u.FacebookId, }).ExecuteCommandWithOptLockAsync(true);

        if (executeCommandAsync > 0)
        {
            var output = entity.Adapt<GetTenantGameUserEntityResponse>();
            output.CreatedTime = entity.CreatedTime;

            output.IsGuest = IsGuest(entity);
            return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
        }

        return HttpJsonResultData<GetTenantGameUserEntityResponse>.ServerError();
    }


    /// <summary>
    /// 解绑Facebook账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> UnbindFacebookGameUser([FromBody] UnbindFacebookGameUserRequest input)
    {
        // 检查用户是否存在
        var entity = await PRepository.GetFirstAsync(u => u.Id == input.UserId);
        if (entity == null)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNotFound, "用户不存在");
        }

        // 检查用户是否已绑定Facebook账号
        if (string.IsNullOrEmpty(entity.FacebookId))
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNoBindFacebook, "用户未绑定FaceBook账号");
        }

        // 如果提供了密码，验证密码是否正确（可选的安全验证）
        if (!string.IsNullOrEmpty(input.Password))
        {
            if (!_tenantGameUserPasswordService.Verify(input.Password, entity.EmailPassword))
            {
                return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountOrPasswordError, "密码错误");
            }
        }

        // 清除Facebook账号信息
        entity.FacebookId = null;
        var executeCommandAsync = await PRepository.AsUpdateable(entity).UpdateColumns(u => new { u.FacebookId, }).ExecuteCommandWithOptLockAsync(true);

        if (executeCommandAsync > 0)
        {
            var output = entity.Adapt<GetTenantGameUserEntityResponse>();
            output.CreatedTime = entity.CreatedTime;

            output.IsGuest = IsGuest(entity);
            return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
        }

        return HttpJsonResultData<GetTenantGameUserEntityResponse>.ServerError();
    }

    #endregion


    /// <summary>
    /// 重置游戏邮箱账号密码
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(BaseOperationAckResponse))]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> EmailReset([FromBody] GameUserResetEntityRequest input)
    {
        var gameUserEntity = await PRepository.GetFirstAsync(m => m.Email == input.UserName);
        if (gameUserEntity == null)
        {
            // 如果账号不存在，则直接返回错误
            return GameApiResult.Error<BaseOperationAckResponse>(AccountErrorCode.AccountNotFound, "账号不存在");
        }

        var gameUserEmailEntity = await _gameUserEmailService.GetFirstAsync(u => u.Email == input.UserName && u.Code == input.Code && u.ExpirationTime > TimerHelper.UnixTimeSeconds());

        if (gameUserEmailEntity == null)
        {
            return GameApiResult.Error<BaseOperationAckResponse>(AccountErrorCode.EmailCodeNotFoundOrExpired, "验证码错误或已过期，请重新获取验证码。");
        }

        gameUserEntity.EmailPassword = _tenantGameUserPasswordService.Hash(input.Password);
        var updateRows = await PRepository.Context.UpdateWithOptimisticLockAsync(gameUserEntity);
        if (updateRows > 0)
        {
            return HttpJsonResultData<BaseOperationAckResponse>.Success();
        }

        return HttpJsonResultData<BaseOperationAckResponse>.ServerError();
    }

    #region Email

    /// <summary>
    /// 注册游戏邮箱账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> EmailRegister([FromBody] GameUserEmailRegisterEntityRequest input)
    {
        var isAny = await PRepository.IsAnyAsync(m => m.Email == input.UserName);
        if (isAny)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountEmailExists, "账号已存在");
        }

        var gameUserEmailEntity = await _gameUserEmailService.GetFirstAsync(u => u.Email == input.UserName && u.Code == input.Code && u.ExpirationTime > TimerHelper.UnixTimeSeconds());

        if (gameUserEmailEntity == null)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.EmailCodeNotFoundOrExpired, "验证码错误或已过期，请重新获取验证码。");
        }

        var entity = input.Adapt<TenantGameUserEntity>();
        entity.Name = input.UserName ?? string.Empty;
        entity.Email = input.UserName ?? string.Empty;
        entity.Status = GameUserStatus.Active;
        entity.EmailPassword = _tenantGameUserPasswordService.Hash(input.Password);
        EnsureUserStringDefaults(entity);
        var insertAsync = await PRepository.InsertAsync(entity);
        if (insertAsync)
        {
            var output = await CreateLoginOutputAsync(entity, "Email");
            return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
        }

        return HttpJsonResultData<GetTenantGameUserEntityResponse>.ServerError();
    }

    /// <summary>
    /// 登录游戏账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> EmailLogin([FromBody] LoginTenantGameUserEntityRequest input)
    {
        var entity = await PRepository.GetFirstAsync(m => m.Email == input.UserName);
        if (entity == null)
        {
            // 账号不存在：执行一次假 Verify 抵御账号枚举（耗时与密码错误路径一致），统一返回账号或密码错误，不泄露账号是否存在
            _tenantGameUserPasswordService.Verify(input.Password, TenantGameUserPasswordService.EnumerationDecoyHash);
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountOrPasswordByEmailError, "账号或密码错误");
        }

        if (!_tenantGameUserPasswordService.Verify(input.Password, entity.EmailPassword))
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountOrPasswordByEmailError, "账号或密码错误");
        }

        if (entity.Status == GameUserStatus.Deleted)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountDeleting, "账号处于注销状态");
        }

        if (entity.Status != GameUserStatus.Active)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountDisabled, "账号已被禁用");
        }

        var output = await CreateLoginOutputAsync(entity, "Email");
        return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
    }


    /// <summary>
    /// 绑定Email账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> BindEmail([FromBody] BindEmailGameUserRequest input)
    {
        // 检查用户是否存在
        var entity = await PRepository.GetFirstAsync(u => u.Id == input.UserId);
        if (entity == null)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNotFound, "用户不存在");
        }

        // 检查邮箱账号是否已被其他用户绑定
        var existingGoogleUser = await PRepository.GetFirstAsync(u => u.Email == input.Email && u.Id != input.UserId);
        if (existingGoogleUser != null)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountEmailByOtherUser, "该邮箱账号已被其他用户绑定");
        }

        // 验证邮箱验证码
        var gameUserEmailEntity = await _gameUserEmailService.GetFirstAsync(u => u.Email == input.Email && u.Code == input.Code && u.ExpirationTime > TimerHelper.UnixTimeSeconds());
        if (gameUserEmailEntity == null)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.EmailCodeNotFoundOrExpired, "验证码错误或已过期，请重新获取验证码。");
        }

        // 更新用户的邮箱账号信息
        entity.Email = input.Email;
        entity.EmailPassword = _tenantGameUserPasswordService.Hash(input.Password);
        var executeCommandAsync = await PRepository.AsUpdateable(entity).UpdateColumns(u => new { u.Email, u.EmailPassword, }).ExecuteCommandWithOptLockAsync(true);

        if (executeCommandAsync > 0)
        {
            var output = entity.Adapt<GetTenantGameUserEntityResponse>();
            output.CreatedTime = entity.CreatedTime;

            output.IsGuest = IsGuest(entity);
            return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
        }

        return HttpJsonResultData<GetTenantGameUserEntityResponse>.ServerError();
    }


    /// <summary>
    /// 解绑邮箱账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> UnbindEmailGameUser([FromBody] UnbindEmailGameUserRequest input)
    {
        // 检查用户是否存在
        var entity = await PRepository.GetFirstAsync(u => u.Id == input.UserId);
        if (entity == null)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNotFound, "用户不存在");
        }

        // 检查用户是否已绑定邮箱账号
        if (string.IsNullOrEmpty(entity.Email))
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNoBindEmail, "用户未绑定邮箱账号");
        }

        // 如果提供了密码，验证密码是否正确（可选的安全验证）
        if (!string.IsNullOrEmpty(input.Password))
        {
            if (!_tenantGameUserPasswordService.Verify(input.Password, entity.EmailPassword))
            {
                return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountOrPasswordError, "密码错误");
            }
        }

        // 清除邮箱账号信息
        entity.Email = null;
        entity.EmailPassword = null;
        var executeCommandAsync = await PRepository.AsUpdateable(entity).UpdateColumns(u => new { u.Email, u.EmailPassword, }).ExecuteCommandWithOptLockAsync(true);

        if (executeCommandAsync > 0)
        {
            var output = entity.Adapt<GetTenantGameUserEntityResponse>();
            output.CreatedTime = entity.CreatedTime;

            output.IsGuest = IsGuest(entity);
            return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
        }

        return HttpJsonResultData<GetTenantGameUserEntityResponse>.ServerError();
    }

    #endregion

    /// <summary>
    /// 注册游戏游客账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> Register([FromBody] GameUserGuestRegisterEntityRequest input)
    {
        var isAny = await PRepository.IsAnyAsync(m => m.UserName == input.UserName);
        if (isAny)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountExists, "账号已存在");
        }

        var entity = input.Adapt<TenantGameUserEntity>();
        entity.Name = input.UserName;
        entity.Status = GameUserStatus.Active;
        entity.Password = _tenantGameUserPasswordService.Hash(input.Password);
        EnsureUserStringDefaults(entity);
        var insertAsync = await PRepository.InsertAsync(entity);
        if (insertAsync)
        {
            var output = await CreateLoginOutputAsync(entity, "Guest");
            return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
        }

        return HttpJsonResultData<GetTenantGameUserEntityResponse>.ServerError();
    }

    /// <summary>
    /// 登录游戏游客账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> Login([FromBody] LoginTenantGameUserEntityRequest input)
    {
        var entity = await PRepository.GetFirstAsync(m => m.UserName == input.UserName);
        if (entity == null)
        {
            // 账号不存在：执行一次假 Verify 抵御账号枚举（耗时与密码错误路径一致），统一返回账号或密码错误，不泄露账号是否存在
            _tenantGameUserPasswordService.Verify(input.Password, TenantGameUserPasswordService.EnumerationDecoyHash);
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountOrPasswordError, "账号或密码错误");
        }

        if (!_tenantGameUserPasswordService.Verify(input.Password, entity.Password))
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountOrPasswordError, "账号或密码错误");
        }

        if (entity.Status == GameUserStatus.Deleted)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountDeleting, "账号处于注销状态");
        }

        if (entity.Status != GameUserStatus.Active)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountDisabled, "账号已被禁用");
        }

        var output = await CreateLoginOutputAsync(entity, "Guest");
        return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
    }

    /// <summary>
    /// 匿名设备登录（C12 A1：承接旧 SSO Lookup 语义——按设备标识找号或建号，
    /// 无密码，设备标识即凭证）。VendorId/InstallId/Channel/Platform/ClientVersion
    /// 为记录性字段，当前不落库。
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> DeviceLogin([FromBody] LoginDeviceRequest input)
    {
        var entity = await PRepository.GetFirstAsync(m => m.DeviceId == input.DeviceId);
        var isNewRegister = false;
        if (entity == null)
        {
            entity = new TenantGameUserEntity
            {
                UserName = "device:" + input.DeviceId,
                Name = "device:" + input.DeviceId,
                DeviceId = input.DeviceId,
                Status = GameUserStatus.Active,
                Password = _tenantGameUserPasswordService.Hash(Guid.NewGuid().ToString("N"))
            };
            EnsureUserStringDefaults(entity);
            // ponytail: DeviceId 无唯一索引，并发首登可能建重号；客户端按 Token 归属使用
            // 不受影响，如需严格唯一，升级点在 entity 加唯一索引 + 此处捕获重复键后重查
            if (!await PRepository.InsertAsync(entity))
            {
                return HttpJsonResultData<GetTenantGameUserEntityResponse>.ServerError();
            }
            isNewRegister = true;
        }
        else
        {
            if (entity.Status == GameUserStatus.Deleted)
            {
                return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountDeleting, "账号处于注销状态");
            }

            if (entity.Status != GameUserStatus.Active)
            {
                return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountDisabled, "账号已被禁用");
            }
        }

        var deviceLoginOutput = await CreateLoginOutputAsync(entity, "Device");
        deviceLoginOutput.IsNewRegister = isNewRegister;
        return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(deviceLoginOutput);
    }

    /*/// <summary>
    /// 更新游戏账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> UpdateAsync([FromBody] UpdateTenantGameUserEntityRequest input)
    {
        var entity = input.Adapt<TenantGameUserEntity>();
        var res = await PRepository.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
        return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(JsonHelper.Serialize(entity));
    }*/

    /// <summary>
    /// 获取游戏账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> Get([FromBody] QueryByIdTenantGameUserEntityRequest input)
    {
        var tokenValidation = await _clientApiTokenIssuer.ValidateAsync(input.AccessToken);
        if (!tokenValidation.IsValid || tokenValidation.UserId != input.Id)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AuthErrorCode.AuthorizeInvalid, "授权无效");
        }

        var entity = await PRepository.GetFirstAsync(u => u.Id == input.Id);
        if (entity == null)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNotFound, "账号不存在");
        }

        var output = CreateAccountOutput(entity);
        return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
    }

    #region Apple

    /// <summary>
    /// 绑定Apple账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> BindAppleGameUser([FromBody] BindAppleGameUserRequest input)
    {
        // 检查用户是否存在
        var entity = await PRepository.GetFirstAsync(u => u.Id == input.UserId);
        if (entity == null)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNotFound, "用户不存在");
        }

        // 检查谷歌账号是否已被其他用户绑定
        var existingGoogleUser = await PRepository.GetFirstAsync(u => u.AppleId == input.AppleId && u.Id != input.UserId);
        if (existingGoogleUser != null)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountAppleIdByOtherUser, "该Apple账号已被其他用户绑定");
        }

        // 更新用户的谷歌账号信息
        entity.AppleId = input.AppleId;

        var executeCommandAsync = await PRepository.AsUpdateable(entity).UpdateColumns(u => new { u.AppleId, }).ExecuteCommandWithOptLockAsync(true);

        if (executeCommandAsync > 0)
        {
            var output = entity.Adapt<GetTenantGameUserEntityResponse>();
            output.CreatedTime = entity.CreatedTime;

            output.IsGuest = IsGuest(entity);
            return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
        }

        return HttpJsonResultData<GetTenantGameUserEntityResponse>.ServerError();
    }


    /// <summary>
    /// Apple账号登录,如果不存在则自动创建,否则直接登录
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> AppleLogin([FromBody] AppleLoginRequest input)
    {
        var entity = await PRepository.GetFirstAsync(m => m.AppleId == input.AppleId);
        if (entity == null)
        {
            entity = input.Adapt<TenantGameUserEntity>();
            entity.Name = input.AppleName ?? "";
            entity.UserName = input.AppleName ?? string.Empty;
            entity.Status = GameUserStatus.Active;
            entity.Password = _tenantGameUserPasswordService.Hash(entity.Id.ToString());
            entity.AppleId = input.AppleId;
            EnsureUserStringDefaults(entity);
            var insertAsync = await PRepository.InsertAsync(entity);
            if (insertAsync == false)
            {
                return HttpJsonResultData<GetTenantGameUserEntityResponse>.ServerError();
            }
        }

        if (entity.Status == GameUserStatus.Deleted)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountDeleting, "账号处于注销状态");
        }

        if (entity.Status != GameUserStatus.Active)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountDisabled, "账号已被禁用");
        }

        var output = await CreateLoginOutputAsync(entity, "Apple");
        return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
    }

    /// <summary>
    /// 解绑Apple账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> UnbindAppleGameUser([FromBody] UnbindAppleGameUserRequest input)
    {
        // 检查用户是否存在
        var entity = await PRepository.GetFirstAsync(u => u.Id == input.UserId);
        if (entity == null)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNotFound, "用户不存在");
        }

        // 检查用户是否已绑定谷歌账号
        if (string.IsNullOrEmpty(entity.GoogleId))
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNoBindGoogleId, "用户未绑定Apple账号");
        }

        // 如果提供了密码，验证密码是否正确（可选的安全验证）
        if (!string.IsNullOrEmpty(input.Password))
        {
            if (!_tenantGameUserPasswordService.Verify(input.Password, entity.Password))
            {
                return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountOrPasswordError, "密码错误");
            }
        }

        // 清除谷歌账号信息
        entity.AppleId = null;

        var executeCommandAsync = await PRepository.AsUpdateable(entity).UpdateColumns(u => new { u.AppleId, }).ExecuteCommandWithOptLockAsync(true);

        if (executeCommandAsync > 0)
        {
            var output = entity.Adapt<GetTenantGameUserEntityResponse>();
            output.CreatedTime = entity.CreatedTime;

            output.IsGuest = IsGuest(entity);
            return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
        }

        return HttpJsonResultData<GetTenantGameUserEntityResponse>.ServerError();
    }

    #endregion

    #region Google

    /// <summary>
    /// 绑定谷歌账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> BindGoogleGameUser([FromBody] BindGoogleGameUserRequest input)
    {
        // 检查用户是否存在
        var entity = await PRepository.GetFirstAsync(u => u.Id == input.UserId);
        if (entity == null)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNotFound, "用户不存在");
        }

        // 检查谷歌账号是否已被其他用户绑定
        var existingGoogleUser = await PRepository.GetFirstAsync(u => u.GoogleId == input.GoogleId && u.Id != input.UserId);
        if (existingGoogleUser != null)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountGoogleIdByOtherUser, "该谷歌账号已被其他用户绑定");
        }

        // 更新用户的谷歌账号信息
        entity.GoogleId = input.GoogleId;

        var executeCommandAsync = await PRepository.AsUpdateable(entity).UpdateColumns(u => new { u.GoogleId, }).ExecuteCommandWithOptLockAsync(true);

        if (executeCommandAsync > 0)
        {
            var output = entity.Adapt<GetTenantGameUserEntityResponse>();
            output.CreatedTime = entity.CreatedTime;

            output.IsGuest = IsGuest(entity);
            return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
        }

        return HttpJsonResultData<GetTenantGameUserEntityResponse>.ServerError();
    }

    /// <summary>
    /// 谷歌账号登录,如果不存在则自动创建,否则直接登录
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> GoogleLogin([FromBody] LoginGoogleRequest input)
    {
        var entity = await PRepository.GetFirstAsync(m => m.GoogleId == input.GoogleId);
        if (entity == null)
        {
            entity = input.Adapt<TenantGameUserEntity>();
            entity.Name = input.GoogleName ?? "";
            entity.UserName = input.GoogleName ?? string.Empty;
            entity.Status = GameUserStatus.Active;
            entity.Password = _tenantGameUserPasswordService.Hash(entity.Id.ToString());
            entity.GoogleId = input.GoogleId;
            EnsureUserStringDefaults(entity);
            var insertAsync = await PRepository.InsertAsync(entity);
            if (insertAsync == false)
            {
                return HttpJsonResultData<GetTenantGameUserEntityResponse>.ServerError();
            }
        }

        if (entity.Status == GameUserStatus.Deleted)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountDeleting, "账号处于注销状态");
        }

        if (entity.Status != GameUserStatus.Active)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountDisabled, "账号已被禁用");
        }

        var output = await CreateLoginOutputAsync(entity, "Google");
        return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
    }

    /// <summary>
    /// 解绑谷歌账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> UnbindGoogleGameUser([FromBody] UnbindGoogleGameUserRequest input)
    {
        // 检查用户是否存在
        var entity = await PRepository.GetFirstAsync(u => u.Id == input.UserId);
        if (entity == null)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNotFound, "用户不存在");
        }

        // 检查用户是否已绑定谷歌账号
        if (string.IsNullOrEmpty(entity.GoogleId))
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNoBindGoogleId, "用户未绑定谷歌账号");
        }

        // 如果提供了密码，验证密码是否正确（可选的安全验证）
        if (!string.IsNullOrEmpty(input.Password))
        {
            if (!_tenantGameUserPasswordService.Verify(input.Password, entity.Password))
            {
                return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountOrPasswordError, "密码错误");
            }
        }

        // 清除谷歌账号信息
        entity.GoogleId = null;

        var executeCommandAsync = await PRepository.AsUpdateable(entity).UpdateColumns(u => new { u.GoogleId, }).ExecuteCommandWithOptLockAsync(true);

        if (executeCommandAsync > 0)
        {
            var output = entity.Adapt<GetTenantGameUserEntityResponse>();
            output.CreatedTime = entity.CreatedTime;

            output.IsGuest = IsGuest(entity);
            return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
        }

        return HttpJsonResultData<GetTenantGameUserEntityResponse>.ServerError();
    }

    #endregion

    /// <summary>
    /// 解绑账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(GetTenantGameUserEntityResponse))]
    public async Task<HttpJsonResultData<GetTenantGameUserEntityResponse>> Unbind([FromBody] GameUserUnbindRequest input)
    {
        var tokenValidation = await _clientApiTokenIssuer.ValidateAsync(input.AccessToken);
        if (!tokenValidation.IsValid || tokenValidation.UserId != input.Id)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AuthErrorCode.AuthorizeInvalid, "授权无效");
        }

        // 检查用户是否存在
        var entity = await PRepository.GetFirstAsync(u => u.Id == input.Id);
        if (entity == null)
        {
            return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNotFound, "用户不存在");
        }

        switch (input.Type)
        {
            case "google":
                // 检查用户是否已绑定谷歌账号
                if (string.IsNullOrEmpty(entity.GoogleId))
                {
                    return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNoBindGoogleId, "用户未绑定谷歌账号");
                }

                // 清除账号信息
                entity.GoogleId = null;
                break;
            case "apple":
                // 检查用户是否已绑定账号
                if (string.IsNullOrEmpty(entity.AppleId))
                {
                    return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNoBindAppleId, "用户未绑定Apple账号");
                }

                // 清除账号信息
                entity.AppleId = null;
                break;
            case "facebook":
                // 检查用户是否已绑定账号
                if (string.IsNullOrEmpty(entity.FacebookId))
                {
                    return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNoBindFacebookId, "用户未绑定Facebook账号");
                }

                // 清除账号信息
                entity.FacebookId = null;
                break;
            case "twitter":
                // 检查用户是否已绑定账号
                if (string.IsNullOrEmpty(entity.TwitterId))
                {
                    return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNoBindTwitterId, "用户未绑定Twitter账号");
                }

                // 清除账号信息
                entity.TwitterId = null;
                break;
            case "discord":
                // 检查用户是否已绑定账号
                if (string.IsNullOrEmpty(entity.DiscordId))
                {
                    return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNoBindDiscordId, "用户未绑定Discord账号");
                }

                // 清除账号信息
                entity.DiscordId = null;
                break;
            case "wechat":
                // 检查用户是否已绑定账号
                if (string.IsNullOrEmpty(entity.WeChatId))
                {
                    return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNoBindWeChat, "用户未绑定WeChat账号");
                }

                // 清除账号信息
                entity.WeChatId = null;
                break;
            case "qq":
                // 检查用户是否已绑定账号
                if (string.IsNullOrEmpty(entity.QqId))
                {
                    return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountNoBindQQ, "用户未绑定QQ账号");
                }

                // 清除账号信息
                entity.QqId = null;
                break;
            default:
                return GameApiResult.Error<GetTenantGameUserEntityResponse>(AccountErrorCode.AccountUnBindNotFound, "解绑类型不存在");
        }

        var executeCommandAsync = await PRepository.Context.UpdateWithOptimisticLockAsync(entity) > 0;

        if (executeCommandAsync)
        {
            var output = entity.Adapt<GetTenantGameUserEntityResponse>();
            output.CreatedTime = entity.CreatedTime;

            output.IsGuest = IsGuest(entity);
            return HttpJsonResultData<GetTenantGameUserEntityResponse>.Success(output);
        }

        return HttpJsonResultData<GetTenantGameUserEntityResponse>.ServerError();
    }

    private static bool IsGuest(TenantGameUserEntity entity)
    {
        return
            entity.WeChatId.IsNullOrEmptyOrWhiteSpace() &&
            entity.QqId.IsNullOrEmptyOrWhiteSpace() &&
            entity.Email.IsNullOrEmptyOrWhiteSpace() &&
            entity.AppleId.IsNullOrEmptyOrWhiteSpace() &&
            entity.GoogleId.IsNullOrEmptyOrWhiteSpace() &&
            entity.FacebookId.IsNullOrEmptyOrWhiteSpace() &&
            entity.TwitterId.IsNullOrEmptyOrWhiteSpace() &&
            entity.DiscordId.IsNullOrEmptyOrWhiteSpace();
    }

    private static void EnsureUserStringDefaults(TenantGameUserEntity entity)
    {
        entity.Name ??= string.Empty;
        entity.UserName ??= string.Empty;
        entity.Password ??= string.Empty;
        entity.Email ??= string.Empty;
        entity.EmailPassword ??= string.Empty;
        entity.Phone ??= string.Empty;
        entity.Avatar ??= string.Empty;
        entity.GoogleId ??= string.Empty;
        entity.AppleId ??= string.Empty;
        entity.FacebookId ??= string.Empty;
        entity.TwitterId ??= string.Empty;
        entity.DiscordId ??= string.Empty;
        entity.QqId ??= string.Empty;
        entity.WeChatId ??= string.Empty;
        entity.DeviceId ??= string.Empty;
    }

    private async Task<GetTenantGameUserEntityResponse> CreateLoginOutputAsync(TenantGameUserEntity entity, string provider)
    {
        // 处罚事实源登录阻断漏斗：账号级封禁/限制登录命中即拦截 Token 签发
        var appId = ResolveContextAppId();
        var blocking = await _tenantGamePenaltyEnforcementService.GetLoginBlockingPenaltyAsync(PenaltyTargetTypeEnum.Account, entity.Id.ToString(), appId);
        if (blocking != null)
        {
            throw new PenaltyLoginBlockedException((int)AccountErrorCode.AccountBanned, $"账号已被封禁：{blocking.Reason}");
        }

        var output = CreateAccountOutput(entity);
        var token = await _clientApiTokenIssuer.IssueAsync(entity, provider);
        output.AccessToken = token.Token;
        output.ExpireTime = token.ExpireTime;
        output.Provider = provider;

        // C7: token 签发后签发 per-session 会话密钥（AES-256-GCM 信封 kid/secret），到期与 AccessToken 对齐
        var sessionKey = await _clientApiSessionKeyIssuer.IssueAsync(appId, token.ExpireTime);
        output.SessionKeyId = sessionKey.Kid;
        output.SessionKeySecret = sessionKey.Secret;
        return output;
    }

    private long ResolveContextAppId()
    {
        if (HttpContext.Items.TryGetValue(AppResolutionMiddleware.AppIdKey, out var appIdObj) &&
            appIdObj != null &&
            long.TryParse(appIdObj.ToString(), out var appId))
        {
            return appId;
        }

        return 0;
    }

    private GetTenantGameUserEntityResponse CreateAccountOutput(TenantGameUserEntity entity)
    {
        var output = entity.Adapt<GetTenantGameUserEntityResponse>();
        output.CreatedTime = entity.CreatedTime;
        output.IsGuest = IsGuest(entity);
        output.Status = entity.Status.ToString();
        return output;
    }

    /// <summary>
    /// 注销账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(BaseOperationAckResponse))]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> LogOff([FromBody] GameUserLogOffRequest input)
    {
        var tokenValidation = await _clientApiTokenIssuer.ValidateAsync(input.AccessToken);
        if (!tokenValidation.IsValid || tokenValidation.UserId != input.Id)
        {
            return GameApiResult.Error<BaseOperationAckResponse>(AuthErrorCode.AuthorizeInvalid, "授权无效");
        }

        var entity = await PRepository.GetFirstAsync(m => m.Id == input.Id);
        if (entity == null)
        {
            return GameApiResult.Error<BaseOperationAckResponse>(AccountErrorCode.AccountNotFound, "账号不存在");
        }

        if (entity.Status == GameUserStatus.Deleted)
        {
            return GameApiResult.Error<BaseOperationAckResponse>(AccountErrorCode.AccountDeleting, "账号处于注销状态");
        }

        if (entity.Status != GameUserStatus.Active)
        {
            return GameApiResult.Error<BaseOperationAckResponse>(AccountErrorCode.AccountDisabled, "账号已被禁用");
        }

        entity.Status = GameUserStatus.Deleted;
        entity.DeletedTime = TimerHelper.UnixTimeSeconds() + 7 * 24 * 60 * 60;
        var result = await PRepository.Context.UpdateWithOptimisticLockAsync(entity) > 0;
        if (result)
        {
            return HttpJsonResultData<BaseOperationAckResponse>.Success();
        }

        return HttpJsonResultData<BaseOperationAckResponse>.ServerError();
    }

    /// <summary>
    /// 重新激活账号
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(BaseOperationAckResponse))]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> ReActive([FromBody] GameUserReActiveRequest input)
    {
        var entity = await PRepository.GetFirstAsync(m => m.Id == input.Id);
        if (entity == null)
        {
            return GameApiResult.Error<BaseOperationAckResponse>(AccountErrorCode.AccountNotFound, "账号不存在");
        }

        if (entity.Status != GameUserStatus.Deleted)
        {
            return GameApiResult.Error<BaseOperationAckResponse>(AccountErrorCode.AccountDisabled, "账号不是禁用状态");
        }

        entity.Status = GameUserStatus.Active;
        entity.DeletedTime = null;
        var result = await PRepository.Context.UpdateWithOptimisticLockAsync(entity) > 0;
        if (result)
        {
            return HttpJsonResultData<BaseOperationAckResponse>.Success();
        }

        return HttpJsonResultData<BaseOperationAckResponse>.ServerError();
    }
}