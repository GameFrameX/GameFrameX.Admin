using System.ComponentModel.DataAnnotations;
using GameFrameX.Application.Game.Tenant.Integration.TenantWechatConfig.Dto;

namespace GameFrameX.Core.Tests.System;

public sealed class TenantWechatConfigInputContractTests
{
    [Theory]
    [InlineData(typeof(AddTenantWechatConfigRequest), nameof(AddTenantWechatConfigRequest.WechatAppId))]
    [InlineData(typeof(AddTenantWechatConfigRequest), nameof(AddTenantWechatConfigRequest.WechatAppSecret))]
    [InlineData(typeof(AddTenantWechatConfigRequest), nameof(AddTenantWechatConfigRequest.WxOpenAppId))]
    [InlineData(typeof(AddTenantWechatConfigRequest), nameof(AddTenantWechatConfigRequest.WxOpenSecret))]
    [InlineData(typeof(UpdateTenantWechatConfigRequest), nameof(UpdateTenantWechatConfigRequest.WechatAppId))]
    [InlineData(typeof(UpdateTenantWechatConfigRequest), nameof(UpdateTenantWechatConfigRequest.WechatAppSecret))]
    [InlineData(typeof(UpdateTenantWechatConfigRequest), nameof(UpdateTenantWechatConfigRequest.WxOpenAppId))]
    [InlineData(typeof(UpdateTenantWechatConfigRequest), nameof(UpdateTenantWechatConfigRequest.WxOpenSecret))]
    public void CredentialFields_DoNotUseSingleFieldRequiredAttribute(Type inputType, string propertyName)
    {
        var property = inputType.GetProperty(propertyName);

        Assert.NotNull(property);
        Assert.DoesNotContain(property!.GetCustomAttributes(false), attr => attr is RequiredAttribute);
    }
}