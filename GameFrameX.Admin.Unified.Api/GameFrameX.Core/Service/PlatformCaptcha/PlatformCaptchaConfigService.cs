using GameFrameX.Core.Service.PlatformCaptcha.Dto;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Service.PlatformCaptcha;

/// <summary>
/// 平台图形验证码配置服务。
/// <para>宿主表说明（GFX-706）：本服务不持有独立宿主表，共享主库 SystemConfigEntity（system_config 表），以 Code 分区行 <c>platform_captcha_config</c>（PlatformCaptchaConfigConstants.ConfigCode）存储本域配置 JSON。Platform* 前缀指平台级配置域，名实相符保留类名。</para>
/// </summary>
[ApiDescriptionSettings(Order = 442)]
public sealed class PlatformCaptchaConfigService : IAdminApiController
{
    private readonly SqlSugarRepository<SystemConfigEntity> _systemConfigRepository;
    private readonly SystemConfigService _systemConfigService;
    private readonly PlatformCaptchaConfigProvider _configProvider;

    public PlatformCaptchaConfigService(
        SqlSugarRepository<SystemConfigEntity> systemConfigRepository,
        SystemConfigService systemConfigService,
        PlatformCaptchaConfigProvider configProvider)
    {
        _systemConfigRepository = systemConfigRepository;
        _systemConfigService = systemConfigService;
        _configProvider = configProvider;
    }

    /// <summary>
    /// 获取平台图形验证码配置详情。
    /// </summary>
    [ApiDescriptionSettings(Name = "Detail")]
    [DisplayName("获取平台图形验证码配置详情")]
    public async Task<PlatformCaptchaConfigResponse> DetailAsync()
    {
        var config = await _systemConfigRepository.GetFirstAsync(u => u.Code == PlatformCaptchaConfigConstants.ConfigCode);
        var settings = config == null || string.IsNullOrWhiteSpace(config.Value)
                           ? CreateDefaultSettings()
                           : PlatformCaptchaConfigProvider.Deserialize(config.Value);

        PlatformCaptchaConfigProvider.Validate(settings);
        return ToOutput(settings, config?.UpdateTime);
    }

    /// <summary>
    /// 保存平台图形验证码配置。每次保存原子递增版本，使旧验证码立即失效。
    /// 采用乐观并发：以读取时的完整 <see cref="SystemConfigEntity.Value"/> 作为比较基准执行条件更新，
    /// 受影响行数为 0 表示期间有并发写入，重读后重算版本并重试，保证版本号在并发下严格单调递增。
    /// </summary>
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新平台图形验证码配置")]
    [SqlSugarTransaction]
    public async Task UpdateAsync(PlatformCaptchaConfigUpdateRequest input)
    {
        const int maxAttempts = 8;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var config = await _systemConfigRepository.GetFirstAsync(u => u.Code == PlatformCaptchaConfigConstants.ConfigCode);
            var current = config == null || string.IsNullOrWhiteSpace(config.Value)
                              ? CreateDefaultSettings()
                              : PlatformCaptchaConfigProvider.Deserialize(config.Value);

            var next = ToSettings(input, Math.Max(current.Version, 0) + 1);
            var value = PlatformCaptchaConfigProvider.Serialize(next);

            if (config == null)
            {
                try
                {
                    config = new SystemConfigEntity
                    {
                        Name = "平台图形验证码配置",
                        Code = PlatformCaptchaConfigConstants.ConfigCode,
                        Value = value,
                        SysFlag = YesNoEnum.Y,
                        GroupCode = "Security",
                        OrderNo = 20,
                        Remark = "平台私有登录图形验证码配置",
                    };
                    await _systemConfigRepository.InsertAsync(config);
                }
                catch (Exception) when (attempt < maxAttempts - 1)
                {
                    // 并发首次写入：记录已被另一请求创建，重试后走条件更新分支。
                    continue;
                }
            }
            else
            {
                var originalValue = config.Value;
                var now = TimerHelper.UnixTimeSeconds();
                var affected = await _systemConfigRepository.AsUpdateable()
                                                     .SetColumns(u => new SystemConfigEntity
                                                     {
                                                         Name = "平台图形验证码配置",
                                                         Value = value,
                                                         SysFlag = YesNoEnum.Y,
                                                         GroupCode = "Security",
                                                         Remark = "平台私有登录图形验证码配置",
                                                         UpdateTime = now,
                                                     })
                                                     .Where(u => u.Id == config.Id && u.Value == originalValue)
                                                     .ExecuteCommandAsync();

                if (affected == 0)
                {
                    // 并发写入已改变 Value（含版本），重读重算后重试，保证版本单调递增。
                    if (attempt < maxAttempts - 1)
                    {
                        continue;
                    }

                    throw BusinessError.Oh("平台图形验证码配置保存冲突，请重试");
                }
            }

            await _systemConfigService.SetConfigValueAsync(CommonConst.SysCaptcha, next.IsEnabled ? "True" : "False");
            _configProvider.Invalidate();
            return;
        }

