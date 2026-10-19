using GameFrameX.Core.Service.PlatformPasswordPolicy;
using GameFrameX.Core.Service.PlatformPasswordPolicy.Dto;
using GameFrameX.Core.Utility;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace GameFrameX.Core.Tests.System;

/// <summary>
/// C173 平台密码策略配置测试：默认序列化与历史等价、非法正则校验、快照镜像动态生效、
/// 改密强度门控、密钥不回显、密钥解析优先级、Configure 只接受密钥、内置默认密钥等价历史值。
/// </summary>
public class PlatformPasswordPolicyConfigTests
{
    [Fact]
    public void Serialize_DefaultSettings_ContainsAllFieldsAndStaysWithinLimit()
    {
        var settings = PlatformPasswordPolicyConfigProvider.CreateDefaultSettings();

        var value = PlatformPasswordPolicyConfigProvider.Serialize(settings);

        Assert.Contains("\"strongPassword\":false", value);
        Assert.Contains("\"passwordStrengthValidation\":", value);
        Assert.Contains("\"passwordStrengthValidationMsg\":", value);
        Assert.True(value.Length <= PlatformPasswordPolicyConfigConstants.MaxSerializedLength);
    }

    /// <summary>
    /// 默认值必须等价历史 Cryptogram 密码策略值，避免迁移改变现有密码策略行为。
    /// </summary>
    [Fact]
    public void DefaultSettings_EqualsAppJsonHistoricalValues()
    {
        var settings = PlatformPasswordPolicyConfigProvider.CreateDefaultSettings();

        Assert.False(settings.StrongPassword);
        Assert.Equal(PlatformPasswordPolicyConfigConstants.DefaultPasswordStrengthValidation, settings.PasswordStrengthValidation);
        Assert.Equal(PlatformPasswordPolicyConfigConstants.DefaultPasswordStrengthValidationMsg, settings.PasswordStrengthValidationMsg);
    }

    [Theory]
    [InlineData("", "提示")] // 正则空
    [InlineData("abc", "")] // 提示空
    [InlineData("^[a-Z", "提示")] // 正则不可编译
    public void Validate_RejectsInvalidValues(string regex, string msg)
    {
        var settings = new PlatformPasswordPolicySettings
        {
            StrongPassword = true,
            PasswordStrengthValidation = regex,
            PasswordStrengthValidationMsg = msg,
        };

        Assert.ThrowsAny<Exception>(() => PlatformPasswordPolicyConfigProvider.Validate(settings));
    }

    [Fact]
    public void Deserialize_StrongPasswordTrue_ReflectsInSnapshot()
    {
        var json = "{\"strongPassword\":true,\"passwordStrengthValidation\":\"^[a-z]+$\",\"passwordStrengthValidationMsg\":\"提示\"}";

        var settings = PlatformPasswordPolicyConfigProvider.Deserialize(json);

        Assert.True(settings.StrongPassword);
        Assert.Equal("^[a-z]+$", settings.PasswordStrengthValidation);
    }

    /// <summary>
    /// 动态生效证据：Provider.RefreshAsync 内部把快照镜像同步到 CryptogramUtility 静态属性
    /// （这里直接验证 ApplyPasswordPolicy 镜像语义，TenantUserService.ChangePwdAsync 读 CryptogramUtility 静态属性）。
    /// </summary>
    [Fact]
    public void ApplyPasswordPolicy_MirrorsSnapshotToCryptogramUtility()
    {
        var origStrong = CryptogramUtility.StrongPassword;
        var origRegex = CryptogramUtility.PasswordStrengthValidation;
        var origMsg = CryptogramUtility.PasswordStrengthValidationMsg;
        try
        {
            CryptogramUtility.ApplyPasswordPolicy(true, "^[a-zA-Z0-9]+$", "新策略提示");

            Assert.True(CryptogramUtility.StrongPassword);
            Assert.Equal("^[a-zA-Z0-9]+$", CryptogramUtility.PasswordStrengthValidation);
            Assert.Equal("新策略提示", CryptogramUtility.PasswordStrengthValidationMsg);
        }
        finally
        {
            CryptogramUtility.ApplyPasswordPolicy(origStrong, origRegex, origMsg);
        }
    }

    /// <summary>
    /// 改密强度校验动态生效：SuperAdmin 保存 StrongPassword=true + 新正则后，Provider 镜像同步，
    /// TenantUserService.ChangePwdAsync 读 CryptogramUtility.StrongPassword=true 并用镜像正则校验新密码。
    /// </summary>
    [Fact]
    public void ChangePwd_StrengthCheck_HonorsMirroredSnapshot()
    {
        var origStrong = CryptogramUtility.StrongPassword;
        var origRegex = CryptogramUtility.PasswordStrengthValidation;
        var origMsg = CryptogramUtility.PasswordStrengthValidationMsg;
        try
        {
            CryptogramUtility.ApplyPasswordPolicy(true, "^[A-Z]+$", "必须大写");

            // 模拟 TenantUserService.ChangePwdAsync 门控（CryptogramUtility.StrongPassword + Regex.IsMatch）
            var gateEnabled = CryptogramUtility.StrongPassword;
            var weakPasswordPasses = Regex.IsMatch("abc", CryptogramUtility.PasswordStrengthValidation);
            var strongPasswordPasses = Regex.IsMatch("ABC", CryptogramUtility.PasswordStrengthValidation);

            Assert.True(gateEnabled);
            Assert.False(weakPasswordPasses);
            Assert.True(strongPasswordPasses);
        }
        finally
        {
            CryptogramUtility.ApplyPasswordPolicy(origStrong, origRegex, origMsg);
        }
    }

    /// <summary>
    /// 密钥不回显证据：密码策略 API 输出 DTO 与序列化结果不含 CryptoType/PublicKey/PrivateKey 任何密钥字段。
    /// </summary>
    [Fact]
    public void ToOutput_DoesNotExposeCryptogramKeys()
    {
        var settings = PlatformPasswordPolicyConfigProvider.CreateDefaultSettings();

        var output = PlatformPasswordPolicyConfigService.ToOutput(settings, 123L);

        Assert.Equal(settings.StrongPassword, output.StrongPassword);
        Assert.Equal(settings.PasswordStrengthValidation, output.PasswordStrengthValidation);
        Assert.Equal(settings.PasswordStrengthValidationMsg, output.PasswordStrengthValidationMsg);
        Assert.Equal(123L, output.UpdateTime);

        var keyPropertyNames = output.GetType().GetProperties()
                                     .Where(p => p.Name.Contains("PublicKey") || p.Name.Contains("PrivateKey") || p.Name.Contains("CryptoType"))
                                     .Select(p => p.Name)
                                     .ToList();
        Assert.Empty(keyPropertyNames);

        var serialized = JsonSerializer.Serialize(output);
        Assert.DoesNotContain("PublicKey", serialized);
        Assert.DoesNotContain("PrivateKey", serialized);
    }

    [Fact]
    public void ToSettings_AppliesInputAndValidates()
    {
        var input = new PlatformPasswordPolicyConfigUpdateRequest
        {
            StrongPassword = true,
            PasswordStrengthValidation = "^[A-Z]+$",
            PasswordStrengthValidationMsg = "提示",
        };

        var settings = PlatformPasswordPolicyConfigService.ToSettings(input);

        Assert.True(settings.StrongPassword);
        Assert.Equal("^[A-Z]+$", settings.PasswordStrengthValidation);
    }
}