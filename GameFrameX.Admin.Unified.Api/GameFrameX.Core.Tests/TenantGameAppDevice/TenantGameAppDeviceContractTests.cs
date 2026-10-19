using System.ComponentModel.DataAnnotations;
using System.Reflection;
using GameFrameX.Application.System.Tenant.Device.TenantGameAppDevice;
using GameFrameX.Application.System.Tenant.Device.TenantGameAppDevice.Dto;
using GameFrameX.Application.System.Tenant.Device.TenantDeviceGroup;
using GameFrameX.Application.System.Tenant.Device.TenantDeviceGroup.Dto;
using GameFrameX.Entity.Tenant.Device;
using Mapster;

namespace GameFrameX.Core.Tests.TenantGameAppDevice;

public sealed class TenantGameAppDeviceContractTests
{
    [Fact]
    public void AddInput_MapsDeviceUniqueIdentifierAndAppId()
    {
        var input = new AddTenantGameAppDeviceRequest
        {
            Name = "test-device",
            AppId = 1001,
            DeviceUniqueIdentifier = "machine-code-001",
        };

        var entity = input.Adapt<TenantGameAppDeviceEntity>();

        Assert.Equal("machine-code-001", entity.DeviceUniqueIdentifier);
        Assert.Equal(1001, entity.AppId);

        input.DeviceUniqueIdentifier = null;
        var isValid = Validate(input, out var results);
        Assert.False(isValid);
        Assert.Contains(results, r =>
                            r.ErrorMessage == "机器码不能为空" &&
                            r.MemberNames.Contains(nameof(AddTenantGameAppDeviceRequest.DeviceUniqueIdentifier)));
    }

    /// <summary>
    /// 设备码归属具体应用，AppId 必填且 &gt; 0（C211：禁止 AppId=0 的租户默认机器码）。
    /// </summary>
    [Fact]
    public void AddInput_RequiresAppId()
    {
        var input = new AddTenantGameAppDeviceRequest
        {
            Name = "test-device",
            AppId = 0,
            DeviceUniqueIdentifier = "machine-code-001",
        };

        var isValid = Validate(input, out var results);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(AddTenantGameAppDeviceRequest.AppId)));
    }

    /// <summary>
    /// 设备分组同样为应用级资源，AppId 必填。
    /// </summary>
    [Fact]
    public void AddDeviceGroupInput_RequiresAppId()
    {
        var input = new AddTenantDeviceGroupRequest
        {
            Name = "group-1",
            AppId = 0,
        };

        var isValid = Validate(input, out var results);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(AddTenantDeviceGroupRequest.AppId)));
    }

    [Fact]
    public void UpdateInput_RowVersionZeroPassesValidation()
    {
        var input = new UpdateTenantGameAppDeviceRequest
        {
            Id = 1,
            Name = "test-device",
            RowVersion = 0,
            DeviceUniqueIdentifier = "machine-code-001",
        };

        var isValid = Validate(input, out var results);

        Assert.True(isValid);
        Assert.DoesNotContain(results, r => r.ErrorMessage == "版本号不能为空");
    }

    /// <summary>
    /// UpdateAsync 路径 AppId 不可变：DTO 不带 Required/Range，由服务端从既有实体回填，前端无需传入。
    /// </summary>
    [Fact]
    public void UpdateInput_AppIdIsNotValidated()
    {
        var appIdProp = typeof(UpdateTenantGameAppDeviceRequest).GetProperty(nameof(UpdateTenantGameAppDeviceRequest.AppId));

        Assert.NotNull(appIdProp);
        var hasRequired = appIdProp!.GetCustomAttributes(typeof(RequiredAttribute), false).Any();
        var hasRange = appIdProp!.GetCustomAttributes(typeof(RangeAttribute), false).Any();
        Assert.False(hasRequired);
        Assert.False(hasRange);
    }

    /// <summary>
    /// C83 契约不回归：TenantGameAppDeviceService.UpdateAsync 仍走统一 GenericCrud 路径（乐观锁）。
    /// </summary>
    [Fact]
    public void TenantGameAppDeviceService_UpdateUsesGenericCrudPath()
    {
        var updateMethod = typeof(TenantGameAppDeviceService).GetMethod(
            "UpdateAsync",
            BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(updateMethod);
        Assert.NotEqual(typeof(TenantGameAppDeviceService), updateMethod!.DeclaringType);
        Assert.Contains("GenericCrudService", updateMethod.DeclaringType!.Name);
    }

    /// <summary>
    /// 设备分组 UpdateAsync 走统一 GenericCrud 路径（乐观锁/审计/创建列保护），不得用手写 SetColumns 绕过。
    /// </summary>
    [Fact]
    public void TenantDeviceGroupService_UpdateUsesGenericCrudPath()
    {
        var updateMethod = typeof(TenantDeviceGroupService).GetMethod(
            "UpdateAsync",
            BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(updateMethod);
        Assert.NotEqual(typeof(TenantDeviceGroupService), updateMethod!.DeclaringType);
        Assert.Contains("GenericCrudService", updateMethod.DeclaringType!.Name);
    }

    private static bool Validate(object input, out List<ValidationResult> results)
    {
        results = new List<ValidationResult>();
        return Validator.TryValidateObject(input, new ValidationContext(input), results, true);
    }
}