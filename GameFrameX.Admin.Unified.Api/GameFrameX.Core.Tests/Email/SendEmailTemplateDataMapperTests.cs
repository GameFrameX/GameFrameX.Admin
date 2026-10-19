using GameFrameX.Email.TencentSes;

namespace GameFrameX.Core.Tests.Email;

public class SendEmailTemplateDataMapperTests
{
    [Fact]
    public void FromDictionary_MapsSupportedTenantEmailTemplateKeys()
    {
        var data = SendEmailTemplateDataMapper.FromDictionary(new Dictionary<string, string>
        {
            ["username"] = "player@example.test",
            ["verification_code"] = "123456",
            ["expiry_minutes"] = "5",
            ["content"] = "ignored until template contract supports it",
        });

        Assert.Equal("player@example.test", data.Username);
        Assert.Equal("123456", data.VerificationCode);
        Assert.Equal("5", data.ExpiryMinutes);
    }

    [Fact]
    public void FromDictionary_ReturnsEmptyDataWhenInputIsNull()
    {
#pragma warning disable CS8625 // 测试意图：传入 null 模板数据验证空对象降级
        var data = SendEmailTemplateDataMapper.FromDictionary(null);
#pragma warning restore CS8625

        Assert.Null(data.Username);
        Assert.Null(data.VerificationCode);
        Assert.Equal("5", data.ExpiryMinutes);
    }
}