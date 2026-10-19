/*// Jobs/MyJob.cs

using GameFrameX.Application.Game;
using System.ComponentModel;
using GameFrameX.Application.Game.ServerManager;
using GameFrameX.Core.Enum;
using GameFrameX.Core.Job;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Foundation.Http.Extension;
using GameFrameX.Foundation.Http.Normalization;
using GameFrameX.Foundation.Json;
using Microsoft.Extensions.Logging;

namespace GameFrameX.Application.Game.Job;

/// <summary>
/// 邮件定时任务
/// </summary>
public class EmailTask : BaseService<TenantGameMailEntity>, IScheduleJob
{
    private readonly ILogger<EmailTask> _logger;
    private readonly HttpClient _httpClientFactory;
    private readonly SystemCacheService _systemCacheService;
    private readonly SqlSugarRepository<TenantGameAreaEntity> _gameAreaEntityRepository;
    private readonly SqlSugarRepository<TenantGameRoleEntity> _gameRoleEntityRepository;

    public EmailTask(ILogger<EmailTask> logger, SqlSugarRepository<TenantGameMailEntity> repository, SqlSugarRepository<TenantGameAreaEntity> systemTenantGameAreaEntity, SqlSugarRepository<TenantGameRoleEntity> systemTenantGameRoleEntity, HttpClient httpClientFactory, SystemCacheService systemCacheService) : base(repository)
    {
        _gameAreaEntityRepository = systemTenantGameAreaEntity;
        _gameRoleEntityRepository = systemTenantGameRoleEntity;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _systemCacheService = systemCacheService;
    }

    /// <summary>
    ///  执行任务
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Task> ExecuteAsync(CancellationToken cancellationToken)
    {
        var id = (long)_systemCacheService.GetValue("MailId");
        if (id.IsNullOrEmpty() || id == 0)
        {
            return null;
        }

        _logger.LogInformation($"EmailTask 执行时间: {DateTime.Now}");

        // 查找要发送的邮件
        var gameMailEntity = await Repository.GetSingleAsync(u => u.Id == id && u.Status == GameMailStatus.Draft, cancellationToken);
        if (gameMailEntity == null)
        {
            return null;
        }

        var gameAreaEntity = await _gameAreaEntityRepository.GetSingleAsync(u => u.Id == gameMailEntity.AreaId, cancellationToken);

        ListAsync<long> playerIdArray;

        // 如果不是全区发的获取角色列表
        if (gameMailEntity.IsAllPost)
        {
            playerIdArray = (await _gameRoleEntityRepository.GetListAsync(u => gameMailEntity.AreaId == u.AreaId, cancellationToken)).SelectAsync(m => m.Id).ToList();
        }
        else
        {
            var playerIdList = JsonHelper.Deserialize<ListAsync<string>>(gameMailEntity.PlayerIdListJson);
            playerIdArray = (await _gameRoleEntityRepository.GetListAsync(u => gameMailEntity.AreaId == u.AreaId && playerIdList.Contains(u.Id.ToString()), cancellationToken)).SelectAsync(m => m.Id).ToList();
        }

        var expirationTimestamp = TimerHelper.DateTimeToSeconds(gameMailEntity.ExpirationTime.ToUniversalTime(), utc: true);
        var reqPostMailRequest = new PostMailRequest()
        {
            Roles = playerIdArray,
            Title = gameMailEntity.Title,
            SubTitle = gameMailEntity.SubTitle ?? "",
            Content = gameMailEntity.Body,
            PostType = gameMailEntity.IsAllPost,
            ExpirationTime = expirationTimestamp,
            MailType = gameMailEntity.MailType,
        };
        if (!gameMailEntity.AttachmentsJson.IsNullOrEmpty())
        {
            // 发送邮件
            var items = JsonHelper.Deserialize<ListAsync<GamePropItem>>(gameMailEntity.AttachmentsJson);
            reqPostMailRequest.Items = items;
            foreach (var gamePropItemEntity in reqPostMailRequest.Items)
            {
                gamePropItemEntity.Id = gamePropItemEntity.ItemId;
            }
        }

        var content = await _httpClientFactory.PostJsonToStringAsync(GameServerEndpointUrlBuilder.Build(gameAreaEntity, GameServerEndpointKindEnum.Fulfillment, "post_mail"), reqPostMailRequest, cancellationToken);
        var resultData = content.ToHttpJsonResult<PostMailResponse>();
        gameMailEntity.Status = GameMailStatus.Sent;
        _systemCacheService.Remove("MailId");
        await Repository.UpdateAsync(gameMailEntity, cancellationToken);
        // if (resultData.IsSuccess)
        // {
        //     gameMailEntity.Status = GameMailStatus.Sent;
        //     await Repository.AsSugarClient().Updateable(gameMailEntity).ExecuteCommandAsync(cancellationToken);
        //     // 销毁缓存
        //     _systemCacheService.Remove("MailId");
        // }
        // else
        // {
        //     _systemCacheService.Remove("MailId");
        //     _logger.LogError($"发送邮件失败: {resultData.Data.ErrorCode}");
        // }


        return Task.CompletedTask;
    }

    /// <summary>具体处理逻辑</summary>
    /// <param name="context">作业执行前上下文</param>
    /// <param name="stoppingToken">取消任务 Token</param>
    /// <returns><see cref="T:System.Threading.Tasks.Task" /></returns>
    public Task ExecuteAsync(JobExecutingContext context, CancellationToken stoppingToken)
    {
        return ExecuteAsync(stoppingToken);
    }


}*/

