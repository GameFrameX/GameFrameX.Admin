using GameFrameX.Application.ServerCenter.GameArea;
using GameFrameX.Application.ServerCenter.GameArea.Dto;
using GameFrameX.Client.Api.GameArea;
using GameFrameX.Client.Api.GameArea.Dto;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;

namespace GameFrameX.Core.Tests.ServerCenter.GameAreaMerge;

public sealed class GameAreaMergeRehearsalTests
{
    [Fact]
    public async Task Rehearsal_OnlineEnvelopeRoundtrip_RetiresSourcesFromClientButKeepsManagementRows()
    {
        using (var context = new GameAreaMergeTestContext())
        using (var httpClient = new HttpClient())
        {
            var plan = await context.ApproveAsync();
            await context.Service.ExecuteAsync(new() { Id = plan.Id, BackupConfirmed = true, BackupIdentifier = "rehearsal-backup" });
            var client = new GameAreaController(context.Repository<TenantGameAreaEntity>(), FakeOptionsProvider.WithAreaPolicy(false));
            var result = await client.GetAreaList(new GameAreaGetAreaListRequest());
            var areas = ClientApiResultReader.Data(ClientApiResultReader.Read(result))[0].GetProperty("GameAreaList");
            var identifiers = areas.EnumerateArray().Select(area => area.GetProperty("Id").GetInt64()).ToArray();
            Assert.Contains(101, identifiers);
            Assert.DoesNotContain(102, identifiers);
            Assert.DoesNotContain(103, identifiers);
            var management = new GameAreaService(context.Repository<TenantGameAreaEntity>(), context.Repository<TenantGameAreaClassifyEntity>(), httpClient);
            var page = await management.PageAsync(new PageTenantGameAreaEntityRequest());
            foreach (var identifier in new long[] { 102, 103 })
            {
                var source = Assert.Single(page.Items.Where(area => area.Id == identifier));
                Assert.Equal(GameServerStatus.Merged, source.ServerStatus);
                Assert.Equal(101, source.MergedIntoServerId);
                Assert.False(source.IsEnabled);
            }
        }
    }
}
