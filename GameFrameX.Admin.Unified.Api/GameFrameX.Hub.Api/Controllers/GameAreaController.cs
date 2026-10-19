using Microsoft.AspNetCore.Authorization;
using GameFrameX.Core.Service;
using System.Net;
using GameFrameX.Application.Game.ServerManager;
using GameFrameX.Hub.Api.Dto.GameArea;
using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Hub.Api.Controllers;

/// <summary>
/// 游戏区服控制器
/// </summary>
[Authorize]
public class GameAreaController : BaseController<TenantGameAreaEntity>
{
    private readonly Repository<TenantGameDockerTagEntity> _dockerTagService;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// </summary>
    /// <param name="service"></param>
    /// <param name="dockerTagService">镜像版本</param>
    /// <param name="httpClient"></param>
    public GameAreaController(
        Repository<TenantGameAreaEntity> service,
        Repository<TenantGameDockerTagEntity> dockerTagService,
        HttpClient httpClient) : base(service)
    {
        _dockerTagService = dockerTagService;
        _httpClient = httpClient;
    }

    /// <summary>
    /// 停止容器
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> Stop([FromBody] StopTenantGameAreaEntityRequest input)
    {
        var gameAreaEntity = await PRepository.GetFirstAsync(m => m.Id == input.Id);
        if (gameAreaEntity == null)
        {
            return HttpJsonResultData<BaseOperationAckResponse>.NotFound();
        }

        var gameAreaClassifyEntity = await PRepository.AsSugarClient().Queryable<TenantGameAreaClassifyEntity>().Where(m => m.Id == gameAreaEntity.ClassifyId).FirstAsync();
        var url = gameAreaClassifyEntity.HttpManagerUrl;

        if (!string.IsNullOrEmpty(url))
        {
            try
            {
                // 执行停止
                await _httpClient.PostJsonToStringAsync(url + "api/Containers/Stop", new BaseRequest { Id = gameAreaEntity.ServerContainerId, });
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        await PRepository.FakeDeleteAsync(gameAreaEntity);
        return HttpJsonResultData<BaseOperationAckResponse>.Success();
    }

    /// <summary>
    /// 删除容器
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> Remove([FromBody] RemoveTenantGameAreaEntityRequest input)
    {
        var gameAreaEntity = await PRepository.GetFirstAsync(m => m.Id == input.Id);
        if (gameAreaEntity == null)
        {
            return HttpJsonResultData<BaseOperationAckResponse>.NotFound();
        }

        var gameAreaClassifyEntity = await PRepository.AsSugarClient().Queryable<TenantGameAreaClassifyEntity>().Where(m => m.Id == gameAreaEntity.ClassifyId).FirstAsync();
        var url = gameAreaClassifyEntity.HttpManagerUrl;

        if (!string.IsNullOrEmpty(url))
        {
            try
            {
                // 执行停止
                await _httpClient.PostJsonToStringAsync(url + "api/Containers/Remove", new BaseRequest { Id = gameAreaEntity.ServerContainerId, });
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        await PRepository.FakeDeleteAsync(gameAreaEntity);
        return HttpJsonResultData<BaseOperationAckResponse>.Success();
    }

    /// <summary>
    /// 增加游戏区服
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> Add([FromBody] AddTenantGameAreaEntityRequest input)
    {
        var gameAreaClassifyEntity = await PRepository.AsSugarClient().Queryable<TenantGameAreaClassifyEntity>().Where(m => m.Id == input.ClassifyId).FirstAsync();
        if (gameAreaClassifyEntity == null)
        {
            gameAreaClassifyEntity = new TenantGameAreaClassifyEntity
            {
                Id = input.ClassifyId,
                Name = "开发区服",
                Description = "开发区服",
            };
            await PRepository.AsSugarClient().Insertable<TenantGameAreaClassifyEntity>(gameAreaClassifyEntity).ExecuteCommandAsync();
        }

        var gameAreaCatEntity = await PRepository.GetFirstAsync(m => m.Id == input.Id);
        if (gameAreaCatEntity == null)
        {
            gameAreaCatEntity = input.Adapt<TenantGameAreaEntity>();
            gameAreaCatEntity.ClassifyId = gameAreaClassifyEntity.Id;
            gameAreaCatEntity.Id = input.Id;
            gameAreaCatEntity.IsDefault = false;
            if (gameAreaCatEntity.ServerVersion.IsNullOrEmpty())
            {
                gameAreaCatEntity.ServerVersion = "1.0.0";
            }

            gameAreaCatEntity.LastMaintenanceTime = TimerHelper.UnixTimeSeconds();
            gameAreaCatEntity.CreationTime = TimerHelper.UnixTimeSeconds();
            gameAreaCatEntity.IsEnabled = true;
            gameAreaCatEntity.IsRecommend = false;
            gameAreaCatEntity.Language = GameLanguage.Unspecified;
            gameAreaCatEntity.ServerStatus = GameServerStatus.Online;
            gameAreaCatEntity.Capacity = 5000; // 默认容量
            await PRepository.InsertAsync(gameAreaCatEntity);
        }
        else
        {
            gameAreaCatEntity = input.Adapt(gameAreaCatEntity);
            await PRepository.UpdateWithOptimisticLockAsync(gameAreaCatEntity);
        }

        return HttpJsonResultData<BaseOperationAckResponse>.Success();
    }

    /// <summary>
    /// 添加镜像tag
    /// </summary>
    public sealed class AddDockerTagRequest
    {
        /// <summary>
        /// 镜像名
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// 镜像描述
        /// </summary>
        public string Description { get; set; } = null!;

        /// <summary>
        /// 镜像编号
        /// </summary>
        public string ServerVersion { get; set; } = null!;
    }

    /// <summary>
    /// 增加游戏区服镜像tag
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200, Type = typeof(BaseOperationAckResponse))]
    public async Task<HttpJsonResultData<BaseOperationAckResponse>> ServerVersionAdd([FromBody] AddDockerTagRequest input)
    {
        // {
        // \"Name\": \"\${CI_BUILD_NUMBER}\",
        // \"Description\": \"\${CI_BUILD_NUMBER}\",
        // \"ServerVersion\":\"\${CI_BUILD_NUMBER}\"
        // }
        var dockerTag = await _dockerTagService.GetFirstAsync(x => x.ServerVersion == input.ServerVersion);
        if (dockerTag != null)
        {
            return HttpJsonResultData<BaseOperationAckResponse>.Success();
        }

        var dockerTagEntity = new TenantGameDockerTagEntity
        {
            Name = input.Name,
            Description = input.Description,
            ServerVersion = input.ServerVersion,
        };
        var result = await _dockerTagService.InsertAsync(dockerTagEntity);
        if (!result)
        {
            return HttpJsonResultData<BaseOperationAckResponse>.Error((int)HttpStatusCode.Found, "已经存在");
        }

        return HttpJsonResultData<BaseOperationAckResponse>.Success();
    }
}