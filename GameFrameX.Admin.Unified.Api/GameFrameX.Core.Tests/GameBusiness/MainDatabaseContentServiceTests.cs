using GameFrameX.Application.NoticeCenter.GameAnnouncement;
using GameFrameX.Application.NoticeCenter.GameAnnouncement.Dto;
using GameFrameX.Application.NoticeCenter.GameMailTemplateCategory;
using GameFrameX.Application.NoticeCenter.GameMailTemplateCategory.Dto;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Business.Game;
using GameFrameX.Entity.Business.Game.Announcement;

namespace GameFrameX.Core.Tests.GameBusiness;

public sealed class MainDatabaseContentServiceTests
{
    [Fact]
    public async Task Announcement_CreateNormalizeAndFilter_WithoutApplicationContext()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitBusinessTables(typeof(GameAnnouncementEntity));
            var service = new GameAnnouncementService(new SqlSugarRepository<GameAnnouncementEntity>
            {
                Context = database.MainDb,
            }, null!);
            var created = await service.AddAsync(new AddGameAnnouncementEntityRequest
            {
                Title = "notice", Content = "content", Language = "  ",
                TitleLocalizationKey = " title.key ", ContentLocalizationKey = " content.key ",
            });
            var entity = await database.Repository<GameAnnouncementEntity>().GetByIdAsync(created.Id);
            Assert.Equal("default", entity.Language);
            Assert.Equal("title.key", entity.TitleLocalizationKey);
            Assert.Equal("content.key", entity.ContentLocalizationKey);
            Assert.Equal(created.Id, Assert.Single((await service.PageAsync(new PageGameAnnouncementEntityRequest
            {
                Language = "default", Title = "notice",
            })).Items).Id);
            Assert.Empty((await service.PageAsync(new PageGameAnnouncementEntityRequest { Language = "en" })).Items);
            await Assert.ThrowsAsync<BusinessException>(() => service.UpdateAsync(new UpdateGameAnnouncementEntityRequest
            {
                Id = created.Id + 1, Title = "missing", Content = "content",
            }));
        }
    }

    [Fact]
    public async Task MailTemplateCategory_CreateFilterAndMissingUpdateGuard_WithoutApplicationContext()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitBusinessTables(typeof(GameMailTemplateCategoryEntity));
            var service = new GameMailTemplateCategoryService(new SqlSugarRepository<GameMailTemplateCategoryEntity>
            {
                Context = database.MainDb,
            });
            var created = await service.AddAsync(new AddGameMailTemplateCategoryEntityRequest
            {
                Name = "operations", Description = "mail", Icon = "mail", IsEnabled = true,
            });
            Assert.Equal(created.Id, Assert.Single((await service.PageAsync(new PageGameMailTemplateCategoryEntityRequest
            {
                Name = "operations", IsEnabled = true,
            })).Items).Id);
            Assert.Empty((await service.PageAsync(new PageGameMailTemplateCategoryEntityRequest { Name = "missing" })).Items);
            await Assert.ThrowsAsync<BusinessException>(() => service.UpdateAsync(new UpdateGameMailTemplateCategoryEntityRequest
            {
                Id = created.Id + 1, Name = "missing", Description = "mail", Icon = "mail",
            }));
        }
    }
}
