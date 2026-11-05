using GameFrameX.Application.NoticeCenter.GameMail.Dto;
using GameFrameX.Application.ServerCenter;
using GameFrameX.Core.Enum;
using GameFrameX.Core.Job;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Entity.Enum;
using GameFrameX.Foundation.Http.Extension;
using GameFrameX.Foundation.Http.Normalization;
using GameFrameX.Foundation.Json;
using Microsoft.Extensions.Logging;

// 邮件定时任务实现（C404/GFX-1216 调度合一：解除自单仓整合以来的整体块注释，
// 并把执行体移植到当前 API 面——旧文引用的 ListAsync&lt;&gt; 等类型已随框架迁移退役）。
namespace GameFrameX.Application.Platform.Job;

/// <summary>
/// 邮件定时任务：按 SystemCacheService "MailId" 通道传入的邮件 Id，把草稿邮件经 Fulfillment
/// 通道投递到区服游戏服。由 <see cref="GameTimeJobDispatchJob"/> 按行业务定时任务 cron 到期触发。
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
    /// 执行任务：MailId 为空或 0 时直接返回（派发侧总会写入该键，0 表示无邮件载荷）。
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Task> ExecuteAsync(CancellationToken cancellationToken)
    {
        var cachedValue = _systemCacheService.GetValue("MailId");
        if (cachedValue == null || (long)cachedValue == 0)
        {
            return null;
        }

        var mailId = (long)cachedValue;
        _logger.LogInformation("EmailTask 执行时间: {ExecutionTime}, MailId: {MailId}", DateTime.Now, mailId);

        // 查找要发送的邮件（仅草稿态；非草稿视作已处理）
        var gameMailEntity = await Repository.GetSingleAsync(u => u.Id == mailId && u.Status == GameMailStatus.Draft, cancellationToken);
        if (gameMailEntity == null)
        {
            _systemCacheService.Remove("MailId");
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
        _systemCacheService.Remove("MailId");
        if (resultData.IsSuccess)
        {
            gameMailEntity.Status = GameMailStatus.Sent;
            await Repository.UpdateAsync(gameMailEntity, cancellationToken);
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
