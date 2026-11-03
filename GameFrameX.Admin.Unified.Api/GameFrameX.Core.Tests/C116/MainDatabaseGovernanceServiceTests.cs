using GameFrameX.Application.Game.Tenant.Penalty.TenantGamePenalty;
using GameFrameX.Application.Game.Tenant.Penalty.TenantGamePenaltyAppeal;
using GameFrameX.Application.Game.Tenant.Report.TenantGameReport;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;
using Microsoft.AspNetCore.Http;

namespace GameFrameX.Core.Tests.C116;

public sealed class MainDatabaseGovernanceServiceTests
{
    [Fact]
    public async Task PenaltyAndAppeal_KeepStateTransitionsWithoutApplication()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitTenantTables(typeof(TenantGamePenaltyEntity), typeof(TenantGamePenaltyAppealEntity));
            var penalties = new SqlSugarRepository<TenantGamePenaltyEntity> { Context = database.MainDb };
            var appeals = new SqlSugarRepository<TenantGamePenaltyAppealEntity> { Context = database.MainDb };
            var user = new CurrentUserContext(new HttpContextAccessor());
            var penaltyService = new TenantGamePenaltyEntityService(penalties, user, null!);
            var appealService = new TenantGamePenaltyAppealEntityService(appeals, penalties, user);
            var penalty = await penaltyService.AddAsync(new() { TargetId = "account-1", Reason = "test", ActionType = PenaltyActionTypeEnum.Ban });
            Assert.Equal(PenaltyStatusEnum.Active, penalty.Status);
            Assert.Single((await penaltyService.PageAsync(new() { TargetId = "account-1" })).Items);
            var appeal = await appealService.AddAsync(new() { PenaltyId = penalty.Id, Reason = "appeal" });
            await Assert.ThrowsAnyAsync<Exception>(() => appealService.AddAsync(new() { PenaltyId = penalty.Id, Reason = "duplicate" }));
            await appealService.ReviewAsync(new() { Id = appeal.Id, Approve = true, Remark = "approved" });
            Assert.Equal(PenaltyStatusEnum.Revoked, (await penalties.GetFirstAsync(x => x.Id == penalty.Id)).Status);
            await Assert.ThrowsAnyAsync<Exception>(() => appealService.ReviewAsync(new() { Id = appeal.Id, Approve = true }));
        }
    }

    [Fact]
    public async Task Report_RequiresConfirmationBeforeLinkingPenalty()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitTenantTables(typeof(TenantGamePenaltyEntity), typeof(TenantGameReportEntity));
            var reports = new SqlSugarRepository<TenantGameReportEntity> { Context = database.MainDb };
            var service = new TenantGameReportEntityService(reports,
                new SqlSugarRepository<TenantGamePenaltyEntity> { Context = database.MainDb },
                new CurrentUserContext(new HttpContextAccessor()));
            var report = await service.AddAsync(new() { ReporterId = 1, TargetId = "account-1", EvidenceJson = "{}" });
            Assert.Equal(ReportStatusEnum.Pending, report.Status);
            Assert.Single((await service.PageAsync(new() { TargetId = "account-1" })).Items);
            await Assert.ThrowsAnyAsync<Exception>(() => service.LinkPenaltyAsync(new() { Id = report.Id, PenaltyId = 1 }));
            await Assert.ThrowsAnyAsync<Exception>(() => service.AddAsync(new() { TargetId = "account-1", EvidenceJson = "" }));
        }
    }
}
