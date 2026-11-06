using GameFrameX.Client.Api.Services.Email;
using GameFrameX.Entity.Business.ClientApiConfig;

namespace GameFrameX.Core.Tests.ClientApi;

public class EmailSenderTests
{
    [Fact]
    public async Task SendTemplate_ReturnsFalseWhenUnconfigured_NoSystemFallback()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitBusinessTables(typeof(EmailConfigEntity));
            var sender = new EmailSender(database.MainDb);
            var result = await sender.SendTemplateAsync(EmailPurpose.PlayerRegisterCode,
                "player@example.test", 145225, "Subject", new Dictionary<string, string>());
            Assert.False(result);
        }
    }

    [Fact]
    public async Task GetFromEmailAddress_ReturnsNullWhenUnconfigured()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitBusinessTables(typeof(EmailConfigEntity));
            var sender = new EmailSender(database.MainDb);
            Assert.Null(await sender.GetFromEmailAddressAsync());
        }
    }

    [Fact]
    public async Task GetSnapshot_ReadsMainDatabaseWithoutRequestContext_RequiresCompleteCredentials()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitBusinessTables(typeof(EmailConfigEntity));
            database.MainDb.Insertable(new EmailConfigEntity
            {
                Id = 1, IsEnabled = true, FromEmailAddress = "sender@example.test",
                FromEmailSecretId = "secret-id", FromEmailSecretKey = "secret-key",
            }).ExecuteCommand();
            var sender = new EmailSender(database.MainDb);
            Assert.Equal("sender@example.test", await sender.GetFromEmailAddressAsync());
            database.MainDb.Updateable<EmailConfigEntity>()
                .SetColumns(row => row.FromEmailSecretKey == "").Where(row => row.Id == 1).ExecuteCommand();
            Assert.Null(await sender.GetSnapshotAsync(EmailPurpose.Default));
        }
    }
}
