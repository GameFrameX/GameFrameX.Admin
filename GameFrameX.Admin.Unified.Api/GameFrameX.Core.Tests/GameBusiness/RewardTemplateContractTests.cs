using System.ComponentModel.DataAnnotations;
using System.Reflection;
using ApiDescriptionSettingsAttribute = GameFrameX.Core.Api.ApiDescriptionSettingsAttribute;
using GameFrameX.Application.Game.Reward.GameRewardTemplate;
using GameFrameX.Application.Game.Reward.GameRewardTemplate.Dto;
using GameFrameX.Application.Game.Reward.GameRewardTemplateGroup;
using GameFrameX.Application.Game.Reward.GameRewardTemplateGroup.Dto;
using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Service.Base;
using GameFrameX.Entity.Tenant.Game;
using Microsoft.AspNetCore.Mvc;

namespace GameFrameX.Core.Tests.GameBusiness;

public class RewardTemplateContractTests
{
    [Fact]
    public void RewardTemplateEntities_CarryRequiredAppScopedFields()
    {
        var groupProps = PropNames<TenantGameRewardTemplateGroupEntity>();
        var templateProps = PropNames<TenantGameRewardTemplateEntity>();

        Assert.Contains("AppId", groupProps);
        Assert.DoesNotContain("Code", groupProps);
        Assert.DoesNotContain("Icon", groupProps);
        Assert.Contains("Sort", groupProps);

        Assert.Contains("AppId", templateProps);
        Assert.Contains("GroupId", templateProps);
        Assert.Contains("RewardItemsJson", templateProps);
        Assert.Contains("SnapshotSchemaVersion", templateProps);
        Assert.Contains("Sort", templateProps);
    }

    [Fact]
    public void RewardTemplateInputs_ExposeRequiredAppAndRewardFields()
    {
        AssertRequired<AddTenantGameRewardTemplateGroupEntityRequest>(nameof(AddTenantGameRewardTemplateGroupEntityRequest.AppId));
        Assert.Null(typeof(AddTenantGameRewardTemplateGroupEntityRequest).GetProperty("Code"));
        Assert.Null(typeof(AddTenantGameRewardTemplateGroupEntityRequest).GetProperty("Icon"));
        AssertRequired<AddTenantGameRewardTemplateEntityRequest>(nameof(AddTenantGameRewardTemplateEntityRequest.AppId));
        AssertRequired<AddTenantGameRewardTemplateEntityRequest>(nameof(AddTenantGameRewardTemplateEntityRequest.GroupId));
        AssertRequired<AddTenantGameRewardTemplateEntityRequest>(nameof(AddTenantGameRewardTemplateEntityRequest.RewardItemsJson));
        AssertRequired<TenantGameRewardTemplateSnapshotRequest>(nameof(TenantGameRewardTemplateSnapshotRequest.AppId));
    }

    [Fact]
    public void RewardTemplateService_ExposesSelectorAndSnapshotActions()
    {
        AssertAction<TenantGameRewardTemplateGroupEntityService>("SelectAsync", "Select", CrudActionRoutes.Select);
        AssertAction<TenantGameRewardTemplateEntityService>("SelectAsync", "Select", CrudActionRoutes.Select);
        AssertAction<TenantGameRewardTemplateEntityService>("CopySnapshotAsync", "CopySnapshot", "copySnapshot");
    }

    [Fact]
    public void RewardTemplateMenus_LiveUnderContentCenterWithSelectorPermissions()
    {
        var menus = GlobalMenuSeedData.BuildData();
        var rewardRoot = menus.Single(u => u.Key == "route:rewardTemplate");
        var groupPage = menus.Single(u => u.Key == "route:gameRewardTemplateGroupEntity");
        var templatePage = menus.Single(u => u.Key == "route:gameRewardTemplateEntity");
        var permissions = menus.Where(u => !string.IsNullOrEmpty(u.Permission)).Select(u => u.Permission!).ToHashSet(StringComparer.Ordinal);

        Assert.Equal("/contentCenter/reward", rewardRoot.Path);
        Assert.Equal(rewardRoot.Id, groupPage.Pid);
        Assert.Equal("/app/contentCenter/reward/gameRewardTemplateGroupEntity", groupPage.Path);
        Assert.Equal("/app/contentCenter/reward/gameRewardTemplateGroupEntity/index", groupPage.Component);
        Assert.Equal(rewardRoot.Id, templatePage.Pid);
        Assert.Equal("/app/contentCenter/reward/gameRewardTemplateEntity", templatePage.Path);
        Assert.Equal("/app/contentCenter/reward/gameRewardTemplateEntity/index", templatePage.Component);

        Assert.Contains("tenantGameRewardTemplateGroupEntity:Select", permissions);
        Assert.Contains("tenantGameRewardTemplateEntity:Select", permissions);
        Assert.Contains("tenantGameRewardTemplateEntity:CopySnapshot", permissions);
    }

    [Fact]
    public void RewardTemplateServices_GuardAppScopeAndRewardSnapshotContracts()
    {
        var groupSource = File.ReadAllText(Path.Combine(FindRepoRoot(), "GameFrameX.Application.Game/Reward/GameRewardTemplateGroup/TenantGameRewardTemplateGroupEntityService.cs"));
        var templateSource = File.ReadAllText(Path.Combine(FindRepoRoot(), "GameFrameX.Application.Game/Reward/GameRewardTemplate/TenantGameRewardTemplateEntityService.cs"));

        Assert.Contains("EnsureFactAppId(input.AppId", groupSource);
        Assert.Contains("EnsureFactAppId(input.AppId", templateSource);
        Assert.Contains("ValidateGroupBelongsToAppAsync(input.GroupId, input.AppId)", templateSource);
        Assert.Contains("ValidateRewardItemsJson(input.RewardItemsJson)", templateSource);
        Assert.Contains("RowVersion = t.RowVersion", templateSource);
        Assert.DoesNotContain("GroupCode", templateSource);
        Assert.DoesNotContain("EnsureCodeUniqueAsync", groupSource);
        Assert.Contains("entity.AppId != appId", templateSource);
        Assert.Contains("entity.IsEnabled == false", templateSource);
    }

    private static HashSet<string> PropNames<T>()
    {
        return typeof(T).GetProperties().Select(p => p.Name).ToHashSet();
    }

    private static void AssertRequired<T>(string propertyName)
    {
        var property = typeof(T).GetProperty(propertyName);

        Assert.NotNull(property);
        Assert.NotNull(property.GetCustomAttribute<RequiredAttribute>());
    }

    private static void AssertAction<T>(string methodName, string expectedApiName, string expectedTemplate)
    {
        var method = typeof(T).GetMethod(methodName);

        Assert.NotNull(method);
        var post = Assert.IsType<HttpPostAttribute>(method.GetCustomAttributes(typeof(HttpPostAttribute), false).Single());
        Assert.Equal(expectedTemplate, post.Template);
        var apiSettings = Assert.IsType<ApiDescriptionSettingsAttribute>(
            method.GetCustomAttributes(typeof(ApiDescriptionSettingsAttribute), false).Single());
        Assert.Equal(expectedApiName, apiSettings.Name);
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