using GameFrameX.Client.Api.GameArea.Dto;
using GameFrameX.Client.Api.Services.ClientApiTenantOptions;
using GameFrameX.Core.Service;

namespace GameFrameX.Client.Api.GameArea;

/// <summary>
///     程序版本控制器
/// </summary>
public class GameAreaController : BaseController<TenantGameAreaEntity>
{
    private readonly IClientApiTenantOptionsProvider _optionsProvider;

    /// <summary>
    /// </summary>
    /// <param name="service"></param>
    /// <param name="optionsProvider"></param>
    public GameAreaController(SqlSugarRepository<TenantGameAreaEntity> service, IClientApiTenantOptionsProvider optionsProvider) : base(service)
    {
        _optionsProvider = optionsProvider;
    }

    /// <summary>
    /// 按大区分类获取游戏区服
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(List<TenantGameAreaClassifyEntityResponse>))]
    public async Task<HttpJsonResultData<List<TenantGameAreaClassifyEntityResponse>>> GetAreaList([FromBody] GameAreaGetAreaListRequest gameAreaGetAreaListRequest)
    {
        var catList = await PRepository.AsSugarClient().Queryable<TenantGameAreaClassifyEntity>().ToListAsync();
        var gameAreaCatEntityList = new List<TenantGameAreaClassifyEntityResponse>(catList.Count);
        var gameAreaEntities = await PRepository.GetListAsync();
        var areaPolicy = await _optionsProvider.GetAreaPolicyAsync();
        foreach (var gameAreaCatEntity in catList)
        {
            var gameAreaCatEntityDto = gameAreaCatEntity.Adapt<TenantGameAreaClassifyEntityResponse>();
            // var gameAreaList = gameAreaEntities
            //     .Where(m => m.ClassifyId == gameAreaCatEntity.Id && m.IsEnabled.GetValueOrDefault())
            //     .WhereIf(!string.IsNullOrWhiteSpace(gameAreaGetAreaListRequest.PackageName),
            //         x => x.PackageName == gameAreaGetAreaListRequest.PackageName)
            //     .WhereIf(!string.IsNullOrWhiteSpace(gameAreaGetAreaListRequest.Platform),
            //         x => x.Platform == gameAreaGetAreaListRequest.Platform)
            //     .WhereIf(!string.IsNullOrWhiteSpace(gameAreaGetAreaListRequest.AppVersion),
            //         x => x.AppVersion == gameAreaGetAreaListRequest.AppVersion)
            //     .WhereIf(!string.IsNullOrWhiteSpace(gameAreaGetAreaListRequest.Channel),
            //         x => x.Channel == gameAreaGetAreaListRequest.Channel)
            //     .OrderByDescending(m => m.CreatedTime)
            //     .ThenBy(m => m.IsRecommend)
            //     .ToList();
            var gameAreaList = gameAreaEntities.Where(m => m.ClassifyId == gameAreaCatEntity.Id && m.IsEnabled.GetValueOrDefault()).OrderByDescending(m => m.CreatedTime).ThenBy(m => m.IsRecommend).ToList();

            gameAreaCatEntityDto.GameAreaList = new List<TenantGameAreaEntityResponse>(gameAreaList.Count);
            foreach (var gameAreaEntity in gameAreaList)
            {
                var gameAreaEntityDto = gameAreaEntity.Adapt<TenantGameAreaEntityResponse>();
                if (areaPolicy.EnableReturnHttpManager)
                {
                    gameAreaEntityDto.HttpManageUrl = gameAreaEntity.HttpManageUrl;
                }
                else
                {
                    gameAreaEntityDto.HttpManageUrl = string.Empty;
                }

                gameAreaCatEntityDto.GameAreaList.Add(gameAreaEntityDto);
            }

            gameAreaCatEntityList.Add(gameAreaCatEntityDto);
        }

        return HttpJsonResultData<List<TenantGameAreaClassifyEntityResponse>>.Success(gameAreaCatEntityList);
    }
}