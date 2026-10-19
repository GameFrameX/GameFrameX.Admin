using System.Collections.Concurrent;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Client.Api.GameUserEmail.Dto;
using GameFrameX.Client.Api.Middleware;
using GameFrameX.Client.Api.Services.Email;
using GameFrameX.Core.Service;

namespace GameFrameX.Client.Api.GameUserEmail;

/// <summary>
/// 用户邮件验证码控制器
/// </summary>
public class GameUserEmailController : BaseController<TenantGameUserEmailEntity>
{
    private readonly Repository<TenantGameUserEntity> _gameUserEmailService;
    private readonly ITenantEmailSender _emailSender;

    /// <summary>
    /// 邮件发送频率限制记录（邮箱 → 最后发送时间）
    /// </summary>
    private static readonly ConcurrentDictionary<string, DateTime> _sendRateLimiter = new();

    /// <summary>
    /// 邮件每日发送计数（邮箱+日期 → 已发送次数）
    /// </summary>
    private static readonly ConcurrentDictionary<string, int> _dailySendCounter = new();

    private const int MinSendIntervalSeconds = 60;
    private const int MaxDailySendCount = 10;

    /// <summary>
    ///
    /// </summary>
    /// <param name="service"></param>
    /// <param name="gameUserEmailService"></param>
    /// <param name="emailSender"></param>
    public GameUserEmailController(SqlSugarRepository<TenantGameUserEmailEntity> service, Repository<TenantGameUserEntity> gameUserEmailService, ITenantEmailSender emailSender) : base(service)
    {
        _gameUserEmailService = gameUserEmailService;
        _emailSender = emailSender;
    }

    /// <summary>
    /// 发送注册邮件验证码
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> SendRegister(GameUserMailRegisterEntityRequest input)
    {
        // 应用作用域：AppId 只来自可信中间件上下文，不接受请求体覆盖（app-scope-backend-foundation.md §5.2）。
        if (!TryGetContextLong(AppResolutionMiddleware.AppIdKey, out var appId))
        {
            return GameApiResult.Error<BaseOperationAckResponse>(AuthErrorCode.AuthorizeInvalid, "应用未认证");
        }

        AppScopeRules.EnsureFactAppId(appId, "用户邮箱验证码");

        var rateLimitError = CheckEmailRateLimit(input.Email);
        if (rateLimitError != null)
        {
            return rateLimitError;
        }

        var entity = input.Adapt<TenantGameUserEmailEntity>();
        entity.UserName = input.Email;
        entity.Code = Random.Shared.Next(100000, 1000000).ToString();
        entity.ExpirationTime = TimerHelper.UnixTimeSeconds() + 5 * 60; //设置过期时间为5分钟后
        // 事实数据应用作用域，来自可信中间件上下文
        entity.AppId = appId;
        if (input.Language.Contains("zh"))
        {
            entity.TemplateId = 145225.ToString();
        }
        else
        {
            entity.TemplateId = 145226.ToString();
        }

        var templateData = new Dictionary<string, string>
        {
            { "expiry_minutes", "5" }, // 设置过期时间为5分钟
            { "username", input.Email },
            { "verification_code", entity.Code },
        };
        var subject = input.Language.Contains("zh") ? "欢迎注册" : "Welcome to Register";
        var result = await _emailSender.SendTemplateAsync(TenantEmailPurpose.PlayerRegisterCode, input.Email, Convert.ToUInt64(entity.TemplateId), subject, templateData);

        if (result)
        {
            await PRepository.InsertAsync(entity);
            RecordEmailSent(input.Email);
            return HttpJsonResultData<BaseOperationAckResponse>.Success();
        }

        return HttpJsonResultData<BaseOperationAckResponse>.ServerError();
    }

    /// <summary>
    /// 验证邮件验证码
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> VerifyCode(GameUserMailVerifyEntityRequest input)
    {
        var gameUserEntity = await PRepository.GetAsync(m => m.UserName == input.Email && m.Code == input.Code && m.ExpirationTime > TimerHelper.UnixTimeSeconds());
        if (gameUserEntity == null)
        {
            // 如果账号不存在，则直接返回错误
            return GameApiResult.Error<BaseOperationAckResponse>(AccountErrorCode.EmailCodeNotFoundOrExpired, "验证码错误或已过期，请重新获取验证码。");
        }

        // 验证成功后使验证码失效，防止重用
        gameUserEntity.ExpirationTime = TimerHelper.UnixTimeSeconds() - 60;
        await PRepository.Context.UpdateWithOptimisticLockAsync(gameUserEntity);

        return HttpJsonResultData<BaseOperationAckResponse>.Success();
    }

