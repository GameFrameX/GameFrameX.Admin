using GameFrameX.Entity.Tenant.Game.App;
using System.ComponentModel.DataAnnotations;
using GameFrameX.Application.Game.App.GameAppPackage.Dto;
using GameFrameX.Application.Game.App.GameAppPackage;
using GameFrameX.Core.Enum;
using ApiDescriptionSettingsAttribute = GameFrameX.Core.Api.ApiDescriptionSettingsAttribute;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Tests.AppManagement;

public class GameAppPackageTests
{
    [Fact]
    public void GameAppPackage_Defaults_AreEnabledAndNotDefault()
    {
        var package = new TenantGameAppPackageEntity();

        Assert.True(package.IsEnabled);
        Assert.False(package.IsDefault);
        Assert.Equal(PackageKindEnum.Normal, package.PackageKind);
        Assert.Equal(InheritanceModeEnum.Override, package.PlatformInheritanceMode);
    }

    [Fact]
    public void GameAppPackageService_ExposesStandardSelectApi()
    {
        var method = typeof(TenantGameAppPackageService).GetMethod(nameof(TenantGameAppPackageService.SelectAsync));

        Assert.NotNull(method);
        Assert.NotNull(method.GetCustomAttributes(typeof(HttpPostAttribute), false).SingleOrDefault());
        var apiSettings = Assert.IsType<ApiDescriptionSettingsAttribute>(
            method.GetCustomAttributes(typeof(ApiDescriptionSettingsAttribute), false).Single());
        Assert.Equal("Select", apiSettings.Name);
    }

    [Fact]
    public void GameAppPackage_CapturesDistributionIdentity()
    {
        var package = new TenantGameAppPackageEntity
        {
            GameAppId = 100,
            Name = "iOS Global",
            PackageName = "com.demo.game",
            PlatformCode = "iOS",
            PlatformName = "iOS",
            ChannelId = 31850206682437,
            ChannelCode = "AppStore",
            ChannelName = "App Store",
            PackageVersion = "1.0.0",
        };

        Assert.Equal(100, package.GameAppId);
        Assert.Equal("com.demo.game", package.PackageName);
        Assert.Equal("iOS", package.PlatformCode);
        Assert.Equal(31850206682437, package.ChannelId);
        Assert.Equal("AppStore", package.ChannelCode);
    }

    [Fact]
    public void AddGameAppPackageInput_RequiresDistributionIdentity()
    {
        var input = new AddGameAppPackageRequest();

        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(input, new ValidationContext(input), results, true);

        Assert.False(isValid);
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(AddGameAppPackageRequest.AppId)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(AddGameAppPackageRequest.Name)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(AddGameAppPackageRequest.PackageName)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(AddGameAppPackageRequest.PlatformCode)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(AddGameAppPackageRequest.ChannelId)));
    }

    [Fact]
    public void AddGameAppPackageInput_RequiresChannelIdEvenWhenChannelCodeProvided()
    {
        var input = new AddGameAppPackageRequest
        {
            AppId = 100,
            Name = "Android Official",
            PackageName = "com.demo.game",
            PlatformCode = "Android",
            ChannelId = 0,
        };

        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(input, new ValidationContext(input), results, true);

        Assert.False(isValid);
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(AddGameAppPackageRequest.ChannelId)));
    }

    [Fact]
    public void ChannelEnums_UseStableMigrationValues()
    {
        Assert.Equal(1, (int)ChannelScopeEnum.System);
        Assert.Equal(2, (int)ChannelScopeEnum.Tenant);
        Assert.Equal(1, (int)ChannelTypeEnum.Official);
        Assert.Equal(2, (int)ChannelTypeEnum.Store);
        Assert.Equal(3, (int)ChannelTypeEnum.Ad);
        Assert.Equal(4, (int)ChannelTypeEnum.Custom);
    }

    [Fact]
    public void PackageInheritanceEnums_UseStableMigrationValues()
    {
        Assert.Equal(1, (int)PackageKindEnum.Normal);
        Assert.Equal(2, (int)PackageKindEnum.Master);
        Assert.Equal(3, (int)PackageKindEnum.Derived);
        Assert.Equal(1, (int)InheritanceModeEnum.Inherit);
        Assert.Equal(2, (int)InheritanceModeEnum.Override);
        Assert.Equal(3, (int)InheritanceModeEnum.Disable);
    }

    [Fact]
    public void GameAppPackage_CapturesParentPackageInheritance()
    {
        var package = new TenantGameAppPackageEntity
        {
            ParentPackageId = 123,
            PackageKind = PackageKindEnum.Derived,
            PlatformInheritanceMode = InheritanceModeEnum.Inherit,
        };

        Assert.Equal(123, package.ParentPackageId);
        Assert.Equal(PackageKindEnum.Derived, package.PackageKind);
        Assert.Equal(InheritanceModeEnum.Inherit, package.PlatformInheritanceMode);
    }

    [Fact]
    public void GameAppPackageOutput_SerializesPackageVersionAsPackageVersion()
    {
        var output = new GameAppPackageResponse
        {
            PackageVersion = "1.0.0",
        };

        var json = JsonSerializer.Serialize(output);

        Assert.Contains("\"PackageVersion\":\"1.0.0\"", json);
        Assert.DoesNotContain("\"version\":\"1.0.0\"", json);
        Assert.DoesNotContain("\"Version\":", json);
    }

    [Fact]
    public void EnsurePackageKindConsistency_DerivedWithParent_Accepts()
    {
        TenantGameAppPackageService.EnsurePackageKindConsistency(9201, PackageKindEnum.Derived);
    }

    [Fact]
    public void EnsurePackageKindConsistency_NormalOrMasterWithoutParent_Accepts()
    {
        TenantGameAppPackageService.EnsurePackageKindConsistency(null, PackageKindEnum.Normal);
        TenantGameAppPackageService.EnsurePackageKindConsistency(null, PackageKindEnum.Master);
    }

    [Fact]
    public void NormalizePackageKind_WithParent_ForcesDerived()
    {
        // 指定母包即视为子包，Master/Normal 自动纠正为 Derived。
        var masterDeclared = new TenantGameAppPackageEntity { ParentPackageId = 9201, PackageKind = PackageKindEnum.Master, };
        var normalDeclared = new TenantGameAppPackageEntity { ParentPackageId = 9201, PackageKind = PackageKindEnum.Normal, };

        TenantGameAppPackageService.NormalizePackageKind(masterDeclared);
        TenantGameAppPackageService.NormalizePackageKind(normalDeclared);

        Assert.Equal(PackageKindEnum.Derived, masterDeclared.PackageKind);
        Assert.Equal(PackageKindEnum.Derived, normalDeclared.PackageKind);
    }

    [Fact]
    public void NormalizePackageKind_WithoutParent_PreservesDeclaredKind()
    {
        var master = new TenantGameAppPackageEntity { ParentPackageId = null, PackageKind = PackageKindEnum.Master, };
        var normal = new TenantGameAppPackageEntity { ParentPackageId = null, PackageKind = PackageKindEnum.Normal, };

        TenantGameAppPackageService.NormalizePackageKind(master);
        TenantGameAppPackageService.NormalizePackageKind(normal);

        Assert.Equal(PackageKindEnum.Master, master.PackageKind);
        Assert.Equal(PackageKindEnum.Normal, normal.PackageKind);
    }
}