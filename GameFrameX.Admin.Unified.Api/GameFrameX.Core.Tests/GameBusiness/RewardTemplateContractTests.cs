using System.ComponentModel.DataAnnotations;
using System.Reflection;
using ApiDescriptionSettingsAttribute = GameFrameX.Core.Api.ApiDescriptionSettingsAttribute;
using GameFrameX.Application.NoticeCenter.GameRewardTemplate;
using GameFrameX.Application.NoticeCenter.GameRewardTemplate.Dto;
using GameFrameX.Application.NoticeCenter.GameRewardTemplateGroup;
using GameFrameX.Application.NoticeCenter.GameRewardTemplateGroup.Dto;
using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Service.Base;
using GameFrameX.Entity.Business.Game;
using Microsoft.AspNetCore.Mvc;

namespace GameFrameX.Core.Tests.GameBusiness;

public class RewardTemplateContractTests
{
    [Fact]
    public void RewardTemplateEntities_CarryBusinessFieldsWithoutApplication()
    {
        var groupProps = PropNames<GameRewardTemplateGroupEntity>();
        var templateProps = PropNames<GameRewardTemplateEntity>();

        Assert.DoesNotContain("AppId", groupProps);
        Assert.DoesNotContain("Code", groupProps);
        Assert.DoesNotContain("Icon", groupProps);
        Assert.Contains("Sort", groupProps);

        Assert.DoesNotContain("AppId", templateProps);
        Assert.Contains("GroupId", templateProps);
        Assert.Contains("RewardItemsJson", templateProps);
        Assert.Contains("SnapshotSchemaVersion", templateProps);
        Assert.Contains("Sort", templateProps);
    }

    [Fact]
    public void RewardTemplateInputs_ExposeRewardFieldsWithoutApplication()
    {
        Assert.Null(typeof(AddGameRewardTemplateGroupEntityRequest).GetProperty("AppId"));
        Assert.Null(typeof(AddGameRewardTemplateGroupEntityRequest).GetProperty("Code"));
        Assert.Null(typeof(AddGameRewardTemplateGroupEntityRequest).GetProperty("Icon"));
        Assert.Null(typeof(AddGameRewardTemplateEntityRequest).GetProperty("AppId"));
        AssertRequired<AddGameRewardTemplateEntityRequest>(nameof(AddGameRewardTemplateEntityRequest.GroupId));
        AssertRequired<AddGameRewardTemplateEntityRequest>(nameof(AddGameRewardTemplateEntityRequest.RewardItemsJson));
        Assert.Null(typeof(GameRewardTemplateSnapshotRequest).GetProperty("AppId"));
    }

    [Fact]
    public void RewardTemplateService_ExposesSelectorAndSnapshotActions()
    {
        AssertAction<GameRewardTemplateGroupService>("SelectAsync", "Select", CrudActionRoutes.Select);
        AssertAction<GameRewardTemplateService>("SelectAsync", "Select", CrudActionRoutes.Select);
        AssertAction<GameRewardTemplateService>("CopySnapshotAsync", "CopySnapshot", "copySnapshot");
    }

    [Fact]
    public void RewardTemplateMenus_LiveUnderContentCenterWithSelectorPermissions()
    {
        var menus = GlobalMenuSeedData.BuildData();
        var rewardRoot = menus.Single(u => u.Key == "route:rewardTemplate");
        var groupPage = menus.Single(u => u.Key == "route:gameRewardTemplateGroupEntity");
        var templatePage = menus.Single(u => u.Key == "route:gameRewardTemplateEntity");
        var permissions = menus.Where(u => !string.IsNullOrEmpty(u.Permission)).Select(u => u.Permission!).ToHashSet(StringComparer.Ordinal);

        Assert.Equal("/noticeCenter/reward", rewardRoot.Path);
        Assert.Equal(rewardRoot.Id, groupPage.Pid);
        Assert.Equal("/noticeCenter/reward/gameRewardTemplateGroupEntity", groupPage.Path);
        Assert.Equal("/noticeCenter/reward/gameRewardTemplateGroupEntity/index", groupPage.Component);
        Assert.Equal(rewardRoot.Id, templatePage.Pid);
        Assert.Equal("/noticeCenter/reward/gameRewardTemplateEntity", templatePage.Path);
        Assert.Equal("/noticeCenter/reward/gameRewardTemplateEntity/index", templatePage.Component);

        Assert.Contains("gameRewardTemplateGroup:Select", permissions);
        Assert.Contains("gameRewardTemplate:Select", permissions);
        Assert.Contains("gameRewardTemplate:CopySnapshot", permissions);
    }

    [Fact]
    public void RewardTemplateServices_GuardRewardSnapshotContracts()
    {
        var groupSource = File.ReadAllText(Path.Combine(FindRepoRoot(), "GameFrameX.Application/NoticeCenter/GameRewardTemplateGroup/GameRewardTemplateGroupService.cs"));
        var templateSource = File.ReadAllText(Path.Combine(FindRepoRoot(), "GameFrameX.Application/NoticeCenter/GameRewardTemplate/GameRewardTemplateService.cs"));

        Assert.DoesNotContain("EnsureFactAppId", groupSource);
        Assert.DoesNotContain("EnsureFactAppId", templateSource);
        Assert.Contains("ValidateGroupAsync(input.GroupId)", templateSource);
        Assert.Contains("ValidateRewardItemsJson(input.RewardItemsJson)", templateSource);
        Assert.Contains("RowVersion = t.RowVersion", templateSource);
        Assert.DoesNotContain("GroupCode", templateSource);
        Assert.DoesNotContain("EnsureCodeUniqueAsync", groupSource);
        Assert.DoesNotContain("entity.AppId", templateSource);
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