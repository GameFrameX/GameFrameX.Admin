using GameFrameX.Application.NoticeCenter.GameRewardTemplate;
using GameFrameX.Application.NoticeCenter.GameRewardTemplate.Dto;
using GameFrameX.Application.NoticeCenter.GameRewardTemplateGroup;
using GameFrameX.Application.NoticeCenter.GameRewardTemplateGroup.Dto;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Business.Game;

namespace GameFrameX.Core.Tests.GameBusiness;

public sealed class MainDatabaseRewardServiceTests
{
    [Fact]
    public async Task CreateSelectAndCopySnapshot_PreserveRewardValidationWithoutApplication()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitBusinessTables(typeof(GameRewardTemplateEntity), typeof(GameRewardTemplateGroupEntity));
            var groups = new SqlSugarRepository<GameRewardTemplateGroupEntity> { Context = database.MainDb };
            var groupService = new GameRewardTemplateGroupService(groups);
            var group = await groupService.AddAsync(new AddGameRewardTemplateGroupEntityRequest
            {
                Name = "rewards", Description = "rewards", IsEnabled = true,
            });
            var service = new GameRewardTemplateService(new SqlSugarRepository<GameRewardTemplateEntity>
            {
                Context = database.MainDb,
            }, groups);
            var created = await service.AddAsync(new AddGameRewardTemplateEntityRequest
            {
                Name = "gift", Description = "gift", GroupId = group.Id,
                RewardItemsJson = "[{\"id\":1,\"itemId\":10,\"count\":2}]", IsEnabled = true,
            });
            var snapshot = await service.CopySnapshotAsync(new GameRewardTemplateSnapshotRequest { Id = created.Id });
            Assert.Equal(group.Id, snapshot.GroupId);
            Assert.Equal(1, snapshot.SnapshotSchemaVersion);
            Assert.Equal(created.Id, Assert.Single(await service.SelectAsync(new() { SearchKey = "gift" })).Id);
            Assert.Equal("rewards", Assert.Single((await service.PageAsync(new() { GroupId = group.Id })).Items).GroupName);
            await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(new AddGameRewardTemplateEntityRequest
            {
                Name = "invalid", Description = "invalid", GroupId = group.Id,
                RewardItemsJson = "[{\"id\":1,\"count\":0}]",
            }));
            await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(new AddGameRewardTemplateEntityRequest
            {
                Name = "missing", Description = "missing", GroupId = group.Id + 1,
                RewardItemsJson = snapshot.RewardItemsJson,
            }));
        }
    }
}
