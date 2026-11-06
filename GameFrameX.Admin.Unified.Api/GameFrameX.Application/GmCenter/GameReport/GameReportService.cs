using GameFrameX.Application.GmCenter.GameReport.Dto;
using GameFrameX.Core.Service.User;
using GameFrameX.Entity.Enum;

#pragma warning disable CS8602, CS8604, CS8629 // SqlSugar WhereIF(condition, lambda)：lambda 仅在 condition=true 时执行；NRT 无法跨方法传播条件守卫，lambda 内 .Trim()/.Value 实际安全
namespace GameFrameX.Application.GmCenter.GameReport;

/// <summary>
/// 举报事实源管理服务。路由前缀 /api/gameReportEntity。
/// <para>AddAsync：服务端强制 Status=Pending、HandlerId=null。</para>
/// <para>LinkPenaltyAsync：仅 Confirmed 举报可关联已存在处罚；写 ResultPenaltyId + HandlerId，不自动创建处罚。</para>
/// </summary>
[ApiDescriptionSettings(GameConst.GroupName, Order = 100)]
public class GameReportService
    : GenericCrudService<Entity.Business.Game.GameReportEntity,
        AddGameReportEntityRequest, UpdateGameReportEntityRequest,
        GameReportResponse, PageGameReportEntityRequest>
{
    private readonly CurrentUserContext _currentUserContext;
    private readonly SqlSugarRepository<Entity.Business.Game.GamePenaltyEntity> _penaltyRepository;

    public GameReportService(SqlSugarRepository<Entity.Business.Game.GameReportEntity> repository,
        SqlSugarRepository<Entity.Business.Game.GamePenaltyEntity> penaltyRepository,
        CurrentUserContext currentUserContext)
        : base(repository)
    {
        _currentUserContext = currentUserContext;
        _penaltyRepository = penaltyRepository;
    }

    /// <summary>
    /// 录入举报：Status/HandlerId 服务端强制。
    /// </summary>
    public override async Task<GameReportResponse> AddAsync(AddGameReportEntityRequest input)
    {
        if (string.IsNullOrWhiteSpace(input.TargetId))
        {
            throw BusinessError.Oh("被举报对象标识不能为空");
        }

        if (string.IsNullOrWhiteSpace(input.EvidenceJson))
        {
            throw BusinessError.Oh("证据JSON不能为空");
        }

        var entity = input.Adapt<Entity.Business.Game.GameReportEntity>();
        entity.Status = ReportStatusEnum.Pending;
        entity.HandlerId = null;
        await Repository.InsertWithDiffLogAsync(entity);
        return entity.Adapt<GameReportResponse>();
    }

    /// <summary>
    /// 分页查询：按 TargetType / TargetId / ReportType / Status 过滤。
    /// </summary>
    protected override ISugarQueryable<GameReportResponse> BuildPageQuery(PageGameReportEntityRequest input)
    {
        return Repository.AsQueryable()

                         .WhereIF(input.TargetType.HasValue, r => r.TargetType == input.TargetType.Value)
                         .WhereIF(!string.IsNullOrWhiteSpace(input.TargetId), r => r.TargetId == input.TargetId)
                         .WhereIF(input.ReportType.HasValue, r => r.ReportType == input.ReportType.Value)
                         .WhereIF(input.Status.HasValue, r => r.Status == input.Status.Value)
                         .Select<GameReportResponse>();
    }

    /// <summary>
    /// 关联处罚：仅 Confirmed 举报可关联；处罚必须存在；写 ResultPenaltyId + HandlerId，不自动创建处罚。
    /// </summary>
    /// <param name="input"></param>
    [HttpPost("linkPenalty")]
    [ApiDescriptionSettings(Name = "LinkPenalty")]
    public async Task LinkPenaltyAsync(LinkPenaltyGameReportEntityRequest input)
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

        report.ResultPenaltyId = input.PenaltyId;
        report.HandlerId = _currentUserContext.UserId;
        await Repository.Context.Updateable(report)
                        .UpdateColumns(r => new { r.ResultPenaltyId, r.HandlerId, })
                        .ExecuteCommandAsync();
    }
}
#pragma warning restore CS8602, CS8604, CS8629