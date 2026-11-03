using GameFrameX.Entity.Global;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Service.Platform;

public interface IPlatformReadOnlyProvider
{




    Task<List<GlobalMenuEntity>> GetMenusAsync(IEnumerable<long> menuIds);

    Task<List<GlobalPlatformEntity>> GetGlobalPlatformsAsync();

    Task<List<GlobalLanguageEntity>> GetGlobalLanguagesAsync();

    Task<List<GlobalChannelDefinitionEntity>> GetSystemChannelDefinitionsAsync();
}