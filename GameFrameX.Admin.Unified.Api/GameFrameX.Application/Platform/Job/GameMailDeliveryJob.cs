using GameFrameX.Application.NoticeCenter.GameMail.Dto;
using GameFrameX.Application.ServerCenter;
using GameFrameX.Core.Job;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Foundation.Http.Extension;
using GameFrameX.Foundation.Http.Normalization;
using GameFrameX.Foundation.Json;
using Microsoft.Extensions.Logging;

// 游戏内邮件定时投递任务（调度合一：解除自单仓整合以来的整体块注释，并把执行体移植到当前 API 面）。
// 注意：本类不是 Admin API 控制器（不继承 BaseService），经 Startup 显式注册进 DI，
// 由 GameTimeJobDispatchJob 按业务定时任务 cron 到期触发——继承 BaseService 会被动态 API
// 发现成控制器并撞上 AdminActionStyle / EndpointGolden / ParameterShape 守护测试。
// 命名规范化（2026-11）：原类名 EmailTask 与平台真邮件系统（Platform/Email、GameFrameX.Email.Smtp/TencentSes）
// 撞词根且名不副实——本任务投递的是游戏内邮件（GameMailEntity → Fulfillment post_mail），按 GameMail 域改名。
namespace GameFrameX.Application.Platform.Job;

/// <summary>
/// 游戏内邮件定时投递任务：按 <see cref="GameTimeJobDispatchJob.MailIdCacheKey"/> 缓存通道传入的邮件 Id，
/// 把草稿邮件经 Fulfillment 通道投递到区服游戏服。
/// </summary>
public class GameMailDeliveryJob : IScheduleJob
{
    private readonly ILogger<GameMailDeliveryJob> _logger;
    private readonly HttpClient _httpClientFactory;
    private readonly SystemCacheService _systemCacheService;
    private readonly SqlSugarRepository<GameMailEntity> _mailRepository;
    private readonly SqlSugarRepository<GameAreaEntity> _gameAreaEntityRepository;
    private readonly SqlSugarRepository<GameRoleEntity> _gameRoleEntityRepository;

    public GameMailDeliveryJob(ILogger<GameMailDeliveryJob> logger, SqlSugarRepository<GameMailEntity> mailRepository, SqlSugarRepository<GameAreaEntity> systemGameAreaEntity, SqlSugarRepository<GameRoleEntity> systemGameRoleEntity, HttpClient httpClientFactory, SystemCacheService systemCacheService)
    {
        _mailRepository = mailRepository;
        _gameAreaEntityRepository = systemGameAreaEntity;
        _gameRoleEntityRepository = systemGameRoleEntity;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _systemCacheService = systemCacheService;
    }

    /// <summary>
    /// 执行任务：MailId 为空或 0 时直接返回（派发侧总会写入该键，0 表示无邮件载荷）。
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Task> ExecuteAsync(CancellationToken cancellationToken)
    {
        var cachedValue = _systemCacheService.GetValue(GameTimeJobDispatchJob.MailIdCacheKey);
        if (cachedValue == null || (long)cachedValue == 0)
        {
            return null;
        }

        var mailId = (long)cachedValue;
        _logger.LogInformation("GameMailDeliveryJob 执行时间: {ExecutionTime}, MailId: {MailId}", DateTime.Now, mailId);

        // 查找要发送的邮件（仅草稿态；非草稿视作已处理）
        var gameMailEntity = await _mailRepository.GetSingleAsync(u => u.Id == mailId && u.Status == GameMailStatus.Draft, cancellationToken);
        if (gameMailEntity == null)
        {
            _systemCacheService.Remove(GameTimeJobDispatchJob.MailIdCacheKey);
            return null;
        }

        var gameAreaEntity = await _gameAreaEntityRepository.GetSingleAsync(u => u.Id == gameMailEntity.AreaId, cancellationToken);

        List<long> playerIdArray;

        // 如果不是全区发的获取角色列表
        if (gameMailEntity.IsAllPost)
        {
            playerIdArray = _gameRoleEntityRepository.AsQueryable()
                                                    .Where(u => gameMailEntity.AreaId == u.AreaId)
                                                    .ToList(m => m.Id);
        }
        else
        {
            var playerIdList = JsonHelper.Deserialize<List<string>>(gameMailEntity.PlayerIdListJson);
            playerIdArray = _gameRoleEntityRepository.AsQueryable()
                                                    .Where(u => gameMailEntity.AreaId == u.AreaId && playerIdList.Contains(u.Id.ToString()))
                                                    .ToList(m => m.Id);
        }

        var reqPostMailRequest = new PostMailRequest
        {
            Roles = playerIdArray,
            Title = gameMailEntity.Title,
            SubTitle = gameMailEntity.SubTitle ?? "",
            Content = gameMailEntity.Body,
            PostType = gameMailEntity.IsAllPost,
            ExpirationTime = gameMailEntity.ExpirationTime,
            MailType = gameMailEntity.MailType,
        };
        if (!gameMailEntity.AttachmentsJson.IsNullOrEmpty())
        {
            // 发送邮件
            var items = JsonHelper.Deserialize<List<GamePropItem>>(gameMailEntity.AttachmentsJson);
            reqPostMailRequest.Items = items;
            foreach (var gamePropItemEntity in reqPostMailRequest.Items)
            {
                gamePropItemEntity.Id = gamePropItemEntity.ItemId;
            }
        }

        var content = await _httpClientFactory.PostJsonToStringAsync(GameServerEndpointUrlBuilder.Build(gameAreaEntity, GameServerEndpointKindEnum.Fulfillment, "post_mail"), reqPostMailRequest, cancellationToken);
        var resultData = content.ToHttpJsonResult<PostMailResponse>();
        _systemCacheService.Remove(GameTimeJobDispatchJob.MailIdCacheKey);
        if (resultData.IsSuccess)
        {
            gameMailEntity.Status = GameMailStatus.Sent;
            await _mailRepository.UpdateAsync(gameMailEntity, cancellationToken);
        }
        else
        {
            _logger.LogError("发送邮件失败: {Message}", resultData.Message);
        }

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
}
