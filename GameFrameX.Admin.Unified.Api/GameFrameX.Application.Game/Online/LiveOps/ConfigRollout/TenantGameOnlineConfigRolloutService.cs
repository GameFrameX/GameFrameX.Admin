// GameFrameX 组织下的以及组织衍生的项目的版权、商标和其他相关权利均受相应法律法规的保护。使用本项目应遵守相应法律法规和许可证的要求。
//
// 本项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。许可证位于源代码树根目录中的 LICENSE 文件。
//
// 不得利用本项目从事危害国家安全、扰乱社会秩序、侵犯用户合法权益等法律法规禁止的活动！任何基于本项目二次开发而产生的一切法律纠纷和责任，我们不承担任何责任！

using System.ComponentModel;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Online.LiveOps.ConfigRollout.Dto;
using GameFrameX.Application.Game.Online.LiveOps.Dispatch;
using GameFrameX.Application.Game.Online.RolloutSemanticMap;
using GameFrameX.Core.Enum.Errors;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Game.Online;
using GameFrameX.Entity.Tenant.Game.Online;

namespace GameFrameX.Application.Game.Online.LiveOps.ConfigRollout;

/// <summary>
/// 配置灰度发布（Rollout）版本行管理服务（来源 change C343 / vault:C8 S7.7-S7.8）。路由前缀 /api/tenantGameOnlineConfigRollout（去后缀 + lowerCamelCase）。
/// <para>版本化铁律：版本行只由 publish / rollback 两个动作追加，历史行不可修改不可删除（无 add/update/delete action）；
/// <c>ConfigVersion</c> 按 AppId + ConfigKind + ConfigKey 维度递增分配（发布与回滚共用序列）；
/// 回滚 = 版本化新下发（新行 + TargetVersion 指向恢复目标，语义对照见 <c>OnlineRolloutSemanticMap</c>），
/// 生效/被替代由 Status 表达（Published=生效中，RolledBack=已被替代的历史版本）。</para>
/// <para>下发经 <see cref="IOnlineLiveOpsDispatchService"/> 单一入口（publishConfigRollout / rollbackConfigRollout 通道）；
/// Admin 只编排不解释（分群命中与百分比桶展开在 Online 侧）。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameOnlineConfigRolloutService : IAdminApiController
{
    private readonly SqlSugarRepository<TenantGameOnlineConfigRolloutEntity> _repository;
    private readonly SqlSugarRepository<TenantGameOnlinePlayerSegmentEntity> _segmentRepository;
    private readonly IOnlineLiveOpsDispatchService _dispatchService;
    private readonly CurrentUserContext _currentUserContext;

    /// <summary>
    ///
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="segmentRepository"></param>
    /// <param name="dispatchService"></param>
    /// <param name="currentUserContext"></param>
    public TenantGameOnlineConfigRolloutService(SqlSugarRepository<TenantGameOnlineConfigRolloutEntity> repository,
        SqlSugarRepository<TenantGameOnlinePlayerSegmentEntity> segmentRepository,
        IOnlineLiveOpsDispatchService dispatchService,
        CurrentUserContext currentUserContext)
    {
        _repository = repository;
        _segmentRepository = segmentRepository;
        _dispatchService = dispatchService;
        _currentUserContext = currentUserContext;
    }

    /// <summary>
    /// 分页查询版本行：按 AppId（必填事实作用域）/ ConfigKind / ConfigKey / Status / ServerId 过滤，新版本在前。
    /// </summary>
    [HttpPost(CrudActionRoutes.Page)]
    [ApiDescriptionSettings(Name = "Page")]
    [DisplayName("分页查询灰度版本行")]
    public async Task<SqlSugarPagedList<TenantGameOnlineConfigRolloutResponse>> PageAsync(PageTenantGameOnlineConfigRolloutRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "配置灰度发布");
        return await _repository.AsQueryable()
                         .Where(u => u.AppId == appId)
                         .WhereIF(input.ConfigKind.HasValue, u => u.ConfigKind == input.ConfigKind.Value)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.ConfigKey), u => u.ConfigKey == input.ConfigKey.Trim())
                         .WhereIF(input.Status.HasValue, u => u.Status == input.Status.Value)
                         .WhereIF(input.ServerId.HasValue, u => u.ServerId == input.ServerId.Value)
                         .OrderByDescending(u => u.Id)
                         .Select<TenantGameOnlineConfigRolloutResponse>()
                         .ToPagedListAsync(input.Page, input.PageSize);
    }

    /// <summary>
    /// 灰度发布：校验（Percent 边界 / 来源存在归属 / 分群已同步）→ 分配 ConfigVersion → 落版本行 → 经单一入口下发 Online → 回写确认版本并替代旧生效行。
    /// <para>下发失败（Online 非 0 协议码 / 作用域拒绝）时删除刚插入的版本行后原样抛出——Online 未确认即无生效事实，不留半生效审计行，可整重试。</para>
    /// </summary>
    [HttpPost("publish")]
    [ApiDescriptionSettings(Name = "Publish")]
    [DisplayName("发布配置灰度版本")]
    public async Task<PublishConfigRolloutResponse> PublishAsync(PublishConfigRolloutRequest input)
    {
        EnsurePublishPercent(input.Percent);
        await EnsureConfigSourceAsync(input.ConfigKind, input.ConfigKey, input.AppId);
        var segment = await EnsureSegmentSyncedAsync(input.SegmentId, input.AppId);

        var entity = new TenantGameOnlineConfigRolloutEntity
        {
            AppId = input.AppId,
            ServerId = input.ServerId,
            ConfigKind = input.ConfigKind,
            ConfigKey = input.ConfigKey.Trim(),
            ConfigVersion = await AllocateConfigVersionAsync(input.AppId, input.ConfigKind, input.ConfigKey.Trim()),
            SegmentId = segment.Id,
            Percent = input.Percent,
            Status = OnlineConfigRolloutStatus.Published,
            TargetVersion = 0,
            Reason = input.Reason.Trim(),
            DispatchOperator = GetCurrentOperator(),
            PublishedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };
        await _repository.InsertWithDiffLogAsync(entity);

        try
        {
            var result = await _dispatchService.PublishRolloutToOnlineAsync(entity);
            await ConfirmDispatchedAsync(entity, result.ConfirmVersion);
        }
        catch (BusinessException)
        {
            // Online 未确认：无生效事实，删除未确认版本行后原样抛出（版本号可复用，整动作可重试）
            await _repository.DeleteByIdAsync(entity.Id);
            throw;
        }

        return await GetRolloutResponseAsync<PublishConfigRolloutResponse>(entity.Id);
    }

    /// <summary>
    /// 灰度回滚：版本化新下发——目标版本语义（SegmentId/Percent）恢复到新行，新行 TargetVersion 指向目标，经单一入口 rollbackConfigRollout 通道生效。
    /// <para>目标版本必须存在于该配置的版本序列（无论当前是否生效——恢复任一历史版本行为均合法）；成功后旧生效行置 RolledBack。</para>
    /// </summary>
    [HttpPost("rollback")]
    [ApiDescriptionSettings(Name = "Rollback")]
    [DisplayName("回滚配置灰度版本")]
    public async Task<RollbackConfigRolloutResponse> RollbackAsync(RollbackConfigRolloutRequest input)
    {
        await EnsureConfigSourceAsync(input.ConfigKind, input.ConfigKey, input.AppId);

        var configKey = input.ConfigKey.Trim();
        var target = await _repository.GetFirstAsync(u => u.AppId == input.AppId
                                                       && u.ConfigKind == input.ConfigKind
                                                       && u.ConfigKey == configKey
                                                       && u.ConfigVersion == input.TargetVersion);
        if (target == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound,
                new { input.ConfigKind, configKey, input.TargetVersion, message = "回滚目标版本不存在", });
        }

        var entity = new TenantGameOnlineConfigRolloutEntity
        {
            AppId = input.AppId,
            ServerId = input.ServerId,
            ConfigKind = input.ConfigKind,
            ConfigKey = configKey,
            ConfigVersion = await AllocateConfigVersionAsync(input.AppId, input.ConfigKind, configKey),
            SegmentId = target.SegmentId,
            Percent = target.Percent,
            Status = OnlineConfigRolloutStatus.Published,
            TargetVersion = input.TargetVersion,
            Reason = input.Reason.Trim(),
            DispatchOperator = GetCurrentOperator(),
            PublishedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };
        await _repository.InsertWithDiffLogAsync(entity);

        try
        {
            var result = await _dispatchService.RollbackRolloutToOnlineAsync(entity);
            await ConfirmDispatchedAsync(entity, result.ConfirmVersion);
        }
        catch (BusinessException)
        {
            await _repository.DeleteByIdAsync(entity.Id);
            throw;
        }

        return await GetRolloutResponseAsync<RollbackConfigRolloutResponse>(entity.Id);
    }

    /// <summary>
    /// Percent 边界校验（边界常量唯一来源 <c>OnlineRolloutSemanticMap</c>；0=无意义拒绝、101=越界拒绝、1/100=合法收）。
    /// </summary>
    private static void EnsurePublishPercent(int percent)
    {
        if (percent < OnlineRolloutSemanticMap.PublishPercentMin || percent > OnlineRolloutSemanticMap.PublishPercentMax)
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed,
                new
                {
                    percent,
                    min = OnlineRolloutSemanticMap.PublishPercentMin,
                    max = OnlineRolloutSemanticMap.PublishPercentMax,
                });
        }
    }

    /// <summary>
    /// 配置来源校验：ConfigKey 必须是来源实体 Id 数字串，且来源存在并归属目标 App（校验收口在统一下发入口，此处转调）。
    /// </summary>
    private async Task EnsureConfigSourceAsync(OnlineConfigRolloutKind kind, string configKey, long appId)
    {
        if (!Enum.IsDefined(typeof(OnlineConfigRolloutKind), kind))
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { kind, });
        }

        if (!long.TryParse(configKey.Trim(), out var businessKey) || businessKey <= 0)
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed,
                new { configKey, message = "配置业务键必须是所辖配置域实体 Id（正整数）", });
        }

        await _dispatchService.EnsureSourceExistsAsync(MapToLiveOpsSource(kind), businessKey, appId);
    }

    /// <summary>
    /// 分群校验：必须存在、归属目标 App 且已同步 Online（未同步分群 Online 无法解释命中）。
    /// </summary>
    private async Task<TenantGameOnlinePlayerSegmentEntity> EnsureSegmentSyncedAsync(long segmentId, long appId)
    {
        var segment = await _segmentRepository.GetFirstAsync(u => u.Id == segmentId);
        if (segment == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound, new { segmentId, });
        }

        if (segment.AppId != appId)
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed,
                new { segmentId, appId, message = "分群不属于目标应用", });
        }

        if (segment.SyncStatus != OnlinePlayerSegmentSyncStatus.Synced)
        {
            throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed,
                new { segmentId, syncStatus = segment.SyncStatus.ToString(), message = "分群未同步 Online，请先 syncOnline 再发布灰度", });
        }

        return segment;
    }

    /// <summary>
    /// 分配下一个 ConfigVersion（AppId + ConfigKind + ConfigKey 维度递增；发布与回滚共用序列。
    /// 当前实现为 max+1 非原子自增，极端并发下可能同号——灰度发布为低频运营操作且入口经操作人确认，残余风险记录于 change review）。
    /// </summary>
    private async Task<int> AllocateConfigVersionAsync(long appId, OnlineConfigRolloutKind kind, string configKey)
    {
        var maxVersion = await _repository.AsQueryable()
            .Where(u => u.AppId == appId && u.ConfigKind == kind && u.ConfigKey == configKey)
            .MaxAsync(u => u.ConfigVersion);
        return maxVersion + 1;
    }

    /// <summary>
    /// 下发确认收尾：回写 Online 确认版本 + 将同配置其余生效行（Published）置 RolledBack（新版本替代，历史行不可变仅状态流转）。
    /// </summary>
    private async Task ConfirmDispatchedAsync(TenantGameOnlineConfigRolloutEntity entity, string? confirmVersion)
    {
        entity.OnlineConfirmVersion = confirmVersion;
        await _repository.Context.Updateable(entity)
            .UpdateColumns(p => new { p.OnlineConfirmVersion, })
            .ExecuteCommandAsync();

        await _repository.Context.Updateable<TenantGameOnlineConfigRolloutEntity>()
            .SetColumns(p => p.Status == OnlineConfigRolloutStatus.RolledBack)
            .Where(u => u.AppId == entity.AppId
                     && u.ConfigKind == entity.ConfigKind
                     && u.ConfigKey == entity.ConfigKey
                     && u.Status == OnlineConfigRolloutStatus.Published
                     && u.Id != entity.Id)
            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 按主键回读版本行映射为响应。
    /// </summary>
    private async Task<TResponse> GetRolloutResponseAsync<TResponse>(long id) where TResponse : class
    {
        var response = await _repository.AsQueryable()
            .Where(u => u.Id == id)
            .Select<TResponse>()
            .FirstAsync();
        return response;
    }

    /// <summary>
    /// 配置族 → LiveOps 来源（OnlineConfigRolloutKind 与 LiveOpsSourceKind 的四类配置成员一一对应；邮件在两个枚举语义中均不参与灰度编排）。
    /// </summary>
    private static LiveOpsSourceKind MapToLiveOpsSource(OnlineConfigRolloutKind kind)
    {
        switch (kind)
        {
            case OnlineConfigRolloutKind.RemoteConfig:
                return LiveOpsSourceKind.RemoteConfig;
            case OnlineConfigRolloutKind.Announcement:
                return LiveOpsSourceKind.Announcement;
            case OnlineConfigRolloutKind.DeviceGroup:
                return LiveOpsSourceKind.DeviceGroup;
            case OnlineConfigRolloutKind.ScheduledTask:
                return LiveOpsSourceKind.ScheduledTask;
            default:
                throw BusinessError.Oh(CommonErrorCode.ParameterValidationFailed, new { kind, });
        }
    }

    /// <summary>
    /// 当前操作人显示名（RealName 优先，回退 Account）。
    /// </summary>
    private string GetCurrentOperator()
    {
        var realName = _currentUserContext.RealName;
        if (!string.IsNullOrWhiteSpace(realName))
        {
            return realName;
        }

        return _currentUserContext.Account ?? $"user_{_currentUserContext.UserId}";
    }
}
