using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Config.Dto;
using GameFrameX.Core.Service.PlatformCaptcha;
using GameFrameX.Core.Service.PlatformLogging;
using GameFrameX.Core.Service.PlatformPasswordPolicy;
using GameFrameX.Core.Service.PlatformRateLimit;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Service.Config;

/// <summary>
/// 系统参数配置服务
/// </summary>
[ApiDescriptionSettings(Order = 440)]
public class SystemConfigService : IAdminApiController
{
    private static readonly HashSet<string> ProtectedConfigCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        PlatformCaptchaConfigConstants.ConfigCode,
        PlatformRateLimitConfigConstants.ConfigCode,
        PlatformLoggingConfigConstants.ConfigCode,
        PlatformPasswordPolicyConfigConstants.ConfigCode,
    };

    private readonly SqlSugarRepository<SystemConfigEntity> _systemConfigRepository;
    private readonly SystemCacheService _systemCacheService;

    public SystemConfigService(SqlSugarRepository<SystemConfigEntity> systemConfigRepository,
        SystemCacheService systemCacheService)
    {
        _systemConfigRepository = systemConfigRepository;
        _systemCacheService = systemCacheService;
    }

    /// <summary>
    /// 获取参数配置分页列表
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取参数配置分页列表")]
    [ApiDescriptionSettings(Name = "Page")]
    public async Task<SqlSugarPagedList<SystemConfigResponse>> PageAsync(PageConfigRequest input)
    {
        return await _systemConfigRepository.AsQueryable()
                                     .Where(u => u.Code != PlatformCaptchaConfigConstants.ConfigCode && u.Code != PlatformRateLimitConfigConstants.ConfigCode && u.Code != PlatformLoggingConfigConstants.ConfigCode && u.Code != PlatformPasswordPolicyConfigConstants.ConfigCode)
                                     .WhereIF(!string.IsNullOrWhiteSpace(input.Name?.Trim()), u => u.Name.Contains(input.Name ?? string.Empty))
                                     .WhereIF(!string.IsNullOrWhiteSpace(input.Code?.Trim()), u => (u.Code ?? string.Empty).Contains(input.Code ?? string.Empty))
                                     .WhereIF(!string.IsNullOrWhiteSpace(input.GroupCode?.Trim()), u => (u.GroupCode ?? string.Empty).Equals(input.GroupCode ?? string.Empty))
                                     .OrderBuilder(input)
                                     .Select<SystemConfigResponse>()
                                     .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 获取参数配置列表
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取参数配置列表")]
    [ApiDescriptionSettings(Name = "GetList")]
    public async Task<List<SystemConfigResponse>> GetListAsync()
    {
        return await _systemConfigRepository.AsQueryable()
                                     .Where(u => u.Code != PlatformCaptchaConfigConstants.ConfigCode && u.Code != PlatformRateLimitConfigConstants.ConfigCode && u.Code != PlatformLoggingConfigConstants.ConfigCode && u.Code != PlatformPasswordPolicyConfigConstants.ConfigCode)
                                     .Select<SystemConfigResponse>()
                                     .ToListAsync();
    }

    /// <summary>
    /// 增加参数配置
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Add")]
    [HttpPost]
    [DisplayName("增加参数配置")]
    public async Task AddConfigAsync(AddConfigRequest input)
    {
        EnsureNotProtected(input.Code);
        var isExist = await _systemConfigRepository.IsAnyAsync(u => u.Name == input.Name || u.Code == input.Code);
        if (isExist)
        {
            throw BusinessError.Oh(PlatformConfigErrorCode.ParameterAlreadyExists);
        }

        await _systemConfigRepository.InsertAsync(input.Adapt<SystemConfigEntity>());
    }

    /// <summary>
    /// 更新参数配置
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新参数配置")]
    public async Task UpdateConfigAsync(UpdateConfigRequest input)
    {
        var oldConfig = await _systemConfigRepository.GetFirstAsync(u => u.Id == input.Id);
        EnsureNotProtected(oldConfig?.Code);
        EnsureNotProtected(input.Code);

        var isExist = await _systemConfigRepository.IsAnyAsync(u => (u.Name == input.Name || u.Code == input.Code) && u.Id != input.Id);
        if (isExist)
        {
            throw BusinessError.Oh(PlatformConfigErrorCode.ParameterAlreadyExists);
        }

        var config = input.Adapt<SystemConfigEntity>();
        await _systemConfigRepository.AsUpdateable(config).IgnoreColumns(true).ExecuteCommandWithOptLockAsync(true);

        _systemCacheService.Remove(config.Code ?? string.Empty);
    }

    /// <summary>
    /// 删除参数配置
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "Delete")]
    [HttpPost]
    [DisplayName("删除参数配置")]
    public async Task DeleteConfigAsync(DeleteConfigRequest input)
    {
        var config = await _systemConfigRepository.GetFirstAsync(u => u.Id == input.Id);
        if (config == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        EnsureNotProtected(config.Code);

        if (config.SysFlag == YesNoEnum.Y) // 禁止删除系统参数
        {
            throw BusinessError.Oh(PlatformConfigErrorCode.DeleteSystemParameterForbidden);
        }

        await _systemConfigRepository.DeleteAsync(config);

        _systemCacheService.Remove(config.Code ?? string.Empty);
    }

    /// <summary>
    /// 批量删除参数配置
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [ApiDescriptionSettings(Name = "BatchDelete")]
    [HttpPost]
    [DisplayName("批量删除参数配置")]
    public async Task BatchDeleteConfigAsync(List<long> ids)
    {
        foreach (var id in ids)
        {
            var config = await _systemConfigRepository.GetFirstAsync(u => u.Id == id);
            if (config == null)
            {
                continue;
            }

            EnsureNotProtected(config.Code);
            if (config.SysFlag == YesNoEnum.Y) // 禁止删除系统参数
            {
                continue;
            }

            await _systemConfigRepository.DeleteAsync(config);

            _systemCacheService.Remove(config.Code ?? string.Empty);
        }
    }

    /// <summary>
    /// 获取参数配置详情
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [DisplayName("获取参数配置详情")]
    [ApiDescriptionSettings(Name = "GetDetail")]
    public async Task<SystemConfigResponse> GetDetailAsync([FromQuery] ConfigRequest input)
    {
        var config = await _systemConfigRepository.GetFirstAsync(u => u.Id == input.Id);
        EnsureNotProtected(config?.Code);
#pragma warning disable CS8603 // 查询未命中时返回 null 是既有 API 契约（前端按 null 判断 not-found）
        return config?.Adapt<SystemConfigResponse>();
#pragma warning restore CS8603
    }

    /// <summary>
    /// 获取参数配置值
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [NonAction]
    public async Task<T> GetConfigValueAsync<T>(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
#pragma warning disable CS8603 // 配置未命中时返回 default(T) 是泛型查找方法的合法语义
            return default;
#pragma warning restore CS8603
        }

        var value = _systemCacheService.Get<string>(code);
        if (string.IsNullOrEmpty(value))
        {
            var config = await _systemConfigRepository.GetFirstAsync(u => u.Code == code);
            value = config != null ? config.Value : default;
#pragma warning disable CS8604 // value 可能是 default(string)；缓存层允许存储空串占位以避免重复回源
            _systemCacheService.Set(code, value);
#pragma warning restore CS8604
        }

        if (string.IsNullOrWhiteSpace(value))
        {
#pragma warning disable CS8603 // 配置值为空时返回 default(T) 是泛型查找方法的合法语义
            return default;
#pragma warning restore CS8603
        }

        return (T)Convert.ChangeType(value, typeof(T));
    }

    /// <summary>
    /// 设置内置配置值，仅供专用平台配置服务同步关联开关使用。
    /// </summary>
    [NonAction]
    public async Task SetConfigValueAsync(string code, string value)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return;
        }

        var config = await _systemConfigRepository.GetFirstAsync(u => u.Code == code);
        if (config == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        config.Value = value;
        await _systemConfigRepository.AsUpdateable(config).UpdateColumns(u => new { u.Value, u.UpdateTime, }).ExecuteCommandAsync();
        _systemCacheService.Remove(code);
    }

    /// <summary>
    /// 获取分组列表
    /// </summary>
    /// <returns></returns>
    [DisplayName("获取分组列表")]
    [ApiDescriptionSettings(Name = "GetGroupList")]
    public async Task<List<string>> GetGroupListAsync()
    {
        return (await _systemConfigRepository.AsQueryable()
                                      .Where(u => u.Code != PlatformCaptchaConfigConstants.ConfigCode && u.Code != PlatformRateLimitConfigConstants.ConfigCode && u.Code != PlatformLoggingConfigConstants.ConfigCode && u.Code != PlatformPasswordPolicyConfigConstants.ConfigCode)
                                      .GroupBy(u => u.GroupCode)
                                      .Select(u => u.GroupCode)
                                      .ToListAsync())
               .Where(c => !string.IsNullOrEmpty(c))
               .Select(c => c ?? string.Empty)
               .ToList();
    }

    /// <summary>
    /// 获取 Token 过期时间
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<int> GetTokenExpireAsync()
    {
        var tokenExpireStr = await GetConfigValueAsync<string>(CommonConst.SysTokenExpire);
        _ = int.TryParse(tokenExpireStr, out var tokenExpire);
        return tokenExpire == 0 ? 20 : tokenExpire;
    }

    /// <summary>
    /// 获取 RefreshToken 过期时间
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public async Task<int> GetRefreshTokenExpireAsync()
    {
        var refreshTokenExpireStr = await GetConfigValueAsync<string>(CommonConst.SysRefreshTokenExpire);
        _ = int.TryParse(refreshTokenExpireStr, out var refreshTokenExpire);
        return refreshTokenExpire == 0 ? 40 : refreshTokenExpire;
    }

    private static void EnsureNotProtected(string? code)
    {
        if (!string.IsNullOrWhiteSpace(code) && ProtectedConfigCodes.Contains(code))
        {
            throw BusinessError.Oh("该平台私有配置只能通过专用页面维护");
        }
    }
}