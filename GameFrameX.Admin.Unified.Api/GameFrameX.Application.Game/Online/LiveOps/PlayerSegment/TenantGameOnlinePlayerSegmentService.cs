// GameFrameX 组织下的以及组织衍生的项目的版权、商标和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯用户合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.ComponentModel;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Online.LiveOps.Dispatch;
using GameFrameX.Application.Game.Online.LiveOps.PlayerSegment.Dto;
using GameFrameX.Application.Game.Online.RolloutSemanticMap;
using GameFrameX.Entity.Game.Online;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Entity.Tenant.Game.Online;
using GameFrameX.Foundation.Json;

#pragma warning disable CS8602, CS8604 // WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim() 实际安全
namespace GameFrameX.Application.Game.Online.LiveOps.PlayerSegment;

/// <summary>
/// 玩家分群管理服务（来源 change C343 / vault:C8 S7.5）。路由前缀 /api/tenantGameOnlinePlayerSegment（去后缀 + lowerCamelCase）。
/// <para>Admin 只编排分群定义（名称 + 条件白名单校验），不解释、不复算命中（语义解释在 Online 侧，对照见 <c>OnlineRolloutSemanticMap</c>）；
/// 定义生效须经 <c>syncOnline</c> 经统一下发入口同步 Online（Online 分配分群标识回写 LastOnlineSegmentId）；
/// 编辑定义不会自动重同步，须再次 syncOnline（显式生效，避免隐式半生效状态）。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameOnlinePlayerSegmentService
    : GenericCrudService<TenantGameOnlinePlayerSegmentEntity,
        AddTenantGameOnlinePlayerSegmentRequest, UpdateTenantGameOnlinePlayerSegmentRequest,
        TenantGameOnlinePlayerSegmentResponse, PageTenantGameOnlinePlayerSegmentRequest>
{
    private readonly SqlSugarRepository<TenantGameAppEntity> _gameAppRepository;
    private readonly SqlSugarRepository<TenantGameOnlineConfigRolloutEntity> _rolloutRepository;
    private readonly IOnlineLiveOpsDispatchService _dispatchService;

    /// <summary>
    ///
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="gameAppRepository"></param>
    /// <param name="rolloutRepository"></param>
    /// <param name="dispatchService"></param>
    public TenantGameOnlinePlayerSegmentService(SqlSugarRepository<TenantGameOnlinePlayerSegmentEntity> repository,
        SqlSugarRepository<TenantGameAppEntity> gameAppRepository,
        SqlSugarRepository<TenantGameOnlineConfigRolloutEntity> rolloutRepository,
        IOnlineLiveOpsDispatchService dispatchService)
        : base(repository)
    {
        _gameAppRepository = gameAppRepository;
        _rolloutRepository = rolloutRepository;
        _dispatchService = dispatchService;
    }

    /// <summary>
    /// 分页查询：按 AppId（必填事实作用域）/ Name / SyncStatus 过滤。
    /// </summary>
    protected override ISugarQueryable<TenantGameOnlinePlayerSegmentResponse> BuildPageQuery(PageTenantGameOnlinePlayerSegmentRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "玩家分群");
        return Repository.AsQueryable()
                         .Where(u => u.AppId == appId)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.Name), u => u.Name.Contains(input.Name.Trim()))
                         .WhereIF(input.SyncStatus.HasValue, u => u.SyncStatus == input.SyncStatus.Value)
                         .Select<TenantGameOnlinePlayerSegmentResponse>();
    }

    /// <summary>
    /// 新增前：事实作用域 AppId + 应用归属校验 + 条件白名单校验（初始 NotSynced，须 syncOnline 后才可被灰度引用）。
    /// </summary>
    protected override async Task OnAddingAsync(AddTenantGameOnlinePlayerSegmentRequest input)
    {
        AppScopeRules.EnsureFactAppId(input.AppId, "玩家分群");
        await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(input.AppId);
        ValidateConditions(input.ConditionsJson);
    }

    /// <summary>
    /// 更新前：AppId 与同步状态机字段从既有实体回填不可变（AppId 不可改、同步回执不可经编辑清除）。
    /// </summary>
    protected override async Task OnUpdatingAsync(UpdateTenantGameOnlinePlayerSegmentRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        input.AppId = existing.AppId;
        // 状态机字段回填：编辑只改定义，同步状态/回执保持原值（编辑后须重新 syncOnline 生效）
        input.SyncStatus = existing.SyncStatus;
        input.LastSyncTime = existing.LastSyncTime;
        input.LastSyncServerId = existing.LastSyncServerId;
        input.LastOnlineSegmentId = existing.LastOnlineSegmentId;
        input.LastServerCode = existing.LastServerCode;
        input.LastServerMessage = existing.LastServerMessage;
        ValidateConditions(input.ConditionsJson);
    }

    /// <summary>
    /// 删除：已被灰度版本行引用（任一状态）的分群不可删除——版本行是不可变审计历史，引用悬空会破坏回溯。
    /// </summary>
    [DisplayName("删除玩家分群")]
    public override async Task DeleteAsync(BaseDeleteRequest input)
    {
        var existing = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (existing == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        var referenced = await _rolloutRepository.GetFirstAsync(u => u.SegmentId == input.Id);
        if (referenced != null)
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed,
                new { message = "分群已被灰度版本行引用，不可删除（版本行为不可变审计历史）", });
        }

        await base.DeleteAsync(input);
    }

    /// <summary>
    /// 分群同步 Online：定义快照经统一下发入口（syncPlayerSegment 通道）送达 Online，Online 分配分群标识回写本行。
    /// <para>成功置 Synced 并回写回执；失败（含 Online 非 0 协议码镜像异常）置 Failed 落库可见后原样抛出，可重试。</para>
    /// </summary>
    [HttpPost("syncOnline")]
    [ApiDescriptionSettings(Name = "SyncOnline")]
    [DisplayName("同步分群到Online")]
    public async Task<SyncPlayerSegmentOnlineResponse> SyncOnlineAsync(SyncPlayerSegmentOnlineRequest input)
    {
        var entity = await Repository.GetFirstAsync(u => u.Id == input.Id);
        if (entity == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound);
        }

        if (string.IsNullOrWhiteSpace(input.Reason))
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { reason = input.Reason, });
        }

        try
        {
            var result = await _dispatchService.SyncSegmentToOnlineAsync(entity, input.ServerId, input.Reason);

            entity.SyncStatus = OnlinePlayerSegmentSyncStatus.Synced;
            entity.LastSyncTime = result.SyncedTime;
            entity.LastSyncServerId = input.ServerId;
            if (result.OnlineSegmentId > 0)
            {
                // Online 侧分配的分群标识；0 = 本次未返回（幂等重同步可能不重复分配），保留既有值
                entity.LastOnlineSegmentId = result.OnlineSegmentId;
            }

            entity.LastServerCode = 0;
            entity.LastServerMessage = result.Message;
            await Repository.Context.Updateable(entity)
                            .UpdateColumns(p => new
                            {
                                p.SyncStatus, p.LastSyncTime, p.LastSyncServerId, p.LastOnlineSegmentId,
                                p.LastServerCode, p.LastServerMessage,
                            })
                            .ExecuteCommandAsync();

            return new SyncPlayerSegmentOnlineResponse
            {
                Id = entity.Id,
                SyncStatus = entity.SyncStatus,
                OnlineSegmentId = entity.LastOnlineSegmentId,
                ConfirmVersion = result.ConfirmVersion,
                Message = result.Message,
                SyncedTime = result.SyncedTime,
            };
        }
        catch (BusinessException ex)
        {
            // 失败态落库（列表可见、可重试）后原样抛出，与 MailCampaign 发布失败同范式
            entity.SyncStatus = OnlinePlayerSegmentSyncStatus.Failed;
            entity.LastSyncTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            entity.LastSyncServerId = input.ServerId;
            entity.LastServerCode = ex.ErrorCode != null ? Convert.ToInt32(ex.ErrorCode) : 0;
            entity.LastServerMessage = ex.Message;
            await Repository.Context.Updateable(entity)
                            .UpdateColumns(p => new
                            {
                                p.SyncStatus, p.LastSyncTime, p.LastSyncServerId, p.LastServerCode, p.LastServerMessage,
                            })
                            .ExecuteCommandAsync();
            throw;
        }
    }

    /// <summary>
    /// 条件白名单校验：JSON 可解析、Field / Op 在 <c>OnlineRolloutSemanticMap</c> 白名单内、Value 非空。
    /// Admin 只做结构性校验，不解释字段语义（语义解释在 Online 侧）。
    /// </summary>
    private static void ValidateConditions(string? conditionsJson)
    {
        if (string.IsNullOrWhiteSpace(conditionsJson))
        {
            return;
        }

        List<OnlineSegmentCondition> conditions;
        try
        {
            conditions = JsonHelper.Deserialize<List<OnlineSegmentCondition>>(conditionsJson);
        }
        catch (Exception ex)
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { message = "分群条件不是合法的 JSON 数组：" + ex.Message, });
        }

        foreach (var condition in conditions)
        {
            if (!OnlineRolloutSemanticMap.SegmentConditionFields.Contains(condition.Field))
            {
                throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed,
                    new { field = condition.Field, allowed = string.Join("/", OnlineRolloutSemanticMap.SegmentConditionFields), });
            }

            if (!OnlineRolloutSemanticMap.SegmentConditionOperators.Contains(condition.Op))
            {
                throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed,
                    new { op = condition.Op, allowed = string.Join("/", OnlineRolloutSemanticMap.SegmentConditionOperators), });
            }

            if (string.IsNullOrWhiteSpace(condition.Value))
            {
                throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { field = condition.Field, message = "条件值不能为空", });
            }
        }
    }
}
#pragma warning restore CS8602, CS8604
