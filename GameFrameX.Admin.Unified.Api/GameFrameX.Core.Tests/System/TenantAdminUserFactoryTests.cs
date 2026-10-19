using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.Service.Tenant.Dto;
using System.Reflection;

namespace GameFrameX.Core.Tests.System;

public class TenantAdminUserFactoryTests
{
    [Fact]
    public void CreateTenantAdminUser_FillsRequiredStringColumns()
    {
        var tenant = new TenantResponse
        {
            Id = 1300000000999,
            AdminAccount = "tenant_admin",
            Email = "tenant@example.com",
            Phone = "13884998275",
            Name = "Tenant",
        };

        var user = SystemTenantService.CreateTenantAdminUser(
            tenant,
            "encrypted-password",
            1300000000888,
            1300000000777);

        var nullableStringColumns = new HashSet<string>
        {
            nameof(TenantUserEntity.ManagerUser),
        };

        foreach (var property in typeof(TenantUserEntity).GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (property.PropertyType != typeof(string) || nullableStringColumns.Contains(property.Name))
            {
                continue;
            }

            Assert.True(property.GetValue(user) != null, $"{property.Name} should not be null.");
        }
    }
}