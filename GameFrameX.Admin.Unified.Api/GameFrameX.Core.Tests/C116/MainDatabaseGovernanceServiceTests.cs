using GameFrameX.Application.PlayerCenter.GamePenalty;
using GameFrameX.Application.PlayerCenter.GamePenaltyAppeal;
using GameFrameX.Application.GmCenter.GameReport;
using GameFrameX.Core.Service.User;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Business.Game;
using Microsoft.AspNetCore.Http;

namespace GameFrameX.Core.Tests.C116;

public sealed class MainDatabaseGovernanceServiceTests
{
    [Fact]
    public async Task PenaltyAndAppeal_KeepStateTransitionsWithoutApplication()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitBusinessTables(typeof(GamePenaltyEntity), typeof(GamePenaltyAppealEntity));
            var penalties = new SqlSugarRepository<GamePenaltyEntity> { Context = database.MainDb };
            var appeals = new SqlSugarRepository<GamePenaltyAppealEntity> { Context = database.MainDb };
            var user = new CurrentUserContext(new HttpContextAccessor());
            var penaltyService = new GamePenaltyService(penalties, user, null!);
            var appealService = new GamePenaltyAppealService(appeals, penalties, user);
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
            database.InitBusinessTables(typeof(GamePenaltyEntity), typeof(GameReportEntity));
            var reports = new SqlSugarRepository<GameReportEntity> { Context = database.MainDb };
            var service = new GameReportService(reports,
                new SqlSugarRepository<GamePenaltyEntity> { Context = database.MainDb },
                new CurrentUserContext(new HttpContextAccessor()));
            var report = await service.AddAsync(new() { ReporterId = 1, TargetId = "account-1", EvidenceJson = "{}" });
            Assert.Equal(ReportStatusEnum.Pending, report.Status);
            Assert.Single((await service.PageAsync(new() { TargetId = "account-1" })).Items);
            await Assert.ThrowsAnyAsync<Exception>(() => service.LinkPenaltyAsync(new() { Id = report.Id, PenaltyId = 1 }));
            await Assert.ThrowsAnyAsync<Exception>(() => service.AddAsync(new() { TargetId = "account-1", EvidenceJson = "" }));
        }
    }
}
