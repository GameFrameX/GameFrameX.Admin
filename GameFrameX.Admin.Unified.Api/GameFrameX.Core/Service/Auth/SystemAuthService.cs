using Microsoft.Extensions.Caching.Memory;
using GameFrameX.Core.Authorization;
using GameFrameX.Core.Service.Auth.Dto;
using GameFrameX.Core.Service.Menu;
using GameFrameX.Core.Service.Message;
using GameFrameX.Core.Service.OnlineUser;
using GameFrameX.Core.Service.PlatformCaptcha;
using GameFrameX.Core.Service.PlatformJwt;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.Filter;
using GameFrameX.Core.Extension.Const;
using GameFrameX.Entity.System;
using GameFrameX.Foundation.Http.Normalization;
using Lazy.Captcha.Core;
using System.Diagnostics.CodeAnalysis;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Service.Auth;

/// <summary>
/// 系统登录授权服务。
/// </summary>
[ApiDescriptionSettings(Order = 500)]
public class SystemAuthService : IAdminApiController
{
    private readonly CurrentUserContext _currentUserContext;
    private readonly SqlSugarRepository<SystemUserEntity> _systemUserRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private readonly SystemConfigService _systemConfigService;
    private readonly ICaptcha _captcha;
    private readonly IPlatformJwtTokenService _platformJwtTokenService;
    private readonly IPlatformCaptchaConfigProvider _platformCaptchaConfigProvider;
    private static readonly MemoryCache _loginFailCache = new(new MemoryCacheOptions());
    private const int MaxLoginFailCount = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan FailCountDuration = TimeSpan.FromMinutes(30);
    private static readonly ConcurrentDictionary<string, DateTime> _setupPasswordAttemptLimiter = new();
    private static readonly ConcurrentDictionary<string, int> _setupPasswordDailyAttemptCounter = new();
    private const int SetupPasswordMinIntervalSeconds = 10;
    private const int SetupPasswordMaxDailyAttemptCount = 30;

    public SystemAuthService(CurrentUserContext currentUserContext,
        SqlSugarRepository<SystemUserEntity> systemUserRepository,
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider serviceProvider,
        SystemConfigService systemConfigService,
        ICaptcha captcha,
        IPlatformJwtTokenService platformJwtTokenService,
        IPlatformCaptchaConfigProvider platformCaptchaConfigProvider)
    {
        _currentUserContext = currentUserContext;
        _systemUserRepository = systemUserRepository;
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
        _systemConfigService = systemConfigService;
        _captcha = captcha;
        _platformJwtTokenService = platformJwtTokenService;
        _platformCaptchaConfigProvider = platformCaptchaConfigProvider;
    }

    /// <summary>
    /// 账号密码登录
    /// </summary>
    /// <param name="input"></param>
    /// <remarks>用户名/密码：superadmin/密码首次安装时随机生成并打印于控制台</remarks>
    /// <returns></returns>
    [AllowAnonymous]
    [DisplayName("账号密码登录")]
    [ApiDescriptionSettings(Name = "Login")]
    public async Task<LoginResponse> LoginAsync([Required] LoginRequest input)
    {
        var account = input.Account;

        // 暴力破解防护：检查锁定状态
        var lockoutKey = $"lockout:pwd:{account}";
        if (_loginFailCache.Get(lockoutKey) is DateTime lockoutEnd && lockoutEnd > DateTime.UtcNow)
        {
            var remaining = (int)(lockoutEnd - DateTime.UtcNow).TotalMinutes;
            throw BusinessError.Oh($"登录已锁定，请 {remaining} 分钟后重试");
        }

        var captchaSettings = await _platformCaptchaConfigProvider.GetRequiredSettingsAsync();
        if (captchaSettings.IsEnabled)
        {
            if (input.CodeId <= 0 || string.IsNullOrWhiteSpace(input.Code))
            {
                throw BusinessError.Oh(SystemUserErrorCode.CaptchaInvalid);
            }

            if (!_captcha.Validate(input.CodeId.ToString(), input.Code))
            {
                throw BusinessError.Oh(SystemUserErrorCode.CaptchaInvalid);
            }
        }

        // 账号是否存在（枚举防护：user null 跑假 Verify 对齐耗时 + 统一响应，不再抛 D0009）
        var user = await GetLoginUserByAccountAsync(account);
        if (user == null)
        {
            _ = CryptogramUtility.VerifyPassword(input.Password, CryptogramUtility.EnumerationDecoyHash);
            RecordLoginFailure($"fail:pwd:{account}", lockoutKey);
        }

        // 首装向导：超管密码未初始化时拒绝密码登录，引导先完成初始化设置
        if (user.AccountType == AccountTypeEnum.SuperAdmin && user is SystemUserEntity { PasswordConfigured: false })
        {
            throw BusinessError.Oh(SystemUserErrorCode.SuperAdminPasswordNotConfigured);
        }

        // 账号是否被冻结
        if (user.IsEnabled == false)
        {
            throw BusinessError.Oh(SystemUserErrorCode.AccountFrozen);
        }

        // 密码是否正确（Argon2id 校验，不认历史 MD5/SM2/SM4 密文）
        var passwordCorrect = CryptogramUtility.VerifyPassword(input.Password, user.Password);

        if (!passwordCorrect)
        {
            RecordLoginFailure($"fail:pwd:{account}", lockoutKey);
        }

        // 登录成功，清除计数
        _loginFailCache.Remove($"fail:pwd:{account}");
        _loginFailCache.Remove($"lockout:pwd:{account}");

        return await CreateTokenAsync(user);
    }

