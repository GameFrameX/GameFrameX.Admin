using GameFrameX.Core.SqlSugar;

namespace GameFrameX.Application.SeedData.System;

/// <summary>
/// 系统用户扩展机构表种子数据
/// </summary>
public class TenantUserExtensionOrganizationSeedData : ISqlSugarEntitySeedData<TenantUserExtensionOrganizationEntity>
{
    /// <summary>
    /// 种子数据
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TenantUserExtensionOrganizationEntity> HasData()
    {
        return new[]
        {
            new TenantUserExtensionOrganizationEntity { Id = 1300000000101, UserId = 1300000000111, OrganizationId = 1300000000202, PosId = 1300000000106, },
            new TenantUserExtensionOrganizationEntity { Id = 1300000000102, UserId = 1300000000112, OrganizationId = 1300000000104, PosId = 1300000000102, },
            new TenantUserExtensionOrganizationEntity { Id = 1300000000103, UserId = 1300000000113, OrganizationId = 1300000000106, PosId = 1300000000114, },
            new TenantUserExtensionOrganizationEntity { Id = 1300000000104, UserId = 1300000000114, OrganizationId = 1300000000104, PosId = 1300000000107, },
            new TenantUserExtensionOrganizationEntity { Id = 1300000000105, UserId = 1300000000115, OrganizationId = 1300000000303, PosId = 1300000000108, },
            new TenantUserExtensionOrganizationEntity { Id = 1300000000106, UserId = 1300000000116, OrganizationId = 1300000000101, PosId = 1300000000116, },
            new TenantUserExtensionOrganizationEntity { Id = 1300000000107, UserId = 1300000000117, OrganizationId = 1300000000101, PosId = 1300000000112, },
            new TenantUserExtensionOrganizationEntity { Id = 1300000000108, UserId = 1300000000118, OrganizationId = 1300000000101, PosId = 1300000000115, },
            new TenantUserExtensionOrganizationEntity { Id = 1300000000109, UserId = 1300000000119, OrganizationId = 1300000000101, PosId = 1300000000103, },
            new TenantUserExtensionOrganizationEntity { Id = 1300000000110, UserId = 1300000000120, OrganizationId = 1300000000104, PosId = 1300000000104, },
        };
    }
}