using GameFrameX.Application.Game.App.GameApp;
using GameFrameX.Core.Service.TenantPackage.Dto;

namespace GameFrameX.Core.Tests.AppManagement;

public class GameAppQuotaTests
{
    [Fact]
    public void EnsureCanCreateApp_RejectsWhenQuotaReached()
    {
        var quota = new EffectiveQuotaResponse { MaxGameAppCount = 1, };

        var error = TenantGameAppEntityService.GetCreateAppQuotaError(1, quota);

        Assert.Contains("应用数已达上限", error);
    }
}