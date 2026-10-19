using GameFrameX.Application.Game.App.AppScope;
using GameFrameX.Application.Game.Tenant.Report.TenantGameReport.Dto;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Enum;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.Game.Tenant.Report.TenantGameReport;

/// <summary>
/// 举报事实源管理服务。路由前缀 /api/tenantGameReportEntity。
/// <para>AddAsync：服务端强制 Status=Pending、HandlerId=null。</para>
/// <para>LinkPenaltyAsync：仅 Confirmed 举报可关联已存在处罚；写 ResultPenaltyId + HandlerId，不自动创建处罚。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class TenantGameReportEntityService
    : GenericCrudService<Entity.Tenant.Game.TenantGameReportEntity,
        AddTenantGameReportEntityRequest, UpdateTenantGameReportEntityRequest,
        TenantGameReportEntityResponse, PageTenantGameReportEntityRequest>
{
    private readonly CurrentUserContext _currentUserContext;
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGamePenaltyEntity> _penaltyRepository;

    public TenantGameReportEntityService(SqlSugarRepository<Entity.Tenant.Game.TenantGameReportEntity> repository,
        SqlSugarRepository<Entity.Tenant.Game.TenantGamePenaltyEntity> penaltyRepository,
        CurrentUserContext currentUserContext)
        : base(repository)
    {
        _currentUserContext = currentUserContext;
        _penaltyRepository = penaltyRepository;
    }

    /// <summary>
    /// 录入举报：AppId 必填且必须大于 0；Status/HandlerId 服务端强制。
    /// </summary>
    public override async Task<TenantGameReportEntityResponse> AddAsync(AddTenantGameReportEntityRequest input)
    {
        AppScopeRules.EnsureFactAppId(input.AppId, "举报");
        if (string.IsNullOrWhiteSpace(input.TargetId))
        {
            throw BusinessError.Oh("被举报对象标识不能为空");
        }

        if (string.IsNullOrWhiteSpace(input.EvidenceJson))
        {
            throw BusinessError.Oh("证据JSON不能为空");
        }

        var entity = input.Adapt<Entity.Tenant.Game.TenantGameReportEntity>();
        entity.Status = ReportStatusEnum.Pending;
        entity.HandlerId = null;
        await Repository.InsertWithDiffLogAsync(entity);
        return entity.Adapt<TenantGameReportEntityResponse>();
    }

    /// <summary>
    /// 分页查询：按 AppId / TargetType / TargetId / ReportType / Status 过滤。
    /// </summary>
    protected override ISugarQueryable<TenantGameReportEntityResponse> BuildPageQuery(PageTenantGameReportEntityRequest input)
    {
        var appId = AppScopeRules.EnsureFactAppId(input.AppId, "举报");
        return Repository.AsQueryable()
                         .Where(r => r.AppId == appId)
                         .WhereIF(input.TargetType.HasValue, r => r.TargetType == input.TargetType.Value)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.TargetId), r => r.TargetId == input.TargetId)
                         .WhereIF(input.ReportType.HasValue, r => r.ReportType == input.ReportType.Value)
                         .WhereIF(input.Status.HasValue, r => r.Status == input.Status.Value)
                         .Select<TenantGameReportEntityResponse>();
    }

    /// <summary>
    /// 关联处罚：仅 Confirmed 举报可关联；处罚必须存在；写 ResultPenaltyId + HandlerId，不自动创建处罚。
    /// </summary>
    /// <param name="input"></param>
    [HttpPost("linkPenalty")]
    [ApiDescriptionSettings(Name = "LinkPenalty")]
    public async Task LinkPenaltyAsync(LinkPenaltyTenantGameReportEntityRequest input)
    {
        var report = await Repository.GetFirstAsync(r => r.Id == input.Id);
        if (report == null)
        {
            throw BusinessError.Oh("举报不存在");
        }

        if (report.Status != ReportStatusEnum.Confirmed)
        {
            throw BusinessError.Oh("举报不是已坐实状态，无法关联处罚");
        }

        var penalty = await _penaltyRepository.GetFirstAsync(p => p.Id == input.PenaltyId);
        if (penalty == null)
        {
            throw BusinessError.Oh("处罚不存在");
        }

        if (penalty.AppId != report.AppId)
        {
            throw BusinessError.Oh("举报只能关联同应用下的处罚");
        }

        report.ResultPenaltyId = input.PenaltyId;
        report.HandlerId = _currentUserContext.UserId;
        await Repository.Context.Updateable(report)
                        .UpdateColumns(r => new { r.ResultPenaltyId, r.HandlerId, })
                        .ExecuteCommandAsync();
    }
}
#pragma warning restore CS8602, CS8604, CS8629