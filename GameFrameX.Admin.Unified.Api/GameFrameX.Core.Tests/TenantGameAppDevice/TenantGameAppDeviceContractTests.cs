using System.ComponentModel.DataAnnotations;
using System.Reflection;
using GameFrameX.Application.ClientDelivery.Device.GameAppDevice;
using GameFrameX.Application.ClientDelivery.Device.GameAppDevice.Dto;
using GameFrameX.Application.ClientDelivery.Device.DeviceGroup;
using GameFrameX.Application.ClientDelivery.Device.DeviceGroup.Dto;
using GameFrameX.Entity.Tenant.Device;
using Mapster;

namespace GameFrameX.Core.Tests.TenantGameAppDevice;

public sealed class TenantGameAppDeviceContractTests
{
    [Fact]
    public void AddInput_MapsDeviceUniqueIdentifier()
    {
        var input = new AddTenantGameAppDeviceRequest
        {
            Name = "test-device",
            DeviceUniqueIdentifier = "machine-code-001",
        };

        var entity = input.Adapt<TenantGameAppDeviceEntity>();

        Assert.Equal("machine-code-001", entity.DeviceUniqueIdentifier);

        input.DeviceUniqueIdentifier = null;
        var isValid = Validate(input, out var results);
        Assert.False(isValid);
        Assert.Contains(results, r =>
                            r.ErrorMessage == "机器码不能为空" &&
                            r.MemberNames.Contains(nameof(AddTenantGameAppDeviceRequest.DeviceUniqueIdentifier)));
    }

    [Fact]
    public void AddInput_ValidWithoutApplication()
    {
        var input = new AddTenantGameAppDeviceRequest
        {
            Name = "test-device",
            DeviceUniqueIdentifier = "machine-code-001",
        };

        var isValid = Validate(input, out var results);

        Assert.True(isValid);
        Assert.Null(typeof(AddTenantGameAppDeviceRequest).GetProperty("AppId"));
    }

    [Fact]
    public void AddDeviceGroupInput_ValidWithoutApplication()
    {
        var input = new AddTenantDeviceGroupRequest
        {
            Name = "group-1",
        };

        var isValid = Validate(input, out var results);

        Assert.True(isValid);
        Assert.Null(typeof(AddTenantDeviceGroupRequest).GetProperty("AppId"));
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

    [Fact]
    public void UpdateInput_HasNoApplicationField()
    {
        Assert.Null(typeof(UpdateTenantGameAppDeviceRequest).GetProperty("AppId"));
    }

    [Fact]
    public void TenantGameAppDeviceService_UpdateUsesGenericCrudPath()
    {
        var updateMethod = typeof(GameAppDeviceService).GetMethod(
            "UpdateAsync",
            BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(updateMethod);
        Assert.NotEqual(typeof(GameAppDeviceService), updateMethod!.DeclaringType);
        Assert.Contains("GenericCrudService", updateMethod.DeclaringType!.Name);
    }

    [Fact]
    public void TenantDeviceGroupService_UpdateUsesGenericCrudPath()
    {
        var updateMethod = typeof(DeviceGroupService).GetMethod(
            "UpdateAsync",
            BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(updateMethod);
        Assert.NotEqual(typeof(DeviceGroupService), updateMethod!.DeclaringType);
        Assert.Contains("GenericCrudService", updateMethod.DeclaringType!.Name);
    }

    private static bool Validate(object input, out List<ValidationResult> results)
    {
        results = new List<ValidationResult>();
        return Validator.TryValidateObject(input, new ValidationContext(input), results, true);
    }
}