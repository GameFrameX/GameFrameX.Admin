using GameFrameX.Core.Enum;
using GameFrameX.Core.Service.PlatformPasswordPolicy;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;

namespace GameFrameX.Application.SeedData.System;

/// <summary>
/// 系统配置表种子数据
/// </summary>
public class SystemConfigSeedData : ISqlSugarEntitySeedData<SystemConfigEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<SystemConfigEntity> HasData()
    {
        return new[]
        {
            new SystemConfigEntity { Id = 1300000000101, Name = "演示环境", Code = "sys_demo", Value = "False", SysFlag = YesNoEnum.Y, Remark = "演示环境", OrderNo = 1, GroupCode = "Default", CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new SystemConfigEntity { Id = 1300000000102, Name = "默认密码", Code = "sys_password", Value = "123456", SysFlag = YesNoEnum.Y, Remark = "默认密码", OrderNo = 2, GroupCode = "Default", CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new SystemConfigEntity { Id = 1300000000103, Name = "Token过期时间", Code = "sys_token_expire", Value = "10080", SysFlag = YesNoEnum.Y, Remark = "Token过期时间", OrderNo = 3, GroupCode = "Default", CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new SystemConfigEntity { Id = 1300000000104, Name = "操作日志", Code = "sys_oplog", Value = "True", SysFlag = YesNoEnum.Y, Remark = "开启操作日志", OrderNo = 4, GroupCode = "Default", CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new SystemConfigEntity { Id = 1300000000105, Name = "单设备登录", Code = "sys_single_login", Value = "False", SysFlag = YesNoEnum.Y, Remark = "开启单设备登录", OrderNo = 5, GroupCode = "Default", CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new SystemConfigEntity { Id = 1300000000106, Name = "登录二次验证", Code = "sys_second_ver", Value = "False", SysFlag = YesNoEnum.Y, Remark = "登录二次验证", OrderNo = 6, GroupCode = "Default", CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new SystemConfigEntity { Id = 1300000000107, Name = "开启图形验证码", Code = "sys_captcha", Value = "False", SysFlag = YesNoEnum.Y, Remark = "开启图形验证码", OrderNo = 7, GroupCode = "Default", CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new SystemConfigEntity { Id = 1300000000108, Name = "开启水印", Code = "sys_watermark", Value = "False", SysFlag = YesNoEnum.Y, Remark = "开启水印", OrderNo = 8, GroupCode = "Default", CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new SystemConfigEntity { Id = 1300000000109, Name = "RefreshToken过期时间", Code = "sys_refresh_token_expire", Value = "20160", SysFlag = YesNoEnum.Y, Remark = "RefreshToken过期时间，单位分钟（一般 refresh_token 的有效时间 > 2 * access_token 的有效时间）", OrderNo = 9, GroupCode = "Default", CreatedTime = DateTimeOffset.Parse("2022-02-10 00:00:00").ToUnixTimeSeconds(), },
            new SystemConfigEntity { Id = 1300000000110, Name = "平台图形验证码配置", Code = "platform_captcha_config", Value = "{\"isEnabled\":false,\"version\":1,\"captchaType\":10,\"codeLength\":1,\"expirySeconds\":60,\"ignoreCase\":true,\"imageOption\":{\"animation\":true,\"fontSize\":36,\"width\":150,\"height\":50,\"bubbleMinRadius\":5,\"bubbleMaxRadius\":10,\"bubbleCount\":3,\"bubbleThickness\":1,\"interferenceLineCount\":3,\"fontFamily\":\"kaiti\",\"frameDelay\":300,\"backgroundColor\":\"#ffffff\",\"foregroundColors\":\"\",\"quality\":100,\"textBold\":true}}", SysFlag = YesNoEnum.Y, Remark = "平台私有登录图形验证码配置", OrderNo = 20, GroupCode = "Security", CreatedTime = DateTimeOffset.Parse("2026-07-11 00:00:00").ToUnixTimeSeconds(), },
            new SystemConfigEntity { Id = 1300000000111, Name = "平台限流配置", Code = "platform_rate_limit_config", Value = "{\"isEnabled\":true,\"version\":1,\"permitLimit\":20,\"windowSeconds\":1,\"queueLimit\":0}", SysFlag = YesNoEnum.Y, Remark = "平台私有请求限流配置", OrderNo = 30, GroupCode = "Security", CreatedTime = DateTimeOffset.Parse("2026-07-27 00:00:00").ToUnixTimeSeconds(), },
            new SystemConfigEntity { Id = 1300000000112, Name = "平台日志配置", Code = "platform_logging_config", Value = "{\"file\":{\"isEnabled\":true,\"fileName\":\"data/logs/{0:yyyyMMdd}_{1}.log\",\"append\":true,\"fileSizeLimitBytes\":10485760,\"maxRollingFiles\":30},\"database\":{\"isEnabled\":true,\"minimumLevel\":\"Information\"},\"elasticSearch\":{\"isEnabled\":false,\"serverUris\":[],\"defaultIndex\":\"adminnet\"},\"monitor\":{\"globalEnabled\":true,\"bahLogLevel\":\"Information\",\"withReturnValue\":true,\"returnValueThreshold\":500,\"jsonBehavior\":\"OnlyJson\",\"jsonIndented\":false,\"useUtcTimestamp\":false}}", SysFlag = YesNoEnum.Y, Remark = "平台私有日志运行时配置", OrderNo = 40, GroupCode = "Security", CreatedTime = DateTimeOffset.Parse("2026-07-27 00:00:00").ToUnixTimeSeconds(), },
            new SystemConfigEntity { Id = 1300000000113, Name = PlatformPasswordPolicyConfigConstants.ConfigName, Code = PlatformPasswordPolicyConfigConstants.ConfigCode, Value = PlatformPasswordPolicyConfigProvider.Serialize(PlatformPasswordPolicyConfigProvider.CreateDefaultSettings()), SysFlag = YesNoEnum.Y, Remark = PlatformPasswordPolicyConfigConstants.ConfigRemark, OrderNo = PlatformPasswordPolicyConfigConstants.OrderNo, GroupCode = PlatformPasswordPolicyConfigConstants.GroupCode, CreatedTime = DateTimeOffset.Parse("2026-07-28 00:00:00").ToUnixTimeSeconds(), },
            new SystemConfigEntity { Id = 1300000000114, Name = "邮箱自助注册", Code = "sys_email_register", Value = "False", SysFlag = YesNoEnum.Y, Remark = "开启邮箱自助注册", OrderNo = 10, GroupCode = "Default", CreatedTime = DateTimeOffset.Parse("2026-08-30 00:00:00").ToUnixTimeSeconds(), },
        };
    }
}