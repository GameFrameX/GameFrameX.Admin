using Microsoft.Extensions.Caching.Memory;
using GameFrameX.Core.Authorization;
using GameFrameX.Core.Service.Auth.Dto;
using GameFrameX.Core.Service.Menu;
using GameFrameX.Core.Service.Message;
using GameFrameX.Core.Service.OnlineUser;
using GameFrameX.Core.Service.PlatformCaptcha;
using GameFrameX.Core.Service.PlatformJwt;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.Service.Tenant.Dto;
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
/// 系统登录授权服务（主库系统账号语义，GFX-706 名实说明）。
/// <para>本服务注入 <see cref="SqlSugarRepository{SystemUserEntity}"/>，操作主库系统账号表 system_user_entity，
/// 与租户库侧 TenantUserService（GFX-695 已更名，管租户用户 TenantUserEntity）构成双库镜像命名协同：
/// System* 前缀在此处指主库系统域，名实相符，保留类名。</para>
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
    private readonly ITenantDbConnectionProvider _tenantDbConnectionProvider;
    private readonly IPlatformJwtTokenService _platformJwtTokenService;
    private readonly IPlatformCaptchaConfigProvider _platformCaptchaConfigProvider;
    private static readonly MemoryCache _loginFailCache = new(new MemoryCacheOptions());
    private const int MaxLoginFailCount = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan FailCountDuration = TimeSpan.FromMinutes(30);
    private static readonly ConcurrentDictionary<string, DateTime> _registerCodeSendRateLimiter = new();
    private static readonly ConcurrentDictionary<string, int> _registerCodeDailySendCounter = new();
    private const int RegisterCodeMinSendIntervalSeconds = 60;
    private const int RegisterCodeMaxDailySendCount = 10;
    private const int RegisterCodeValidMinutes = 5;

    private static readonly ConcurrentDictionary<string, DateTime> _setupPasswordAttemptLimiter = new();
    private static readonly ConcurrentDictionary<string, int> _setupPasswordDailyAttemptCounter = new();
    private const int SetupPasswordMinIntervalSeconds = 10;
    private const int SetupPasswordMaxDailyAttemptCount = 30;

    /// <summary>
    /// Admin 登录账号解析结果。
    /// </summary>
    public sealed class LoginAccountResolution
    {
        /// <summary>
        /// 用于真实用户查询的账号。租户后缀语法命中时会去掉最后一个 <c>@租户ID</c>。
        /// </summary>
        public string Account { get; init; } = null!;

        /// <summary>
        /// 登录请求最终使用的租户 ID。显式 TenantId 优先；仅在没有 TenantId / TenantHost 时才从账号后缀推导。
        /// </summary>
        public long? TenantId { get; init; }

        /// <summary>
        /// 暴力破解防护使用的归一化键，避免同账号不同租户互相污染失败计数。
        /// </summary>
        public string LoginFailKey { get; init; } = null!;
    }

    public SystemAuthService(CurrentUserContext currentUserContext,
        SqlSugarRepository<SystemUserEntity> systemUserRepository,
        IHttpContextAccessor httpContextAccessor,
        IServiceProvider serviceProvider,
        SystemConfigService systemConfigService,
        ICaptcha captcha,
        ITenantDbConnectionProvider tenantDbConnectionProvider,
        IPlatformJwtTokenService platformJwtTokenService,
        IPlatformCaptchaConfigProvider platformCaptchaConfigProvider)
    {
        _currentUserContext = currentUserContext;
        _systemUserRepository = systemUserRepository;
        _httpContextAccessor = httpContextAccessor;
        _serviceProvider = serviceProvider;
        _systemConfigService = systemConfigService;
        _captcha = captcha;
        _tenantDbConnectionProvider = tenantDbConnectionProvider;
        _platformJwtTokenService = platformJwtTokenService;
        _platformCaptchaConfigProvider = platformCaptchaConfigProvider;
    }

    /// <summary>
    /// 解析 Admin 登录账号中的租户后缀。
    /// </summary>
    /// <param name="account">用户输入的账号。</param>
    /// <param name="tenantId">显式租户 ID。</param>
    /// <param name="tenantHost">显式租户 Host。</param>
    /// <returns>用于登录查询的账号与租户上下文。</returns>
    public static LoginAccountResolution ResolveLoginAccount(string account, long? tenantId, string tenantHost)
    {
        var resolvedAccount = account;
        var resolvedTenantId = tenantId;

        if (!resolvedTenantId.HasValue && string.IsNullOrWhiteSpace(tenantHost) && !string.IsNullOrWhiteSpace(account))
        {
            var separatorIndex = account.LastIndexOf('@');
            if (separatorIndex > 0 && separatorIndex < account.Length - 1)
            {
                var suffix = account[(separatorIndex + 1)..];
                if (suffix.All(u => u >= '0' && u <= '9') && long.TryParse(suffix, out var suffixTenantId) && suffixTenantId > 0)
                {
                    resolvedAccount = account[..separatorIndex];
                    resolvedTenantId = suffixTenantId;
                }
            }
        }

        return new LoginAccountResolution
        {
            Account = resolvedAccount,
            TenantId = resolvedTenantId,
            LoginFailKey = BuildLoginFailKey(resolvedAccount, resolvedTenantId, tenantHost),
        };
    }

    private static string BuildLoginFailKey(string account, long? tenantId, string tenantHost)
    {
        if (tenantId.HasValue && tenantId.Value > 0)
        {
            return $"{account}@tenant:{tenantId.Value}";
        }

        if (!string.IsNullOrWhiteSpace(tenantHost))
        {
            return $"{account}@host:{tenantHost.Trim()}";
        }

        return account;
    }

    /// <summary>
    /// 账号密码登录
    /// </summary>
    /// <param name="input"></param>
    /// <remarks>用户名/密码：superadmin/密码首次安装时随机生成并打印于控制台</remarks>
    /// <returns></returns>
    [AllowAnonymous]
    [BypassTenantExpiration]
    [DisplayName("账号密码登录")]
    [ApiDescriptionSettings(Name = "Login")]
    public async Task<LoginResponse> LoginAsync([Required] LoginRequest input)
    {
        var loginAccount = ResolveLoginAccount(input.Account, input.TenantId, input.TenantHost);

        // 暴力破解防护：检查锁定状态
        var lockoutKey = $"lockout:pwd:{loginAccount.LoginFailKey}";
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

        // 账号是否存在（C216 枚举防护：user null 跑假 Verify 对齐耗时 + 统一响应，不再抛 D0009）
        var user = await GetLoginUserByAccountAsync(loginAccount.Account, loginAccount.TenantId, input.TenantHost);
        if (user == null)
        {
            _ = CryptogramUtility.VerifyPassword(input.Password, CryptogramUtility.EnumerationDecoyHash);
            RecordLoginFailure($"fail:pwd:{loginAccount.LoginFailKey}", lockoutKey);
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

        // 租户是否被禁用：显式走主库 SystemTenantEntity，避免租户上下文/边界导致漏查而放行禁用租户（Z1003）
        var mainDb = _systemUserRepository.AsTenant().GetConnectionScope(SqlSugarConst.MainConfigId);
        var tenant = await mainDb.Queryable<SystemTenantEntity>().Filter(null, true).FirstAsync(u => u.Id == user.TenantId);
        if (tenant != null && tenant.IsEnabled == false)
        {
            throw BusinessError.Oh(TenantErrorCode.TenantDisabled);
        }

        // 密码是否正确（Argon2id 校验，不认历史 MD5/SM2/SM4 密文）
        var passwordCorrect = CryptogramUtility.VerifyPassword(input.Password, user.Password);

        if (!passwordCorrect)
        {
            RecordLoginFailure($"fail:pwd:{loginAccount.LoginFailKey}", lockoutKey);
        }

        // 登录成功，清除计数
        _loginFailCache.Remove($"fail:pwd:{loginAccount.LoginFailKey}");
        _loginFailCache.Remove($"lockout:pwd:{loginAccount.LoginFailKey}");

        return await CreateTokenAsync(user);
    }

    /// <summary>
    /// 记录登录失败并抛统一响应（C216 枚举防护）：<paramref name="failKey"/> 累计失败次数，达 <see cref="MaxLoginFailCount"/> 设置
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
        // 租户用户单点登录必须在明确租户上下文内执行；SuperAdmin 平台登录不访问 tenant_online_user。
        if (user.AccountType != AccountTypeEnum.SuperAdmin && user.TenantId > 0)
        {
            using (TenantOperationContext.UseTenant(user.TenantId.Value))
            {
                await _serviceProvider.GetRequiredService<TenantOnlineUserService>().SingleLoginAsync(user.Id);
            }
        }

        // 生成Token令牌
        var tokenExpire = await _systemConfigService.GetTokenExpireAsync();
        var accessToken = await _platformJwtTokenService.CreateAccessTokenAsync(new Dictionary<string, object>
        {
            { ClaimConst.UserId, user.Id },
            { ClaimConst.TenantId, user.TenantId ?? 0L },
            { ClaimConst.Account, user.Account },
            { ClaimConst.RealName, user.RealName },
            { ClaimConst.AccountType, user.AccountType },
            { ClaimConst.OrganizationId, user.OrganizationId },
            { ClaimConst.OrganizationName, user.TenantOrganizationEntity?.Name ?? string.Empty },
            { ClaimConst.OrganizationType, user.TenantOrganizationEntity?.Type ?? string.Empty },
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

    private async Task<SystemUserBase?> GetLoginUserByAccountAsync(string account, long? tenantId, string tenantHost)
    {
        return await GetLoginUserAsync(tenantId, tenantHost, account);
    }

    private async Task<SystemUserBase?> GetLoginUserAsync(long? tenantId, string tenantHost, string account)
    {
        var mainDb = _systemUserRepository.AsTenant().GetConnectionScope(SqlSugarConst.MainConfigId);
        mainDb.QueryFilter.ClearAndBackup();
        try
        {
            var platformUser = await mainDb.Queryable<SystemUserEntity>()
                                           .Where(u => u.Account == account)
                                           .Where(u => u.AccountType == AccountTypeEnum.SuperAdmin)
                                           .FirstAsync();
            if (platformUser != null)
            {
                return platformUser;
            }
        }
        finally
        {
            mainDb.QueryFilter.Restore();
        }

        var resolvedTenantId = ResolveLoginTenantId(tenantId, tenantHost);
        if (!resolvedTenantId.HasValue)
        {
            return null;
        }

        var tenantDb = _tenantDbConnectionProvider.GetTenantDbConnectionScope(resolvedTenantId.Value);
        if (tenantDb == null)
        {
            return null;
        }

        tenantDb.QueryFilter.ClearAndBackup();
        try
        {
            return await tenantDb.Queryable<TenantUserEntity>()
                                 .Includes(u => u.TenantOrganizationEntity)
                                 .Where(u => u.Account == account)
                                 .FirstAsync();
        }
        finally
        {
            tenantDb.QueryFilter.Restore();
        }
    }

    private long? ResolveLoginTenantId(long? tenantId, string tenantHost)
    {
        if (tenantId.HasValue && tenantId.Value > 0)
        {
            return tenantId.Value;
        }

        var request = _httpContextAccessor.HttpContext?.Request;
        var tenantIdText = request?.Headers[GlobalConst.TenantIdHeader].FirstOrDefault();
        if (long.TryParse(tenantIdText, out var tenantIdFromHeader) && tenantIdFromHeader > 0)
        {
            return tenantIdFromHeader;
        }

        var host = tenantHost;
        if (string.IsNullOrWhiteSpace(host))
        {
            host = request?.Headers[GlobalConst.TenantHostHeader].FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(host))
        {
            host = request?.Host.Host;
        }

        if (string.IsNullOrWhiteSpace(host))
        {
            return null;
        }

        var mainDb = _systemUserRepository.AsTenant().GetConnectionScope(SqlSugarConst.MainConfigId);
        return mainDb.Queryable<SystemTenantEntity>()
                     .Filter(null, true)
                     .Where(u => u.Host == host)
                     .Select(u => (long?)u.Id)
                     .First();
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
        SystemUserBase? user;
        TenantOrganizationEntity? org = null;
        TenantPositionEntity? pos = null;
        if (_currentUserContext.SuperAdmin)
        {
            user = await _systemUserRepository.GetFirstAsync(u => u.Id == userId);
        }
        else
        {
            var tenantDb = _tenantDbConnectionProvider.GetTenantDbConnectionScope(_currentUserContext.TenantId);
            if (tenantDb == null)
            {
                throw BusinessError.Oh(SystemUserErrorCode.NoPermission);
            }

            user = await tenantDb.Queryable<TenantUserEntity>().FirstAsync(u => u.Id == userId);
            if (user != null)
            {
                org = await tenantDb.Queryable<TenantOrganizationEntity>().FirstAsync(u => u.Id == user.OrganizationId);
                pos = await tenantDb.Queryable<TenantPositionEntity>().FirstAsync(u => u.Id == user.PosId);
            }
        }

        if (user == null)
        {
            throw BusinessError.Oh(SystemUserErrorCode.NotLoggedIn).StatusCode(401);
        }

        // 获取拥有按钮权限集合。菜单服务会按需构造租户角色仓储，平台 SuperAdmin 路径不能在构造阶段触发租户表访问。
        var buttons = _currentUserContext.SuperAdmin
                          ? await _serviceProvider.GetRequiredService<GlobalMenuService>().GetOwnButtonPermListAsync()
                          : await GetTenantUserButtonPermissionsAsync();

        return new LoginUserResponse
        {
            Id = user.Id,
            Account = user.Account,
            RealName = user.RealName,
            AccountType = user.AccountType,
            Avatar = user.Avatar,
            Address = user.Address,
            Signature = user.Signature,
            OrganizationId = user.OrganizationId,
            OrganizationName = org?.Name ?? string.Empty,
            OrganizationType = org?.Type ?? string.Empty,
            PosName = pos?.Name ?? string.Empty,
            Buttons = buttons,
        };
    }

    private async Task<List<string>> GetTenantUserButtonPermissionsAsync()
    {
        using (TenantOperationContext.UseTenant(_currentUserContext.TenantId))
        {
            return await _serviceProvider.GetRequiredService<GlobalMenuService>().GetOwnButtonPermListAsync();
        }
    }

    /// <summary>
    /// 获取刷新Token
    /// </summary>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    [AuthenticatedOnly]
    [BypassTenantExpiration]
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
    [BypassTenantExpiration]
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
        var emailRegisterEnabled = await _systemConfigService.GetConfigValueAsync<bool>(CommonConst.SysEmailRegister);
        var captchaSettings = await _platformCaptchaConfigProvider.GetRequiredSettingsAsync();
        var superAdminUser = await _systemUserRepository.AsQueryable()
            .Where(u => u.AccountType == AccountTypeEnum.SuperAdmin)
            .FirstAsync();
        return HttpJsonResultData<LoginConfigResponse>.Success(new LoginConfigResponse
        {
            SecondVerEnabled = secondVerEnabled,
            CaptchaEnabled = captchaSettings.IsEnabled,
            EmailRegisterEnabled = emailRegisterEnabled,
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
    [BypassTenantExpiration]
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
    /// 发送邮箱自助注册验证码
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [SuppressMonitor]
    [BypassTenantExpiration]
    [DisplayName("发送邮箱自助注册验证码")]
    [ApiDescriptionSettings(Name = "SendRegisterEmailCode")]
    public async Task SendRegisterEmailCodeAsync(SendRegisterEmailCodeRequest input)
    {
        var emailRegisterEnabled = await _systemConfigService.GetConfigValueAsync<bool>(CommonConst.SysEmailRegister);
        if (!emailRegisterEnabled)
        {
            throw BusinessError.Oh(SystemUserErrorCode.EmailRegisterDisabled);
        }

        EnsureRegisterCodeSendAllowed(input.Email);

        var email = input.Email.Trim();
        var code = Random.Shared.Next(100000, 1000000).ToString();
        var codeRepository = _serviceProvider.GetRequiredService<SqlSugarRepository<SystemRegisterEmailCodeEntity>>();
        await codeRepository.InsertAsync(new SystemRegisterEmailCodeEntity
        {
            Email = email,
            Code = code,
            ExpirationTime = TimerHelper.UnixTimeSeconds() + RegisterCodeValidMinutes * 60,
        });

        var body = $"您正在注册 GameFrameX 平台租户，验证码：{code}，{RegisterCodeValidMinutes} 分钟内有效。若非本人操作请忽略本邮件。";
        var sent = await _serviceProvider.GetRequiredService<ISystemEmailSender>()
            .SendAsync(SystemEmailPurpose.RegisterVerificationCode, email, "邮箱自助注册验证码", body);
        if (!sent)
        {
            throw BusinessError.Oh(SystemUserErrorCode.EmailSendFailed);
        }

        RecordRegisterCodeSent(email);
    }

    /// <summary>
    /// 邮箱自助注册：校验验证码后创建租户并签发登录令牌
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [AllowAnonymous]
    [SuppressMonitor]
    [BypassTenantExpiration]
    [DisplayName("邮箱自助注册")]
    [ApiDescriptionSettings(Name = "RegisterByEmail")]
    public async Task<LoginResponse> RegisterByEmailAsync(RegisterByEmailRequest input)
    {
        var emailRegisterEnabled = await _systemConfigService.GetConfigValueAsync<bool>(CommonConst.SysEmailRegister);
        if (!emailRegisterEnabled)
        {
            throw BusinessError.Oh(SystemUserErrorCode.EmailRegisterDisabled);
        }

        var email = input.Email.Trim();
        var codeRepository = _serviceProvider.GetRequiredService<SqlSugarRepository<SystemRegisterEmailCodeEntity>>();
        // 取最新一条匹配且未过期的验证码
        var registerCode = await codeRepository.AsQueryable()
            .Where(u => u.Email == email && u.Code == input.Code && u.ExpirationTime > TimerHelper.UnixTimeSeconds())
            .OrderByDescending(u => u.Id)
            .FirstAsync();
        var validCode = EnsureRegisterCodeValid(registerCode);

        // 验证成功后使验证码失效，防止重用
        validCode.ExpirationTime = TimerHelper.UnixTimeSeconds() - 60;
        await codeRepository.Context.UpdateWithOptimisticLockAsync(validCode);

        var adminUser = await _serviceProvider.GetRequiredService<SystemTenantService>()
            .AddTenantCoreAsync(new AddTenantRequest
            {
                Name = input.TenantName,
                AdminAccount = email,
                Email = email,
                Phone = string.Empty,
                TenantType = TenantTypeEnum.Db,
                DbType = DbType.PostgreSQL,
            }, CryptogramUtility.HashPassword(input.Password));
        if (adminUser == null)
        {
            throw new InvalidOperationException("邮箱自助注册初始化租户管理员失败。");
        }

        return await CreateTokenAsync(adminUser);
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
    [BypassTenantExpiration]
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

        // 密码强度（与 TenantUserService.ChangePwd 同构：平台密码策略开关控制正则校验）
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
    /// 校验邮箱注册验证码查询结果：未命中或已过期抛统一错误。
    /// </summary>
    /// <param name="registerCode">按邮箱+验证码查询到的最新一条记录，可为 <c>null</c></param>
    /// <returns>有效验证码实体（原实例）</returns>
    internal static SystemRegisterEmailCodeEntity EnsureRegisterCodeValid(SystemRegisterEmailCodeEntity? registerCode)
    {
        if (registerCode == null || registerCode.ExpirationTime <= TimerHelper.UnixTimeSeconds())
        {
            throw BusinessError.Oh(SystemUserErrorCode.EmailCodeNotFoundOrExpired);
        }

        return registerCode;
    }

    /// <summary>
    /// 校验邮箱注册验证码发送频率（间隔与每日上限），超限抛业务错误。
    /// </summary>
    /// <param name="email">注册邮箱</param>
    private static void EnsureRegisterCodeSendAllowed(string email)
    {
        if (_registerCodeSendRateLimiter.TryGetValue(email, out var lastSent))
        {
            var elapsed = (DateTime.UtcNow - lastSent).TotalSeconds;
            if (elapsed < RegisterCodeMinSendIntervalSeconds)
            {
                throw BusinessError.Oh(SystemUserErrorCode.EmailCodeSendTooFrequent);
            }
        }

        var dailyKey = $"{email}:{DateTime.UtcNow:yyyyMMdd}";
        if (_registerCodeDailySendCounter.TryGetValue(dailyKey, out var count) && count >= RegisterCodeMaxDailySendCount)
        {
            throw BusinessError.Oh(SystemUserErrorCode.EmailCodeDailyLimitReached);
        }
    }

    /// <summary>
    /// 记录邮箱注册验证码发送（更新频率限制计数器）
    /// </summary>
    /// <param name="email">注册邮箱</param>
    private static void RecordRegisterCodeSent(string email)
    {
        _registerCodeSendRateLimiter[email] = DateTime.UtcNow;

        var dailyKey = $"{email}:{DateTime.UtcNow:yyyyMMdd}";
        _registerCodeDailySendCounter.AddOrUpdate(dailyKey, 1, (_, sentCount) => sentCount + 1);
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