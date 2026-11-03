using GameFrameX.Client.Api.GameUserEmail;
using GameFrameX.Client.Api.GameUserEmail.Dto;
using GameFrameX.Client.Api.Services.Email;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Core.Tests.ClientApi;

public sealed class ClientApiUserEmailEndpointTests
{
    [Fact]
    public async Task SendRegister_UsesMainDatabaseWithoutLegacyContext_AndEnforcesInterval()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitTenantTables(typeof(TenantGameUserEmailEntity), typeof(TenantGameUserEntity));
            var sender = new RecordingEmailSender();
            var controller = CreateController(database, sender);
            var request = new GameUserMailRegisterEntityRequest
            {
                Email = $"{Guid.NewGuid():N}@example.test", Language = "zh",
            };
            var first = ClientApiResultReader.Read(await controller.SendRegister(request));
            var second = ClientApiResultReader.Read(await controller.SendRegister(request));
            Assert.Equal(0, ClientApiResultReader.Code(first));
            Assert.NotEqual(0, ClientApiResultReader.Code(second));
            Assert.Equal(1, sender.SendCount);
            var record = await database.MainDb.Queryable<TenantGameUserEmailEntity>().SingleAsync();
            Assert.Equal(request.Email, record.UserName);
            Assert.Equal(6, record.Code.Length);
            Assert.Equal(TenantEmailPurpose.PlayerRegisterCode, sender.Purpose);
        }
    }

    [Fact]
    public async Task SendReset_UnknownAccountReturnsSuccessWithoutSending()
    {
        using (var database = new ClientApiTestDb())
        {
            database.InitTenantTables(typeof(TenantGameUserEmailEntity), typeof(TenantGameUserEntity));
            var sender = new RecordingEmailSender();
            var controller = CreateController(database, sender);
            var result = ClientApiResultReader.Read(await controller.SendReset(new GameUserMailResetEntityRequest
            {
                Email = $"{Guid.NewGuid():N}@example.test", Language = "en",
            }));
            Assert.Equal(0, ClientApiResultReader.Code(result));
            Assert.Equal(0, sender.SendCount);
            Assert.Equal(0, await database.MainDb.Queryable<TenantGameUserEmailEntity>().CountAsync());
        }
    }

    private static GameUserEmailController CreateController(ClientApiTestDb database, RecordingEmailSender sender)
    {
        var controller = new GameUserEmailController(database.Repository<TenantGameUserEmailEntity>(),
            database.Repository<TenantGameUserEntity>(), sender);
        database.AttachContext(controller, includeTenant: false, includeApp: false);
        return controller;
    }

    private sealed class RecordingEmailSender : ITenantEmailSender
    {
        public int SendCount { get; private set; }
        public TenantEmailPurpose Purpose { get; private set; }

        public Task<bool> SendTemplateAsync(TenantEmailPurpose purpose, string receiverEmail, ulong templateId,
            string subject, IDictionary<string, string> templateData)
        {
            SendCount++;
            Purpose = purpose;
            return Task.FromResult(true);
        }

        public Task<string?> GetFromEmailAddressAsync() => Task.FromResult<string?>(null);
        public Task<TenantEmailOptionsSnapshot?> GetSnapshotAsync(TenantEmailPurpose purpose) =>
            Task.FromResult<TenantEmailOptionsSnapshot?>(null);
    }
}
