using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;

namespace GameFrameX.Core.Tests.AppManagement;

public class ChannelDefinitionNamingTests
{
    [Fact]
    public void ChannelDefinition_IsCanonicalEntityName()
    {
        var channel = new GlobalChannelDefinitionEntity
        {
            Code = "official",
            Name = "Official",
            Scope = ChannelScopeEnum.System,
            ChannelType = ChannelTypeEnum.Official,
            PlatformCode = "all",
            IsSystemLocked = true,
        };

        Assert.Equal("official", channel.Code);
        Assert.Equal(ChannelScopeEnum.System, channel.Scope);
        Assert.Equal(ChannelTypeEnum.Official, channel.ChannelType);

        var tenantChannel = new TenantChannelDefinitionEntity
        {
            Code = "tenant-official",
            Name = "Tenant Official",
            Scope = ChannelScopeEnum.Tenant,
            ChannelType = ChannelTypeEnum.Custom,
            PlatformCode = "all",
        };

        Assert.Equal("tenant-official", tenantChannel.Code);
        Assert.Equal(ChannelScopeEnum.Tenant, tenantChannel.Scope);
    }

    [Fact]
    public void ChannelDefinition_ManagementServices_AreSplitByScope()
    {
        var assembly = typeof(GameFrameX.Application.System.ChannelDefinition.GlobalChannelDefinitionService).Assembly;

        Assert.NotNull(assembly.GetType("GameFrameX.Application.System.ChannelDefinition.GlobalChannelDefinitionService"));
        Assert.NotNull(assembly.GetType("GameFrameX.Application.System.ChannelDefinition.TenantChannelDefinitionService"));
        Assert.Null(assembly.GetType("GameFrameX.Application.System.ChannelDefinition.ChannelDefinitionService"));
    }
}