    /// <summary>
    /// 发送重置密码邮件验证码
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> SendReset(GameUserMailResetEntityRequest input)
    {
        // 应用作用域：AppId 只来自可信中间件上下文，不接受请求体覆盖（app-scope-backend-foundation.md §5.2）。
        if (!TryGetContextLong(AppResolutionMiddleware.AppIdKey, out var appId))
        {
            return GameApiResult.Error<BaseOperationAckResponse>(AuthErrorCode.AuthorizeInvalid, "应用未认证");
        }

        AppScopeRules.EnsureFactAppId(appId, "用户邮箱验证码");

        var rateLimitError = CheckEmailRateLimit(input.Email);
        if (rateLimitError != null)
        {
            return rateLimitError;
        }

        var gameUserEntity = await _gameUserEmailService.GetAsync(m => m.Email == input.Email);
        if (gameUserEntity == null)
        {
            // 不暴露账号不存在信息，返回成功避免枚举攻击
            return HttpJsonResultData<BaseOperationAckResponse>.Success();
        }

        var entity = input.Adapt<TenantGameUserEmailEntity>();
        entity.UserName = input.Email;
        entity.Code = Random.Shared.Next(100000, 1000000).ToString();
        entity.ExpirationTime = TimerHelper.UnixTimeSeconds() + 5 * 60; //设置过期时间为5分钟后
        // 事实数据应用作用域，来自可信中间件上下文
        entity.AppId = appId;
        if (input.Language.Contains("zh"))
        {
            entity.TemplateId = 145227.ToString();
        }
        else
        {
            entity.TemplateId = 145228.ToString();
        }

        var templateData = new Dictionary<string, string>
        {
            { "expiry_minutes", "5" }, // 设置过期时间为5分钟
            { "username", input.Email },
            { "verification_code", entity.Code },
        };
        var subject = input.Language.Contains("zh") ? "找回密码" : "Password Recovery";
        var result = await _emailSender.SendTemplateAsync(TenantEmailPurpose.PlayerPasswordReset, input.Email, Convert.ToUInt64(entity.TemplateId), subject, templateData);

        if (result)
        {
            await PRepository.InsertAsync(entity);
            RecordEmailSent(input.Email);
            return HttpJsonResultData<BaseOperationAckResponse>.Success();
        }

        return HttpJsonResultData<BaseOperationAckResponse>.ServerError();
    }

    /// <summary>
    /// 检查邮件发送频率限制（60秒间隔 + 每日10次上限）
    /// </summary>
    private HttpJsonResultData<BaseOperationAckResponse>? CheckEmailRateLimit(string email)
    {
        // 60 秒间隔检查
        if (_sendRateLimiter.TryGetValue(email, out var lastSent))
        {
            var elapsed = (DateTime.UtcNow - lastSent).TotalSeconds;
            if (elapsed < MinSendIntervalSeconds)
            {
                var remaining = (int)(MinSendIntervalSeconds - elapsed);
                return GameApiResult.Error<BaseOperationAckResponse>(SystemErrorCode.InnerError, $"发送太频繁，请{remaining}秒后再试");
            }
        }

        // 每日次数检查
        var dailyKey = $"{email}:{DateTime.UtcNow:yyyyMMdd}";
        if (_dailySendCounter.TryGetValue(dailyKey, out var count) && count >= MaxDailySendCount)
        {
            return GameApiResult.Error<BaseOperationAckResponse>(SystemErrorCode.InnerError, "今日发送次数已达上限");
        }

        return null;
    }

    /// <summary>
    /// 记录邮件发送（更新频率限制计数器）
    /// </summary>
    private static void RecordEmailSent(string email)
    {
        _sendRateLimiter[email] = DateTime.UtcNow;

        var dailyKey = $"{email}:{DateTime.UtcNow:yyyyMMdd}";
        _dailySendCounter.AddOrUpdate(dailyKey, 1, (_, v) => v + 1);
    }

    /// <summary>
    /// 从 <see cref="HttpContext.Items"/> 读取中间件写入的 long 型上下文值（应用 / 租户等）。
    /// </summary>
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