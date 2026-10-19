using GameFrameX.Core.Service.PlatformPasswordPolicy.Dto;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Service.PlatformPasswordPolicy;

/// <summary>
/// 平台密码策略配置服务（C173，SuperAdmin-only）。DetailAsync/UpdateAsync 仅暴露密码强度策略字段，
/// 绝不回显 Cryptogram 密钥；UpdateAsync 用 CAS 乐观锁写回并刷新同 Host 快照与 CryptogramUtility 镜像。
/// <para>宿主表说明（GFX-706）：本服务不持有独立宿主表，共享主库 SystemConfigEntity（system_config 表），以 Code 分区行 <c>platform_password_policy_config</c>（PlatformPasswordPolicyConfigConstants.ConfigCode）存储本域配置 JSON。Platform* 前缀指平台级配置域，名实相符保留类名。</para>
/// </summary>
[ApiDescriptionSettings(Order = 445)]
public sealed class PlatformPasswordPolicyConfigService : IAdminApiController
{
    private readonly SqlSugarRepository<SystemConfigEntity> _systemConfigRepository;
    private readonly PlatformPasswordPolicyConfigProvider _configProvider;

    public PlatformPasswordPolicyConfigService(SqlSugarRepository<SystemConfigEntity> systemConfigRepository, PlatformPasswordPolicyConfigProvider configProvider)
    {
        _systemConfigRepository = systemConfigRepository;
        _configProvider = configProvider;
    }

    /// <summary>
    /// 获取平台密码策略配置详情。仅返回 strongPassword/passwordStrengthValidation/passwordStrengthValidationMsg，
    /// 不包含任何 Cryptogram 密钥。
    /// </summary>
    [ApiDescriptionSettings(Name = "Detail")]
    [DisplayName("获取平台密码策略配置详情")]
    public async Task<PlatformPasswordPolicyConfigResponse> DetailAsync()
    {
        var config = await _systemConfigRepository.GetFirstAsync(u => u.Code == PlatformPasswordPolicyConfigConstants.ConfigCode);
        var settings = config == null || string.IsNullOrWhiteSpace(config.Value)
                           ? PlatformPasswordPolicyConfigProvider.CreateDefaultSettings()
                           : PlatformPasswordPolicyConfigProvider.Deserialize(config.Value);

        PlatformPasswordPolicyConfigProvider.Validate(settings);
        return ToOutput(settings, config?.UpdateTime);
    }

    /// <summary>
    /// 保存平台密码策略配置。CAS 乐观锁冲突重试；成功后刷新同 Host 快照并镜像同步 CryptogramUtility 密码策略静态属性，
    /// 后续 TenantUserService.ChangePwdAsync 立即按新规则校验。
    /// </summary>
    [ApiDescriptionSettings(Name = "Update")]
    [HttpPost]
    [DisplayName("更新平台密码策略配置")]
    [SqlSugarTransaction]
    public async Task UpdateAsync(PlatformPasswordPolicyConfigUpdateRequest input)
    {
        const int maxAttempts = 8;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var config = await _systemConfigRepository.GetFirstAsync(u => u.Code == PlatformPasswordPolicyConfigConstants.ConfigCode);
            var next = ToSettings(input);
            var value = PlatformPasswordPolicyConfigProvider.Serialize(next);

            if (config == null)
            {
                try
                {
                    await _systemConfigRepository.InsertAsync(new SystemConfigEntity
                    {
                        Name = PlatformPasswordPolicyConfigConstants.ConfigName,
                        Code = PlatformPasswordPolicyConfigConstants.ConfigCode,
                        Value = value,
                        SysFlag = YesNoEnum.Y,
                        GroupCode = PlatformPasswordPolicyConfigConstants.GroupCode,
                        OrderNo = PlatformPasswordPolicyConfigConstants.OrderNo,
                        Remark = PlatformPasswordPolicyConfigConstants.ConfigRemark,
                    });
                }
                catch (Exception) when (attempt < maxAttempts - 1)
                {
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
                                                         Name = PlatformPasswordPolicyConfigConstants.ConfigName,
                                                         Value = value,
                                                         SysFlag = YesNoEnum.Y,
                                                         GroupCode = PlatformPasswordPolicyConfigConstants.GroupCode,
                                                         Remark = PlatformPasswordPolicyConfigConstants.ConfigRemark,
                                                         UpdateTime = now,
                                                     })
                                                     .Where(u => u.Id == config.Id && u.Value == originalValue)
                                                     .ExecuteCommandAsync();

                if (affected == 0)
                {
                    if (attempt < maxAttempts - 1)
                    {
                        continue;
                    }

                    throw BusinessError.Oh("平台密码策略配置保存冲突，请重试");
                }
            }

            await _configProvider.RefreshAsync();
            return;
        }

        throw BusinessError.Oh("平台密码策略配置保存冲突，请重试");
    }

    internal static PlatformPasswordPolicyConfigResponse ToOutput(PlatformPasswordPolicySettings settings, long? updateTime)
    {
        return new PlatformPasswordPolicyConfigResponse
        {
            StrongPassword = settings.StrongPassword,
            PasswordStrengthValidation = settings.PasswordStrengthValidation,
            PasswordStrengthValidationMsg = settings.PasswordStrengthValidationMsg,
            UpdateTime = updateTime,
        };
    }

    internal static PlatformPasswordPolicySettings ToSettings(PlatformPasswordPolicyConfigUpdateRequest input)
    {
        var settings = new PlatformPasswordPolicySettings
        {
            StrongPassword = input.StrongPassword,
            PasswordStrengthValidation = input.PasswordStrengthValidation,
            PasswordStrengthValidationMsg = input.PasswordStrengthValidationMsg,
        };
        PlatformPasswordPolicyConfigProvider.Validate(settings);
        return settings;
    }
}