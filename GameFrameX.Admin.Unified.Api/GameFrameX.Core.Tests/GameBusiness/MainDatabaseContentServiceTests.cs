using GameFrameX.Application.Game.Announcement.GameAnnouncement;
using GameFrameX.Application.Game.Announcement.GameAnnouncement.Dto;
using GameFrameX.Application.Game.Mail.GameMailTemplateCategory;
using GameFrameX.Application.Game.Mail.GameMailTemplateCategory.Dto;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Entity.Tenant.Game.Announcement;

namespace GameFrameX.Core.Tests.GameBusiness;

public sealed class MainDatabaseContentServiceTests
{
    [Fact]
    public async Task Announcement_CreateNormalizeAndFilter_WithoutApplicationContext()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitTenantTables(typeof(TenantGameAnnouncementEntity));
            var service = new TenantGameAnnouncementEntityService(new SqlSugarRepository<TenantGameAnnouncementEntity>
            {
                Context = database.MainDb,
            }, null!);
            var created = await service.AddAsync(new AddTenantGameAnnouncementEntityRequest
            {
                Title = "notice", Content = "content", Language = "  ",
                TitleLocalizationKey = " title.key ", ContentLocalizationKey = " content.key ",
            });
            var entity = await database.Repository<TenantGameAnnouncementEntity>().GetByIdAsync(created.Id);
            Assert.Equal("default", entity.Language);
            Assert.Equal("title.key", entity.TitleLocalizationKey);
            Assert.Equal("content.key", entity.ContentLocalizationKey);
            Assert.Equal(created.Id, Assert.Single((await service.PageAsync(new PageTenantGameAnnouncementEntityRequest
            {
                Language = "default", Title = "notice",
            })).Items).Id);
            Assert.Empty((await service.PageAsync(new PageTenantGameAnnouncementEntityRequest { Language = "en" })).Items);
            await Assert.ThrowsAsync<BusinessException>(() => service.UpdateAsync(new UpdateTenantGameAnnouncementEntityRequest
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
            database.InitTenantTables(typeof(TenantGameMailTemplateCategoryEntity));
            var service = new TenantGameMailTemplateCategoryEntityService(new SqlSugarRepository<TenantGameMailTemplateCategoryEntity>
            {
                Context = database.MainDb,
            });
            var created = await service.AddAsync(new AddTenantGameMailTemplateCategoryEntityRequest
            {
                Name = "operations", Description = "mail", Icon = "mail", IsEnabled = true,
            });
            Assert.Equal(created.Id, Assert.Single((await service.PageAsync(new PageTenantGameMailTemplateCategoryEntityRequest
            {
                Name = "operations", IsEnabled = true,
            })).Items).Id);
            Assert.Empty((await service.PageAsync(new PageTenantGameMailTemplateCategoryEntityRequest { Name = "missing" })).Items);
            await Assert.ThrowsAsync<BusinessException>(() => service.UpdateAsync(new UpdateTenantGameMailTemplateCategoryEntityRequest
            {
                Id = created.Id + 1, Name = "missing", Description = "mail", Icon = "mail",
            }));
        }
    }
}
