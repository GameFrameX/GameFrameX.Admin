namespace GameFrameX.Core.Tests.AppScope;

public class AppScopedServiceSourceGuardTests
{
    public static IEnumerable<object[]> AppScopedServices()
    {
        yield return new object[] { "GameFrameX.Application.Game/Mail/GameMail/TenantGameMailEntityService.cs", "AppId", };
        yield return new object[] { "GameFrameX.Application.Game/Mail/GameMailTemplateCategory/TenantGameMailTemplateCategoryEntityService.cs", "AppId", };
        yield return new object[] { "GameFrameX.Application.Game/Mail/GameMailTemplate/TenantGameMailTemplateEntityService.cs", "AppId", };
        yield return new object[] { "GameFrameX.Application.Game/Order/GameOrder/TenantGameOrderEntityService.cs", "AppId", };
        yield return new object[] { "GameFrameX.Application.Game/Order/GameOrderFulfillment/TenantGameOrderFulfillmentEntityService.cs", "AppId", };
        yield return new object[] { "GameFrameX.Application.Game/User/GameRole/TenantGameRoleEntityService.cs", "AppId", };
        yield return new object[] { "GameFrameX.Application.Game/User/GameUser/TenantGameUserEntityService.cs", "AppId", };
        yield return new object[] { "GameFrameX.Application.Game/App/GameAppPackage/TenantGameAppPackageService.cs", "AppId", };
        yield return new object[] { "GameFrameX.Application.Game/Tenant/AntiCheat/TenantGameAntiCheatEvent/TenantGameAntiCheatEventEntityService.cs", "AppId", };
        yield return new object[] { "GameFrameX.Application.Game/Tenant/Report/TenantGameReport/TenantGameReportEntityService.cs", "AppId", };
        yield return new object[] { "GameFrameX.Application.Game/Tenant/Penalty/TenantGamePenalty/TenantGamePenaltyEntityService.cs", "AppId", };
        yield return new object[] { "GameFrameX.Application.Game/Tenant/Penalty/TenantGamePenaltyAppeal/TenantGamePenaltyAppealEntityService.cs", "AppId", };
        yield return new object[] { "GameFrameX.Application.Game/Reward/GameRewardTemplateGroup/TenantGameRewardTemplateGroupEntityService.cs", "AppId", };
        yield return new object[] { "GameFrameX.Application.Game/Reward/GameRewardTemplate/TenantGameRewardTemplateEntityService.cs", "AppId", };
    }

    [Theory]
    [MemberData(nameof(AppScopedServices))]
    public void AppScopedPageQueries_RequireExplicitCurrentApp(string relativePath, string appIdProperty)
    {
        var source = File.ReadAllText(Path.Combine(FindRepoRoot(), relativePath));

        Assert.Contains($"EnsureFactAppId(input.{appIdProperty}", source);
        Assert.DoesNotContain($"ApplyAppIdFilter(input.{appIdProperty}", source);
        Assert.DoesNotContain($"WhereIF(input.{appIdProperty}.HasValue", source);
    }

    [Fact]
    public void ClientRemoteConfigAdminPageAndWrites_RequireExplicitCurrentApp()
    {
        var source = File.ReadAllText(Path.Combine(FindRepoRoot(), "GameFrameX.Application.Game/App/GameAppRemoteConfig/TenantGameAppRemoteConfigService.cs"));

        Assert.Contains("RequireAppId(input.AppId", source);
        Assert.Contains("input.AppId = RequireAppId(input.AppId", source);
        Assert.DoesNotContain("WhereIF(input.AppId.HasValue", source);
    }

    [Fact]
    public void PaymentConfigAdminPageAndWrites_RequireExplicitCurrentApp()
    {
        var source = File.ReadAllText(Path.Combine(FindRepoRoot(), "GameFrameX.Application.Game/Order/GamePaymentConfig/TenantGamePaymentConfigEntityService.cs"));

        Assert.Contains("EnsureFactAppId(input.GameAppId, \"支付配置\"", source);
        Assert.Contains("EnsureFactAppId(gameAppId, \"支付配置\"", source);
        Assert.DoesNotContain("WhereIF(input.AppId.HasValue", source);
        Assert.DoesNotContain("WhereIF(input.GameAppId.HasValue", source);
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "GameFrameX.Admin.Unified.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Cannot locate GameFrameX.Admin.Unified.slnx");
    }
}