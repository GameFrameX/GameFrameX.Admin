using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using GameFrameX.Application.Game.Tenant.OpenAccess.TenantGameAppOpenAccess;
using GameFrameX.Application.Game.Tenant.OpenAccess.TenantGameAppOpenAccess.Dto;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game.App;

namespace GameFrameX.Core.Tests.AppManagement;

/// <summary>
/// 应用开放访问凭证（TenantGameAppOpenAccessEntity）纯单元测试。
/// <para>覆盖实体默认值、DTO 校验、Service 静态方法（GenerateAccessKey/Secret、EnsureValidScopes、NormalizeScopesJson）、
/// Output 脱敏与 SecretResult 契约。DB 逻辑（PageAsync/AddAsync/UpdateAsync 查库校验）由 .menu-test 浏览器验收覆盖。</para>
/// </summary>
public class TenantGameAppOpenAccessEntityTests
{
    [Fact]
    public void TenantGameAppOpenAccessEntity_Default_IsEnabledTrue()
    {
        var entity = new TenantGameAppOpenAccessEntity();
        Assert.True(entity.IsEnabled);
    }

    [Fact]
    public void TenantGameAppOpenAccessEntity_DerivesPhysicalTableNameFromClassName()
    {
        var physical = SqlSugarEntityBoundary.GetPhysicalTableName(typeof(TenantGameAppOpenAccessEntity), nameof(TenantGameAppOpenAccessEntity));

        Assert.Equal("tenant_game_app_open_access_entity", physical);
    }

    [Fact]
    public void AddTenantGameAppOpenAccessInput_Requires_AllMandatoryFields()
    {
        var input = new AddTenantGameAppOpenAccessRequest();
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(input, new ValidationContext(input), results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(AddTenantGameAppOpenAccessRequest.AppId)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(AddTenantGameAppOpenAccessRequest.BindUserId)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(AddTenantGameAppOpenAccessRequest.Scenario)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(AddTenantGameAppOpenAccessRequest.Purpose)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(AddTenantGameAppOpenAccessRequest.SubjectName)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(AddTenantGameAppOpenAccessRequest.ScopesJson)));
    }

    [Fact]
    public void AddTenantGameAppOpenAccessInput_AcceptsValidInput()
    {
        var input = new AddTenantGameAppOpenAccessRequest
        {
            AppId = 100,
            BindUserId = 200,
            Scenario = "ci",
            Purpose = "build pipeline",
            SubjectName = "builder-1",
            ScopesJson = "[\"package-report\"]",
        };
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(input, new ValidationContext(input), results, true);

        Assert.True(isValid);
    }

    [Fact]
    public void AddTenantGameAppOpenAccessInput_RejectsAppIdZero()
    {
        // AppId=0 必须被拒绝（强制 AppId > 0，不允许租户默认凭证）。
        var input = new AddTenantGameAppOpenAccessRequest
        {
            AppId = 0,
            BindUserId = 1,
            Scenario = "ci",
            Purpose = "x",
            SubjectName = "x",
            ScopesJson = "[\"package-report\"]",
        };
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(input, new ValidationContext(input), results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(AddTenantGameAppOpenAccessRequest.AppId)));
    }

    [Fact]
    public void UpdateTenantGameAppOpenAccessInput_RequiresAppId_ForOwnershipCheck()
    {
        var input = new UpdateTenantGameAppOpenAccessRequest();
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(input, new ValidationContext(input), results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateTenantGameAppOpenAccessRequest.AppId)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateTenantGameAppOpenAccessRequest.BindUserId)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateTenantGameAppOpenAccessRequest.ScopesJson)));
    }

    [Fact]
    public void TenantGameAppOpenAccessActionInput_RequiresAppId()
    {
        var input = new TenantGameAppOpenAccessActionRequest();
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(input, new ValidationContext(input), results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(TenantGameAppOpenAccessActionRequest.AppId)));
    }

    [Fact]
    public void GenerateAccessKey_ReturnsNonEmptyAndUnique()
    {
        var k1 = TenantGameAppOpenAccessService.GenerateAccessKey();
        var k2 = TenantGameAppOpenAccessService.GenerateAccessKey();

        Assert.False(string.IsNullOrEmpty(k1));
        Assert.NotEqual(k1, k2);
    }

    [Fact]
    public void GenerateAccessSecret_ReturnsNonEmptyAndUnique()
    {
        var s1 = TenantGameAppOpenAccessService.GenerateAccessSecret();
        var s2 = TenantGameAppOpenAccessService.GenerateAccessSecret();

        Assert.False(string.IsNullOrEmpty(s1));
        Assert.NotEqual(s1, s2);
        // 32 字节 Base64 至少 43 字符
        Assert.True(s1.Length >= 43);
    }

    [Fact]
    public void EnsureValidScopes_AcceptsValidArray()
    {
        var scopes = TenantGameAppOpenAccessService.EnsureValidScopes("[\"package-report\",\"ci-trigger\"]");

        Assert.Equal(2, scopes.Count);
        Assert.Contains("package-report", scopes);
        Assert.Contains("ci-trigger", scopes);
    }

    [Fact]
    public void EnsureValidScopes_RejectsNullOrWhitespace()
    {
        Assert.ThrowsAny<Exception>(() => TenantGameAppOpenAccessService.EnsureValidScopes(null!));
        Assert.ThrowsAny<Exception>(() => TenantGameAppOpenAccessService.EnsureValidScopes(string.Empty));
        Assert.ThrowsAny<Exception>(() => TenantGameAppOpenAccessService.EnsureValidScopes("   "));
    }

    [Fact]
    public void EnsureValidScopes_RejectsInvalidJson()
    {
        Assert.ThrowsAny<Exception>(() => TenantGameAppOpenAccessService.EnsureValidScopes("not-json"));
    }

    [Fact]
    public void EnsureValidScopes_RejectsEmptyArrayOrBlankItem()
    {
        Assert.ThrowsAny<Exception>(() => TenantGameAppOpenAccessService.EnsureValidScopes("[]"));
        Assert.ThrowsAny<Exception>(() => TenantGameAppOpenAccessService.EnsureValidScopes("[\"\",\"x\"]"));
    }

    [Fact]
    public void NormalizeScopesJson_RoundTripsThroughEnsureValidScopes()
    {
        var scopes = new List<string> { "package-report", "ci-trigger", };
        var json = TenantGameAppOpenAccessService.NormalizeScopesJson(scopes);
        var parsed = TenantGameAppOpenAccessService.EnsureValidScopes(json);

        Assert.Equal(scopes.Count, parsed.Count);
        Assert.Equal("package-report", parsed[0]);
        Assert.Equal("ci-trigger", parsed[1]);
    }

    [Fact]
    public void TenantGameAppOpenAccessSecretResult_SerializesSecretForOneTimeDisplay()
    {
        // SecretResult 仅签发/轮换响应返回，前端一次性展示；序列化含明文 AccessSecret。
        var result = new TenantGameAppOpenAccessSecretResponse
        {
            Id = 1,
            AccessKey = "ak_test",
            AccessSecret = "sk_test",
            ScopesJson = "[\"package-report\"]",
        };
        var json = JsonSerializer.Serialize(result);

        Assert.Contains("\"AccessSecret\":\"sk_test\"", json);
        Assert.Contains("\"AccessKey\":\"ak_test\"", json);
    }
}