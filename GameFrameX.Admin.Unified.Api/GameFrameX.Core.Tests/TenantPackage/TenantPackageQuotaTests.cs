using System.ComponentModel.DataAnnotations;
using GameFrameX.Core.Service.TenantPackage;
using GameFrameX.Core.Service.TenantPackage.Dto;
using GameFrameX.Entity.Global;

namespace GameFrameX.Core.Tests.TenantPackage;

public class TenantPackageQuotaTests
{
    [Fact]
    public void GlobalPackage_DefaultGameQuota_IsOne()
    {
        var package = new GlobalPackageEntity();

        Assert.Equal(1, package.MaxGameAppCount);
        Assert.Equal(1, package.MaxPackageCountPerApp);
    }

    [Fact]
    public void AddTenantPackageInput_RejectsInvalidGameQuota()
    {
        var input = new AddTenantPackageRequest
        {
            Name = "basic",
            Code = "basic",
            MaxGameAppCount = 0,
            MaxPackageCountPerApp = 0,
        };

        var results = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(input, new ValidationContext(input), results, true);

        Assert.False(isValid);
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(AddTenantPackageRequest.MaxGameAppCount)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(AddTenantPackageRequest.MaxPackageCountPerApp)));
    }

    [Fact]
    public void CreateEffectiveQuota_UsesTenantGracePeriodOverride()
    {
        var package = new GlobalPackageEntity
        {
            MaxUserCount = 20,
            MaxOrganizationCount = 15,
            MaxOrganizationLevel = 4,
            MaxRoleCount = 8,
            MaxGameAppCount = 3,
            MaxPackageCountPerApp = 5,
            GracePeriodDays = 7,
        };

        var quota = TenantQuotaHelper.CreateEffectiveQuota(package, 14);

        Assert.Equal(20, quota.MaxUserCount);
        Assert.Equal(3, quota.MaxGameAppCount);
        Assert.Equal(5, quota.MaxPackageCountPerApp);
        Assert.Equal(14, quota.GracePeriodDays);
    }
}