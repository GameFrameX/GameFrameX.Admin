using System.ComponentModel;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Online.Social;
using GameFrameX.Application.Game.Online.Social.Dto;
using GameFrameX.Application.Game.Tenant.Penalty.TenantGamePenalty.Dto;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Enum;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Tenant.Penalty.TenantGamePenalty;

/// <summary>
/// 处罚事实源管理服务。路由前缀 /api/tenantGamePenaltyEntity（历史框架 去后缀 + lowerCamelCase）。
/// <para>AddAsync：服务端强制 Status=Active、OperatorId=当前操作人，忽略请求体审计字段（防越权）。</para>
/// <para>RevokeAsync：撤销生效中处罚，写 RevokeOperatorId / RevokeReason / RevokeTime 审计。</para>
/// <para>SyncOnlineRuntimeAsync：把生效中处罚同步到 Online 运行时（经 OnlinePenaltyLinkCommandService 唯一联动入口，change C350）。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGamePenaltyEntityService
    : GenericCrudService<Entity.Tenant.Game.TenantGamePenaltyEntity,
        AddTenantGamePenaltyEntityRequest, UpdateTenantGamePenaltyEntityRequest,
        TenantGamePenaltyEntityResponse, PageTenantGamePenaltyEntityRequest>
{
    private readonly CurrentUserContext _currentUserContext;
    private readonly OnlinePenaltyLinkCommandService _penaltyLinkCommandService;

    public TenantGamePenaltyEntityService(SqlSugarRepository<Entity.Tenant.Game.TenantGamePenaltyEntity> repository,
        CurrentUserContext currentUserContext,
        OnlinePenaltyLinkCommandService penaltyLinkCommandService)
        : base(repository)
    {
        _currentUserContext = currentUserContext;
        _penaltyLinkCommandService = penaltyLinkCommandService;
    }

    /// <summary>
    /// 新增处罚。AppId 必填且必须大于 0；审计字段服务端强制。
    /// </summary>
    public override async Task<TenantGamePenaltyEntityResponse> AddAsync(AddTenantGamePenaltyEntityRequest input)
    {
        AppScopeRules.EnsureFactAppId(input.AppId, "处罚");
        if (string.IsNullOrWhiteSpace(input.TargetId))
        {
            throw BusinessError.Oh("对象标识不能为空");
        }

        if (string.IsNullOrWhiteSpace(input.Reason))
        {
            throw BusinessError.Oh("处罚原因不能为空");
        }

        var entity = input.Adapt<Entity.Tenant.Game.TenantGamePenaltyEntity>();
        entity.Status = PenaltyStatusEnum.Active;
        entity.OperatorId = _currentUserContext.UserId;
        await Repository.InsertWithDiffLogAsync(entity);
        return entity.Adapt<TenantGamePenaltyEntityResponse>();
    }

    /// <summary>
    /// 分页查询：按 AppId / TargetType / TargetId / Status / ActionType 过滤。
    /// </summary>
    protected override ISugarQueryable<TenantGamePenaltyEntityResponse> BuildPageQuery(PageTenantGamePenaltyEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "处罚");
        return Repository.AsQueryable()
                         .Where(p => p.AppId == appId)
                         .WhereIF(input.TargetType.HasValue, p => p.TargetType == input.TargetType.Value)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.TargetId), p => p.TargetId == input.TargetId)
                         .WhereIF(input.Status.HasValue, p => p.Status == input.Status.Value)
                         .WhereIF(input.ActionType.HasValue, p => p.ActionType == input.ActionType.Value)
                         .Select<TenantGamePenaltyEntityResponse>();
    }

    /// <summary>
    /// 撤销生效中处罚。仅 Active 可撤销；写撤销审计三元组。Enforcement 直读库无缓存，撤销后即时生效。
    /// </summary>
    /// <param name="input"></param>
    [HttpPost("revoke")]
    [ApiDescriptionSettings(Name = "Revoke")]
    public async Task RevokeAsync(RevokeTenantGamePenaltyEntityRequest input)
    {
        var entity = await Repository.GetFirstAsync(p => p.Id == input.Id);
        if (entity == null)
        {
            throw BusinessError.Oh("处罚不存在");
        }

        if (entity.Status != PenaltyStatusEnum.Active)
        {
            throw BusinessError.Oh("处罚不是生效状态，无法撤销");
        }

        entity.Status = PenaltyStatusEnum.Revoked;
        entity.RevokeOperatorId = _currentUserContext.UserId;
        entity.RevokeReason = input.Reason;
        entity.RevokeTime = TimerHelper.UnixTimeSeconds();
        await Repository.Context.Updateable(entity)
                        .UpdateColumns(p => new { p.Status, p.RevokeOperatorId, p.RevokeReason, p.RevokeTime, })
                        .ExecuteCommandAsync();
    }

    /// <summary>
    /// 把生效中处罚同步到 Online 运行时（手动补发联动；Add/Revoke 语义不变）。联动命令按 ActionType 服务端推导，
    /// 经 <see cref="OnlinePenaltyLinkCommandService"/> 唯一入口下发——事实源先行、联动可重试（同 IdempotencyKey 重试幂等）。
    /// </summary>
    /// <param name="input">处罚 Id（必须生效中）。</param>
    /// <returns>各联动目标区服的聚合执行结果。</returns>
    [HttpPost("syncOnlineRuntime")]
    [ApiDescriptionSettings(Name = "SyncOnlineRuntime")]
    [DisplayName("同步Online运行时")]
    public async Task<OnlinePenaltyLinkResponse> SyncOnlineRuntimeAsync(SyncOnlineRuntimeTenantGamePenaltyRequest input)
    {
        var command = await MapActionTypeToLinkCommand(input.Id);
        return await _penaltyLinkCommandService.ExecuteLinkAsync(input.Id, command);
    }

    /// <summary>
    /// 处罚动作 → 联动命令推导：Ban / RestrictLogin（登录阻断集合）→ Ban（吊销 Token）；Mute → Mute（禁言）。
    /// <para>调用前必经 ExecuteLinkAsync 的 ActionType 白名单校验——RestrictTrade / RestrictMatch 在那里抛
    /// PenaltyActionNotLinkable（纯落账动作无 Online 运行时对应物）；本方法取到的事实行经同一校验，不会返回未定义命令。</para>
    /// </summary>
    /// <param name="penaltyId">处罚 Id（取事实行 ActionType 用）。</param>
    /// <returns>联动命令（Ban / Mute）。</returns>
    private async Task<OnlinePenaltyCommandKind> MapActionTypeToLinkCommand(long penaltyId)
    {
        var entity = await Repository.GetFirstAsync(p => p.Id == penaltyId);
        if (entity == null)
        {
            throw BusinessError.Oh(CommonErrorCode.RecordNotFound, new { penaltyId, });
        }

        return entity.ActionType == PenaltyActionTypeEnum.Ban || entity.ActionType == PenaltyActionTypeEnum.RestrictLogin
            ? OnlinePenaltyCommandKind.Ban
            : OnlinePenaltyCommandKind.Mute;
    }
}
#pragma warning restore CS8602, CS8604, CS8629