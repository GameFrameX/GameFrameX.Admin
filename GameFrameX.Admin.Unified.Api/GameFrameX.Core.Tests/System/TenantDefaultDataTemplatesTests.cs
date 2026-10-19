using GameFrameX.Application.SeedData.System;
using GameFrameX.Core.Const;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Tests.System;

public class TenantDefaultDataTemplatesTests
{
    [Fact]
    public void DefaultTemplates_ContainTenantAdminRoleRootOrganizationAndPositions()
    {
        Assert.Contains(TenantDefaultDataTemplates.OrganizationTemplates, org => org.Key == TenantDefaultDataTemplates.RootOrganizationKey && org.ParentKey == null && org.Type == "publisher");
        Assert.Contains(TenantDefaultDataTemplates.RoleTemplates, role => role.Code == TenantDefaultDataTemplates.AdminRoleCode && role.DataScope == DataScopeEnum.All);
        Assert.Contains(TenantDefaultDataTemplates.PositionTemplates, pos => pos.Code == TenantDefaultDataTemplates.AdminPositionCode);
    }

    [Fact]
    public void DefaultTemplates_AttributeMigrationKeepsExpectedShape()
    {
        Assert.Equal(14, TenantDefaultDataTemplates.OrganizationTemplates.Count);
        Assert.Equal(10, TenantDefaultDataTemplates.RoleTemplates.Count);
        Assert.Equal(16, TenantDefaultDataTemplates.PositionTemplates.Count);
    }

    [Fact]
    public void DefaultOrganizationTemplates_HaveUniqueKeysAndValidParents()
    {
        var orgKeys = TenantDefaultDataTemplates.OrganizationTemplates
                                                .Select(org => org.Key)
                                                .ToArray();

        Assert.Equal(orgKeys.Length, orgKeys.Distinct(StringComparer.Ordinal).Count());

        var orgKeySet = orgKeys.ToHashSet(StringComparer.Ordinal);
        foreach (var org in TenantDefaultDataTemplates.OrganizationTemplates.Where(org => org.ParentKey != null))
        {
            Assert.Contains(org.ParentKey!, orgKeySet);
        }
    }

    [Fact]
    public void DefaultRoleTemplates_HaveUniqueCodes()
    {
        var roleCodes = TenantDefaultDataTemplates.RoleTemplates
                                                  .Select(role => role.Code)
                                                  .ToArray();

        Assert.Equal(roleCodes.Length, roleCodes.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void DefaultPositionTemplates_HaveUniqueCodes()
    {
        var positionCodes = TenantDefaultDataTemplates.PositionTemplates
                                                      .Select(position => position.Code)
                                                      .ToArray();

        Assert.Equal(positionCodes.Length, positionCodes.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void SeedData_UsesSharedTenantDefaultTemplates()
    {
        var seedRoleCodes = new TenantRoleSeedData().HasData()
                                                    .Select(role => role.Code)
                                                    .OrderBy(code => code, StringComparer.Ordinal)
                                                    .ToArray();
        var templateRoleCodes = TenantDefaultDataTemplates.RoleTemplates
                                                          .Select(role => role.Code)
                                                          .OrderBy(code => code, StringComparer.Ordinal)
                                                          .ToArray();

        var seedPositionCodes = new TenantPositionSeedData().HasData()
                                                            .Select(position => position.Code)
                                                            .OrderBy(code => code, StringComparer.Ordinal)
                                                            .ToArray();
        var templatePositionCodes = TenantDefaultDataTemplates.PositionTemplates
                                                              .Select(position => position.Code)
                                                              .OrderBy(code => code, StringComparer.Ordinal)
                                                              .ToArray();

        var seedOrganizationCodes = new TenantOrganizationSeedData().HasData()
                                                           .Select(org => org.Code)
                                                           .OrderBy(code => code, StringComparer.Ordinal)
                                                           .ToArray();
        var templateOrganizationCodes = TenantDefaultDataTemplates.OrganizationTemplates
                                                         .Select(org => org.Code)
                                                         .OrderBy(code => code, StringComparer.Ordinal)
                                                         .ToArray();

        Assert.Equal(templateRoleCodes, seedRoleCodes);
        Assert.Equal(templatePositionCodes, seedPositionCodes);
        Assert.Equal(templateOrganizationCodes, seedOrganizationCodes);
    }
}