        throw BusinessError.Oh("平台图形验证码配置保存冲突，请重试");
    }

    internal static PlatformCaptchaSettings CreateDefaultSettings()
    {
        return new PlatformCaptchaSettings();
    }

    internal static PlatformCaptchaConfigResponse ToOutput(PlatformCaptchaSettings settings, long? updateTime)
    {
        return new PlatformCaptchaConfigResponse
        {
            IsEnabled = settings.IsEnabled,
            Version = settings.Version,
            CaptchaType = settings.CaptchaType,
            CodeLength = settings.CodeLength,
            ExpirySeconds = settings.ExpirySeconds,
            IgnoreCase = settings.IgnoreCase,
            ImageOption = ToImageInput(settings.ImageOption),
            UpdateTime = updateTime,
        };
    }

    internal static PlatformCaptchaSettings ToSettings(PlatformCaptchaConfigUpdateRequest input, long version)
    {
        var settings = new PlatformCaptchaSettings
        {
            IsEnabled = input.IsEnabled,
            Version = version,
            CaptchaType = input.CaptchaType,
            CodeLength = input.CodeLength,
            ExpirySeconds = input.ExpirySeconds,
            IgnoreCase = input.IgnoreCase,
            ImageOption = new PlatformCaptchaImageSettings
            {
                Animation = input.ImageOption.Animation,
                FontSize = input.ImageOption.FontSize,
                Width = input.ImageOption.Width,
                Height = input.ImageOption.Height,
                BubbleMinRadius = input.ImageOption.BubbleMinRadius,
                BubbleMaxRadius = input.ImageOption.BubbleMaxRadius,
                BubbleCount = input.ImageOption.BubbleCount,
                BubbleThickness = input.ImageOption.BubbleThickness,
                InterferenceLineCount = input.ImageOption.InterferenceLineCount,
                FontFamily = input.ImageOption.FontFamily?.Trim() ?? string.Empty,
                FrameDelay = input.ImageOption.FrameDelay,
                BackgroundColor = input.ImageOption.BackgroundColor?.Trim() ?? string.Empty,
                ForegroundColors = input.ImageOption.ForegroundColors?.Trim() ?? string.Empty,
                Quality = input.ImageOption.Quality,
                TextBold = input.ImageOption.TextBold,
            },
        };
        PlatformCaptchaConfigProvider.Validate(settings);
        return settings;
    }

    private static PlatformCaptchaImageRequest ToImageInput(PlatformCaptchaImageSettings image)
    {
        return new PlatformCaptchaImageRequest
        {
            Animation = image.Animation,
            FontSize = image.FontSize,
            Width = image.Width,
            Height = image.Height,
            BubbleMinRadius = image.BubbleMinRadius,
            BubbleMaxRadius = image.BubbleMaxRadius,
            BubbleCount = image.BubbleCount,
            BubbleThickness = image.BubbleThickness,
            InterferenceLineCount = image.InterferenceLineCount,
            FontFamily = image.FontFamily,
            FrameDelay = image.FrameDelay,
            BackgroundColor = image.BackgroundColor,
            ForegroundColors = image.ForegroundColors,
            Quality = image.Quality,
            TextBold = image.TextBold,
        };
    }
}