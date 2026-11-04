using System.Text.Json;
using GameFrameX.Core.Logging;

namespace GameFrameX.Core.Tests.Logging;

public sealed class SensitiveLogRedactorTests
{
    [Theory]
    [InlineData("password")]
    [InlineData("PasswordOld")]
    [InlineData("PasswordNew")]
    [InlineData("confirmPassword")]
    [InlineData("initialPassword")]
    [InlineData("ACCESS_TOKEN")]
    [InlineData("refreshToken")]
    [InlineData("token")]
    [InlineData("secret")]
    [InlineData("accessSecret")]
    [InlineData("appSecret")]
    [InlineData("privateKey")]
    [InlineData("signingKey")]
    [InlineData("sessionKey")]
    public void NestedSnapshotsAndDescriptorValues_AreMaskedAndIdempotent(string field)
    {
        const string plain = "credential-value-unique";
        var inner = JsonSerializer.Serialize(new Dictionary<string, string> { [field] = plain });
        var payload = JsonSerializer.Serialize(new { Nested = inner, Columns = new[] { new { ColumnName = field, Value = plain } }, parameters = new[] { new { name = field, value = plain } } });
        var redacted = SensitiveLogRedactor.Redact(payload);
        Assert.DoesNotContain(plain, redacted);
        Assert.Equal(redacted, SensitiveLogRedactor.Redact(redacted));
        Assert.Contains(SensitiveLogRedactor.Mask, redacted);
        Assert.Contains(plain, payload);
    }

    [Theory]
    [InlineData("quote\"and\\slash")]
    [InlineData("short")]
    [InlineData("*")]
    public void SecretsEchoedAcrossSerializedColumns_RemainValidJsonAndAreMasked(string plain)
    {
        var payload = JsonSerializer.Serialize(new { Exception = "failed with " + plain, RequestParam = JsonSerializer.Serialize(new { Password = plain }) });
        var result = SensitiveLogRedactor.Redact(payload);
        using (var document = JsonDocument.Parse(result!))
        {
            Assert.Equal("failed with " + SensitiveLogRedactor.Mask, document.RootElement.GetProperty("Exception").GetString());
            using (var parameters = JsonDocument.Parse(document.RootElement.GetProperty("RequestParam").GetString()!))
            {
                Assert.Equal(SensitiveLogRedactor.Mask, parameters.RootElement.GetProperty("Password").GetString());
            }
        }

        Assert.Equal(result, SensitiveLogRedactor.Redact(result));
    }

    [Theory]
    [InlineData("ColumnName", "OldValue", "NewValue")]
    [InlineData("column_name", "old_value", "new_value")]
    public void HistoricalAuditColumnDescriptor_MasksOldAndNewValues(string descriptor, string before, string after)
    {
        var result = SensitiveLogRedactor.Redact(JsonSerializer.Serialize(new Dictionary<string, string> { [descriptor] = "Password", [before] = "old-credential", [after] = "new-credential" }));
        using (var document = JsonDocument.Parse(result!))
        {
            Assert.Equal(SensitiveLogRedactor.Mask, document.RootElement.GetProperty(before).GetString());
            Assert.Equal(SensitiveLogRedactor.Mask, document.RootElement.GetProperty(after).GetString());
        }

        Assert.Equal(result, SensitiveLogRedactor.Redact(result));
    }

    [Fact]
    public void EndpointMetadataAndOrdinaryParameters_RemainReadable()
    {
        const string value = "{\"ControllerName\":\"TenantSessionKeyEntity\",\"ActionName\":\"SetupSuperAdminPassword\",\"Keyword\":\"ordinary\"}";
        Assert.Equal(value, SensitiveLogRedactor.Redact(value));
    }

    [Fact]
    public void RepeatedJsonStringEncoding_CannotHideCredentials()
    {
        var value = "{\"Password\":\"multilayer-credential\"}";
        for (var index = 0; index < 4; index++)
        {
            value = JsonSerializer.Serialize(value);
        }

        var result = SensitiveLogRedactor.Redact(value);
        Assert.DoesNotContain("multilayer-credential", result);
        Assert.Equal(result, SensitiveLogRedactor.Redact(result));
    }

    [Theory]
    [InlineData("{\"Password\":\"truncated-credential")]
    [InlineData("https://localhost/api/auth?accessToken=truncated-credential")]
    public void UnstructuredOrTruncatedCredentials_FailClosed(string value)
    {
        Assert.Equal(SensitiveLogRedactor.Mask, SensitiveLogRedactor.Redact(value));
    }
}
