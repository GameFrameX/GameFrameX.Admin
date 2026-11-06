using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;

namespace GameFrameX.Core.Tests.AppManagement;

public class ChannelDefinitionNamingTests
{
    [Fact]
    public void ChannelDefinition_IsCanonicalEntityName()
    {
        var globalChannel = new GlobalChannelDefinitionEntity
        {
            Code = "official",
            Name = "Official",
            Scope = ChannelScopeEnum.System,
            ChannelType = ChannelTypeEnum.Official,
            PlatformCode = "all",
            IsSystemLocked = true,
        };

        Assert.Equal("official", globalChannel.Code);
        Assert.Equal(ChannelScopeEnum.System, globalChannel.Scope);
        Assert.Equal(ChannelTypeEnum.Official, globalChannel.ChannelType);

        var channel = new ChannelDefinitionEntity
        {
            Code = "official",
            Name = "Business Official",
            Scope = ChannelScopeEnum.Business,
            ChannelType = ChannelTypeEnum.Custom,
            PlatformCode = "all",
        };

        Assert.Equal("official", channel.Code);
        Assert.Equal(ChannelScopeEnum.Business, channel.Scope);
    }

    [Fact]
    public void ChannelDefinition_ManagementServices_AreSplitByScope()
    {
        var assembly = typeof(GameFrameX.Application.ClientDelivery.ChannelDefinition.GlobalChannelDefinitionService).Assembly;

        Assert.NotNull(assembly.GetType("GameFrameX.Application.ClientDelivery.ChannelDefinition.GlobalChannelDefinitionService"));
        Assert.NotNull(assembly.GetType("GameFrameX.Application.ClientDelivery.ChannelDefinition.ChannelDefinitionService"));
        Assert.Null(assembly.GetType("GameFrameX.Application.ClientDelivery.ChannelDefinition.ChannelDefinitionService"));
    }
}