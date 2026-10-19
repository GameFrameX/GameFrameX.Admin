using GameFrameX.Core.Hub;
using GameFrameX.Core.SqlSugar;

namespace GameFrameX.Core.Tests.System;

public class OnlineUserHubBoundaryTests
{
    [Fact]
    public void Constructor_DoesNotResolveTenantOnlineUserRepository()
    {
        var constructorParameterTypes = typeof(OnlineUserHub)
                                        .GetConstructors()
                                        .SelectMany(u => u.GetParameters())
                                        .Select(u => u.ParameterType);

        Assert.DoesNotContain(typeof(SqlSugarRepository<TenantOnlineUserEntity>), constructorParameterTypes);
    }

    [Fact]
    public void HubPath_IsStableAndDoesNotUse历史框架MapHubAttribute()
    {
        Assert.Equal("/hubs/onlineUser", OnlineUserHub.HubPath);

        var attributeTypeNames = typeof(OnlineUserHub)
                                 .GetCustomAttributes(false)
                                 .Select(u => u.GetType().FullName);

        Assert.DoesNotContain("历史框架.InstantMessaging.MapHubAttribute", attributeTypeNames);
    }
}