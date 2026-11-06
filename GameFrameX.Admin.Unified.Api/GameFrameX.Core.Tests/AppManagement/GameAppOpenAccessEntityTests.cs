using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using GameFrameX.Application.Platform.OpenAccess.GameAppOpenAccess;
using GameFrameX.Application.Platform.OpenAccess.GameAppOpenAccess.Dto;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game.App;

namespace GameFrameX.Core.Tests.AppManagement;

/// <summary>
/// 应用开放访问凭证（GameAppOpenAccessEntity）纯单元测试。
/// <para>覆盖实体默认值、DTO 校验、Service 静态方法（GenerateAccessKey/Secret、EnsureValidScopes、NormalizeScopesJson）、
/// Output 脱敏与 SecretResult 契约。DB 逻辑（PageAsync/AddAsync/UpdateAsync 查库校验）由 .menu-test 浏览器验收覆盖。</para>
/// </summary>
public class GameAppOpenAccessEntityTests
{
    [Fact]
    public void GameAppOpenAccessEntity_Default_IsEnabledTrue()
    {
        var entity = new GameAppOpenAccessEntity();
        Assert.True(entity.IsEnabled);
    }

    [Fact]
    public void GameAppOpenAccessEntity_DerivesPhysicalTableNameFromClassName()
    {
        var physical = SqlSugarEntityBoundary.GetPhysicalTableName(typeof(GameAppOpenAccessEntity), nameof(GameAppOpenAccessEntity));

        Assert.Equal("game_app_open_access_entity", physical);
    }

    [Fact]
    public void AddGameAppOpenAccessInput_Requires_AllMandatoryFields()
    {
        var input = new AddGameAppOpenAccessRequest();
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(input, new ValidationContext(input), results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(AddGameAppOpenAccessRequest.BindUserId)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(AddGameAppOpenAccessRequest.Scenario)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(AddGameAppOpenAccessRequest.Purpose)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(AddGameAppOpenAccessRequest.SubjectName)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(AddGameAppOpenAccessRequest.ScopesJson)));
    }

    [Fact]
    public void AddGameAppOpenAccessInput_AcceptsValidInput()
    {
        var input = new AddGameAppOpenAccessRequest
        {
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
    public void AddGameAppOpenAccessInput_AcceptsWithoutApplication()
    {
        var input = new AddGameAppOpenAccessRequest
        {
            BindUserId = 1,
            Scenario = "ci",
            Purpose = "x",
            SubjectName = "x",
            ScopesJson = "[\"package-report\"]",
        };
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(input, new ValidationContext(input), results, true);

        Assert.True(isValid);
    }

    [Fact]
    public void UpdateGameAppOpenAccessInput_RequiresUserAndScopes()
    {
        var input = new UpdateGameAppOpenAccessRequest();
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(input, new ValidationContext(input), results, true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateGameAppOpenAccessRequest.BindUserId)));
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateGameAppOpenAccessRequest.ScopesJson)));
    }

    [Fact]
    public void GameAppOpenAccessActionInput_HasNoApplicationDimension()
    {
        var input = new GameAppOpenAccessActionRequest();
        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(input, new ValidationContext(input), results, true);

        Assert.False(isValid);
        Assert.Null(typeof(GameAppOpenAccessActionRequest).GetProperty("AppId")); // c406-keep（负向断言：旧应用作用域字段不得回归）
    }

    [Fact]
    public void GenerateAccessKey_ReturnsNonEmptyAndUnique()
    {
        var k1 = GameAppOpenAccessService.GenerateAccessKey();
        var k2 = GameAppOpenAccessService.GenerateAccessKey();

        Assert.False(string.IsNullOrEmpty(k1));
        Assert.NotEqual(k1, k2);
    }

    [Fact]
    public void GenerateAccessSecret_ReturnsNonEmptyAndUnique()
    {
        var s1 = GameAppOpenAccessService.GenerateAccessSecret();
        var s2 = GameAppOpenAccessService.GenerateAccessSecret();

        Assert.False(string.IsNullOrEmpty(s1));
        Assert.NotEqual(s1, s2);
        // 32 字节 Base64 至少 43 字符
        Assert.True(s1.Length >= 43);
    }

    [Fact]
    public void EnsureValidScopes_AcceptsValidArray()
    {
        var scopes = GameAppOpenAccessService.EnsureValidScopes("[\"package-report\",\"ci-trigger\"]");

        Assert.Equal(2, scopes.Count);
        Assert.Contains("package-report", scopes);
        Assert.Contains("ci-trigger", scopes);
    }

    [Fact]
    public void EnsureValidScopes_RejectsNullOrWhitespace()
    {
        Assert.ThrowsAny<Exception>(() => GameAppOpenAccessService.EnsureValidScopes(null!));
        Assert.ThrowsAny<Exception>(() => GameAppOpenAccessService.EnsureValidScopes(string.Empty));
        Assert.ThrowsAny<Exception>(() => GameAppOpenAccessService.EnsureValidScopes("   "));
    }

    [Fact]
    public void EnsureValidScopes_RejectsInvalidJson()
    {
        Assert.ThrowsAny<Exception>(() => GameAppOpenAccessService.EnsureValidScopes("not-json"));
    }

    [Fact]
    public void EnsureValidScopes_RejectsEmptyArrayOrBlankItem()
    {
        Assert.ThrowsAny<Exception>(() => GameAppOpenAccessService.EnsureValidScopes("[]"));
        Assert.ThrowsAny<Exception>(() => GameAppOpenAccessService.EnsureValidScopes("[\"\",\"x\"]"));
    }

    [Fact]
    public void NormalizeScopesJson_RoundTripsThroughEnsureValidScopes()
    {
        var scopes = new List<string> { "package-report", "ci-trigger", };
        var json = GameAppOpenAccessService.NormalizeScopesJson(scopes);
        var parsed = GameAppOpenAccessService.EnsureValidScopes(json);

        Assert.Equal(scopes.Count, parsed.Count);
        Assert.Equal("package-report", parsed[0]);
        Assert.Equal("ci-trigger", parsed[1]);
    }

    [Fact]
    public void GameAppOpenAccessSecretResult_SerializesSecretForOneTimeDisplay()
    {
        // SecretResult 仅签发/轮换响应返回，前端一次性展示；序列化含明文 AccessSecret。
        var result = new GameAppOpenAccessSecretResponse
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