using GameFrameX.Application.System.GlobalPlatform.Dto;

namespace GameFrameX.Application.System.GlobalPlatform;

/// <summary>
/// 平台管理服务
/// </summary>
[ApiDescriptionSettings(Order = 100)]
public class GlobalPlatformService
    : GenericCrudSelectService<GameFrameX.Entity.Global.GlobalPlatformEntity,
        AddGlobalPlatformRequest, UpdateGlobalPlatformRequest,
        GlobalPlatformResponse, PageGlobalPlatformRequest>
{
    public GlobalPlatformService(SqlSugarRepository<GameFrameX.Entity.Global.GlobalPlatformEntity> repository)
        : base(repository)
    {
    }

    protected override async Task OnAddingAsync(AddGlobalPlatformRequest input)
    {
        await CheckNameUniqueAsync(input.Name ?? string.Empty);
    }
}