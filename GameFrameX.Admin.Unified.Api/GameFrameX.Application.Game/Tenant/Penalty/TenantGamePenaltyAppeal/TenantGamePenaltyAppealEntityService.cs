using GameFrameX.Application.Game.Tenant.Penalty.TenantGamePenaltyAppeal.Dto;
using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Enum;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Tenant.Penalty.TenantGamePenaltyAppeal;

/// <summary>
/// 申诉事实源管理服务。路由前缀 /api/tenantGamePenaltyAppealEntity。
/// <para>AddAsync：校验关联处罚生效中 + 同一处罚不存在待审申诉；AppId 从处罚回填；服务端强制 Status=Pending。</para>
/// <para>ReviewAsync：通过→同步撤销关联处罚（写 RevokeAsync* + SourceAppealId 审计链）；驳回→写 Rejected。同一申诉不可重复 ReviewAsync。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGamePenaltyAppealEntityService
    : GenericCrudService<Entity.Tenant.Game.TenantGamePenaltyAppealEntity,
        AddTenantGamePenaltyAppealEntityRequest, UpdateTenantGamePenaltyAppealEntityRequest,
        TenantGamePenaltyAppealEntityResponse, PageTenantGamePenaltyAppealEntityRequest>
{
    private readonly CurrentUserContext _currentUserContext;
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGamePenaltyEntity> _penaltyRepository;

    public TenantGamePenaltyAppealEntityService(SqlSugarRepository<Entity.Tenant.Game.TenantGamePenaltyAppealEntity> repository,
        SqlSugarRepository<Entity.Tenant.Game.TenantGamePenaltyEntity> penaltyRepository,
        CurrentUserContext currentUserContext)
        : base(repository)
    {
        _currentUserContext = currentUserContext;
        _penaltyRepository = penaltyRepository;
    }

    /// <summary>
    /// 提交申诉：校验处罚生效中、同一处罚无待审申诉；AppId 跟随处罚；Status 服务端强制为 Pending。
    /// </summary>
    public override async Task<TenantGamePenaltyAppealEntityResponse> AddAsync(AddTenantGamePenaltyAppealEntityRequest input)
    {
        var penalty = await _penaltyRepository.GetFirstAsync(p => p.Id == input.PenaltyId);
        if (penalty == null)
        {
            throw BusinessError.Oh("关联处罚不存在");
        }

        if (penalty.Status != PenaltyStatusEnum.Active)
        {
            throw BusinessError.Oh("关联处罚不是生效状态，无法申诉");
        }

        AppScopeRules.EnsureFactAppId(penalty.AppId, "处罚申诉");

        var pendingExists = await Repository.IsAnyAsync(a => a.PenaltyId == input.PenaltyId
                                                             && a.Status == AppealStatusEnum.Pending);
        if (pendingExists)
        {
            throw BusinessError.Oh("该处罚已存在待审核申诉，请勿重复提交");
        }

        if (string.IsNullOrWhiteSpace(input.Reason))
        {
            throw BusinessError.Oh("申诉理由不能为空");
        }

        var entity = input.Adapt<Entity.Tenant.Game.TenantGamePenaltyAppealEntity>();
        entity.AppId = penalty.AppId;
        entity.Status = AppealStatusEnum.Pending;
        await Repository.InsertWithDiffLogAsync(entity);
        return entity.Adapt<TenantGamePenaltyAppealEntityResponse>();
    }

    /// <summary>
    /// 分页查询：按 AppId / PenaltyId / Status 过滤。
    /// </summary>
    protected override ISugarQueryable<TenantGamePenaltyAppealEntityResponse> BuildPageQuery(PageTenantGamePenaltyAppealEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "处罚申诉");
        return Repository.AsQueryable()
                         .Where(a => a.AppId == appId)
                         .WhereIF(input.PenaltyId.HasValue, a => a.PenaltyId == input.PenaltyId.Value)
                         .WhereIF(input.Status.HasValue, a => a.Status == input.Status.Value)
                         .Select<TenantGamePenaltyAppealEntityResponse>();
    }

    /// <summary>
    /// 审核申诉。仅 Pending 可审核。通过→撤销关联处罚并写 SourceAppealId 审计；驳回→Rejected。审核人/备注/时间服务端写。
    /// </summary>
    /// <param name="input"></param>
    [HttpPost("review")]
    [ApiDescriptionSettings(Name = "Review")]
    public async Task ReviewAsync(ReviewTenantGamePenaltyAppealEntityRequest input)
    {
        var appeal = await Repository.GetFirstAsync(a => a.Id == input.Id);
        if (appeal == null)
        {
            throw BusinessError.Oh("申诉不存在");
        }

        if (appeal.Status != AppealStatusEnum.Pending)
        {
            throw BusinessError.Oh("申诉已审核，不可重复审核");
        }

        var reviewerId = _currentUserContext.UserId;
        var reviewTime = TimerHelper.UnixTimeSeconds();
        var approvedStatus = input.Approve ? AppealStatusEnum.Approved : AppealStatusEnum.Rejected;

        appeal.Status = approvedStatus;
        appeal.ReviewerId = reviewerId;
        appeal.ReviewRemark = input.Remark;
        appeal.ReviewTime = reviewTime;
        await Repository.Context.Updateable(appeal)
                        .UpdateColumns(a => new { a.Status, a.ReviewerId, a.ReviewRemark, a.ReviewTime, })
                        .ExecuteCommandAsync();

        if (input.Approve)
        {
            var penalty = await _penaltyRepository.GetFirstAsync(p => p.Id == appeal.PenaltyId);
            if (penalty != null && penalty.Status == PenaltyStatusEnum.Active)
            {
                penalty.Status = PenaltyStatusEnum.Revoked;
                penalty.RevokeOperatorId = reviewerId;
                penalty.RevokeReason = "申诉通过";
                penalty.RevokeTime = reviewTime;
                penalty.SourceAppealId = appeal.Id;
                await _penaltyRepository.Context.Updateable(penalty)
                                        .UpdateColumns(p => new { p.Status, p.RevokeOperatorId, p.RevokeReason, p.RevokeTime, p.SourceAppealId, })
                                        .ExecuteCommandAsync();
            }
        }
    }
}
#pragma warning restore CS8602, CS8604, CS8629