    /// <summary>
    /// 记录登录失败并抛统一响应（枚举防护）：<paramref name="failKey"/> 累计失败次数，达 <see cref="MaxLoginFailCount"/> 设置
    /// <paramref name="lockoutKey"/> 锁定并抛锁定提示；未达阈值抛"账号或密码错误"。user null 与密码错误共用此路径，响应不可区分。
    /// </summary>
    /// <param name="failKey">失败计数缓存键（账号维度）。</param>
    /// <param name="lockoutKey">锁定缓存键（账号维度）。</param>
    [DoesNotReturn]
    internal static void RecordLoginFailure(string failKey, string lockoutKey)
    {
        var failCount = (_loginFailCache.Get(failKey) as int? ?? 0) + 1;
        _loginFailCache.Set(failKey, failCount, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = FailCountDuration, });
        if (failCount >= MaxLoginFailCount)
        {
            _loginFailCache.Set(lockoutKey, DateTime.UtcNow.Add(LockoutDuration), new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = LockoutDuration, });
            _loginFailCache.Remove(failKey);
            throw BusinessError.Oh($"登录失败次数过多，已锁定 {(int)LockoutDuration.TotalMinutes} 分钟");
        }

        throw BusinessError.Oh("账号或密码错误");
    }

    /// <summary>
    /// 生成Token令牌
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<LoginResponse> CreateTokenAsync(SystemUserBase user)
    {
        if (user.AccountType != AccountTypeEnum.SuperAdmin)
        {
            await _serviceProvider.GetRequiredService<OnlineUserService>().SingleLoginAsync(user.Id);
        }

        // 生成Token令牌
        var tokenExpire = await _systemConfigService.GetTokenExpireAsync();
        var accessToken = await _platformJwtTokenService.CreateAccessTokenAsync(new Dictionary<string, object>
        {
            { ClaimConst.UserId, user.Id },
            { ClaimConst.Account, user.Account },
            { ClaimConst.RealName, user.RealName },
            { ClaimConst.AccountType, user.AccountType },
        }, tokenExpire);

        // 生成刷新Token令牌
        var refreshTokenExpire = await _systemConfigService.GetRefreshTokenExpireAsync();
        var refreshToken = await _platformJwtTokenService.GenerateRefreshTokenAsync(accessToken, refreshTokenExpire);

        // 设置响应报文头
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new InvalidOperationException("CreateToken 必须在 HTTP 请求上下文中调用。");
        }

        httpContext.SetTokensOfResponseHeaders(accessToken, refreshToken);

        // Swagger Knife4UI-AfterScript登录脚本
        // ke.global.setAllHeader('Authorization', 'Bearer ' + ke.response.headers['access-token']);

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };
    }

    private async Task<SystemUserBase?> GetLoginUserByAccountAsync(string account)
    {
        var db = _systemUserRepository.Context;
        var systemUser = await db.Queryable<SystemUserEntity>()
                                 .Where(u => u.IsDeleted == false && u.Account == account)
                                 .FirstAsync();
        if (systemUser != null)
        {
            return systemUser;
        }

        return await db.Queryable<UserEntity>()
                       .Where(u => u.IsDeleted == false && u.Account == account)
                       .FirstAsync();
    }

    /// <summary>
    /// 获取登录账号
    /// </summary>
    /// <returns></returns>
    [AuthenticatedOnly]
    [DisplayName("获取登录账号")]
    [ApiDescriptionSettings(Name = "GetUserInfo")]
    public async Task<LoginUserResponse> GetUserInfoAsync()
    {
        var userId = _currentUserContext.UserId;
        SystemUserBase? user = _currentUserContext.SuperAdmin
            ? await _systemUserRepository.GetFirstAsync(u => u.Id == userId)
            : await _systemUserRepository.Context.Queryable<UserEntity>().FirstAsync(u => u.IsDeleted == false && u.Id == userId);

        if (user == null)
        {
            throw BusinessError.Oh(SystemUserErrorCode.NotLoggedIn).StatusCode(401);
        }

        var buttons = await _serviceProvider.GetRequiredService<GlobalMenuService>().GetOwnButtonPermListAsync();

        return new LoginUserResponse
        {
            Id = user.Id,
            Account = user.Account,
            RealName = user.RealName,
            AccountType = user.AccountType,
            Avatar = user.Avatar,
            Address = user.Address,
            Signature = user.Signature,
            Buttons = buttons,
        };
    }

    /// <summary>
    /// 获取刷新Token
    /// </summary>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    [AuthenticatedOnly]
    [DisplayName("获取刷新Token")]
    [ApiDescriptionSettings(Name = "GetRefreshToken")]
    public async Task<HttpJsonResultData<RefreshTokenResponse>> GetRefreshTokenAsync([FromQuery] string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw BusinessError.Oh("AccessToken不能为空");
        }

        var refreshTokenExpire = await _systemConfigService.GetRefreshTokenExpireAsync();
        var refreshToken = await _platformJwtTokenService.GenerateRefreshTokenAsync(accessToken, refreshTokenExpire);
        return HttpJsonResultData<RefreshTokenResponse>.Success(new RefreshTokenResponse
        {
            RefreshToken = refreshToken,
        });
    }

    /// <summary>
    /// 退出系统
    /// </summary>
    [AuthenticatedOnly]
    [DisplayName("退出系统")]
    public void Logout()
    {
        if (string.IsNullOrWhiteSpace(_currentUserContext.Account))
        {
            throw BusinessError.Oh(SystemUserErrorCode.NotLoggedIn);
        }
    }

    /// <summary>
    /// 获取登录配置
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [SuppressMonitor]
    [DisplayName("获取登录配置")]
    [ApiDescriptionSettings(Name = "GetLoginConfig")]
    public async Task<HttpJsonResultData<LoginConfigResponse>> GetLoginConfigAsync()
    {
        var secondVerEnabled = await _systemConfigService.GetConfigValueAsync<bool>(CommonConst.SysSecondVer);
        var captchaSettings = await _platformCaptchaConfigProvider.GetRequiredSettingsAsync();
        var superAdminUser = await _systemUserRepository.AsQueryable()
            .Where(u => u.AccountType == AccountTypeEnum.SuperAdmin)
            .FirstAsync();
        return HttpJsonResultData<LoginConfigResponse>.Success(new LoginConfigResponse
        {
            SecondVerEnabled = secondVerEnabled,
            CaptchaEnabled = captchaSettings.IsEnabled,
            EmailRegisterEnabled = false,
            // 无超管行属异常态，按已配置处理避免误触发向导（列本身为严格二值，不再承载 NULL 语义）
            SuperAdminPasswordConfigured = superAdminUser?.PasswordConfigured ?? true,
        });
    }

    /// <summary>
    /// 获取水印配置
    /// </summary>
    /// <returns></returns>
    [AuthenticatedOnly]
    [SuppressMonitor]
    [DisplayName("获取水印配置")]
    [ApiDescriptionSettings(Name = "GetWatermarkConfig")]
    public async Task<HttpJsonResultData<WatermarkConfigResponse>> GetWatermarkConfigAsync()
    {
        var watermarkEnabled = await _systemConfigService.GetConfigValueAsync<bool>(CommonConst.SysWatermark);
        return HttpJsonResultData<WatermarkConfigResponse>.Success(new WatermarkConfigResponse
        {
            WatermarkEnabled = watermarkEnabled,
        });
    }

    /// <summary>
    /// 获取验证码
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [SuppressMonitor]
    [DisplayName("获取验证码")]
    [ApiDescriptionSettings(Name = "GetCaptcha")]
    public async Task<HttpJsonResultData<CaptchaResponse>> GetCaptchaAsync()
    {
        var captchaSettings = await _platformCaptchaConfigProvider.GetRequiredSettingsAsync();
        if (!captchaSettings.IsEnabled)
        {
            throw BusinessError.Oh("图形验证码未启用");
        }

        var codeId = SnowFlakeIdHelper.Instance.NextId().ToString();
        var captcha = _captcha.Generate(codeId);
        return HttpJsonResultData<CaptchaResponse>.Success(new CaptchaResponse
        {
            Id = codeId,
            Img = captcha.Base64,
        });
    }

    /// <summary>
    /// 平台超管首装密码初始化：仅在密码未配置窗口内可用，设置成功即自动登录。
    /// <para>fail-closed：密码一旦配置（向导设置或环境变量注入）该端点永久拒绝；
    /// 条件更新（WHERE password_configured = false）保证并发竞速只有一票生效。</para>
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [SuppressMonitor]
    [DisplayName("平台超管首装密码初始化")]
    [ApiDescriptionSettings(Name = "SetupSuperAdminPassword")]
    public async Task<LoginResponse> SetupSuperAdminPasswordAsync([Required] SetupSuperAdminPasswordRequest input)
    {
        var clientKey = _httpContextAccessor.HttpContext?.GetRemoteIpAddressToIPv4();
        if (string.IsNullOrWhiteSpace(clientKey))
        {
            clientKey = "unknown-client";
        }

        EnsureSetupPasswordAttemptAllowed(clientKey);

        if (!string.Equals(input.Password, input.ConfirmPassword, StringComparison.Ordinal))
        {
            throw BusinessError.Oh(SystemUserErrorCode.SetupPasswordConfirmMismatch);
        }

        // 密码强度（与 UserService.ChangePwd 同构：平台密码策略开关控制正则校验）
        if (CryptogramUtility.StrongPassword && !Regex.IsMatch(input.Password, CryptogramUtility.PasswordStrengthValidation))
        {
            throw BusinessError.Oh(CryptogramUtility.PasswordStrengthValidationMsg);
        }

        // 条件更新防竞速：仅当仍处于未配置窗口时生效，抢占失败即 fail-closed
        var affectedRows = await _systemUserRepository.AsUpdateable()
            .SetColumns(u => new SystemUserEntity
            {
                Password = CryptogramUtility.HashPassword(input.Password),
                PasswordConfigured = true,
            })
            .Where(u => u.AccountType == AccountTypeEnum.SuperAdmin && u.PasswordConfigured == false)
            .ExecuteCommandAsync();

        if (affectedRows <= 0)
        {
            throw BusinessError.Oh(SystemUserErrorCode.SuperAdminPasswordAlreadyConfigured).StatusCode(403);
        }

        var superAdminUser = await _systemUserRepository.AsQueryable()
            .FirstAsync(u => u.AccountType == AccountTypeEnum.SuperAdmin);
        if (superAdminUser == null)
        {
            throw BusinessError.Oh(SystemUserErrorCode.AccountNotFound);
        }

        return await CreateTokenAsync(superAdminUser);
    }

    /// <summary>
    /// 校验首装密码初始化调用频率（IP 维度间隔与每日上限），超限抛业务错误；每次尝试均计数。
    /// </summary>
    /// <param name="clientKey">客户端标识（IP）。</param>
    private static void EnsureSetupPasswordAttemptAllowed(string clientKey)
    {
        if (_setupPasswordAttemptLimiter.TryGetValue(clientKey, out var lastAttempt))
        {
            var elapsed = (DateTime.UtcNow - lastAttempt).TotalSeconds;
            if (elapsed < SetupPasswordMinIntervalSeconds)
            {
                throw BusinessError.Oh(SystemUserErrorCode.OperationTooFrequent);
            }
        }

        var dailyKey = $"{clientKey}:setup-password:{DateTime.UtcNow:yyyyMMdd}";
        if (_setupPasswordDailyAttemptCounter.TryGetValue(dailyKey, out var count) && count >= SetupPasswordMaxDailyAttemptCount)
        {
            throw BusinessError.Oh(SystemUserErrorCode.DailyOperationLimitReached);
        }

        _setupPasswordAttemptLimiter[clientKey] = DateTime.UtcNow;
        _setupPasswordDailyAttemptCounter.AddOrUpdate(dailyKey, 1, (_, attemptCount) => attemptCount + 1);
    }

    /// <summary>
    /// Swagger登录检查
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("/swagger/checkUrl")]
    [NonUnify]
    [DisplayName("Swagger登录检查")]
    public int SwaggerCheckUrl()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return 401;
        }

        return httpContext.User.Identity?.IsAuthenticated == true ? 200 : 401;
    }

    /// <summary>
    /// Swagger登录提交
    /// </summary>
    /// <param name="auth"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [HttpPost("/swagger/submitUrl")]
    [NonUnify]
    [DisplayName("Swagger登录提交")]
    public async Task<int> SwaggerSubmitUrlAsync([FromForm] SwaggerAuthRequest auth)
    {
        try
        {
            var user = await _systemUserRepository.AsQueryable().Filter(null, true)
                                           .FirstAsync(u => u.Account.Equals(auth.UserName));
            if (user == null)
            {
                return 401;
            }

            if (user.IsEnabled == false)
            {
                return 401;
            }

            var passwordCorrect = CryptogramUtility.VerifyPassword(auth.Password, user.Password);
            if (!passwordCorrect)
            {
                return 401;
            }

            await CreateTokenAsync(user);
            return 200;
        }
        catch (Exception)
        {
            return 401;
        }
    }
}
