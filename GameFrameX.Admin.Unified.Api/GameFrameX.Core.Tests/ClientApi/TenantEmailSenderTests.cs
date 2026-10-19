using GameFrameX.Client.Api.Services.Email;
using Microsoft.AspNetCore.Http;

namespace GameFrameX.Core.Tests.ClientApi;

public class TenantEmailSenderTests
{
    [Fact]
    public async Task SendTemplate_ReturnsFalseWhenTenantEmailUnconfigured_NoSystemFallback()
    {
        // 租户业务邮件未配置时必须返回 false，禁止 fallback 到系统邮件
        var provider = FakeOptionsProvider.WithoutEmailOptions();
        // 缺 HttpContext 时 GetSnapshotAsync 提前返回 null，不需要真实 tenantDbProvider
        var sender = new TenantEmailSender(provider, null!, new HttpContextAccessor());

        var data = new Dictionary<string, string>
        {
            { "username", "player@example.test" },
            { "verification_code", "123456" },
        };

        var ok = await sender.SendTemplateAsync(TenantEmailPurpose.PlayerRegisterCode, "player@example.test", 145225, "Subject", data);

        Assert.False(ok);
    }

    [Fact]
    public async Task GetFromEmailAddress_ReturnsNullWhenUnconfigured()
    {
        var provider = FakeOptionsProvider.WithoutEmailOptions();
        var sender = new TenantEmailSender(provider, null!, new HttpContextAccessor());

        Assert.Null(await sender.GetFromEmailAddressAsync());
    }
}