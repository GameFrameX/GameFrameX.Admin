using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Global;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Service.Platform;

public sealed class PlatformReadOnlyProvider : IPlatformReadOnlyProvider
{
    private readonly SqlSugarRepository<GlobalMenuEntity> _menuRepository;
    private readonly SqlSugarRepository<GlobalPlatformEntity> _globalPlatformRepository;
    private readonly SqlSugarRepository<GlobalLanguageEntity> _globalLanguageRepository;
    private readonly SqlSugarRepository<GlobalChannelDefinitionEntity> _channelDefinitionRepository;

    public PlatformReadOnlyProvider(
        SqlSugarRepository<GlobalMenuEntity> menuRepository,
        SqlSugarRepository<GlobalPlatformEntity> globalPlatformRepository,
        SqlSugarRepository<GlobalLanguageEntity> globalLanguageRepository,
        SqlSugarRepository<GlobalChannelDefinitionEntity> channelDefinitionRepository)
    {
        _menuRepository = menuRepository;
        _globalPlatformRepository = globalPlatformRepository;
        _globalLanguageRepository = globalLanguageRepository;
        _channelDefinitionRepository = channelDefinitionRepository;
    }


    public Task<List<GlobalMenuEntity>> GetMenusAsync(IEnumerable<long> menuIds)
    {
        var ids = menuIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return Task.FromResult(new List<GlobalMenuEntity>());
        }

        return _menuRepository.GetListAsync(u => ids.Contains(u.Id));
    }

    public Task<List<GlobalPlatformEntity>> GetGlobalPlatformsAsync()
    {
        return _globalPlatformRepository.GetListAsync();
    }

    public Task<List<GlobalLanguageEntity>> GetGlobalLanguagesAsync()
    {
        return _globalLanguageRepository.GetListAsync();
    }

    public Task<List<GlobalChannelDefinitionEntity>> GetSystemChannelDefinitionsAsync()
    {
        return _channelDefinitionRepository.GetListAsync(u => u.Scope == ChannelScopeEnum.System);
    }
}